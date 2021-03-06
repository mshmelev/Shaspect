﻿using System;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class ReturnValueTests : IDisposable
    {
        private static readonly object sync = new object();
        private static object returnRes;
        private readonly TestClass t;


        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            public override void OnSuccess (MethodExecInfo methodExecInfo)
            {
                returnRes = methodExecInfo.ReturnValue;
            }
        }

        public class ChangeReturnValueAttribute : BaseAspectAttribute
        {
            public override void OnSuccess (MethodExecInfo methodExecInfo)
            {
                if (methodExecInfo.Arguments.Length > 0)
                    methodExecInfo.ReturnValue = methodExecInfo.Arguments[0];
                else
                    methodExecInfo.ReturnValue = "new_value";
            }
        }


        [SimpleAspect]
        internal class TestClass
        {
            public string SimpleReturn (int i)
            {
                return i.ToString();
            }


            public byte ByteReturn (byte b)
            {
                return (byte) (b + 1);
            }


            public sbyte SByteReturn (sbyte b)
            {
                return (sbyte) (b + 1);
            }


            public short ShortReturn (short b)
            {
                return (short) (b + 1);
            }


            public ushort UShortReturn (ushort b)
            {
                return (ushort) (b + 1);
            }


            public int IntReturn (int i)
            {
                return i + 1;
            }


            public uint UIntReturn (uint i)
            {
                return i + 1;
            }


            public long LongReturn (long i)
            {
                return i + 1;
            }


            public ulong ULongReturn (ulong i)
            {
                return i + 1;
            }


            public double DoubleReturn (double d)
            {
                return d + 1;
            }


            public float FloatReturn (float f)
            {
                return f + 1;
            }


            public char CharReturn (char c)
            {
                return (char) (c + 1);
            }


            public DateTime StructReturn (DateTime d)
            {
                return d.AddYears (1);
            }


            public TestClass ReferenceTypeReturn (TestClass t)
            {
                return t;
            }


            public int[] ArrayReturn (int a, int b, int c)
            {
                return new[] {a, b, c};
            }


            public dynamic DynamicTypeReturn (int i)
            {
                return new {A = i};
            }


            public T GenericReturn<T> (T i)
            {
                return i;
            }


            public void VoidFunction()
            {
                var d = DateTime.Now;
                d.AddDays (2);
            }


            public int ThrowsException (string s)
            {
                if (s == null)
                    throw new ArgumentNullException ("s");
                return 42;
            }


            [ChangeReturnValue]
            [SimpleAspect (Exclude = true)]
            public int ChangeIntReturnValue (int i)
            {
                return i + 1;
            }


            [ChangeReturnValue]
            [SimpleAspect (Exclude = true)]
            public DateTime ChangeStructReturnValue (DateTime d)
            {
                return d.AddYears (1);
            }


            [ChangeReturnValue]
            [SimpleAspect (Exclude = true)]
            public string ChangeObjectReturnValue (string s)
            {
                return s + "_modified";
            }


            [ChangeReturnValue]
            [SimpleAspect (Exclude = true)]
            public string ChangePropertyReturnValue { get; set; }


            [ChangeReturnValue]
            [SimpleAspect (Exclude = true)]
            public T ChangeGenericReturnValue<T> (T s)
            {
                return default(T);
            }
        }


        public ReturnValueTests()
        {
            t = new TestClass();
            Monitor.Enter (sync);
            returnRes = null;
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void SimpleReturn_Test()
        {
            Assert.Equal ("42", t.SimpleReturn (42));
            Assert.Equal ("42", returnRes);
        }


        [Fact]
        public void ValueTypes_Returned()
        {
            Assert.Equal (3, t.ByteReturn (2));
            Assert.Equal ((byte) 3, returnRes);

            Assert.Equal (5, t.SByteReturn (4));
            Assert.Equal ((sbyte) 5, returnRes);

            Assert.Equal (7, t.ShortReturn (6));
            Assert.Equal ((short) 7, returnRes);

            Assert.Equal (9, t.UShortReturn (8));
            Assert.Equal ((ushort) 9, returnRes);

            Assert.Equal (11, t.IntReturn (10));
            Assert.Equal (11, returnRes);

            Assert.Equal ((uint) 13, t.UIntReturn (12));
            Assert.Equal ((uint) 13, returnRes);

            Assert.Equal (15L, t.LongReturn (14));
            Assert.Equal (15L, returnRes);

            Assert.Equal (17UL, t.ULongReturn (16));
            Assert.Equal (17UL, returnRes);

            Assert.Equal ((float) 19.1, t.FloatReturn ((float) 18.1));
            Assert.Equal ((float) 19.1, returnRes);

            Assert.Equal (21.1, t.DoubleReturn (20.1));
            Assert.Equal (21.1, returnRes);

            Assert.Equal ('b', t.CharReturn ('a'));
            Assert.Equal ('b', returnRes);
        }


        [Fact]
        public void Struct_Returned()
        {
            Assert.Equal (new DateTime (2018, 11, 7), t.StructReturn (new DateTime (2017, 11, 7)));
            Assert.Equal (new DateTime (2018, 11, 7), returnRes);
        }


        [Fact]
        public void ReferenceTypes_Returned()
        {
            Assert.Same (t, t.ReferenceTypeReturn (t));
            Assert.Same (t, returnRes);
        }


        [Fact]
        public void Array_Returned()
        {
            Assert.Equal (new[] {42, 43, 44}, t.ArrayReturn (42, 43, 44));
            Assert.Equal (new[] {42, 43, 44}, returnRes);
        }


        [Fact]
        public void DynamicTypes_Returned()
        {
            t.DynamicTypeReturn (42);
            Assert.Equal (42, ((dynamic) returnRes).A);
        }


        [Fact]
        public void Generic_Returned()
        {
            Assert.Equal (42, t.GenericReturn (42));
            Assert.Equal (42, returnRes);

            Assert.Equal ("a", t.GenericReturn ("a"));
            Assert.Equal ("a", returnRes);
        }


        [Fact]
        public void VoidMethod_DoesntSetResult()
        {
            returnRes = 42;
            t.VoidFunction();
            Assert.Null (returnRes);
        }


        [Fact]
        public void Exception_Not_Set_ReturnValue()
        {
            t.ThrowsException ("");
            Assert.Equal (42, returnRes);

            try
            {
                returnRes = "notChanged";
                t.ThrowsException (null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.Equal ("s", ex.ParamName);
                Assert.Equal ("notChanged", returnRes);
            }
        }


        [Fact]
        public void ChangeReturnValue_Int()
        {
            Assert.Equal (42, t.ChangeIntReturnValue (42));
        }


        [Fact]
        public void ChangeReturnValue_Struct()
        {
            Assert.Equal (new DateTime (2017, 11, 7), t.ChangeStructReturnValue (new DateTime (2017, 11, 7)));
        }


        [Fact]
        public void ChangeReturnValue_Object()
        {
            Assert.Equal ("a", t.ChangeObjectReturnValue ("a"));
        }


        [Fact]
        public void ChangeReturnValue_Generic()
        {
            Assert.Equal (42, t.ChangeGenericReturnValue (42));
            Assert.Equal ("a", t.ChangeGenericReturnValue ("a"));
        }


        [Fact]
        public void ChangeReturnValue_Property()
        {
            t.ChangePropertyReturnValue = "a";
            Assert.Equal ("new_value", t.ChangePropertyReturnValue);
        }
    }
}