using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class CalleeArgsTests : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<object> argsBag= new List<object>();


        /// <summary>
        /// 
        /// </summary>
        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            public override void OnEntry (MethodExecInfo methodExecInfo)
            {
                lock (sync)
                {
                    foreach (object argument in methodExecInfo.Arguments)
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
                int i = 2;          // some local var
                ++i;
                return s + i;
            }

            public void DefaultArgs (string s, string s2 = "pr2")
            {
            }

            public string ComplexArgs (int i, byte b, long l, float f, double d, string s, TestClass t, DateTime dt)
            {
                return i + "_" + b + "_" + l + "_" + f + "_" + d + "_" + s;
            }
        }

        private readonly TestClass t;


        public CalleeArgsTests()
        {
            t = new TestClass();
            Monitor.Enter (sync);
            argsBag.Clear();       // there's already something from ctor of TestClass
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
            Assert.Equal ("pr3", t.SimpleArgs("pr"));
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
            Assert.Equal ("1_2_3_4.5_5.6_qqq", t.ComplexArgs (1, 2, 3L, 4.5f, 5.6, "qqq", t, new DateTime(2017, 11, 7)));
            Assert.Equal (8, argsBag.Count);
            Assert.Equal (1, argsBag[0]);
            Assert.Equal ((byte)2, argsBag[1]);
            Assert.Equal (3L, argsBag[2]);
            Assert.Equal (4.5f, argsBag[3]);
            Assert.Equal (5.6, argsBag[4]);
            Assert.Equal ("qqq", argsBag[5]);
            Assert.Equal (t, argsBag[6]);
            Assert.Equal (new DateTime(2017, 11, 7), argsBag[7]);
        }


        [Fact]
        public void ThisArgument()
        {
            t.ThisParam ("qqq");
            Assert.Equal (2, argsBag.Count);
            Assert.Equal (t, argsBag[0]);
            Assert.Equal ("qqq", argsBag[1]);
        }
    }

    static class TestArgsStaticClass
    {
        [CalleeArgsTests.SimpleAspectAttribute]
        public static void ThisParam (this CalleeArgsTests.TestClass t, string s)
        {
        }
    }

}