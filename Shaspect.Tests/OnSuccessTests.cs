using System;
using System.Threading;
using Xunit;


namespace Shaspect.Tests
{
    public class OnSuccessTests
    {
        private static readonly object sync = new object();
        private static object returnRes;
        private static object returnRes2;
        private readonly TestClass t;

        public class SimpleAspectAttribute : BaseAspectAttribute
        {
            public override void OnSuccess (MethodExecInfo methodExecInfo)
            {
                returnRes = methodExecInfo.ReturnValue;
            }
        }

        public class SimpleAspect2Attribute : BaseAspectAttribute
        {
            public override void OnSuccess (MethodExecInfo methodExecInfo)
            {
                returnRes2 = methodExecInfo.ReturnValue;
            }
        }

        
        [SimpleAspectAttribute]
        internal class TestClass
        {
            private string prop;


            public string SimpleReturn (int i)
            {
                return i.ToString();
            }


            public void VoidEmptyFunction()
            {
            }

            public void VoidFunction()
            {
                var d = DateTime.Now;
                d.AddDays (2);
            }


            public string MultipleReturns (int i)
            {
                if (i == 1)
                    return "a_"+i;

                i += 10;        // pretend some work is done
                i -= 5;
                i -= 5;
                
                if (i == 2)
                    return "b_"+i;
                return "c_" + i;
            }


            public string MultipleReturnsTryCatch (int i)
            {
                if (i == 1)
                    return "a_"+i;

                try
                {
                    if (i == 2)
                        return "b_" + i;
                    try
                    {
                        if (i == 3)
                            return "c_" + i;
                        if (i== 4)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        return "d_" + i;
                    }
                 
                    if (i== 5)
                        return "e_" + i;

                    try
                    {
                        if (i == 6 || i == 7)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        return "f_" + i;
                    }
                    finally
                    {
                        if (i== 7)
                            throw new Exception();
                    }

                    if (i== 8)
                        return "h_" + i;
                }
                catch (Exception)
                {
                    return "g_" + i;
                }
                finally
                {
                    i = 42;
                }

                return "i_"+i;
            }


            public string AutoProp { get; set; }


            public string Prop
            {
                get { return prop; }
                set { prop = value; }
            }


            public int ThrowsException (string s)
            {
                if (s== null)
                    throw new ArgumentNullException("s");
                return 42;
            }


            [SimpleAspect2]
            public int MultipleAspects (int i)
            {
                return i + 1;
            }
        }

        public OnSuccessTests()
        {
            t = new TestClass();
            Monitor.Enter (sync);
            returnRes = null;
            returnRes2 = null;
        }


        public void Dispose()
        {
            Monitor.Exit (sync);
        }




        [Fact]
        public void Basic_OnSuccess()
        {
            t.SimpleReturn (42);
            Assert.Equal ("42", returnRes);
        }


        [Fact]
        public void VoidMethod_OnSuccess_Called()
        {
            returnRes = 42;
            t.VoidEmptyFunction();
            Assert.Null (returnRes);

            returnRes = 42;
            t.VoidFunction();
            Assert.Null (returnRes);
        }


        [Fact]
        public void AutoProperty_OnSuccess()
        {
            returnRes = 42;
            t.AutoProp = "mmm";
            Assert.Null (returnRes);

            string s = t.AutoProp;
            Assert.Equal ("mmm", s);
            Assert.Equal ("mmm", returnRes);
        }



        [Fact]
        public void StandardProperty_OnSuccess()
        {
            returnRes = 42;
            t.Prop = "mmm";
            Assert.Null (returnRes);

            string s = t.Prop;
            Assert.Equal ("mmm", s);
            Assert.Equal ("mmm", returnRes);
        }


        [Fact]
        public void Exception_OnSuccess_NotCalled()
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
        public void MultipleReturns_AllProcessed()
        {
            t.MultipleReturns (1);
            Assert.Equal ("a_1", returnRes);

            t.MultipleReturns (2);
            Assert.Equal ("b_2", returnRes);

            t.MultipleReturns (3);
            Assert.Equal ("c_3", returnRes);
        }


        [Fact]
        public void MultipleReturns_In_TryCatch()
        {
            t.MultipleReturnsTryCatch (1);
            Assert.Equal ("a_1", returnRes);

            t.MultipleReturnsTryCatch (2);
            Assert.Equal ("b_2", returnRes);

            t.MultipleReturnsTryCatch (3);
            Assert.Equal ("c_3", returnRes);

            t.MultipleReturnsTryCatch (4);
            Assert.Equal ("d_4", returnRes);

            t.MultipleReturnsTryCatch (5);
            Assert.Equal ("e_5", returnRes);

            t.MultipleReturnsTryCatch (6);
            Assert.Equal ("f_6", returnRes);

            t.MultipleReturnsTryCatch (7);
            Assert.Equal ("g_7", returnRes);

            t.MultipleReturnsTryCatch (8);
            Assert.Equal ("h_8", returnRes);

            t.MultipleReturnsTryCatch (9);
            Assert.Equal ("i_42", returnRes);
        }


        [Fact]
        public void MultipleAspects_AllWorks()
        {
            Assert.Equal (43, t.MultipleAspects (42));
            Assert.Equal (43, returnRes);
            Assert.Equal (43, returnRes2);
        }




    }
}