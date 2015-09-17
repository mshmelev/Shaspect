using System;
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


        [SimpleAspect]
        internal class TestClass
        {
            public string SimpleReturn (int i)
            {
                return i.ToString();
            }


            public int ValueTypeReturn (int a, int b)
            {
                return a + b;
            }


            public DateTime StructReturn (DateTime d)
            {
                return d.AddYears (1);
            }


            public TestClass ReferenceTypeReturn (TestClass t)
            {
                return t;
            }


            public dynamic DynamicTypeReturn (int i)
            {
                return new {A = i};
            }


            public void VoidFunction()
            {
                var d = DateTime.Now;
                d.AddDays (2);
            }


            public int ThrowsException (string s)
            {
                if (s== null)
                    throw new ArgumentNullException("s");
                return 42;
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
            t.SimpleReturn (42);
            Assert.Equal ("42", returnRes);
        }


        [Fact]
        public void ValueType_Returned()
        {
            t.ValueTypeReturn (42, 7);
            Assert.Equal (42 + 7, returnRes);
        }


        [Fact]
        public void Struct_Returned()
        {
            t.StructReturn (new DateTime (2017, 11, 7));
            Assert.Equal (new DateTime (2018, 11, 7), returnRes);
        }


        [Fact]
        public void ReferenceTypes_Returned()
        {
            t.ReferenceTypeReturn (t);
            Assert.Same (t, returnRes);
        }


        [Fact]
        public void DynamicTypes_Returned()
        {
            t.DynamicTypeReturn (42);
            Assert.Equal (42, ((dynamic)returnRes).A);
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

    }
}