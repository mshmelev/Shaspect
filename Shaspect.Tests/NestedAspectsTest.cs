using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public sealed class NestedAspectsTest : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<string> calls= new List<string>();
        

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


        [SimpleAspect ("ComplexClass", ElementTargets = ElementTargets.Method | ElementTargets.Property)]
        public class ComplexClass
        {
            [SimpleAspect ("NestedClass", ElementTargets = ElementTargets.Method, Exclude = true)]
            public class NestedClass
            {
                public void SimpleMethod()
                {
                }


                [SimpleAspect ("MethodWithAspect", ElementTargets = ElementTargets.Method)]
                public void MethodWithAspect()
                {
                }

                public int Prop { get; set; }
            }



            [SimpleAspect ("NestedClass", ElementTargets = ElementTargets.Method, Replace = true)]
            public class NestedClass2
            {
                public void SimpleMethod()
                {
                }


                [SimpleAspect ("MethodWithAspect", ElementTargets = ElementTargets.Method, Replace = true)]
                public void MethodWithAspect()
                {
                }

                public int Prop { get; set; }
            }
        }



        private readonly TestClass t;


        public NestedAspectsTest()
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
        public void Aspect_On_Method_From_DeclaringType()
        {
            t.EmptyMethod();
            Assert.Equal (new[] {"Class"}, calls);
        }


        [Fact]
        public void AspectOnMethod_AndDeclaringType()
        {
            t.AspectMethod();
            Assert.Equal (new[] {"AspectMethod", "Class"}, calls);
        }


        [Fact]
        public void AspectOnProperty_FromDeclaringType()
        {
            t.Prop1 = 42;
            Assert.Equal (new[] {"Class"}, calls);
        }


        [Fact]
        public void AspectOnProperty_AndDeclaringType()
        {
            t.AspectProperty = 42;
            Assert.Contains ("Class", calls);
            Assert.Contains ("AspectProperty", calls);
            Assert.DoesNotContain ("AspectProperty_Get", calls);

            int v = t.AspectProperty;
            Assert.Contains ("AspectProperty_Get", calls);
        }


        [Fact]
        public void Exclude_OnMethod()
        {
            t.ExcludeMethod();
            Assert.Empty (calls);
        }


        [Fact]
        public void Exclude_OnProperty()
        {
            t.ExcludeProperty = 42;
            Assert.Empty (calls);

            int i = t.ExcludeProperty;
            Assert.Equal (new[] {"ExcludeProperty_Get"}, calls);
        }


        [Fact]
        public void Exclude_OnProperty_MultiAspects()
        {
            t.ExcludePropertyMultiAspects = 42;
            Assert.Equal (new[] {"ExcludePropertyMultiAspects2"}, calls);

            calls.Clear();
            int i = t.ExcludePropertyMultiAspects;
            Assert.Equal (new[] {"ExcludePropertyMultiAspects_Get"}, calls);
        }


        [Fact]
        public void Replace_OnMethod()
        {
            t.ReplaceMethod();
            Assert.Equal (new[] {"ReplaceMethod"}, calls);
        }


        [Fact]
        public void Replace_OnMethod_Without_Having_Replacing_Aspect()
        {
            t.ReplaceMethod2();
            Assert.Equal (new[] {"ReplaceMethod2", "Class"}, calls);
        }


        [Fact]
        public void Replace_OnProperty()
        {
            t.ReplaceProperty = 42;
            Assert.Equal (new[] {"ReplaceProperty"}, calls);
            
            calls.Clear();
            int i = t.ReplaceProperty;
            Assert.Equal (new[] {"ReplaceProperty_Get", "ReplaceProperty"}, calls);
        }


        [Fact]
        public void Replace_OnProperty_MultiAspects()
        {
            t.ReplacePropertyMultiAspects = 42;
            Assert.Equal (3, calls.Count);
            Assert.Contains ("Class", calls);
            Assert.Contains ("ReplacePropertyMultiAspects", calls);
            Assert.Contains ("ReplacePropertyMultiAspects2", calls);

            calls.Clear();
            int i = t.ReplacePropertyMultiAspects;
            Assert.Equal (2, calls.Count);
            Assert.Contains ("ReplacePropertyMultiAspects_Get", calls);
            Assert.Contains ("ReplacePropertyMultiAspects2_Get", calls);
        }


        [Fact]
        public void Aspect_On_Method_From_Declaring_Declaring_Type()
        {
            var t2 = new TestClass.NestedClass();
            calls.Clear();

            t2.EmptyMethod();
            Assert.Equal (new[] {"Class"}, calls);
        }


        [Fact]
        public void Aspect_On_Method_AND_From_Declaring_Declaring_Type()
        {
            var t2 = new TestClass.NestedClass();
            calls.Clear();

            t2.AspectMethod();
            Assert.Equal (new[] {"AspectMethod", "Class"}, calls);
        }
		
		
        [Fact]
        public void Target_Method_and_Exclude()
        {
            var t2 = new ComplexClass.NestedClass();
            t2.SimpleMethod();
            Assert.Empty (calls);

            t2.Prop = 42;
            Assert.Equal (new[] {"ComplexClass"}, calls);
        }


        [Fact]
        public void Target_Method_and_Exclude_Reinclude()
        {
            var t2 = new ComplexClass.NestedClass();
            t2.MethodWithAspect();
            Assert.Equal (new[] {"MethodWithAspect"}, calls);
        }


        [Fact]
        public void Target_Method_and_Replace()
        {
            var t2 = new ComplexClass.NestedClass2();
            t2.SimpleMethod();
            Assert.Equal (new[] {"NestedClass"}, calls);

            calls.Clear();
            t2.Prop = 42;
            Assert.Equal (new[] {"ComplexClass"}, calls);
        }


        [Fact]
        public void Target_Method_and_Replace_Twice()
        {
            var t2 = new ComplexClass.NestedClass2();
            t2.MethodWithAspect();
            Assert.Equal (new[] {"MethodWithAspect"}, calls);
        }
    }
}