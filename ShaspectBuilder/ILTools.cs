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
    }
}