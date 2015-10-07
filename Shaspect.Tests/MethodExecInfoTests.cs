using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class MethodExecInfoTests : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<string> data= new List<string>();
        private readonly TestClass t;


        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            public override void OnEntry (MethodExecInfo methodExecInfo)
            {
                data.Add (methodExecInfo.Data+"");
                methodExecInfo.Data = "OnEntry";
                data.Add (methodExecInfo.Data+"");
            }


            public override void OnExit (MethodExecInfo methodExecInfo)
            {
                data.Add (methodExecInfo.Data+"");
                methodExecInfo.Data = "OnExit";
                data.Add (methodExecInfo.Data+"");
            }
        }


        [SimpleAspect]
        class TestClass
        {
            public void Method1()
            {
                Method2();
            }

            public void Method2()
            {
            }
        }


        public MethodExecInfoTests()
        {
            Monitor.Enter (sync);
            t = new TestClass();
            data.Clear();
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void Data_Property_Passed_Between_Calls()
        {
            t.Method2();

            Assert.Equal (new[] {"", "OnEntry", "OnEntry", "OnExit"}, data);
        }


        [Fact]
        public void Data_Property_Own_Instance_For_Each_Method()
        {
            t.Method1();

            Assert.Equal (new[] {"", "OnEntry", "", "OnEntry", "OnEntry", "OnExit", "OnEntry", "OnExit"}, data);
        }



    }
}