using System.Collections.Concurrent;
using System.Linq;
using Xunit;


namespace Shaspect.Tests
{
    public class OnEntryBasicTests
    {
        private static readonly ConcurrentBag<string> callsBag= new ConcurrentBag<string>();
        

        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            private readonly string callName;


            public SimpleAspectAttribute(string callName)
            {
                this.callName = callName;
            }


            public override void OnEntry()
            {
                callsBag.Add (callName);
            }
        }


        public class SimpleAspect2Attribute : SimpleAspectAttribute
        {
            public SimpleAspect2Attribute (string callName) : base(callName)
            {
            }
        }


        class TestClass
        {
            [SimpleAspect("EmptyMethod")]
            public void EmptyMethod()
            {
            }


            [SimpleAspect("Calc")]
            public int Calc (int a, int b)
            {
                return a + b;
            }


            [SimpleAspect("StaticMethod")]
            public static void StaticMethod()
            {
            }


            public void CallPrivate()
            {
                PrivateMethod();
            }


            [SimpleAspect("PrivateMethod")]
            private void PrivateMethod()
            {
            }


            [SimpleAspect("Multi1")]
            [SimpleAspect2("Multi2")]
            public void MultiAspectsMethod()
            {
            }


            [SimpleAspect ("Prop1")]
            public int Prop1
            {
                get; set;
            }


            [SimpleAspect ("Prop2")]
            public int Prop2
            {
                get; set;
            }


            [SimpleAspect ("ReadOnlyProp")]
            public int ReadOnlyProp
            {
                get { return 42; }
            }


            public int PropGetAspect
            {
                [SimpleAspect ("PropGetAspect")]
                get;
                set;
            }


            public int PropSetAspect
            {
                get;
                [SimpleAspect ("PropSetAspect")]
                set;
            }


            [SimpleAspect ("IndexedProp_Int")]
            public int this[int i]
            {
                get { return i + 1; }
            }


            [SimpleAspect ("IndexedProp_Str")]
            public int this[string s]
            {
                get { return 42; }
            }


            [SimpleAspect ("IndexedProp_Mix")]
            public string this[string s, int i]
            {
                get { return s + "_"+ i; }
            }
        }


        [Fact]
        public void OnEntryIsCalled()
        {
            var t = new TestClass();
            t.EmptyMethod();
            Assert.True (callsBag.Contains ("EmptyMethod"));
        }


        [Fact]
        public void MethodIsCalled()
        {
            var t = new TestClass();
            Assert.Equal (4, t.Calc (1, 3));
            Assert.True (callsBag.Contains ("Calc"));
        }


        [Fact]
        public void StaticMethod()
        {
            TestClass.StaticMethod();
            Assert.True (callsBag.Contains ("StaticMethod"));
        }


        [Fact]
        public void PrivateMethod()
        {
            var t = new TestClass();
            t.CallPrivate();
            Assert.True (callsBag.Contains ("PrivateMethod"));
        }


        [Fact]
        public void GetProperty()
        {
            var t = new TestClass();
            int i = t.Prop1;
            Assert.True (callsBag.Contains ("Prop1"));
        }


        [Fact]
        public void SetProperty()
        {
            var t = new TestClass();
            t.Prop2 = 63;
            Assert.True (callsBag.Contains ("Prop2"));
        }


        [Fact]
        public void ReadOnlyProperty()
        {
            var t = new TestClass();
            int i = t.ReadOnlyProp;
            Assert.True (callsBag.Contains ("ReadOnlyProp"));
        }


        [Fact]
        public void PropGetAspect()
        {
            var t = new TestClass();
            t.PropGetAspect = 42;
            Assert.False (callsBag.Contains ("PropGetAspect"));
            int i = t.PropGetAspect;
            Assert.True (callsBag.Contains ("PropGetAspect"));
        }


        [Fact]
        public void PropSetAspect()
        {
            var t = new TestClass();
            int i = t.PropSetAspect;
            Assert.False (callsBag.Contains ("PropSetAspect"));
            t.PropSetAspect = 42;
            Assert.True (callsBag.Contains ("PropSetAspect"));
        }


        [Fact]
        public void MultipleAspects()
        {
            var t = new TestClass();
            t.MultiAspectsMethod();
            Assert.True (callsBag.Contains ("Multi1"));
            Assert.True (callsBag.Contains ("Multi2"));
        }


        [Fact]
        public void IndexedProperties()
        {
            var t = new TestClass();
            Assert.Equal(43, t[42]);
            Assert.True (callsBag.Contains ("IndexedProp_Int"));

            Assert.Equal(42, t["42"]);
            Assert.True (callsBag.Contains ("IndexedProp_Str"));

            Assert.Equal("42_43", t["42", 43]);
            Assert.True (callsBag.Contains ("IndexedProp_Mix"));
        }




    }
}