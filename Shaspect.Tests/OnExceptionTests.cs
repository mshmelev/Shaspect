using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class OnExceptionTests : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<object> args= new List<object>();
        private static Exception ex;
        private static Exception ex2;
        private readonly TestClass t;

        
        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            public override void OnException (MethodExecInfo methodExecInfo)
            {
                ex = methodExecInfo.Exception;
                args.AddRange (methodExecInfo.Arguments);
            }
        }

        public class SimpleAspect2Attribute : BaseAspectAttribute
        {
            public override void OnException (MethodExecInfo methodExecInfo)
            {
                ex2 = methodExecInfo.Exception;
            }
        }


        [SimpleAspect]
        internal class TestClass
        {
            public void SimpleException (string s)
            {
                throw new ArgumentException (s);
            }

            
            public void NoException ()
            {
            }


            [SimpleAspect2]
            public void MultipleAspects (string s)
            {
                throw new ArgumentException (s);
            }
        }


        public OnExceptionTests()
        {
            t = new TestClass();
            Monitor.Enter (sync);
            ex = ex2= null;
            args.Clear();
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void OnException_Called()
        {
            var thrown= Assert.Throws<ArgumentException> (() => t.SimpleException ("exx"));
            Assert.Equal ("exx", thrown.Message);
            Assert.Same (thrown, ex);
        }


        [Fact]
        public void OnException_NotCalled()
        {
            t.NoException();
            Assert.Null (ex);
        }


        [Fact]
        public void OnException_Method_Args_Passed()
        {
            Assert.Throws<ArgumentException> (() => t.SimpleException ("arg1"));
            Assert.Equal (new[] {"arg1"}, args);
        }


        [Fact]
        public void OnException_Called_For_Multiple_Aspects()
        {
            var thrown= Assert.Throws<ArgumentException> (() => t.MultipleAspects ("max"));
            Assert.Equal ("max", thrown.Message);

            Assert.Same (thrown, ex);
            Assert.Same (thrown, ex2);
        }


    }
}