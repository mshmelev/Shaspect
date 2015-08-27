using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;


namespace Shaspect.Builder
{
    internal class AspectsInjector
    {
        private readonly string assemblyFile;
        private readonly string references;
        private AssemblyDefinition assembly;
        private InitClassGenerator initClassGenerator;


        public AspectsInjector(string assemblyFile, string references)
        {
            this.assemblyFile = assemblyFile;
            this.references = references;
        }


        public int ProcessAssembly()
        {
            LoadAssembly();
            
            var assemblyAspects = Enumerable.Empty<AspectDeclaration>().NestWith (GetAspects (assembly, assembly.MainModule));
            initClassGenerator = new InitClassGenerator (assembly);

            foreach (var module in assembly.Modules)
            {
                var types = module.GetAllTypes().Where (t => t.IsClass && !t.IsAbstract);
                var baseAspectType = module.Import (typeof (BaseAspectAttribute));

                foreach (var type in types)
                {
                    if (IsInheritedFrom (type, baseAspectType))       // ignore aspects themselves
                        continue;

                    var typeAspects = assemblyAspects.NestWith (GetAspects (type, module));
                    var processedMethods = new HashSet<uint>();

                    // properties
                    foreach (var property in type.Properties)
                    {
                        var propAspects = typeAspects.NestWith (GetAspects (property, module));
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

            return initClassGenerator.EmittedAspects;
        }


        private void ProcessMethod (MethodDefinition method, IEnumerable<AspectDeclaration> externalAspects)
        {
            var aspects = externalAspects.NestWith (GetAspects (method, method.Module));

            foreach (var aspect in aspects)
            {
                if ((aspect.Aspect.AttributeType.Resolve().Attributes & TypeAttributes.NestedPrivate) == TypeAttributes.NestedPrivate)
                    throw new ApplicationException (String.Format ("Aspect {0} must be declared as public or internal.", aspect.Aspect.AttributeType));

                if (!IsApplicableElementTarget (method, aspect))
                    continue;

                var aspectField = initClassGenerator.BuildAspectInitCode (aspect);

                method.Body.Instructions.Insert (0,
                    Instruction.Create (OpCodes.Callvirt, method.Module.Import (typeof (BaseAspectAttribute).GetMethod ("OnEntry"))));
                method.Body.Instructions.Insert (0, Instruction.Create (OpCodes.Ldsfld, aspectField));
            }
        }


        private bool IsApplicableElementTarget (MethodDefinition method, AspectDeclaration aspect)
        {
            var elemetTargets = aspect.ElementTargets;
            if (elemetTargets == ElementTargets.Default)
                return true;

            if ((method.Attributes & MethodAttributes.Static) == MethodAttributes.Static
                && method.Name == ".cctor"
                && (elemetTargets & ElementTargets.StaticConstructor) != ElementTargets.StaticConstructor)
            {
                return false;
            }

            if ((method.Attributes & MethodAttributes.Static) != MethodAttributes.Static
                && method.Name == ".ctor"
                && (elemetTargets & ElementTargets.InstanceConstructor) != ElementTargets.InstanceConstructor)
            {
                return false;
            }

            return true;
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

            assembly.Write (assemblyFile, writeParams);
        }


        private List<AspectDeclaration> GetAspects<T> (T member, ModuleDefinition module)
            where T : ICustomAttributeProvider
        {
            var res = new List<AspectDeclaration>();

            var baseAspectType = module.Import (typeof (BaseAspectAttribute));
            foreach (var attr in member.CustomAttributes)
            {
                if (IsInheritedFrom (attr.AttributeType, baseAspectType))
                    res.Add (new AspectDeclaration {Aspect = attr, Declarator = member });
            }

            return res;
        }


        private static bool IsInheritedFrom (TypeReference childType, TypeReference baseType)
        {
            try
            {
                do
                {
                    if (childType.FullName == baseType.FullName)
                        return true;
                    childType = childType.Resolve().BaseType;
                } while (childType != null);
            }
            catch (AssemblyResolutionException)
            {
            }

            return false;
        }
    }
}