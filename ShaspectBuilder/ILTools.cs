using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;


namespace Shaspect.Builder
{
    internal static class ILTools
    {
        public static void Add (this Collection<Instruction> coll, OpCode opcode)
        {
            coll.Add (Instruction.Create (opcode));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, TypeReference type)
        {
            coll.Add (Instruction.Create (opcode, type));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, CallSite site)
        {
            coll.Add (Instruction.Create (opcode, site));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, MethodReference method)
        {
            coll.Add (Instruction.Create (opcode, method));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, FieldReference field)
        {
            coll.Add (Instruction.Create (opcode, field));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, string value)
        {
            coll.Add (Instruction.Create (opcode, value));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, sbyte value)
        {
            coll.Add (Instruction.Create (opcode, value));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, byte value)
        {
            coll.Add (Instruction.Create (opcode, value));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, int value)
        {
            coll.Add (Instruction.Create (opcode, value));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, long value)
        {
            coll.Add (Instruction.Create (opcode, value));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, float value)
        {
            coll.Add (Instruction.Create (opcode, value));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, double value)
        {
            coll.Add (Instruction.Create (opcode, value));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, Instruction target)
        {
            coll.Add (Instruction.Create (opcode, target));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, Instruction[] targets)
        {
            coll.Add (Instruction.Create (opcode, targets));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, VariableDefinition variable)
        {
            coll.Add (Instruction.Create (opcode, variable));
        }


        public static void Add (this Collection<Instruction> coll, OpCode opcode, ParameterDefinition parameter)
        {
            coll.Add (Instruction.Create (opcode, parameter));
        }


        public static void Insert (this Collection<Instruction> coll, int index, Collection<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                coll.Insert (index++, instruction);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="untilInstruction">where to stop optimization</param>
        public static void OptimizeMacros (this MethodBody self, Instruction untilInstruction = null)
        {
            if (self == null)
                throw new ArgumentNullException ("self");

            var method = self.Method;

            foreach (var instruction in self.Instructions)
            {
                if (instruction == untilInstruction)
                    break;

                int index;
                switch (instruction.OpCode.Code)
                {
                    case Code.Ldarg:
                        index = ((ParameterDefinition) instruction.Operand).Index;
                        if (index == -1 && instruction.Operand == self.ThisParameter)
                            index = 0;
                        else if (method.HasThis)
                            index++;

                        switch (index)
                        {
                            case 0:
                                MakeMacro (instruction, OpCodes.Ldarg_0);
                                break;
                            case 1:
                                MakeMacro (instruction, OpCodes.Ldarg_1);
                                break;
                            case 2:
                                MakeMacro (instruction, OpCodes.Ldarg_2);
                                break;
                            case 3:
                                MakeMacro (instruction, OpCodes.Ldarg_3);
                                break;
                            default:
                                if (index < 256)
                                    ExpandMacro (instruction, OpCodes.Ldarg_S, instruction.Operand);
                                break;
                        }
                        break;
                    case Code.Ldloc:
                        index = ((VariableDefinition) instruction.Operand).Index;
                        switch (index)
                        {
                            case 0:
                                MakeMacro (instruction, OpCodes.Ldloc_0);
                                break;
                            case 1:
                                MakeMacro (instruction, OpCodes.Ldloc_1);
                                break;
                            case 2:
                                MakeMacro (instruction, OpCodes.Ldloc_2);
                                break;
                            case 3:
                                MakeMacro (instruction, OpCodes.Ldloc_3);
                                break;
                            default:
                                if (index < 256)
                                    ExpandMacro (instruction, OpCodes.Ldloc_S, instruction.Operand);
                                break;
                        }
                        break;
                    case Code.Stloc:
                        index = ((VariableDefinition) instruction.Operand).Index;
                        switch (index)
                        {
                            case 0:
                                MakeMacro (instruction, OpCodes.Stloc_0);
                                break;
                            case 1:
                                MakeMacro (instruction, OpCodes.Stloc_1);
                                break;
                            case 2:
                                MakeMacro (instruction, OpCodes.Stloc_2);
                                break;
                            case 3:
                                MakeMacro (instruction, OpCodes.Stloc_3);
                                break;
                            default:
                                if (index < 256)
                                    ExpandMacro (instruction, OpCodes.Stloc_S, instruction.Operand);
                                break;
                        }
                        break;
                    case Code.Ldarga:
                        index = ((ParameterDefinition) instruction.Operand).Index;
                        if (index == -1 && instruction.Operand == self.ThisParameter)
                            index = 0;
                        else if (method.HasThis)
                            index++;
                        if (index < 256)
                            ExpandMacro (instruction, OpCodes.Ldarga_S, instruction.Operand);
                        break;
                    case Code.Ldloca:
                        if (((VariableDefinition) instruction.Operand).Index < 256)
                            ExpandMacro (instruction, OpCodes.Ldloca_S, instruction.Operand);
                        break;
                    case Code.Ldc_I4:
                        var i = (int) instruction.Operand;
                        switch (i)
                        {
                            case -1:
                                MakeMacro (instruction, OpCodes.Ldc_I4_M1);
                                break;
                            case 0:
                                MakeMacro (instruction, OpCodes.Ldc_I4_0);
                                break;
                            case 1:
                                MakeMacro (instruction, OpCodes.Ldc_I4_1);
                                break;
                            case 2:
                                MakeMacro (instruction, OpCodes.Ldc_I4_2);
                                break;
                            case 3:
                                MakeMacro (instruction, OpCodes.Ldc_I4_3);
                                break;
                            case 4:
                                MakeMacro (instruction, OpCodes.Ldc_I4_4);
                                break;
                            case 5:
                                MakeMacro (instruction, OpCodes.Ldc_I4_5);
                                break;
                            case 6:
                                MakeMacro (instruction, OpCodes.Ldc_I4_6);
                                break;
                            case 7:
                                MakeMacro (instruction, OpCodes.Ldc_I4_7);
                                break;
                            case 8:
                                MakeMacro (instruction, OpCodes.Ldc_I4_8);
                                break;
                            default:
                                if (i >= -128 && i < 128)
                                    ExpandMacro (instruction, OpCodes.Ldc_I4_S, (sbyte) i);
                                break;
                        }
                        break;
                }
            }
        }


        private static void ExpandMacro (Instruction instruction, OpCode opcode, object operand)
        {
            instruction.OpCode = opcode;
            instruction.Operand = operand;
        }


        private static void MakeMacro (Instruction instruction, OpCode opcode)
        {
            instruction.OpCode = opcode;
            instruction.Operand = null;
        }


        /// <summary>
        /// Returns Ldind instruction specific to the passed type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Instruction GetLdindOpCode (TypeReference type)
        {
            if (type.IsValueType)
            {
                switch (type.MetadataType)
                {
                    case MetadataType.Boolean:
                        return Instruction.Create (OpCodes.Ldind_I1);
                    case MetadataType.Char:
                        return Instruction.Create (OpCodes.Ldind_U2);
                    case MetadataType.SByte:
                        return Instruction.Create (OpCodes.Ldind_I1);
                    case MetadataType.Byte:
                        return Instruction.Create (OpCodes.Ldind_U1);
                    case MetadataType.Int16:
                        return Instruction.Create (OpCodes.Ldind_I2);
                    case MetadataType.UInt16:
                        return Instruction.Create (OpCodes.Ldind_U2);
                    case MetadataType.Int32:
                        return Instruction.Create (OpCodes.Ldind_I4);
                    case MetadataType.UInt32:
                        return Instruction.Create (OpCodes.Ldind_U4);
                    case MetadataType.Int64:
                    case MetadataType.UInt64:
                        return Instruction.Create (OpCodes.Ldind_I8);
                    case MetadataType.Single:
                        return Instruction.Create (OpCodes.Ldind_R4);
                    case MetadataType.Double:
                        return Instruction.Create (OpCodes.Ldind_R8);
                    case MetadataType.ValueType:
                       return Instruction.Create (OpCodes.Ldobj, type);
                }
            }

            return Instruction.Create (OpCodes.Ldind_Ref);
        }
    }
}