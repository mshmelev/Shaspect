using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public sealed class NestedAspectsTest : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly HashSet<string> callsBag= new HashSet<string>();
        

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
                lock (sync)
                    callsBag.Add (callName);
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
        [SimpleAspect("Class")]
        private class TestClass
        {
            public class NestedClass
            {
                public void EmptyMethod() { }

                [SimpleAspect("AspectMethod")]
                public void AspectMethod() { }
            }

            public void EmptyMethod() { }

            public int Prop1 { get; set; }

            [SimpleAspect("AspectMethod")]
            public void AspectMethod() { }

            [SimpleAspect ("AspectProperty")]
            public int AspectProperty
            {
                [SimpleAspect ("AspectProperty_Get")]
                get;
                set;
            }

            [SimpleAspect("ExcludeMethod", Exclude = true)]
            public void ExcludeMethod() { }

            [SimpleAspect ("ExcludeProperty", Exclude = true)]
            public int ExcludeProperty
            {
                [SimpleAspect ("ExcludeProperty_Get")]
                get;
                set;
            }


            [SimpleAspect ("ExcludePropertyMultiAspects", Exclude = true)]
            [SimpleAspect2 ("ExcludePropertyMultiAspects2")]
            public int ExcludePropertyMultiAspects
            {
                [SimpleAspect ("ExcludePropertyMultiAspects_Get")]
                [SimpleAspect2 ("ExcludePropertyMultiAspects2_Get", Exclude = true)]
                get;
                set;
            }

            [SimpleAspect("ReplaceMethod", Replace = true)]
            public void ReplaceMethod() { }


            [SimpleAspect2("ReplaceMethod2", Replace = true)]
            public void ReplaceMethod2() { }


            [SimpleAspect ("ReplaceProperty", Replace = true)]
            public int ReplaceProperty
            {
                [SimpleAspect ("ReplaceProperty_Get")]
                get;
                set;
            }

            [SimpleAspect ("ReplacePropertyMultiAspects")]
            [SimpleAspect2 ("ReplacePropertyMultiAspects2")]
            public int ReplacePropertyMultiAspects
            {
                [SimpleAspect ("ReplacePropertyMultiAspects_Get", Replace = true)]
                [SimpleAspect2 ("ReplacePropertyMultiAspects2_Get", Replace = true)]
                get;
                set;
            }
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
        public void Aspect_On_Method_From_DeclaringType()
        {
            t.EmptyMethod();
            Assert.Contains ("Class", callsBag);
        }


        [Fact]
        public void AspectOnMethod_AndDeclaringType()
        {
            t.AspectMethod();
            Assert.Contains ("Class", callsBag);
            Assert.Contains ("AspectMethod", callsBag);
        }


        [Fact]
        public void AspectOnProperty_FromDeclaringType()
        {
            t.Prop1 = 42;
            Assert.Contains ("Class", callsBag);
        }


        [Fact]
        public void AspectOnProperty_AndDeclaringType()
        {
            t.AspectProperty = 42;
            Assert.Contains ("Class", callsBag);
            Assert.Contains ("AspectProperty", callsBag);
            Assert.DoesNotContain ("AspectProperty_Get", callsBag);

            int v = t.AspectProperty;
            Assert.Contains ("AspectProperty_Get", callsBag);
        }


        [Fact]
        public void Exclude_OnMethod()
        {
            t.ExcludeMethod();
            Assert.Equal (0, callsBag.Count);
        }


        [Fact]
        public void Exclude_OnProperty()
        {
            t.ExcludeProperty = 42;
            Assert.Equal (0, callsBag.Count);

            int i = t.ExcludeProperty;
            Assert.Equal (1, callsBag.Count);
            Assert.Contains ("ExcludeProperty_Get", callsBag);
        }


        [Fact]
        public void Exclude_OnProperty_MultiAspects()
        {
            t.ExcludePropertyMultiAspects = 42;
            Assert.Equal (1, callsBag.Count);
            Assert.Contains ("ExcludePropertyMultiAspects2", callsBag);

            callsBag.Clear();
            int i = t.ExcludePropertyMultiAspects;
            Assert.Equal (1, callsBag.Count);
            Assert.Contains ("ExcludePropertyMultiAspects_Get", callsBag);
        }


        [Fact]
        public void Replace_OnMethod()
        {
            t.ReplaceMethod();
            Assert.Equal (1, callsBag.Count);
            Assert.Contains ("ReplaceMethod", callsBag);
        }


        [Fact]
        public void Replace_OnMethod_Without_Having_Replacing_Aspect()
        {
            t.ReplaceMethod2();
            Assert.Equal (2, callsBag.Count);
            Assert.Contains ("ReplaceMethod2", callsBag);
            Assert.Contains ("Class", callsBag);
        }


        [Fact]
        public void Replace_OnProperty()
        {
            t.ReplaceProperty = 42;
            Assert.Equal (1, callsBag.Count);
            Assert.Contains ("ReplaceProperty", callsBag);
            
            callsBag.Clear();
            int i = t.ReplaceProperty;
            Assert.Equal (2, callsBag.Count);
            Assert.Contains ("ReplaceProperty", callsBag);
            Assert.Contains ("ReplaceProperty_Get", callsBag);
        }


        [Fact]
        public void Replace_OnProperty_MultiAspects()
        {
            t.ReplacePropertyMultiAspects = 42;
            Assert.Equal (3, callsBag.Count);
            Assert.Contains ("Class", callsBag);
            Assert.Contains ("ReplacePropertyMultiAspects", callsBag);
            Assert.Contains ("ReplacePropertyMultiAspects2", callsBag);

            callsBag.Clear();
            int i = t.ReplacePropertyMultiAspects;
            Assert.Equal (2, callsBag.Count);
            Assert.Contains ("ReplacePropertyMultiAspects_Get", callsBag);
            Assert.Contains ("ReplacePropertyMultiAspects2_Get", callsBag);
        }


        [Fact]
        public void Aspect_On_Method_From_Declaring_Declaring_Type()
        {
            var t2 = new TestClass.NestedClass();
            callsBag.Clear();

            t2.EmptyMethod();
            Assert.Contains ("Class", callsBag);
        }


        [Fact]
        public void Aspect_On_Method_AND_From_Declaring_Declaring_Type()
        {
            var t2 = new TestClass.NestedClass();
            callsBag.Clear();

            t2.AspectMethod();
            Assert.Contains ("Class", callsBag);
            Assert.Contains ("AspectMethod", callsBag);
            Assert.Equal (2, callsBag.Count);
        }

    }
}