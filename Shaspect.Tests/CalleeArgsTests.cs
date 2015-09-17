using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public sealed class CalleeArgsTests : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<object> argsBag = new List<object>();


        /// <summary>
        /// 
        /// </summary>
        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            public override void OnEntry (MethodExecInfo methodExecInfo)
            {
                lock (sync)
                {
                    foreach (var argument in methodExecInfo.Arguments)
                        argsBag.Add (argument);
                }
            }
        }


        [SimpleAspect]
        internal class TestClass
        {
            public void EmptyMethod()
            {
            }


            public string SimpleArgs (string s)
            {
                var i = 2; // some local var
                ++i;
                return s + i;
            }


            public void DefaultArgs (string s, string s2 = "pr2")
            {
            }


            public string ComplexArgs (int i, byte b, long l, float f, double d, string s, uint[] arr, TestClass t, DateTime dt, dynamic dy)
            {
                return i + "_" + b + "_" + l + "_" + f + "_" + d + "_" + s;
            }


            public void OutArgs (int ii, string ss,
                out sbyte sb, out byte b, out short s, out ushort us, out int i, out uint ui, out long l, out ulong ul, out float f,
                out double d, out char c, out uint[] uiArr, out string str, out DateTime dt)
            {
                sb = 1;
                b = 1;
                s = 1;
                us = 1;
                i = 1;
                ui = 1;
                l = 1;
                ul = 1;
                f = 1;
                d = 1;
                c = '1';
                uiArr = new uint[] {1};
                str = "1";
                dt = DateTime.Now;
            }


            public void RefArgs (int ii, string ss,
                ref sbyte sb, ref byte b, ref short s, ref ushort us, ref int i, ref uint ui, ref long l, ref ulong ul, ref float f,
                ref double d, ref char c, ref uint[] uiArr, ref string str, ref DateTime dt)
            {
            }


            public void GenericArgs<T> (T a, out T b, ref T c)
            {
                b = default(T);
                c = default(T);
            }
        }

        private readonly TestClass t;


        public CalleeArgsTests()
        {
            t = new TestClass();
            Monitor.Enter (sync);
            argsBag.Clear(); // there's already something from ctor of TestClass
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void EmptyMethod_Args()
        {
            t.EmptyMethod();
            Assert.Equal (0, argsBag.Count);
        }


        [Fact]
        public void SimpleMethod_Args()
        {
            Assert.Equal ("pr3", t.SimpleArgs ("pr"));
            Assert.Equal (1, argsBag.Count);
            Assert.Equal ("pr", argsBag[0]);
        }


        [Fact]
        public void DefaultArgs()
        {
            t.DefaultArgs ("pr1");
            Assert.Equal (2, argsBag.Count);
            Assert.Equal ("pr1", argsBag[0]);
            Assert.Equal ("pr2", argsBag[1]);

            argsBag.Clear();
            t.DefaultArgs ("pr1", "pr2_m");
            Assert.Equal (2, argsBag.Count);
            Assert.Equal ("pr1", argsBag[0]);
            Assert.Equal ("pr2_m", argsBag[1]);
        }


        [Fact]
        public void BoxingArgs()
        {
            Assert.Equal ("1_2_3_4.5_5.6_qqq", t.ComplexArgs (1, 2, 3L, 4.5f, 5.6, "qqq", new uint[] {42, 43}, t, new DateTime (2017, 11, 7), new {qq = 42}));
            Assert.Equal (10, argsBag.Count);
            Assert.Equal (1, argsBag[0]);
            Assert.Equal ((byte) 2, argsBag[1]);
            Assert.Equal (3L, argsBag[2]);
            Assert.Equal (4.5f, argsBag[3]);
            Assert.Equal (5.6, argsBag[4]);
            Assert.Equal ("qqq", argsBag[5]);
            Assert.Equal (new uint[] {42, 43}, argsBag[6]);
            Assert.Equal (t, argsBag[7]);
            Assert.Equal (new DateTime (2017, 11, 7), argsBag[8]);
            Assert.Equal (new {qq = 42}, argsBag[9]);
        }


        [Fact]
        public void ThisArgument()
        {
            t.ThisParam ("qqq");
            Assert.Equal (2, argsBag.Count);
            Assert.Equal (t, argsBag[0]);
            Assert.Equal ("qqq", argsBag[1]);
        }


        [Fact]
        public void OutArguments()
        {
            sbyte sb;
            byte b;
            short s;
            ushort us;
            int i;
            uint ui;
            long l;
            ulong ul;
            float f;
            double d;
            char c;
            uint[] uiArr;
            string str;
            DateTime dt;

            t.OutArgs (42, "qqq", out sb, out b, out s, out us, out i, out ui, out l, out ul, out f, out d, out c, out uiArr, out str, out dt);

            Assert.Equal (16, argsBag.Count);

            Assert.Equal (42, argsBag[0]);
            Assert.Equal ("qqq", argsBag[1]);
            Assert.Equal (default(sbyte), argsBag[2]);
            Assert.Equal (default(byte), argsBag[3]);
            Assert.Equal (default(short), argsBag[4]);
            Assert.Equal (default(ushort), argsBag[5]);
            Assert.Equal (default(int), argsBag[6]);
            Assert.Equal (default(uint), argsBag[7]);
            Assert.Equal (default(long), argsBag[8]);
            Assert.Equal (default(ulong), argsBag[9]);
            Assert.Equal (default(float), argsBag[10]);
            Assert.Equal (default(double), argsBag[11]);
            Assert.Equal (default(char), argsBag[12]);
            Assert.Equal (default(uint[]), argsBag[13]);
            Assert.Equal (default(string), argsBag[14]);
            Assert.Equal (default(DateTime), argsBag[15]);
        }


        [Fact]
        public void RefArguments()
        {
            sbyte sb = 1;
            byte b = 2;
            short s = 3;
            ushort us = 4;
            var i = 5;
            uint ui = 6;
            long l = 7;
            ulong ul = 8;
            var f = 9.1f;
            var d = 10.2;
            var c = 'f';
            uint[] uiArr = {1, 2, 3};
            var str = "quba";
            var dt = new DateTime (1945, 5, 9);

            t.RefArgs (42, "qqq", ref sb, ref b, ref s, ref us, ref i, ref ui, ref l, ref ul, ref f, ref d, ref c, ref uiArr, ref str, ref dt);

            Assert.Equal (16, argsBag.Count);

            Assert.Equal (42, argsBag[0]);
            Assert.Equal ("qqq", argsBag[1]);
            Assert.Equal ((sbyte) 1, argsBag[2]);
            Assert.Equal ((byte) 2, argsBag[3]);
            Assert.Equal ((short) 3, argsBag[4]);
            Assert.Equal ((ushort) 4, argsBag[5]);
            Assert.Equal (5, argsBag[6]);
            Assert.Equal ((uint) 6, argsBag[7]);
            Assert.Equal (7L, argsBag[8]);
            Assert.Equal ((ulong) 8, argsBag[9]);
            Assert.Equal (9.1f, argsBag[10]);
            Assert.Equal (10.2, argsBag[11]);
            Assert.Equal ('f', argsBag[12]);
            Assert.Equal (new uint[] {1, 2, 3}, argsBag[13]);
            Assert.Equal ("quba", argsBag[14]);
            Assert.Equal (new DateTime (1945, 5, 9), argsBag[15]);
        }


        [Fact]
        public void GenericArguments()
        {
            int b, c = 43;
            t.GenericArgs (42, out b, ref c);
            Assert.Equal (42, argsBag[0]);
            Assert.Equal (0, argsBag[1]);
            Assert.Equal (43, argsBag[2]);
            Assert.Equal (0, c);

            argsBag.Clear();

            string s1, s2 = "z";
            t.GenericArgs ("x", out s1, ref s2);
            Assert.Equal ("x", argsBag[0]);
            Assert.Equal (null, argsBag[1]);
            Assert.Equal ("z", argsBag[2]);
            Assert.Equal (null, s2);
        }
    }

    internal static class TestArgsStaticClass
    {
        [CalleeArgsTests.SimpleAspectAttribute]
        public static void ThisParam (this CalleeArgsTests.TestClass t, string s)
        {
        }
    }
}