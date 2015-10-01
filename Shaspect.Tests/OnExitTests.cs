using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class OnExitTests : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<MethodExecInfo> execInfo= new List<MethodExecInfo>();
        private static readonly List<MethodExecInfo> execInfo2= new List<MethodExecInfo>();
        private readonly TestClass t;

        
        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            public override void OnExit (MethodExecInfo methodExecInfo)
            {
                execInfo.Add (methodExecInfo);
            }
        }

        public class SimpleAspect2Attribute : BaseAspectAttribute
        {
            public override void OnExit (MethodExecInfo methodExecInfo)
            {
                execInfo2.Add (methodExecInfo);
            }
        }


        [SimpleAspect]
        internal class TestClass
        {
            public string SimpleMethod (string s)
            {
                return s + "_42";
            }

            
            public void ThrowsException (string s)
            {
                throw new ArgumentException (s);
            }


            [SimpleAspect2]
            public void MultipleAspects (string s)
            {
            }
        }


        public OnExitTests()
        {
            t = new TestClass();
            Monitor.Enter (sync);
            execInfo.Clear();
            execInfo2.Clear();
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void OnExit_Called()
        {
            Assert.Equal (0, execInfo.Count);
            t.SimpleMethod ("a");
            Assert.Equal (1, execInfo.Count);
        }


        [Fact]
        public void OnExit_Called_After_Exception()
        {
            var ex = Assert.Throws<ArgumentException> (() => t.ThrowsException("arg1"));
            Assert.Equal (1, execInfo.Count);
            Assert.Same (ex, execInfo.Single().Exception);
        }


        [Fact]
        public void OnExit_Method_Args_Passed()
        {
            t.SimpleMethod ("abc");
            Assert.Equal (new object[] {"abc"}, execInfo.Single().Arguments);
        }


        [Fact]
        public void OnExit_ReturnValue_Passed()
        {
            string res= t.SimpleMethod ("abc");
            Assert.Equal ("abc_42", res);
            Assert.Equal ("abc_42", execInfo.Single().ReturnValue);
        }


        [Fact]
        public void OnExit_Called_For_Multiple_Aspects()
        {
            t.MultipleAspects ("a");

            Assert.Equal (1, execInfo.Count);
            Assert.Equal (1, execInfo2.Count);
        }


    }
}