using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    /// <summary>
    /// Checks aspects initialization: passing constructor parameters and setting properties.
    /// </summary>
    public class AspectInitTests : IDisposable
    {
        private static readonly object sync = new object();
        private static ConcurrentBag<object> callsBag= new ConcurrentBag<object>();

        public enum TestEnum : byte
        {
            V1=7,
            V2,
            V3,
            V4
        }


        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            public sbyte sb;
            public byte b;
            public short s;
            public ushort us;
            public int i;
            public uint ui;
            public long l;
            public ulong ul;
            public float f;
            public double d;
            public char c;
            public bool bl;
            public TestEnum testEnum;
            public string str;
            private string str2;

            public sbyte[] sbArr;
            public byte[] bArr;
            public short[] sArr;
            public ushort[] usArr;
            public int[] iArr;
            public uint[] uiArr;
            public long[] lArr;
            public ulong[] ulArr;
            public float[] fArr;
            public double[] dArr;
            public char[] cArr;
            public bool[] blArr;
            public TestEnum[] testEnumArr;
            public string[] strArr;


            public SimpleAspectAttribute()
            {
            }


            public SimpleAspectAttribute(string str)
            {
                str2 = str;
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
				bool bl,
                TestEnum testEnum,
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
                this.bl = bl;
                this.testEnum= testEnum;
                this.str = str;
            }


            public SimpleAspectAttribute(
                sbyte[] sbArr,
                byte[] bArr,
                short[] sArr,
                ushort[] usArr,
                int[] iArr,
                uint[] uiArr,
                long[] lArr,
                ulong[] ulArr,
                float[] fArr,
                double[] dArr,
                char[] cArr,
				bool[] blArr,
                TestEnum[] testEnumArr,
                string[] strArr)
            {
                this.sbArr = sbArr;
                this.bArr = bArr;
                this.sArr = sArr;
                this.usArr = usArr;
                this.iArr = iArr;
                this.uiArr = uiArr;
                this.lArr = lArr;
                this.ulArr = ulArr;
                this.fArr = fArr;
                this.dArr = dArr;
                this.cArr = cArr;
                this.blArr = blArr;
                this.testEnumArr = testEnumArr;
                this.strArr = strArr;
            }


            public sbyte Sb {get; set;}
            public byte B { get; set; }
            public short S { get; set; }
            public ushort Us { get; set; }
            public int I { get; set; }
            public uint Ui { get; set; }
            public long L { get; set; }
            public ulong Ul { get; set; }
            public float F { get; set; }
            public double D { get; set; }
            public char C { get; set; }
            public bool Bl { get; set; }
            public TestEnum TestEnum { get; set; }
            public string Str { get; set; }

            public sbyte[] SbArr { get; set; }
            public byte[] BArr { get; set; }
            public short[] SArr { get; set; }
            public ushort[] UsArr { get; set; }
            public int[] IArr { get; set; }
            public uint[] UiArr { get; set; }
            public long[] LArr { get; set; }
            public ulong[] UlArr { get; set; }
            public float[] FArr { get; set; }
            public double[] DArr { get; set; }
            public char[] CArr { get; set; }
            public bool[] BlArr { get; set; }
            public TestEnum[] TestEnumArr { get; set; }
            public string[] StrArr { get; set; }


            public override void OnEntry(MethodExecInfo methodExecInfo)
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
                callsBag.Add (bl);
                callsBag.Add (testEnum);
                callsBag.Add (str);
                callsBag.Add (str2);

                callsBag.Add (sbArr);
                callsBag.Add (bArr);
                callsBag.Add (sArr);
                callsBag.Add (usArr);
                callsBag.Add (iArr);
                callsBag.Add (uiArr);
                callsBag.Add (lArr);
                callsBag.Add (ulArr);
                callsBag.Add (fArr);
                callsBag.Add (dArr);
                callsBag.Add (cArr);
                callsBag.Add (blArr);
                callsBag.Add (testEnumArr);
                callsBag.Add (strArr);

                callsBag.Add (Sb);
                callsBag.Add (B);
                callsBag.Add (S);
                callsBag.Add (Us);
                callsBag.Add (I);
                callsBag.Add (Ui);
                callsBag.Add (L);
                callsBag.Add (Ul);
                callsBag.Add (F);
                callsBag.Add (D);
                callsBag.Add (C);
                callsBag.Add (Bl);
                callsBag.Add (TestEnum);
                callsBag.Add (Str);

                callsBag.Add (SbArr);
                callsBag.Add (BArr);
                callsBag.Add (SArr);
                callsBag.Add (UsArr);
                callsBag.Add (IArr);
                callsBag.Add (UiArr);
                callsBag.Add (LArr);
                callsBag.Add (UlArr);
                callsBag.Add (FArr);
                callsBag.Add (DArr);
                callsBag.Add (CArr);
                callsBag.Add (BlArr);
                callsBag.Add (TestEnumArr);
                callsBag.Add (StrArr);
            }
        }

        public class SimpleAspect2Attribute : SimpleAspectAttribute
        {
            public string str_new;
            public string Str_new { get; set; }


            public override void OnEntry (MethodExecInfo methodExecInfo)
            {
                callsBag.Add (str);
                callsBag.Add (str_new);
                callsBag.Add (Str);
                callsBag.Add (Str_new);
            }
        }


        public class TestClass
        {
            [SimpleAspect (1, 2, 3, 4, 5, 6, 7L, 8L, 9.1f, 10.1, 'f', true, TestEnum.V1, "sampleStr")]
            public void CtorParams()
            {
            }


            [SimpleAspect (new sbyte[]{1,2}, new byte[]{3,4}, new short[]{5,6}, new ushort[]{7,8}, new[]{9,10}, new uint[]{11,12},
                new long[]{13,14}, new ulong[]{15,16}, new[]{17.1f,18.2f}, new[]{19.1,20.2}, new []{'a','b','c'}, 
                new[]{true,false,true}, new[] {TestEnum.V1, TestEnum.V2}, new[]{"aa","bb","cc"})]
            public void CtorArrayParams()
            {
            }


            [SimpleAspect (sb= 101, b= 102, s= 103, us=104, i=105, ui=106, l=107L, ul=108L, f=109.1f, d=110.1, c='g', bl=true, testEnum = TestEnum.V3, str="fieldStr")]
            public void FieldParams()
            {
            }


            [SimpleAspect (sbArr= new sbyte[]{101,102}, bArr= new byte[]{103,104}, sArr=new short[]{105,106}, usArr=new ushort[]{107,108}, iArr=new[]{109,110},
                uiArr=new uint[]{111,112}, lArr=new long[]{113,114}, ulArr=new ulong[]{115,116}, fArr=new[]{117.1f,118.2f}, dArr=new[]{119.1,120.2},
                cArr=new []{'d','e','f'}, blArr=new[]{true,false,false}, testEnumArr = new []{TestEnum.V2, TestEnum.V3}, strArr=new[]{"dd","ee","ff"})]
            public void FieldArrayParams()
            {
            }


            [SimpleAspect (Sb= 127, B= 202, S= 203, Us=204, I=205, Ui=206, L=207L, Ul=208L, F=209.1f, D=210.1, C='h', Bl=true, TestEnum = TestEnum.V4, Str="propStr")]
            public void PropertyParams()
            {
            }

            [SimpleAspect (SbArr= new sbyte[]{126,127}, BArr= new byte[]{203,204}, SArr=new short[]{205,206}, UsArr=new ushort[]{207,208}, IArr=new[]{209,210},
                UiArr=new uint[]{211,212}, LArr=new long[]{213,214}, UlArr=new ulong[]{215,216}, FArr=new[]{217.1f,218.2f}, DArr=new[]{219.1,220.2},
                CArr=new []{'h','i','j'}, BlArr=new[]{true,true,false,false}, TestEnumArr = new [] {TestEnum.V3, TestEnum.V4}, StrArr=new[]{"hh","ii","jj"})]
            public void PropertyArrayParams()
            {
            }

            [SimpleAspect ("mixedParam1", str= "mixedParam2", Str = "mixedParam3")]
            public void MixedParams()
            {
            }


            [SimpleAspect ("overloadedEmpty")]
            public int OverloadedMethod()
            {
                return 1;
            }


            [SimpleAspect ("overloadedModerate")]
            public int OverloadedMethod (int i, string s)
            {
                return 2;
            }


            [SimpleAspect ("overloadedExtreme")]
            public int OverloadedMethod (int i, double d, string s, DateTime dt, object o)
            {
                return 3;
            }


            [SimpleAspect ("genericMethod")]
            public T GenericMethod<T> (T t)
            {
                return t;
            }


            [SimpleAspect ("genericMethod2")]
            public T GenericMethod<T> (T t, int i)
            {
                return t;
            }


            [SimpleAspect ("genericMethod3")]
            public T GenericMethod<T, T2> (T t, T2 t2)
            {
                return t;
            }

            [SimpleAspect ("genericMethod4")]
            public T GenericMethod<T, T2> (T t, IEnumerable<T2> t2)
            {
                return t;
            }


            [SimpleAspect2(str = "str", str_new = "str_new", Str = "Str", Str_new = "Str_new")]
            public void InheritedFieldsAndPropertiesAttribute()
            {
            }
        }


        public class ClassWithGenericProp<T>
        {
            [SimpleAspect ("genericProp")]
            public T GenericProp { get; set; }
        }


        [SimpleAspect ("genericClass")]
        public class GenericTestClass<T>
        {
            public T TestMethod(T t)
            {
                return t;
            }
        }


        [SimpleAspect ("genericClass2")]
        public class GenericTestClass<T,T2>
        {
            public T TestMethod(T t, T2 t2)
            {
                return t;
            }
        }


        public AspectInitTests()
        {
            Monitor.Enter (sync);
            callsBag= new ConcurrentBag<object>();
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
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
            Assert.True (callsBag.Contains (true));
            Assert.True (callsBag.Contains (TestEnum.V1));
            Assert.True (callsBag.Contains ("sampleStr"));
        }


        [Fact]
        public void CtorArrayParamsArePassed()
        {
            var t = new TestClass();
            t.CtorArrayParams();

            Assert.Contains (new sbyte[] {1, 2}, callsBag);
            Assert.Contains (new byte[] {3, 4}, callsBag);
            Assert.Contains (new short[] {5, 6}, callsBag);
            Assert.Contains (new ushort[] {7, 8}, callsBag);
            Assert.Contains (new[] {9, 10}, callsBag);
            Assert.Contains (new uint[] {11, 12}, callsBag);
            Assert.Contains (new long[] {13, 14}, callsBag);
            Assert.Contains (new ulong[] {15, 16}, callsBag);
            Assert.Contains (new[] {17.1f, 18.2f}, callsBag);
            Assert.Contains (new[] {19.1, 20.2}, callsBag);
            Assert.Contains (new[] {'a', 'b', 'c'}, callsBag);
            Assert.Contains (new[] {true, false, true}, callsBag);
            Assert.Contains (new[] {TestEnum.V1, TestEnum.V2}, callsBag);
            Assert.Contains (new[] {"aa", "bb", "cc"}, callsBag);
        }


        [Fact]
        public void FieldsArePassed()
        {
            var t = new TestClass();
            t.FieldParams();
            Assert.True (callsBag.Contains ((sbyte)101));
            Assert.True (callsBag.Contains ((byte)102));
            Assert.True (callsBag.Contains ((short)103));
            Assert.True (callsBag.Contains ((ushort)104));
            Assert.True (callsBag.Contains ((int)105));
            Assert.True (callsBag.Contains ((uint)106));
            Assert.True (callsBag.Contains ((long)107));
            Assert.True (callsBag.Contains ((ulong)108));
            Assert.True (callsBag.Contains ((float)109.1));
            Assert.True (callsBag.Contains ((double)110.1));
            Assert.True (callsBag.Contains ('g'));
            Assert.True (callsBag.Contains (true));
            Assert.True (callsBag.Contains (TestEnum.V3));
            Assert.True (callsBag.Contains ("fieldStr"));
        }


        [Fact]
        public void FieldArraysArePassed()
        {
            var t = new TestClass();
            t.FieldArrayParams();

            Assert.Contains (new sbyte[] {101, 102}, callsBag);
            Assert.Contains (new byte[] {103, 104}, callsBag);
            Assert.Contains (new short[] {105, 106}, callsBag);
            Assert.Contains (new ushort[] {107, 108}, callsBag);
            Assert.Contains (new[] {109, 110}, callsBag);
            Assert.Contains (new uint[] {111, 112}, callsBag);
            Assert.Contains (new long[] {113, 114}, callsBag);
            Assert.Contains (new ulong[] {115, 116}, callsBag);
            Assert.Contains (new[] {117.1f, 118.2f}, callsBag);
            Assert.Contains (new[] {119.1, 120.2}, callsBag);
            Assert.Contains (new[] {'d', 'e', 'f'}, callsBag);
            Assert.Contains (new[] {true, false, false}, callsBag);
            Assert.Contains (new[] {TestEnum.V2, TestEnum.V3}, callsBag);
            Assert.Contains (new[] {"dd", "ee", "ff"}, callsBag);
        }



        [Fact]
        public void PropertiesArePassed()
        {
            var t = new TestClass();
            t.PropertyParams();

            Assert.True (callsBag.Contains ((sbyte)127));
            Assert.True (callsBag.Contains ((byte)202));
            Assert.True (callsBag.Contains ((short)203));
            Assert.True (callsBag.Contains ((ushort)204));
            Assert.True (callsBag.Contains ((int)205));
            Assert.True (callsBag.Contains ((uint)206));
            Assert.True (callsBag.Contains ((long)207));
            Assert.True (callsBag.Contains ((ulong)208));
            Assert.True (callsBag.Contains ((float)209.1));
            Assert.True (callsBag.Contains ((double)210.1));
            Assert.True (callsBag.Contains ('h'));
            Assert.True (callsBag.Contains (true));
            Assert.True (callsBag.Contains (TestEnum.V4));
            Assert.True (callsBag.Contains ("propStr"));
        }


        [Fact]
        public void PropertyArraysArePassed()
        {
            var t = new TestClass();
            t.PropertyArrayParams();

            Assert.Contains (new sbyte[] {126, 127}, callsBag);
            Assert.Contains (new byte[] {203, 204}, callsBag);
            Assert.Contains (new short[] {205, 206}, callsBag);
            Assert.Contains (new ushort[] {207, 208}, callsBag);
            Assert.Contains (new[] {209, 210}, callsBag);
            Assert.Contains (new uint[] {211, 212}, callsBag);
            Assert.Contains (new long[] {213, 214}, callsBag);
            Assert.Contains (new ulong[] {215, 216}, callsBag);
            Assert.Contains (new[] {217.1f, 218.2f}, callsBag);
            Assert.Contains (new[] {219.1, 220.2}, callsBag);
            Assert.Contains (new[] {'h', 'i', 'j'}, callsBag);
            Assert.Contains (new[] {true, true, false, false}, callsBag);
            Assert.Contains (new[] {TestEnum.V3, TestEnum.V4}, callsBag);
            Assert.Contains (new[] {"hh", "ii", "jj"}, callsBag);
        }


        [Fact]
        public void MixedParamsArePassed()
        {
            var t = new TestClass();
            t.MixedParams();
            Assert.True (callsBag.Contains ("mixedParam1"));
            Assert.True (callsBag.Contains ("mixedParam2"));
            Assert.True (callsBag.Contains ("mixedParam3"));
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


        [Fact]
        public void Aspect_On_MethodWithGenericParameters_Initialized()
        {
            var t = new TestClass();
            t.GenericMethod (4);
            Assert.True (callsBag.Contains ("genericMethod"));

            t.GenericMethod (4, 5);
            Assert.True (callsBag.Contains ("genericMethod2"));

            t.GenericMethod (4, "5");
            Assert.True (callsBag.Contains ("genericMethod3"));

            t.GenericMethod (4, (IEnumerable<int>)new[] {1,2,3});
            Assert.True (callsBag.Contains ("genericMethod4"));
        }


        [Fact]
        public void Aspect_On_GenericProperty_Initialized()
        {
            var t = new ClassWithGenericProp<int>();
            t.GenericProp = 42;
            Assert.True (callsBag.Contains ("genericProp"));
        }


        [Fact]
        public void Aspect_On_GenericClass_Initialized()
        {
            var t = new GenericTestClass<int>();
            t.TestMethod (42);
            Assert.True (callsBag.Contains ("genericClass"));

            var t2 = new GenericTestClass<int,string>();
            t2.TestMethod (42, "a");
            Assert.True (callsBag.Contains ("genericClass2"));
        }


        [Fact]
        public void Aspect_WithInheritedFields_Initialized()
        {
            var t = new TestClass();
            t.InheritedFieldsAndPropertiesAttribute();
            Assert.True (callsBag.Contains ("str"));
            Assert.True (callsBag.Contains ("str_new"));
            Assert.True (callsBag.Contains ("Str"));
            Assert.True (callsBag.Contains ("Str_new"));
        }


    }
}