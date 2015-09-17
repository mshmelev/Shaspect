using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;


namespace Shaspect.Builder.Tools
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


        public static void Add (this Collection<Instruction> coll, Collection<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                coll.Add (instruction);
        }


        public static void Insert (this Collection<Instruction> coll, int index, Collection<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                coll.Insert (index++, instruction);
        }


        /// <summary>
        /// Replaces OpCode and Operand leaving instruction reference untouched.
        /// Useful when instruction is referred to from other places.
        /// </summary>
        /// <param name="instr"></param>
        /// <param name="opCode"></param>
        /// <param name="operand"></param>
        public static void ReplaceOpCode (this Instruction instr, OpCode opCode, object operand)
        {
            instr.OpCode = opCode;
            instr.Operand = operand;
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