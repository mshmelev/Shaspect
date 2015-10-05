using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using Shaspect.Builder.Tools;


namespace Shaspect.Builder
{
    internal class MethodAspectInjector
    {
        private readonly MethodDefinition method;
        private readonly FieldDefinition aspectStaticField;
        private VariableDefinition methodExecInfoVar;
        private Instruction returnInstr;


        public MethodAspectInjector(MethodDefinition method, FieldDefinition aspectStaticField)
        {
            this.method = method;
            this.aspectStaticField = aspectStaticField;
        }


        /// <summary>
        /// Modifies the injecting method so it looks like:
        /// <code>
        ///     var methodExecInfo = new MethodExecInfo (new [] {arg1, arg2, ...});
        ///     BaseAspectAttribute.OnEntry (methodExecInfo);
        ///     try
        ///     {
        ///         try
        ///         {
        ///             // Original method code, without return instructions
        ///         }
        ///         catch (Exception ex)
        ///         {
        ///             methodExecInfo.Exception = ex;
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
        public void Inject()
        {
            var methodCode = method.Body.Instructions;
            var firstInstruction = FindFirstInstruction();

            InitMethodExecInfoVar();
            MakeOneReturn();

            // TODO: OnEntry should be called only in the case OnEntry is overridden somewhere in descendants of BaseAspectAttribute
            CallOnEntry (methodCode.IndexOf (firstInstruction));
            var checkFlowCode = BuildCheckExecFlow (firstInstruction);
            methodCode.Insert (methodCode.IndexOf (firstInstruction), checkFlowCode);

            var callOnSuccessCode = CallOnSuccess (methodCode.LastIndexOf (returnInstr));

            AddCatchBlock (firstInstruction, callOnSuccessCode.First());
            AddFinallyBlock (firstInstruction, returnInstr);
            
            checkFlowCode = BuildCheckExecFlow (callOnSuccessCode.Last().Next);     // for OnSuccess, needs to be inserted after FinallyBlock was created already
            methodCode.Insert (methodCode.LastIndexOf (callOnSuccessCode.Last().Next), checkFlowCode);

            method.Body.OptimizeMacros();
        }


        /// <summary>
        /// For regular methods the 1st instruction is really the 1st one. But for instance constructors the 1st instruction goes after calling base or overloaded contructurs.
        /// </summary>
        /// <returns></returns>
        private Instruction FindFirstInstruction()
        {
            var methodCode = method.Body.Instructions;
            if (method.IsSpecialName && !method.IsStatic && method.Name == ".ctor") // custom logis only for instance ctor
            {
                // find a call (not newobj) of other constructor
                foreach (var instruction in methodCode)
                {
                    if (instruction.OpCode == OpCodes.Call)
                    {
                        var callingMethod = instruction.Operand as MethodDefinition;
                        if (callingMethod != null && callingMethod.Name == ".ctor")
                            return instruction.Next;
                    }
                }
            }

            return methodCode.First();
        }


        private Collection<Instruction> BuildCheckExecFlow (Instruction insertBeforeInstr)
        {
            // if (execInfo.ExecFlow == ExecFlow.Return)
            //   goto return_code;
            // if (execInfo.ExecFlow == ExecFlow.ThrowException && execInfo.Exception!= null)
            //   throw execInfo.Exception;
            var code = new Collection<Instruction>();
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (MethodExecInfo).GetProperty ("ExecFlow").GetMethod));
            code.Add (OpCodes.Ldc_I4, (int) ExecFlow.Return);
            var exceptionCheckInstr = Instruction.Create (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Bne_Un, exceptionCheckInstr);
            code.Add (OpCodes.Leave, returnInstr);

            code.Add (exceptionCheckInstr); // OpCodes.Ldloc, methodExecInfoVar
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (MethodExecInfo).GetProperty ("ExecFlow").GetMethod));
            code.Add (OpCodes.Ldc_I4, (int) ExecFlow.ThrowException);
            code.Add (OpCodes.Bne_Un, insertBeforeInstr);
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (MethodExecInfo).GetProperty ("Exception").GetMethod));
            code.Add (OpCodes.Brfalse, insertBeforeInstr);
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (MethodExecInfo).GetProperty ("Exception").GetMethod));
            code.Add (OpCodes.Throw);

            return code;
        }


        private void AddFinallyBlock (Instruction tryStartInstr, Instruction tryEndInstr)
        {
            var methodCode = method.Body.Instructions;

            methodCode.Insert (methodCode.LastIndexOf (tryEndInstr), Instruction.Create (OpCodes.Leave, tryEndInstr)); // needed to jump correctly to finally section
            
            var finallyCode = new Collection<Instruction>();
            finallyCode.Add (BuildOnExitCall());
            finallyCode.Add (OpCodes.Endfinally);
            methodCode.Insert (methodCode.LastIndexOf (tryEndInstr), finallyCode);

            method.Body.ExceptionHandlers.Add (new ExceptionHandler (ExceptionHandlerType.Finally)
            {
                TryStart = tryStartInstr,
                TryEnd = finallyCode.First(),
                HandlerStart = finallyCode.First(),
                HandlerEnd = finallyCode.Last().Next
            });
        }


        private void AddCatchBlock (Instruction tryStartInstr, Instruction tryEndInstr)
        {
            var methodCode = method.Body.Instructions;

            methodCode.Insert (methodCode.LastIndexOf (tryEndInstr), Instruction.Create (OpCodes.Leave, tryEndInstr)); // needed to jump correctly to catch section

            // catch (Exception ex)
            // {
            //    methodExecInfo.Exception = ex;
            //    BaseAspectAttribute.OnException (methodExecInfo);
            //    throw;
            // }
            var exceptionVar = new VariableDefinition (method.Module.Import (typeof (Exception)));
            method.Body.Variables.Add (exceptionVar);
            var catchCode = new Collection<Instruction>();
            catchCode.Add (OpCodes.Stloc, exceptionVar);
            catchCode.Add (OpCodes.Ldloc, methodExecInfoVar);
            catchCode.Add (OpCodes.Ldloc, exceptionVar);
            catchCode.Add (OpCodes.Callvirt, method.Module.Import (typeof (MethodExecInfo).GetProperty ("Exception").SetMethod));
            catchCode.Add (BuildOnExceptionCall());
            var rethrowInstr = Instruction.Create (OpCodes.Rethrow);
            catchCode.Add (BuildCheckExecFlow (rethrowInstr));
            catchCode.Add (rethrowInstr);

            methodCode.Insert (methodCode.LastIndexOf (tryEndInstr), catchCode);

            method.Body.ExceptionHandlers.Add (new ExceptionHandler (ExceptionHandlerType.Catch)
            {
                CatchType = method.Module.Import (typeof (Exception)),
                TryStart = tryStartInstr,
                TryEnd = catchCode.First(),
                HandlerStart = catchCode.First(),
                HandlerEnd = catchCode.Last().Next
            });
        }


        private void CallOnEntry (int offset)
        {
            // The code below is IL generated from:
            // AspectsCollection.Aspect_x.OnEntry (methodExecInfo);
            var code = new Collection<Instruction>();
            code.Add (OpCodes.Ldsfld, aspectStaticField);
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (BaseAspectAttribute).GetMethod ("OnEntry")));

            method.Body.Instructions.Insert (offset, code);
        }


        private Collection<Instruction> CallOnSuccess (int offset)
        {
            // The code below is IL generated from:
            // AspectsCollection.Aspect_x.OnSuccess (methodExecInfo);
            var code = new Collection<Instruction>();
            code.Add (OpCodes.Ldsfld, aspectStaticField);
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (BaseAspectAttribute).GetMethod ("OnSuccess")));

            method.Body.Instructions.Insert (offset, code);

            return code;
        }


        private Collection<Instruction> BuildOnExitCall()
        {
            // The code below is IL generated from:
            // AspectsCollection.Aspect_x.OnExit (methodExecInfo);
            var code = new Collection<Instruction>();
            code.Add (OpCodes.Ldsfld, aspectStaticField);
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (BaseAspectAttribute).GetMethod ("OnExit")));

            return code;
        }


        private Collection<Instruction> BuildOnExceptionCall()
        {
            // The code below is IL generated from:
            // AspectsCollection.Aspect_x.OnException (methodExecInfo);
            var code = new Collection<Instruction>();
            code.Add (OpCodes.Ldsfld, aspectStaticField);
            code.Add (OpCodes.Ldloc, methodExecInfoVar);
            code.Add (OpCodes.Callvirt, method.Module.Import (typeof (BaseAspectAttribute).GetMethod ("OnException")));

            return code;
        }


        private void InitMethodExecInfoVar()
        {
            // The code below is IL generated from:
            // var methodExecInfo = new MethodExecInfo (new object[] {param1, param2, ...})
            var argsArrVar = new VariableDefinition (method.Module.Import (typeof (object[])));
            method.Body.Variables.Add (argsArrVar);

            methodExecInfoVar = new VariableDefinition (method.Module.Import (typeof (MethodExecInfo)));
            method.Body.Variables.Add (methodExecInfoVar);

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
            code.Add (OpCodes.Stloc, methodExecInfoVar);

            method.Body.Instructions.Insert (0, code);
        }


        /// <summary>
        /// Substitute multiple returns in a method with one return instruction at the end
        /// </summary>
        private void MakeOneReturn()
        {
            method.Body.SimplifyMacros();          // required since we are inserting instructions and old "short" instructions (like jump short) could become not short anymore
            
            var methodCode= method.Body.Instructions;

            if (method.ReturnType == method.Module.TypeSystem.Void)
            {
                returnInstr = Instruction.Create (OpCodes.Ret);
                var retPlacehldrInstr = Instruction.Create (OpCodes.Nop);       // placeholder where any additional code could be inserted before the actual return instruction

                for (int i = 0; i < methodCode.Count; ++i)
                {
                    if (methodCode[i].OpCode == OpCodes.Ret)
                        methodCode[i].ReplaceOpCode (OpCodes.Leave, retPlacehldrInstr);
                }
                methodCode.Add (retPlacehldrInstr);
                methodCode.Add (returnInstr);
            }
            else
            {
                var returnVar = new VariableDefinition (method.ReturnType);
                method.Body.Variables.Add (returnVar);

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

                // methodExecInfo.ReturnValue = returnVar;
                methodCode.Add (retInitInstr);
                methodCode.Add (OpCodes.Ldloc, returnVar);
                if (returnVar.VariableType.IsValueType || returnVar.VariableType.IsGenericParameter)
                    methodCode.Add (OpCodes.Box, returnVar.VariableType);
                methodCode.Add (OpCodes.Callvirt, method.Module.Import (typeof (MethodExecInfo).GetProperty ("ReturnValue").SetMethod));

                // return (ReturnType)methodExecInfo.ReturnValue;
                returnInstr = Instruction.Create (OpCodes.Ldloc, methodExecInfoVar);
                methodCode.Add (returnInstr);
                methodCode.Add (OpCodes.Callvirt, method.Module.Import (typeof (MethodExecInfo).GetProperty ("ReturnValue").GetMethod));
                if (returnVar.VariableType.IsValueType || returnVar.VariableType.IsGenericParameter)
                    methodCode.Add (OpCodes.Unbox_Any, returnVar.VariableType);
                else
                    methodCode.Add (OpCodes.Castclass, returnVar.VariableType);
                methodCode.Add (OpCodes.Ret);
            }
        }

    }
}