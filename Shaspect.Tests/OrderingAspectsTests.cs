using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class OrderingAspectsTests : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<string> calls= new List<string>();
        private readonly TestClass t;

        /// <summary>
        /// 
        /// </summary>
        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            private readonly string callName;


            public SimpleAspectAttribute(string callName)
            {
                this.callName = callName;
            }


            public override void OnEntry(MethodExecInfo methodExecInfo)
            {
                calls.Add (callName);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public class SimpleAspect2Attribute : SimpleAspectAttribute
        {
            public SimpleAspect2Attribute(string callName) : base(callName)
            {
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public class SimpleAspect3Attribute : SimpleAspectAttribute
        {
            public SimpleAspect3Attribute(string callName) : base(callName)
            {
            }
        }


        [SimpleAspect ("Class")]
        private class TestClass
        {
            [SimpleAspect ("SimpleProp")]
            public int SimpleProp
            {
                [SimpleAspect ("SimpleProp_get")]
                get;
                set;
            }


            [SimpleAspect ("ReverseOrderProp", Order = 1)]
            public int ReverseOrderProp
            {
                [SimpleAspect ("ReverseOrderProp_get", Order = 2)]
                get;
                set;
            }


            [SimpleAspect ("Method1", Order = 1)]
            [SimpleAspect2 ("Method2", Order = 2)]
            [SimpleAspect3 ("Method3", Order = 3)]
            public void MethodWithManyAspects()
            {
            }

            [SimpleAspect ("Method1", Order = 3)]
            [SimpleAspect2 ("Method2", Order = 2)]
            [SimpleAspect3 ("Method3", Order = 1)]
            public void MethodWithManyAspects_Reverse()
            {
            }


            [SimpleAspect("NestedClass")]
            public class NestedClass
            {
                [SimpleAspect("NestedClass2")]
                public class NestedClass2
                {
                    [SimpleAspect("SimpleMethod")]
                    public void SimpleMethod()
                    {
                    }
                }
            }
        }


        public OrderingAspectsTests()
        {
            t = new TestClass();
            Monitor.Enter (sync);
            calls.Clear();       // there's already something from ctor of TestClass
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void Default_Order_Is_Bottom_Up()
        {
            int i = t.SimpleProp;
            Assert.Equal (new[] {"SimpleProp_get", "SimpleProp", "Class"}, calls);
        }


        [Fact]
        public void Custom_Order_On_Nesting()
        {
            int i = t.ReverseOrderProp;
            Assert.Equal (new[] {"Class", "ReverseOrderProp", "ReverseOrderProp_get"}, calls);
        }


        [Fact]
        public void Custom_Order_On_Same_Level()
        {
            t.MethodWithManyAspects();
            Assert.Equal (new[] {"Class", "Method1", "Method2", "Method3"}, calls);

            calls.Clear();
            t.MethodWithManyAspects_Reverse();
            Assert.Equal (new[] {"Class", "Method3", "Method2", "Method1"}, calls);
        }


        [Fact]
        public void Default_Order_On_Nested_Classes()
        {
            var t2 = new TestClass.NestedClass.NestedClass2();
            calls.Clear();
            t2.SimpleMethod();
            Assert.Equal (new[] {"SimpleMethod", "NestedClass2", "NestedClass", "Class"}, calls);
        }
    }
}