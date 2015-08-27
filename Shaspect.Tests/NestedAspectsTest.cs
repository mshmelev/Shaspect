using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class NestedAspectsTest : IDisposable
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


            public override void OnEntry()
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
        public void AspectOnMethod_AndDeclaringType()
        {
            t.AspectMethod();
            Assert.True (callsBag.Contains ("Class"));
            Assert.True (callsBag.Contains ("AspectMethod"));
        }


        [Fact]
        public void AspectOnProperty_FromDeclaringType()
        {
            t.Prop1 = 42;
            Assert.True (callsBag.Contains ("Class"));
        }


        [Fact]
        public void AspectOnProperty_AndDeclaringType()
        {
            t.AspectProperty = 42;
            Assert.True (callsBag.Contains ("Class"));
            Assert.True (callsBag.Contains ("AspectProperty"));
            Assert.False (callsBag.Contains ("AspectProperty_Get"));

            int v = t.AspectProperty;
            Assert.True (callsBag.Contains ("AspectProperty_Get"));
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
            Assert.True (callsBag.Contains ("ExcludeProperty_Get"));
        }


        [Fact]
        public void Exclude_OnProperty_MultiAspects()
        {
            t.ExcludePropertyMultiAspects = 42;
            Assert.Equal (1, callsBag.Count);
            Assert.True (callsBag.Contains ("ExcludePropertyMultiAspects2"));

            callsBag.Clear();
            int i = t.ExcludePropertyMultiAspects;
            Assert.Equal (1, callsBag.Count);
            Assert.True (callsBag.Contains ("ExcludePropertyMultiAspects_Get"));
        }



    }
}