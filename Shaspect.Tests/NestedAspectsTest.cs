using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class NestedAspectsTest : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly HashSet<string> callsBag= new HashSet<string>();
        

        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            private readonly string callName;


            public SimpleAspectAttribute(string callName)
            {
                this.callName = callName;
            }


            public override void OnEntry()
            {
                lock (sync)
                    callsBag.Add (callName);
            }
        }


        [SimpleAspect("Class")]
        private class TestClass
        {
            public void EmptyMethod() { }

            public int Prop1 { get; set; }

            [SimpleAspect("AspectMethod")]
            public void AspectMethod() { }
        }

        private readonly TestClass t;


        public NestedAspectsTest()
        {
            t = new TestClass();
            Monitor.Enter (sync);
            callsBag.Clear();       // there's already something from ctor of TestClass
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void AspectOnMethod_FromDeclaringType()
        {
            t.EmptyMethod();
            Assert.True (callsBag.Contains ("Class"));
        }


        [Fact]
        public void AspectOnProperty_FromDeclaringType()
        {
            t.Prop1 = 42;
            Assert.True (callsBag.Contains ("Class"));
        }


    }
}