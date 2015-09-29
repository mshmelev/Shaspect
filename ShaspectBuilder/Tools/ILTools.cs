using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
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
        ///     Replaces OpCode and Operand leaving instruction reference untouched.
        ///     Useful when instruction is referred to from other places.
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
        ///     Returns Ldind instruction specific to the passed type.
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


        /// <summary>
        ///     Returns Stelem instruction specific to the passed type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Instruction GetStelemOpCode (TypeReference type)
        {
            if (type.IsValueType)
            {
                var typeDef = type.Resolve();
                if (typeDef.IsEnum)
                    type = typeDef.GetEnumUnderlyingType();

                switch (type.MetadataType)
                {
                    case MetadataType.Boolean:
                    case MetadataType.SByte:
                    case MetadataType.Byte:
                        return Instruction.Create (OpCodes.Stelem_I1);
                    case MetadataType.Char:
                    case MetadataType.Int16:
                    case MetadataType.UInt16:
                        return Instruction.Create (OpCodes.Stelem_I2);
                    case MetadataType.Int32:
                    case MetadataType.UInt32:
                        return Instruction.Create (OpCodes.Stelem_I4);
                    case MetadataType.Int64:
                    case MetadataType.UInt64:
                        return Instruction.Create (OpCodes.Stelem_I8);
                    case MetadataType.Single:
                        return Instruction.Create (OpCodes.Stelem_R4);
                    case MetadataType.Double:
                        return Instruction.Create (OpCodes.Stelem_R8);
                }
            }

            return Instruction.Create (OpCodes.Stelem_Ref);
        }


        /// <summary>
        ///     Returns Ldc instruction specific to the passed type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Instruction GetLdcOpCode (TypeReference type, object value)
        {
            var typeDef = type.Resolve();
            if (typeDef.IsEnum)
                type = typeDef.GetEnumUnderlyingType();

            switch (type.MetadataType)
            {
                case MetadataType.Boolean:
                    return Instruction.Create (OpCodes.Ldc_I4, (bool) value ? 1 : 0);
                case MetadataType.Char:
                    return Instruction.Create (OpCodes.Ldc_I4, (char) value);
                case MetadataType.SByte:
                    return Instruction.Create (OpCodes.Ldc_I4_S, (sbyte) value);
                case MetadataType.Byte:
                    return Instruction.Create (OpCodes.Ldc_I4, (int) (byte) value);
                case MetadataType.Int16:
                    return Instruction.Create (OpCodes.Ldc_I4, (short) value);
                case MetadataType.UInt16:
                    return Instruction.Create (OpCodes.Ldc_I4, (ushort) value);
                case MetadataType.Int32:
                    return Instruction.Create (OpCodes.Ldc_I4, (int) value);
                case MetadataType.UInt32:
                    return Instruction.Create (OpCodes.Ldc_I4, (int) (uint) value);
                case MetadataType.Int64:
                    return Instruction.Create (OpCodes.Ldc_I8, (long) value);
                case MetadataType.UInt64:
                    return Instruction.Create (OpCodes.Ldc_I8, (long) (ulong) value);
                case MetadataType.Single:
                    return Instruction.Create (OpCodes.Ldc_R4, (float) value);
                case MetadataType.Double:
                    return Instruction.Create (OpCodes.Ldc_R8, (double) value);
                case MetadataType.String:
                    return Instruction.Create (OpCodes.Ldstr, (string) value);
            }

            throw new ArgumentException ("Unsupported type: " + type, "type");
        }
    }
}