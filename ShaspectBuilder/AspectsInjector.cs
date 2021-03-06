﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;
using Shaspect.Builder.Tools;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using TypeAttributes = Mono.Cecil.TypeAttributes;


namespace Shaspect.Builder
{
    internal class AspectsInjector
    {
        private readonly string assemblyFile;
        private readonly string references;
        private readonly string keyFilePath;
        private readonly string keyContainerName;
        private AssemblyDefinition assembly;
        private InitClassGenerator initClassGenerator;


        public AspectsInjector (string assemblyFile, string references, string keyFilePath, string keyContainerName)
        {
            this.assemblyFile = assemblyFile;
            this.references = references;
            this.keyFilePath = keyFilePath;
            this.keyContainerName = keyContainerName;
        }


        public bool ProcessAssembly()
        {
            LoadAssembly();

            if (InitClassGenerator.IsInitClassCreated (assembly))       // prevent double processing
                return true;

            initClassGenerator = new InitClassGenerator (assembly);
            var assemblyAspects = GetAspects (assembly, assembly.MainModule);

            foreach (var module in assembly.Modules)
            {
                var types = module.GetAllTypes().Where (t => t.IsClass && !t.IsCompilerGenerated());
                var baseAspectType = module.Import (typeof (BaseAspectAttribute));

                foreach (var type in types)
                {
                    if (type.IsInheritedFrom (baseAspectType))       // ignore aspects themselves
                        continue;

                    var typeAspects = GetAllNestedTypesAspects (type, assemblyAspects);
                    var processedMethods = new HashSet<uint>();

                    // properties
                    foreach (var property in type.Properties)
                    {
                        var propAspects = typeAspects.Union (GetAspects (property, module)).ToList();
                        if (property.GetMethod != null)
                        {
                            ProcessMethod (property.GetMethod, propAspects);
                            processedMethods.Add (property.GetMethod.MetadataToken.ToUInt32());
                        }
                        if (property.SetMethod != null)
                        {
                            ProcessMethod (property.SetMethod, propAspects);
                            processedMethods.Add (property.SetMethod.MetadataToken.ToUInt32());
                        }
                    }

                    // rest of the methods
                    foreach (var method in type.Methods)
                    {
                        if (!processedMethods.Contains (method.MetadataToken.ToUInt32()))
                            ProcessMethod (method, typeAspects);
                    }
                }
            }
         
            initClassGenerator.FinalizeBuilding();

            if (initClassGenerator.EmittedAspects > 0)
                SaveAssembly();

            return initClassGenerator.EmittedAspects > 0;
        }


        private List<AspectDeclaration> GetAllNestedTypesAspects (TypeDefinition type, IEnumerable<AspectDeclaration> assemblyAspects)
        {
            var declaringType = type.DeclaringType;

            List<AspectDeclaration> externalAspects;
            if (declaringType != null)
                externalAspects= GetAllNestedTypesAspects (declaringType, assemblyAspects);
            else
                externalAspects= new List<AspectDeclaration> (assemblyAspects);
            
            externalAspects.AddRange (GetAspects (type, type.Module));
            return externalAspects;
        }


        private void ProcessMethod (MethodDefinition method, IEnumerable<AspectDeclaration> externalAspects)
        {
            var aspects = NestingStrategy.GetApplicableAspects (externalAspects.Union (GetAspects (method, method.Module)), method);
            
            // Reverse order because aspects are injected in reverse order
            aspects = aspects.OrderByDescending (a => a.Order).ThenBy (a => a.NestingLevel);       

            foreach (var aspect in aspects)
            {
                if ((aspect.Aspect.AttributeType.Resolve().Attributes & TypeAttributes.NestedPrivate) == TypeAttributes.NestedPrivate)
                    throw new ApplicationException (String.Format ("Aspect {0} must be declared as public or internal class.", aspect.Aspect.AttributeType));

                FieldDefinition aspectField, methodField;
                initClassGenerator.BuildAspectInitCode (method, aspect.Aspect, out aspectField, out methodField);

                var methodInjector = new MethodAspectInjector (method, aspectField, methodField);
                methodInjector.Inject();
            }
        }


        private void LoadAssembly()
        {
            var readParams = new ReaderParameters (ReadingMode.Deferred);

            if (File.Exists (Path.ChangeExtension (assemblyFile, ".pdb")))
            {
                readParams.ReadSymbols = true;
                readParams.SymbolReaderProvider = new PdbReaderProvider();
            }
            else if (File.Exists (Path.ChangeExtension (assemblyFile, ".mdb")))
            {
                readParams.ReadSymbols = true;
                readParams.SymbolReaderProvider = new MdbReaderProvider();
            }

            readParams.AssemblyResolver = new AssemblyResolver (references);

            assembly = AssemblyDefinition.ReadAssembly (assemblyFile, readParams);
        }


        private void SaveAssembly()
        {
            var writeParams = new WriterParameters();

            // debug symbols
            if (File.Exists (Path.ChangeExtension (assemblyFile, ".pdb")))
            {
                writeParams.WriteSymbols = true;
                writeParams.SymbolWriterProvider = new PdbWriterProvider();
            }
            else if (File.Exists (Path.ChangeExtension (assemblyFile, ".mdb")))
            {
                writeParams.WriteSymbols = true;
                writeParams.SymbolWriterProvider = new MdbWriterProvider();
            }

            // strong name signing
            var signingKey = RetrieveSigningKey();
            if (signingKey != null)
                writeParams.StrongNameKeyPair = signingKey;

            assembly.Write (assemblyFile, writeParams);
        }


        private StrongNameKeyPair RetrieveSigningKey()
        {
            if (!String.IsNullOrEmpty (keyContainerName))
                return new StrongNameKeyPair (keyContainerName);

            if (!String.IsNullOrEmpty (keyFilePath))
            {
                string filePath = keyFilePath;
                if (filePath.EndsWith (".pfx", StringComparison.InvariantCultureIgnoreCase))
                {
                    filePath = Path.ChangeExtension (filePath, ".snk");
                    if (!File.Exists (filePath))
                        throw new ApplicationException (".pfx file cannot be used to re-sign assembly. Either import the .pfx certificate, or put .snk file beside.");
                }
                return new StrongNameKeyPair (File.ReadAllBytes (filePath));
            }

            return null;
        }


        private List<AspectDeclaration> GetAspects<T> (T member, ModuleDefinition module)
            where T : ICustomAttributeProvider
        {
            var res = new List<AspectDeclaration>();

            var baseAspectType = module.Import (typeof (BaseAspectAttribute));
            foreach (var attr in member.CustomAttributes)
            {
                if (attr.AttributeType.IsInheritedFrom (baseAspectType))
                    res.Add (new AspectDeclaration {Aspect = attr, Declarator = member });
            }

            return res;
        }


    }
}