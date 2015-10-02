using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class ExecFlowTests: IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<string> flow= new List<string>();
        private static bool ReturnAfterOnEntry { get; set; }
        private static bool ExceptionAfterOnEntry { get; set; }
        private static bool ReturnAfterOnSuccess { get; set; }
        private static bool ExceptionAfterOnSuccess { get; set; }
        private static bool ReturnAfterOnException { get; set; }
        private static bool ExceptionAfterOnException { get; set; }
        private readonly TestClass t;

        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            public override void OnEntry (MethodExecInfo methodExecInfo)
            {
                flow.Add ("OnEntry");

                if (ReturnAfterOnEntry)
                {
                    methodExecInfo.ExecFlow = ExecFlow.Return;
                    methodExecInfo.ReturnValue = "overriden_from_OnEntry";
                }
                else if (ExceptionAfterOnEntry)
                {
                    methodExecInfo.Exception = new DivideByZeroException("exception_from_OnEntry");
                    methodExecInfo.ExecFlow = ExecFlow.ThrowException;
                }
            }


            public override void OnSuccess (MethodExecInfo methodExecInfo)
            {
                flow.Add ("OnSuccess");

                if (ReturnAfterOnSuccess)
                {
                    methodExecInfo.ExecFlow = ExecFlow.Return;
                    methodExecInfo.ReturnValue = "overriden_from_OnSuccess";
                }
                else if (ExceptionAfterOnSuccess)
                {
                    methodExecInfo.Exception = new DivideByZeroException("exception_from_OnSuccess");
                    methodExecInfo.ExecFlow = ExecFlow.ThrowException;
                }
            }


            public override void OnException (MethodExecInfo methodExecInfo)
            {
                flow.Add ("OnException");

                if (ReturnAfterOnException)
                {
                    methodExecInfo.ExecFlow = ExecFlow.Return;
                    methodExecInfo.ReturnValue = "overriden_from_OnException";
                }
                else if (ExceptionAfterOnException)
                {
                    methodExecInfo.Exception = new DivideByZeroException("exception_from_OnException");
                    methodExecInfo.ExecFlow = ExecFlow.ThrowException;
                }
            }


            public override void OnExit (MethodExecInfo methodExecInfo)
            {
                flow.Add ("OnExit");
            }
        }

        [SimpleAspect]
        internal class TestClass
        {
            public string SimpleMethod (string s)
            {
                flow.Add ("Method");
                return s + "_42";
            }

            public void VoidMethod (string s)
            {
                flow.Add ("Method");
            }

            public string ExceptionMethod (string s)
            {
                flow.Add ("Method");
                throw new ArgumentException (s);
            }
        }


        public ExecFlowTests()
        {
            Monitor.Enter (sync);
            ReturnAfterOnEntry = false;
            ExceptionAfterOnEntry = false;
            ReturnAfterOnSuccess = false;
            ExceptionAfterOnSuccess = false;
            ReturnAfterOnException = false;
            ExceptionAfterOnException = false;
            t = new TestClass();
            flow.Clear();
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void Regular_Flow_Is_Entry_Method_Success_Exit()
        {
            Assert.Equal("a_42", t.SimpleMethod ("a"));
            Assert.Equal (new[] {"OnEntry", "Method", "OnSuccess", "OnExit"}, flow);
        }


        [Fact]
        public void Exception_Flow_Is_Entry_Method_Exception_Exit()
        {
            Assert.Throws<ArgumentException> (() => t.ExceptionMethod ("a"));
            Assert.Equal (new[] {"OnEntry", "Method", "OnException", "OnExit"}, flow);
        }


        [Fact]
        public void Return_After_OnEntry_Has_Flow_Entry()
        {
            ReturnAfterOnEntry = true;
            t.SimpleMethod ("a");
            Assert.Equal (new[] {"OnEntry"}, flow);
        }


        [Fact]
        public void Return_After_OnEntry_Changes_Return_Value()
        {
            ReturnAfterOnEntry = true;
            Assert.Equal ("overriden_from_OnEntry", t.SimpleMethod ("a"));
        }


        [Fact]
        public void Return_After_OnEntry_For_VoidMethod_Doesnt_Crash()
        {
            ReturnAfterOnEntry = true;
            t.VoidMethod ("a");
            Assert.Equal (new[] {"OnEntry"}, flow);
        }


        [Fact]
        public void Exception_After_OnEntry()
        {
            ExceptionAfterOnEntry= true;
            var ex= Assert.Throws<DivideByZeroException> (() => t.SimpleMethod ("a"));
            Assert.Equal ("exception_from_OnEntry", ex.Message);
            Assert.Equal (new[] {"OnEntry"}, flow);
        }


        [Fact]
        public void Return_After_OnSuccess_Has_Flow_Entry_Method_Success_Exit()
        {
            ReturnAfterOnSuccess = true;
            t.SimpleMethod ("a");
            Assert.Equal (new[] {"OnEntry", "Method", "OnSuccess", "OnExit"}, flow);
        }


        [Fact]
        public void Return_After_OnSuccess_Changes_Return_Value()
        {
            ReturnAfterOnSuccess = true;
            Assert.Equal ("overriden_from_OnSuccess", t.SimpleMethod ("a"));
        }


        [Fact]
        public void Return_After_OnSuccess_For_VoidMethod_Doesnt_Crash()
        {
            ReturnAfterOnSuccess = true;
            t.VoidMethod ("a");
            Assert.Equal (new[] {"OnEntry", "Method", "OnSuccess", "OnExit"}, flow);
        }


        [Fact]
        public void Exception_After_OnSuccess()
        {
            ExceptionAfterOnSuccess= true;
            var ex= Assert.Throws<DivideByZeroException> (() => t.SimpleMethod ("a"));
            Assert.Equal ("exception_from_OnSuccess", ex.Message);
            Assert.Equal (new[] {"OnEntry", "Method", "OnSuccess", "OnExit"}, flow);
        }


        [Fact]
        public void Return_After_OnException_Has_Flow_Entry_Method_Exception_Exit()
        {
            ReturnAfterOnException = true;
            t.ExceptionMethod ("a");
            Assert.Equal (new[] {"OnEntry", "Method", "OnException", "OnExit"}, flow);
        }


        [Fact]
        public void Return_After_OnException_Changes_Return_Value()
        {
            ReturnAfterOnException = true;
            Assert.Equal ("overriden_from_OnException", t.ExceptionMethod ("a"));
        }


        [Fact]
        public void Exception_After_OnException()
        {
            ExceptionAfterOnException= true;
            var ex= Assert.Throws<DivideByZeroException> (() => t.ExceptionMethod ("a"));
            Assert.Equal ("exception_from_OnException", ex.Message);
            Assert.Equal (new[] {"OnEntry", "Method", "OnException", "OnExit"}, flow);
        }


    }
}