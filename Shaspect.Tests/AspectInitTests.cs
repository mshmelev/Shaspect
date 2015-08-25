using System;
using System.Collections.Concurrent;
using System.Linq;
using Xunit;


namespace Shaspect.Tests
{
    /// <summary>
    /// Checks aspects initialization: passing constructor parameters and setting properties.
    /// </summary>
    public class AspectInitTests
    {
        private static readonly ConcurrentBag<object> callsBag= new ConcurrentBag<object>();

        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            private readonly sbyte sb;
            private readonly byte b;
            private readonly short s;
            private readonly ushort us;
            private readonly int i;
            private readonly uint ui;
            private readonly long l;
            private readonly ulong ul;
            private readonly float f;
            private readonly double d;
            private readonly char c;
            private readonly uint[] uiArr;
            private readonly string str;


            public SimpleAspectAttribute()
            {
            }


            public SimpleAspectAttribute(string str)
            {
                this.str = str;
            }


            public SimpleAspectAttribute(
                sbyte sb,
                byte b,
                short s,
                ushort us,
                int i,
                uint ui,
                long l,
                ulong ul,
                float f,
                double d,
                char c,
                uint[] uiArr,
                string str)
            {
                this.sb = sb;
                this.b = b;
                this.s = s;
                this.us = us;
                this.i = i;
                this.ui = ui;
                this.l = l;
                this.ul = ul;
                this.f = f;
                this.d = d;
                this.c = c;
                this.uiArr = uiArr;
                this.str = str;
            }


            public int IntProp
            {
                get; set;
            }


            public string StrProp
            {
                get; set;
            }


            public double DoubleProp
            {
                get; set;
            }


            public override void OnEntry()
            {
                callsBag.Add (sb);
                callsBag.Add (b);
                callsBag.Add (s);
                callsBag.Add (us);
                callsBag.Add (i);
                callsBag.Add (ui);
                callsBag.Add (l);
                callsBag.Add (ul);
                callsBag.Add (f);
                callsBag.Add (d);
                callsBag.Add (c);
                callsBag.Add (uiArr);
                callsBag.Add (str);
                callsBag.Add (IntProp);
                callsBag.Add (DoubleProp);
                callsBag.Add (StrProp);
            }
        }


        public class TestClass
        {
            [SimpleAspectAttribute(1, 2, 3, 4, 5, 6, 7L, 8L, (float) 9.1, 10.1, 'f', new uint[] {1, 2, 3}, "sampleStr")]
            public void CtorParams()
            {
            }


            [SimpleAspectAttribute(IntProp = 101, DoubleProp = 101.2, StrProp = "propParam")]
            public void PropertyParams()
            {
            }


            [SimpleAspectAttribute("mixedParam1", StrProp = "mixedParam2")]
            public void MixedParams()
            {
            }


            [SimpleAspectAttribute("overloadedEmpty")]
            public int OverloadedMethod()
            {
                return 1;
            }


            [SimpleAspectAttribute("overloadedModerate")]
            public int OverloadedMethod(int i, string s)
            {
                return 2;
            }


            [SimpleAspectAttribute("overloadedExtreme")]
            public int OverloadedMethod(int i, double d, string s, DateTime dt, object o)
            {
                return 3;
            }

        }


        [Fact]
        public void CtorParamsArePassed()
        {
            var t = new TestClass();
            t.CtorParams();
            Assert.True (callsBag.Contains ((sbyte)1));
            Assert.True (callsBag.Contains ((byte)2));
            Assert.True (callsBag.Contains ((short)3));
            Assert.True (callsBag.Contains ((ushort)4));
            Assert.True (callsBag.Contains ((int)5));
            Assert.True (callsBag.Contains ((uint)6));
            Assert.True (callsBag.Contains ((long)7));
            Assert.True (callsBag.Contains ((ulong)8));
            Assert.True (callsBag.Contains ((float)9.1));
            Assert.True (callsBag.Contains ((double)10.1));
            Assert.True (callsBag.Contains ('f'));
            Assert.Equal (callsBag.First (m => m is Array), new uint[] {1, 2, 3});
            Assert.True (callsBag.Contains ("sampleStr"));
        }


        [Fact]
        public void PropertiesArePassed()
        {
            var t = new TestClass();
            t.PropertyParams();
            Assert.True (callsBag.Contains (101));
            Assert.True (callsBag.Contains (101.2));
            Assert.True (callsBag.Contains ("propParam"));
        }


        [Fact]
        public void MixedParamsArePassed()
        {
            var t = new TestClass();
            t.MixedParams();
            Assert.True (callsBag.Contains ("mixedParam1"));
            Assert.True (callsBag.Contains ("mixedParam2"));
        }


        [Fact]
        public void OverloadedMethodsAreHandled()
        {
            var t = new TestClass();
            Assert.Equal (1, t.OverloadedMethod());
            Assert.True (callsBag.Contains ("overloadedEmpty"));

            Assert.Equal (2, t.OverloadedMethod(1, ""));
            Assert.True (callsBag.Contains ("overloadedModerate"));

            Assert.Equal (3, t.OverloadedMethod(1, 1.2, "", DateTime.Now, this));
            Assert.True (callsBag.Contains ("overloadedExtreme"));
        }


    }
}