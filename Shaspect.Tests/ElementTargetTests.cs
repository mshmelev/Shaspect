using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class ElementTargetTests : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<string> calls= new List<string>();
        private static readonly List<string> calls2= new List<string>();
        private static readonly List<string> calls3= new List<string>();

        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            protected readonly string name;


            public SimpleAspectAttribute(string name)
            {
                this.name = name;
            }


            public override void OnEntry (MethodExecInfo methodExecInfo)
            {
                calls.Add (name);
            }
        }


        public class SimpleAspect2Attribute : SimpleAspectAttribute
        {
            public SimpleAspect2Attribute(string name) : base (name)
            {
            }


            public override void OnEntry (MethodExecInfo methodExecInfo)
            {
                calls2.Add (name);
            }
        }


        public class SimpleAspect3Attribute : SimpleAspectAttribute
        {
            public SimpleAspect3Attribute(string name) : base (name)
            {
            }


            public override void OnEntry (MethodExecInfo methodExecInfo)
            {
                calls3.Add (name);
            }
        }



        [SimpleAspect ("method", ElementTargets = ElementTargets.Method)]
        [SimpleAspect2 ("property", ElementTargets = ElementTargets.Property)]
        public class TestClass
        {
            public TestClass()
            {
            }


            public void SimpleMethod()
            {
            }


            public int Prop { get; set; }
        }


        [SimpleAspect2 ("instance ctor", ElementTargets = ElementTargets.InstanceConstructor)]
        [SimpleAspect3 ("static ctor", ElementTargets = ElementTargets.StaticConstructor)]
        public class TestClass2
        {
            static TestClass2()
            {
            }


            public TestClass2()
            {
            }


            public static void StaticMethod()
            {
            }


            public void InstanceMethod()
            {
            }
        }


        public ElementTargetTests()
        {
            Monitor.Enter (sync);
            calls.Clear();
            calls2.Clear();
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void Target_Method_and_Property_Doesnt_Inject_Each_Other()
        {
            var t = new TestClass();
            int i = t.Prop;
            t.Prop = 42;
            t.SimpleMethod();
            Assert.Equal (new[] {"method"}, calls);
            Assert.Equal (new[] {"property", "property"}, calls2);
        }


        [Fact]
        public void Target_StaticConstructor_Doesnt_Inject_Anything_Else()
        {
            TestClass2.StaticMethod();
            Assert.Equal (new[] {"static ctor"}, calls3);
            Assert.Empty (calls2);
        }


        [Fact]
        public void Target_InstanceConstructor_Doesnt_Inject_Anything_Else()
        {
            var t2 = new TestClass2();
            t2.InstanceMethod();
            Assert.Equal (new[] {"instance ctor"}, calls2);
        }
    }
}