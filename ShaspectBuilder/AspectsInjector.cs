using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using Shaspect.Builder.Tools;


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
                var types = module.GetAllTypes().Where (t => t.IsClass && !t.IsCompilerGenerated());
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

                InjectAspectBehavior(method, aspectField);
            }
        }


        /// <summary>
        /// Modifies the passed method so it looks like:
        /// <code>
        ///     BaseAspectAttribute.OnEntry (methodExecInfo);
        ///     try
        ///     {
        ///         try
        ///         {
        ///             // Original method code, without return instructions
        ///         }
        ///         catch (Exception ex)
        ///         {
        ///             BaseAspectAttribute.OnException (methodExecInfo);
        ///             throw;
        ///         }
        ///         BaseAspectAttribute.OnSuccess (methodExecInfo);
        ///     }
        ///     finally
        ///     {
        ///         BaseAspectAttribute.OnExit (methodExecInfo);
        ///     }
        ///     return result;          // single return instruction
        /// </code>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="aspectField"></param>
        private static void InjectAspectBehavior (MethodDefinition method, FieldDefinition aspectField)
        {
            var methodCode = method.Body.Instructions;
            var firstInstruction = methodCode.FirstOrDefault();

            var methodExecInfoVar= InitMethodExecInfoVar (method);

            // TODO: should not be called if OnSuccess/OnExit are not called below
            var retInstr= MakeOneReturn (method, methodExecInfoVar);

            // TODO: OnEntry should be called only in the case OnEntry is overridden somewhere in descendants of BaseAspectAttribute
            CallOnEntry (method, methodExecInfoVar, aspectField, methodCode.IndexOf (firstInstruction));

            CallOnSuccess (method, methodExecInfoVar, aspectField, methodCode.LastIndexOf (retInstr));

            methodCode.Insert (methodCode.LastIndexOf (retInstr), Instruction.Create (OpCodes.Leave, retInstr));        // needed to correctly get to finally section
            var finallyCode = new Collection<Instruction>();
            finallyCode.Add (OpCodes.Endfinally);              // TODO: add BaseAspectAttribute.OnExit call
            methodCode.Insert (methodCode.LastIndexOf (retInstr), finallyCode);

            method.Body.ExceptionHandlers.Add (new ExceptionHandler (ExceptionHandlerType.Finally)
            {
                TryStart = firstInstruction,
                TryEnd = finallyCode.First(),
                HandlerStart = finallyCode.First(),
                HandlerEnd = finallyCode.Last().Next
            });

            method.Body.OptimizeMacros();
        }


        private static void CallOnEntry (MethodDefinition method, VariableDefinition methodExecInfoVar, FieldDefinition aspectField, int offset)
        {
            // The code below is IL generated from:
            // AspectsCollection.Aspect_x.OnEntry (methodExecInfo);
            var code = new Collection<Instruction>();
            code.Add (OpCodes.Ldsfld, aspectField);
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (BaseAspectAttribute).GetMethod ("OnEntry")));

            method.Body.Instructions.Insert (offset, code);
        }


        private static void CallOnSuccess (MethodDefinition method, VariableDefinition methodExecInfoVar, FieldDefinition aspectField, int offset)
        {
            // The code below is IL generated from:
            // AspectsCollection.Aspect_x.OnSuccess (methodExecInfo);
            var code = new Collection<Instruction>();
            code.Add (OpCodes.Ldsfld, aspectField);
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (BaseAspectAttribute).GetMethod ("OnSuccess")));

            method.Body.Instructions.Insert (offset, code);
        }


        private static void CallOnExit (MethodDefinition method, VariableDefinition methodExecInfoVar, FieldDefinition aspectField, int offset)
        {
            // The code below is IL generated from:
            // AspectsCollection.Aspect_x.OnSuccess (methodExecInfo);
            var code = new Collection<Instruction>();
            code.Add (OpCodes.Ldsfld, aspectField);
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (BaseAspectAttribute).GetMethod ("OnExit")));

            method.Body.Instructions.Insert (offset, code);
        }


        private static void CallOnException (MethodDefinition method, VariableDefinition methodExecInfoVar, FieldDefinition aspectField, int offset)
        {
            // The code below is IL generated from:
            // AspectsCollection.Aspect_x.OnSuccess (methodExecInfo);
            var code = new Collection<Instruction>();
            code.Add (OpCodes.Ldsfld, aspectField);
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (BaseAspectAttribute).GetMethod ("OnException")));

            method.Body.Instructions.Insert (offset, code);
        }


        private static VariableDefinition InitMethodExecInfoVar (MethodDefinition method)
        {
            // The code below is IL generated from:
            // var methodExecInfo = new MethodExecInfo (new object[] {param1, param2, ...})
            var argsArrVar = new VariableDefinition (method.Module.Import (typeof (object[])));
            method.Body.Variables.Add (argsArrVar);

            var execInfoVar = new VariableDefinition (method.Module.Import (typeof (MethodExecInfo)));
            method.Body.Variables.Add (execInfoVar);

            var code = new Collection<Instruction>();
            code.Add (OpCodes.Ldc_I4, method.Parameters.Count);
            code.Add (OpCodes.Newarr, method.Module.Import (typeof (object)));
            code.Add (OpCodes.Stloc, argsArrVar);
            code.Add (OpCodes.Ldloc, argsArrVar);

            for (int i = 0; i < method.Parameters.Count; ++i)
            {
                var param = method.Parameters[i];
                var paramType = param.ParameterType;

                code.Add (OpCodes.Ldc_I4, i);
                code.Add (OpCodes.Ldarg, param);

                if (paramType is ByReferenceType)
                {
                    paramType = ((ByReferenceType) paramType).ElementType;
                    code.Add (ILTools.GetLdindOpCode (paramType));
                }

                if (paramType.IsValueType || paramType.IsGenericParameter)
                    code.Add (OpCodes.Box, paramType);

                code.Add (OpCodes.Stelem_Ref);
                code.Add (OpCodes.Ldloc, argsArrVar);
            }

            code.Add (OpCodes.Newobj, method.Module.Import (typeof (MethodExecInfo).GetConstructor (new[] {typeof (object[])})));
            code.Add (OpCodes.Stloc, execInfoVar);

            method.Body.Instructions.Insert (0, code);

            return execInfoVar;
        }


        /// <summary>
        /// Substitute multiple returns in a method with one return instruction at the end
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodExecInfoVar"></param>
        private static Instruction MakeOneReturn (MethodDefinition method, VariableDefinition methodExecInfoVar)
        {
            method.Body.SimplifyMacros();          // required since we are inserting instructions and old "short" instructions (like jump short) could become not short anymore
            
            var methodCode= method.Body.Instructions;
            Instruction retInstr;

            if (method.ReturnType == method.Module.TypeSystem.Void)
            {
                retInstr = Instruction.Create (OpCodes.Ret);
                var retPlacehldrInstr = Instruction.Create (OpCodes.Nop);       // placeholder where any additional code could be inserted before the actual return instruction

                for (int i = 0; i < methodCode.Count; ++i)
                {
                    if (methodCode[i].OpCode == OpCodes.Ret)
                        methodCode[i].ReplaceOpCode (OpCodes.Leave, retPlacehldrInstr);
                }
                methodCode.Add (retPlacehldrInstr);
                methodCode.Add (retInstr);
            }
            else
            {
                var returnVar = new VariableDefinition (method.ReturnType);
                method.Body.Variables.Add (returnVar);

                retInstr = Instruction.Create (OpCodes.Ldloc, returnVar);
                var retInitInstr = Instruction.Create (OpCodes.Ldloc, methodExecInfoVar);

                for (int i = 0; i < methodCode.Count; ++i)
                {
                    if (methodCode[i].OpCode == OpCodes.Ret)
                    {
                        methodCode[i].ReplaceOpCode (OpCodes.Leave, retInitInstr);
                        methodCode.Insert (i, Instruction.Create (OpCodes.Stloc, returnVar));
                        ++i;
                    }
                }

                // TODO this code should not be emitted in the case an unhandled exception was thrown by method (in this case execInfoVar is unitialized and CLR will throw an exception)
                // methodExecInfo.ReturnValue = returnVar;
                methodCode.Add (retInitInstr);
                methodCode.Add (OpCodes.Ldloc, returnVar);
                if (returnVar.VariableType.IsValueType || returnVar.VariableType.IsGenericParameter)
                    methodCode.Add (OpCodes.Box, returnVar.VariableType);
                methodCode.Add (OpCodes.Callvirt,  method.Module.Import (typeof (MethodExecInfo).GetProperty ("ReturnValue").SetMethod));

                // return returnVar;
                methodCode.Add (retInstr);
                methodCode.Add (OpCodes.Ret);
            }

            return retInstr;
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