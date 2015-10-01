using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class InitializeTests : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<MethodBase> methodInfos = new List<MethodBase>();
        private static readonly List<MethodBase> methodInfos2 = new List<MethodBase>();
        private TestClass t;

        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            public override void Initialize (MethodBase method)
            {
                methodInfos.Add (method);
            }
        }

        public class SimpleAspect2Attribute : BaseAspectAttribute
        {
            public override void Initialize (MethodBase method)
            {
                methodInfos2.Add (method);
            }
        }



        internal class TestClass
        {
            [SimpleAspectAttribute]
            public void SimpleMethod()
            {
            }


            public void OverloadedMethod()
            {
            }


            [SimpleAspectAttribute]
            public void OverloadedMethod(int i)
            {
            }


            [SimpleAspectAttribute]
            public int Prop { get; set; }
        }


        [SimpleAspect2Attribute]
        internal class Test2Class
        {
            public void SimpleMethod()
            {
            }

            public void SimpleMethod2()
            {
            }
        }




        public InitializeTests()
        {
            t = new TestClass();
            Monitor.Enter (sync);
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void Initialize_IsCalled()
        {
            t.SimpleMethod();
            Assert.Contains (typeof (TestClass).GetMethod ("SimpleMethod"), methodInfos);
        }


        [Fact]
        public void Initialize_Correct_MethodInfo_Passed()
        {
            t.OverloadedMethod();
            t.OverloadedMethod (5);

            Assert.DoesNotContain (typeof (TestClass).GetMethod ("OverloadedMethod", new Type[0]), methodInfos);
            Assert.Contains (typeof (TestClass).GetMethod ("OverloadedMethod", new[] {typeof(int)}), methodInfos);
        }


        [Fact]
        public void Initialize_Called_Only_Once()
        {
            t = new TestClass();
            t.SimpleMethod();
            t.OverloadedMethod();
            t.OverloadedMethod (5);

            Assert.Equal (4, methodInfos.Count);        // 2 regular methods + 2 get/set methods for property
        }


        [Fact]
        public void Initialize_Called_For_GetSet_Property_Methods()
        {
            ++t.Prop;

            Assert.Contains (typeof (TestClass).GetProperty ("Prop").GetMethod, methodInfos);
            Assert.Contains (typeof (TestClass).GetProperty ("Prop").SetMethod, methodInfos);
        }



        [Fact]
        public void Initialize_Called_For_Every_Nested_Method()
        {
            var t2= new Test2Class();
            t2.SimpleMethod();
            t2.SimpleMethod2();

            Assert.Contains (typeof (Test2Class).GetMethod ("SimpleMethod"), methodInfos2);
            Assert.Contains (typeof (Test2Class).GetMethod ("SimpleMethod2"), methodInfos2);
        }

    }
}