using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class OnEntryBasicTests : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<string> callsBag = new List<string>();


        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            private readonly string callName;


            public SimpleAspectAttribute (string callName)
            {
                this.callName = callName;
            }


            public override void OnEntry (MethodExecInfo methodExecInfo)
            {
                callsBag.Add (callName);
            }
        }


        public class SimpleAspect2Attribute : SimpleAspectAttribute
        {
            public SimpleAspect2Attribute (string callName) : base (callName)
            {
            }
        }


        private class TestClass
        {
            [SimpleAspect ("default .ctor")]
            public TestClass()
            {
                callsBag.Add ("TestClass()");
            }


            [SimpleAspect ("int .ctor")]
            public TestClass (int i) : this (i.ToString())
            {
                callsBag.Add ("TestClass(int)");
            }


            [SimpleAspect ("string .ctor")]
            public TestClass (string s) : this()
            {
                callsBag.Add ("TestClass(string)");
            }


            [SimpleAspect ("bool .ctor")]
            public TestClass (bool b)
            {
                callsBag.Add ("TestClass(bool)");
            }


            [SimpleAspect ("EmptyMethod")]
            public void EmptyMethod()
            {
            }


            [SimpleAspect ("Calc")]
            public int Calc (int a, int b)
            {
                return a + b;
            }


            [SimpleAspect ("StaticMethod")]
            public static void StaticMethod()
            {
            }


            public void CallPrivate()
            {
                PrivateMethod();
            }


            [SimpleAspect ("PrivateMethod")]
            private void PrivateMethod()
            {
            }


            [SimpleAspect ("Multi1")]
            [SimpleAspect2 ("Multi2")]
            public void MultiAspectsMethod()
            {
            }


            [SimpleAspect ("Prop1")]
            public int Prop1 { get; set; }


            [SimpleAspect ("Prop2")]
            public int Prop2 { get; set; }


            [SimpleAspect ("ReadOnlyProp")]
            public int ReadOnlyProp
            {
                get { return 42; }
            }


            public int PropGetAspect { [SimpleAspect ("PropGetAspect")] get; set; }


            public int PropSetAspect { get; [SimpleAspect ("PropSetAspect")] set; }


            [SimpleAspect ("IndexedProp_Int")]
            public int this [int i]
            {
                get { return i + 1; }
            }


            [SimpleAspect ("IndexedProp_Str")]
            public int this [string s]
            {
                get { return 42; }
            }


            [SimpleAspect ("IndexedProp_Mix")]
            public string this [string s, int i]
            {
                get { return s + "_" + i; }
            }
        }


        private class TestClass2 : TestClass
        {
            [SimpleAspect ("bool2 .ctor")]
            public TestClass2 (bool b) : base (b)
            {
                callsBag.Add ("TestClass2(bool)");
            }
        }


        private static class TestStaticClass
        {


            [SimpleAspect ("StaticClass_StaticMethod")]
            public static void StaticMethod()
            {
            }
        }


        /// <summary>
        /// it's important this class is used only in one test to properly validate static ctor behavior
        /// </summary>
        private class ClassWithStaticCtor 
        {
            [SimpleAspect ("static .ctor")]
            static ClassWithStaticCtor()
            {
                callsBag.Add ("ClassWithStaticCtor()");
            }


            public static void Method()
            {
            }
        }


        /// <summary>
        /// </summary>
        public OnEntryBasicTests()
        {
            Monitor.Enter (sync);
            callsBag.Clear();
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void OnEntry_Called_For_Empty_Method()
        {
            var t = new TestClass();
            t.EmptyMethod();
            Assert.True (callsBag.Contains ("EmptyMethod"));
        }


        [Fact]
        public void OnEntry_Called_For_Method()
        {
            var t = new TestClass();
            Assert.Equal (4, t.Calc (1, 3));
            Assert.True (callsBag.Contains ("Calc"));
        }


        [Fact]
        public void OnEntry_Called_For_StaticMethod()
        {
            TestClass.StaticMethod();
            Assert.True (callsBag.Contains ("StaticMethod"));
        }


        [Fact]
        public void OnEntry_Called_For_StaticMethod_In_StaticClass()
        {
            TestStaticClass.StaticMethod();
            Assert.True (callsBag.Contains ("StaticClass_StaticMethod"));
        }


        [Fact]
        public void OnEntry_Called_For_Private_Method()
        {
            var t = new TestClass();
            t.CallPrivate();
            Assert.True (callsBag.Contains ("PrivateMethod"));
        }


        [Fact]
        public void OnEntry_Called_For_Aspect_On_Property_When_Get_Called()
        {
            var t = new TestClass();
            var i = t.Prop1;
            Assert.True (callsBag.Contains ("Prop1"));
        }


        [Fact]
        public void OnEntry_Called_For_Aspect_On_Property_When_Set_Called()
        {
            var t = new TestClass();
            t.Prop2 = 63;
            Assert.True (callsBag.Contains ("Prop2"));
        }


        [Fact]
        public void OnEntry_Called_For_ReadOnlyProperty()
        {
            var t = new TestClass();
            var i = t.ReadOnlyProp;
            Assert.True (callsBag.Contains ("ReadOnlyProp"));
        }


        [Fact]
        public void OnEntry_Called_For_Aspect_On_PropertyGet()
        {
            var t = new TestClass();
            t.PropGetAspect = 42;
            Assert.False (callsBag.Contains ("PropGetAspect"));
            var i = t.PropGetAspect;
            Assert.True (callsBag.Contains ("PropGetAspect"));
        }


        [Fact]
        public void OnEntry_Called_For_Aspect_On_PropertySet()
        {
            var t = new TestClass();
            var i = t.PropSetAspect;
            Assert.False (callsBag.Contains ("PropSetAspect"));
            t.PropSetAspect = 42;
            Assert.True (callsBag.Contains ("PropSetAspect"));
        }


        [Fact]
        public void OnEntry_Called_For_Multiple_Aspects()
        {
            var t = new TestClass();
            t.MultiAspectsMethod();
            Assert.True (callsBag.Contains ("Multi1"));
            Assert.True (callsBag.Contains ("Multi2"));
        }


        [Fact]
        public void OnEntry_Called_For_Indexed_Properties()
        {
            var t = new TestClass();
            Assert.Equal (43, t[42]);
            Assert.True (callsBag.Contains ("IndexedProp_Int"));

            Assert.Equal (42, t["42"]);
            Assert.True (callsBag.Contains ("IndexedProp_Str"));

            Assert.Equal ("42_43", t["42", 43]);
            Assert.True (callsBag.Contains ("IndexedProp_Mix"));
        }


        [Fact]
        public void OnEntry_Called_For_Instance_Ctor()
        {
            var t = new TestClass();
            Assert.Equal (new[] {"default .ctor", "TestClass()"}, callsBag);
        }


        [Fact]
        public void OnEntry_Called_For_Static_Ctor()
        {
            ClassWithStaticCtor.Method();   // ensure static ctor is called
            Assert.Equal (new[] {"static .ctor", "ClassWithStaticCtor()"}, callsBag);
        }


        [Fact]
        public void OnEntry_Called_For_Ctor_Chain_In_Proper_Order()
        {
            var t = new TestClass (42);
            Assert.Equal (new[]
            {
                "default .ctor", "TestClass()",
                "string .ctor", "TestClass(string)",
                "int .ctor", "TestClass(int)"
            },
                callsBag);
        }


        [Fact]
        public void OnEntry_Called_For_Base_Ctor_In_Proper_Order()
        {
            var t = new TestClass2 (true);
            Assert.Equal (new[]
            {
                "bool .ctor", "TestClass(bool)",
                "bool2 .ctor", "TestClass2(bool)"
            },
                callsBag);
        }
    }
}