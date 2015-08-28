using System.Collections.Concurrent;
using System.Linq;
using Xunit;

// needs to be applied on whole assembly and every nested class to check it works correctly even on compiler generated classes
[assembly: Shaspect.AssemblyTests.AssemlyAspectTests.SimpleAspect ("AssemblyAspect", ElementTargets = Shaspect.ElementTargets.Method)]

namespace Shaspect.AssemblyTests
{

    public class AssemlyAspectTests
    {
        private static readonly ConcurrentBag<string> callsBag= new ConcurrentBag<string>();

        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            private readonly string callName;


            public SimpleAspectAttribute(string callName)
            {
                this.callName = callName;
            }


            public override void OnEntry(MethodExecInfo methodExecInfo)
            {
                callsBag.Add (callName);
            }
        }

        private class TestClass
        {
            public void SimpleMethod() { }
        }


        [Fact]
        public void OnEntryIsCalled()
        {
            var t = new TestClass();
            t.SimpleMethod();
            Assert.True (callsBag.Contains ("AssemblyAspect"));
        }

    }
}