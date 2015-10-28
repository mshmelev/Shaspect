using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class MemberTargetTests : IDisposable
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
            [SimpleAspect ("MatchExactName", MemberTargets = "MatchExactName")]
            public void MatchExactName()
            {
            }

            [SimpleAspect ("NoMatchExactName", MemberTargets = "NoMatchExactNam")]
            public void NoMatchExactName()
            {
            }

            [SimpleAspect ("MatchPattern", MemberTargets = "MatchPatt*")]
            public void MatchPattern()
            {
            }

            [SimpleAspect ("NoMatchPattern", MemberTargets = "NoMatchPata*")]
            public void NoMatchPattern()
            {
            }

            [SimpleAspect ("MatchRegex", MemberTargets = "/matchreg[ex]+/i")]
            public void MatchRegex()
            {
            }

            [SimpleAspect ("NoMatchRegex", MemberTargets = "/NoMatchReg[bc]+/")]
            public void NoMatchRegex()
            {
            }

            [SimpleAspect ("MatchProperty", MemberTargets = "MatchProperty")]
            public int MatchProperty { get; set; }

            [SimpleAspect ("MatchPropertyPattern", MemberTargets = "MatchProp*")]
            public int MatchPropertyPattern { get; set; }

            [SimpleAspect ("MatchPropertyGet", MemberTargets = "get_*")]
            public int MatchPropertyGet { get; set; }

            [SimpleAspect ("MatchPropertySet", MemberTargets = "set_*")]
            public int MatchPropertySet { get; set; }
        }

        private TestClass t;


        public MemberTargetTests()
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
        public void Matches_Exact_Member_Name()
        {
            t.MatchExactName();
            Assert.Equal (new[]{"MatchExactName"}, calls);
        }


        [Fact]
        public void Doesnt_Match_Exact_Member_Name()
        {
            t.NoMatchExactName();
            Assert.Empty (calls);
        }


        [Fact]
        public void Matches_Member_Name_Pattern()
        {
            t.MatchPattern();
            Assert.Equal (new[]{"MatchPattern"}, calls);
        }


        [Fact]
        public void Doesnt_Match_Member_Name_Pattern()
        {
            t.NoMatchPattern();
            Assert.Empty (calls);
        }


        [Fact]
        public void Matches_Member_Name_Regex()
        {
            t.MatchRegex();
            Assert.Equal (new[]{"MatchRegex"}, calls);
        }


        [Fact]
        public void Doesnt_Match_Member_Name_Regex()
        {
            t.NoMatchRegex();
            Assert.Empty (calls);
        }


        [Fact]
        public void Matches_Property_Exact_Name()
        {
            t.MatchProperty = 42;
            Assert.Equal (new[]{"MatchProperty"}, calls);
        }


        [Fact]
        public void Matches_Property_Name_Pattern()
        {
            t.MatchPropertyPattern = 42;
            Assert.Equal (new[]{"MatchPropertyPattern"}, calls);
        }


        [Fact]
        public void Matches_Property_Name_Get_Only()
        {
            t.MatchPropertyGet= 42;
            Assert.Empty (calls);

            int i = t.MatchPropertyGet;
            Assert.Equal (new[]{"MatchPropertyGet"}, calls);
        }


        [Fact]
        public void Matches_Property_Name_Set_Only()
        {
            int i = t.MatchPropertySet;
            Assert.Empty (calls);

            t.MatchPropertySet= 42;
            Assert.Equal (new[]{"MatchPropertySet"}, calls);
        }

    }
}