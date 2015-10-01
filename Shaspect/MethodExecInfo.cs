using System;


namespace Shaspect
{
    public class MethodExecInfo
    {
        public MethodExecInfo(object[] args)
        {
            Arguments = args;
        }


        public object[] Arguments { get; private set; }

        public object ReturnValue { get; set; }

        public Exception Exception { get; set; }
    }
}