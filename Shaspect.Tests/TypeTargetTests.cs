using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class TypeTargetTests : IDisposable
    {
        private static readonly object sync = new object();
        private static readonly List<string> calls= new List<string>();

        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            protected readonly string name;


            public SimpleAspectAttribute(string name)
            {
                this.name = name;
            }


            public override void OnEntry (MethodExecInfo methodExecInfo)
            {
                calls.Add (name);
            }
        }

        public class TestClass
        {
            [SimpleAspect ("MatchExactName", TypeTargets = "TestClass")]
            public void MatchExactName()
            {
            }

            [SimpleAspect ("NoMatchExactName", TypeTargets = "TestClas")]
            public void NoMatchExactName()
            {
            }

            [SimpleAspect ("MatchPattern", TypeTargets = "TestCla*")]
            public void MatchPattern()
            {
            }

            [SimpleAspect ("MatchPatternNamespace", TypeTargets = "Shaspect.Tests.TypeTargetTests/TestCla*")]
            public void MatchPatternNamespace()
            {
            }

            [SimpleAspect ("NoMatchPattern", TypeTargets = "TestClb*")]
            public void NoMatchPattern()
            {
            }

            [SimpleAspect ("MatchRegex", TypeTargets = "/testcl[as]+/i")]
            public void MatchRegex()
            {
            }

            [SimpleAspect ("MatchRegexNamespace", TypeTargets = @"/Shaspect\.Tests.*TestCl[as]+/")]
            public void MatchRegexNamespace()
            {
            }

            [SimpleAspect ("NoMatchRegex", TypeTargets = "/TestCl[bc]+/")]
            public void NoMatchRegex()
            {
            }
        }


        private TestClass t;


        public TypeTargetTests()
        {
            Monitor.Enter (sync);
            t = new TestClass();
            calls.Clear();
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }


        [Fact]
        public void Matches_Exact_Class_Name()
        {
            t.MatchExactName();
            Assert.Equal (new[]{"MatchExactName"}, calls);
        }


        [Fact]
        public void Doesnt_Match_Exact_Class_Name()
        {
            t.NoMatchExactName();
            Assert.Empty (calls);
        }


        [Fact]
        public void Matches_Class_Name_Pattern()
        {
            t.MatchPattern();
            Assert.Equal (new[]{"MatchPattern"}, calls);
        }


        [Fact]
        public void Matches_Class_Name_Pattern_With_Nampesace()
        {
            t.MatchPatternNamespace();
            Assert.Equal (new[]{"MatchPatternNamespace"}, calls);
        }


        [Fact]
        public void Doesnt_Match_Class_Name_Pattern()
        {
            t.NoMatchPattern();
            Assert.Empty (calls);
        }


        [Fact]
        public void Matches_Class_Name_Regex()
        {
            t.MatchRegex();
            Assert.Equal (new[]{"MatchRegex"}, calls);
        }

        [Fact]
        public void Matches_Class_Name_Regex_With_Namespace()
        {
            t.MatchRegexNamespace();
            Assert.Equal (new[]{"MatchRegexNamespace"}, calls);
        }


        [Fact]
        public void Doesnt_Match_Class_Name_Regex()
        {
            t.NoMatchRegex();
            Assert.Empty (calls);
        }
    }
}