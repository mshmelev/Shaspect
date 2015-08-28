namespace Shaspect
{
    public class MethodExecInfo
    {
        public MethodExecInfo(object[] args)
        {
            Arguments = args;
        }


        public object[] Arguments { get; private set; }
    }
}