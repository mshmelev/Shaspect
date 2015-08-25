using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;


namespace Shaspect.Builder
{
    public class ShaspectBuildTask : Task
    {
        private AssemblyDefinition assembly;
        private ModuleDefinition mainModule;
        private TypeDefinition colClass;
        private MethodDefinition colCtor;
        private MethodDefinition getMemberAttrInstanceMethod;
        private int emittedAspects;

        [Required]
        public string AssemblyFile { get; set; }

        [Required]
        public string References { get; set; }

        public string KeyFile { get; set; }


        public override bool Execute()
        {
            try
            {

                var stopwatch = Stopwatch.StartNew();

                LoadAssembly();
                CreateAspectsCollectionClass();

                // TODO: aspect in a referenced assembly (assembly resolver)
                // TODO: handle signed assemblies
                // TODO: on class level
                // TODO: nesting (with overwriting) on assembly/class/method/(get/set property) levels
                // TODO: inheritance
                // TODO: interfaces
                // TODO: specifying targets (properties, functions)
                // TODO: aspect on parameters (e.g. check parameter is not null)



                emittedAspects = 0;
                foreach (var module in assembly.Modules)
                {
                    var methods =
                        module.GetAllTypes().Where (t => t.IsClass && !t.IsAbstract).SelectMany (t => t.Methods);
                    foreach (var method in methods)
                        ProcessMethod (method, Enumerable.Empty<AspectDeclaration>());

                    var properties =
                        module.GetAllTypes().Where (t => t.IsClass && !t.IsAbstract).SelectMany (t => t.Properties);
                    foreach (var property in properties)
                    {
                        var propAspects = GetAspects (property, module);
                        if (property.GetMethod != null)
                            ProcessMethod (property.GetMethod, propAspects);
                        if (property.SetMethod != null)
                            ProcessMethod (property.SetMethod, propAspects);
                    }

                }
                colCtor.Body.Instructions.Add (OpCodes.Ret);

                if (emittedAspects > 0)
                    SaveAssembly();
                else
                    Log.LogWarning (
                        "No aspects detected in {0}. You can uninstall Shaspect package for this assembly to speed up the build.",
                        AssemblyFile);

                stopwatch.Stop();
                Log.LogMessage ("ShaspectBuildTask took {0}ms", stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (ApplicationException ex)
            {
                Log.LogError (ex.Message);
            }

            return false;
        }


        private void ProcessMethod (MethodDefinition method, IEnumerable<AspectDeclaration> externalAspects)
        {
            var aspects = GetAspects (method, method.Module).Union (externalAspects);

            foreach (var aspect in aspects)
            {
                if ((aspect.Aspect.AttributeType.Resolve().Attributes & TypeAttributes.NestedPrivate) == TypeAttributes.NestedPrivate)
                    throw new ApplicationException (String.Format ("Aspect {0} must be declared as public or internal.", aspect.Aspect.AttributeType));

                var aspectField = BuildAspectInitCode(aspect);

                method.Body.Instructions.Insert (0,
                    Instruction.Create (OpCodes.Callvirt, method.Module.Import (typeof (BaseAspectAttribute).GetMethod ("OnEntry"))));
                method.Body.Instructions.Insert (0, Instruction.Create (OpCodes.Ldsfld, aspectField));
                
                ++emittedAspects;
            }
        }


        private FieldDefinition BuildAspectInitCode (AspectDeclaration aspect)
        {
            var ctor = colCtor.Body.Instructions;

            var aspectField = new FieldDefinition ("Aspect_" + emittedAspects,
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, aspect.Aspect.AttributeType);
            colClass.Fields.Add (aspectField);

            // the IL code bellow is generated from:
/*
            var methodInfo= typeof (Class1)
                .GetMethod (        // or GetProperty (...
                    "F1",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                    null,
                    CallingConventions.Any,
                    new[] {typeof (int), typeof (string)},
                    null
                );
            
            Aspect_0= (Test1Attribute)GetMethodAttrInstance (methodInfo, typeof (Test1Attribute));
*/

            ctor.Add (OpCodes.Ldtoken, aspect.Declarator.DeclaringType);
            ctor.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));
            ctor.Add (OpCodes.Ldstr, aspect.Declarator.Name);
            ctor.Add (OpCodes.Ldc_I4,
                (int) (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
            ctor.Add (OpCodes.Ldnull);

            if (aspect.Declarator is PropertyReference)
            {
                ctor.Add (OpCodes.Ldtoken, ((PropertyReference) aspect.Declarator).PropertyType);
                ctor.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));
            }
            else
            {
                ctor.Add (OpCodes.Ldc_I4, (int) CallingConventions.Any);
            }

            var memberParams = (aspect.Declarator is PropertyReference)
                ? ((PropertyReference) aspect.Declarator).Parameters
                : ((MethodReference) aspect.Declarator).Parameters;

            ctor.Add (OpCodes.Ldc_I4, memberParams.Count);
            ctor.Add (OpCodes.Newarr, mainModule.Import (typeof (Type)));
            ctor.Add (OpCodes.Stloc_0);
            ctor.Add (OpCodes.Ldloc_0);

            for (int i = 0; i < memberParams.Count; ++i)
            {
                ctor.Add (OpCodes.Ldc_I4, i);
                ctor.Add (OpCodes.Ldtoken, memberParams[i].ParameterType);
                ctor.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));
                ctor.Add (OpCodes.Stelem_Ref);
                ctor.Add (OpCodes.Ldloc_0);
            }

            ctor.Add (OpCodes.Ldnull);
            if (aspect.Declarator is PropertyReference)
            {
                ctor.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetProperty", new[]
                {
                    typeof (string), typeof (BindingFlags), typeof (Binder), typeof (Type), typeof (Type[]),
                    typeof (ParameterModifier[])
                })));
            }
            else
            {
                ctor.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetMethod", new[]
                {
                    typeof (string), typeof (BindingFlags), typeof (Binder), typeof (CallingConventions), typeof (Type[]),
                    typeof (ParameterModifier[])
                })));
            }

            ctor.Add (OpCodes.Ldtoken, aspect.Aspect.AttributeType);
            ctor.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));

            ctor.Add (OpCodes.Call, getMemberAttrInstanceMethod);
            ctor.Add (OpCodes.Castclass, aspect.Aspect.AttributeType);
            ctor.Add (OpCodes.Stsfld, aspectField);
            return aspectField;
        }


        private void CreateAspectsCollectionClass()
        {
            colClass = new TypeDefinition ("Shaspect.Implementation",
                "AspectsCollection",
                TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.AnsiClass | TypeAttributes.AutoClass |
                TypeAttributes.NotPublic | TypeAttributes.Sealed,
                mainModule.TypeSystem.Object
                );
            colClass.CustomAttributes.Add (
                new CustomAttribute (mainModule.Import (typeof (DebuggerNonUserCodeAttribute).GetConstructor (Type.EmptyTypes))));
            colClass.CustomAttributes.Add (
                new CustomAttribute (mainModule.Import (typeof (CompilerGeneratedAttribute).GetConstructor (Type.EmptyTypes))));
            mainModule.Types.Add (colClass);

            colCtor = new MethodDefinition (".cctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName | MethodAttributes.Static, mainModule.TypeSystem.Void);
            colCtor.Body.InitLocals = true;
            colCtor.Body.Variables.Add (new VariableDefinition (mainModule.Import (typeof(Type[]))));   // used in attributes instantiating
            colClass.Methods.Add (colCtor);

            BuildGetMemberAttrInstance();
        }


        private void BuildGetMemberAttrInstance()
        {
            // The IL code bellow is compiled from:
/*
        private static Attribute GetAttributeInfo (MemberInfo memberInfo, Type attrType)
        {
            var attrs = memberInfo.GetCustomAttributes (attrType);
            foreach (var attr in attrs)
            {
                if (attr.GetType().Equals (attrType))
                    return attr;
            }

            return null;
        }
*/

            getMemberAttrInstanceMethod = new MethodDefinition ("GetAttrInstance", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, mainModule.Import (typeof (Attribute)));

            colClass.Methods.Add (getMemberAttrInstanceMethod);

            getMemberAttrInstanceMethod.Parameters.Add (new ParameterDefinition ("memberInfo", ParameterAttributes.None, mainModule.Import (typeof(MemberInfo))));
            getMemberAttrInstanceMethod.Parameters.Add (new ParameterDefinition ("attrType", ParameterAttributes.None, mainModule.Import (typeof(Type))));

            getMemberAttrInstanceMethod.Body.InitLocals = true;
            getMemberAttrInstanceMethod.Body.Variables.Add (new VariableDefinition ("attrs", mainModule.Import (typeof(IEnumerable<Attribute>))));
            getMemberAttrInstanceMethod.Body.Variables.Add (new VariableDefinition ("attr", mainModule.Import (typeof(Attribute))));
            getMemberAttrInstanceMethod.Body.Variables.Add (new VariableDefinition (mainModule.Import (typeof(Attribute))));
            getMemberAttrInstanceMethod.Body.Variables.Add (new VariableDefinition (mainModule.Import (typeof(IEnumerator<Attribute>))));

            var code = getMemberAttrInstanceMethod.Body.Instructions;

            code.Add (OpCodes.Ldarg_0);
            code.Add (OpCodes.Ldarg_1);
            code.Add (OpCodes.Call, mainModule.Import (typeof (CustomAttributeExtensions).GetMethod ("GetCustomAttributes", new []
                {
                    typeof(MemberInfo), typeof(Type)
                })));
            code.Add (OpCodes.Stloc_0);
            code.Add (OpCodes.Ldloc_0);
            code.Add (OpCodes.Callvirt, mainModule.Import (typeof (IEnumerable<Attribute>).GetMethod ("GetEnumerator")));
            code.Add (OpCodes.Stloc_3);

            var moveNext = Instruction.Create (OpCodes.Ldloc_3);
            var retRes = Instruction.Create (OpCodes.Ldloc_2);
            var retNull = Instruction.Create (OpCodes.Ldnull);

            var tryStart = Instruction.Create (OpCodes.Br_S, moveNext);
            code.Add (tryStart);
            var getCurrent = Instruction.Create (OpCodes.Ldloc_3);
            code.Add (getCurrent);
            code.Add (OpCodes.Callvirt, mainModule.Import (typeof (IEnumerator<Attribute>).GetProperty ("Current").GetMethod));
            code.Add (OpCodes.Stloc_1);
            code.Add (OpCodes.Ldloc_1);
            code.Add (OpCodes.Callvirt, mainModule.Import (typeof (Type).GetMethod("GetType", Type.EmptyTypes)));
            code.Add (OpCodes.Ldarg_1);
            code.Add (OpCodes.Callvirt, mainModule.Import (typeof (Type).GetMethod("Equals", new [] {typeof(Type)})));
            code.Add (OpCodes.Brfalse_S, moveNext);
            code.Add (OpCodes.Ldloc_1);
            code.Add (OpCodes.Stloc_2);
            code.Add (OpCodes.Leave_S, retRes);
            code.Add (moveNext);        // OpCodes.Ldloc_3
            code.Add (OpCodes.Callvirt, mainModule.Import (typeof (IEnumerator).GetMethod ("MoveNext")));
            code.Add (OpCodes.Brtrue_S, getCurrent);
            code.Add (OpCodes.Leave_S, retNull);
            
            var finallyStart = Instruction.Create (OpCodes.Ldloc_3);
            var finallyEnd = Instruction.Create (OpCodes.Endfinally);
            code.Add (finallyStart);
            code.Add (OpCodes.Brfalse_S, finallyEnd);
            code.Add (OpCodes.Ldloc_3);
            code.Add (OpCodes.Callvirt, mainModule.Import (typeof (IDisposable).GetMethod ("Dispose")));
            code.Add (finallyEnd);  // OpCodes.Endfinally

            code.Add (retNull);     // OpCodes.Ldnull
            code.Add (OpCodes.Ret);

            code.Add (retRes);      // OpCodes.Ldloc_2
            code.Add (OpCodes.Ret);
            
            getMemberAttrInstanceMethod.Body.ExceptionHandlers.Add (new ExceptionHandler (ExceptionHandlerType.Finally)
            {
                TryStart = tryStart,
                TryEnd = finallyStart,
                HandlerStart = finallyStart,
                HandlerEnd = retNull
            });
        }


        private void LoadAssembly()
        {
            var readParams = new ReaderParameters (ReadingMode.Deferred);

            if (File.Exists (Path.ChangeExtension (AssemblyFile, ".pdb")))
            {
                readParams.ReadSymbols = true;
                readParams.SymbolReaderProvider = new PdbReaderProvider();
            }
            else if (File.Exists (Path.ChangeExtension (AssemblyFile, ".mdb")))
            {
                readParams.ReadSymbols = true;
                readParams.SymbolReaderProvider = new MdbReaderProvider();
            }

            assembly = AssemblyDefinition.ReadAssembly (AssemblyFile, readParams);
            mainModule = assembly.MainModule;
        }


        private void SaveAssembly()
        {
            var writeParams = new WriterParameters();

            if (File.Exists (Path.ChangeExtension (AssemblyFile, ".pdb")))
            {
                writeParams.WriteSymbols = true;
                writeParams.SymbolWriterProvider = new PdbWriterProvider();
            }
            else if (File.Exists (Path.ChangeExtension (AssemblyFile, ".mdb")))
            {
                writeParams.WriteSymbols = true;
                writeParams.SymbolWriterProvider = new MdbWriterProvider();
            }

            assembly.Write (AssemblyFile, writeParams);
        }


        private List<AspectDeclaration> GetAspects<T> (T member, ModuleDefinition module)
            where T : ICustomAttributeProvider, IMemberDefinition
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