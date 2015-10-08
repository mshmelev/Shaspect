using System;


namespace Shaspect
{
    /// <summary>
    /// Information about current method execution context
    /// </summary>
    public class MethodExecInfo
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="args"></param>
        public MethodExecInfo(object[] args)
        {
            Arguments = args;
        }

        /// <summary>
        /// Current method's arguments
        /// </summary>
        public object[] Arguments { get; private set; }


        /// <summary>
        /// Current return value of the method
        /// </summary>
        public object ReturnValue { get; set; }


        /// <summary>
        /// Currently catched exception in <see cref="BaseAspectAttribute.OnException"/>, or a new exception to be thrown with <see cref="Shaspect.ExecFlow.ThrowException"/>
        /// </summary>
        public Exception Exception { get; set; }


        /// <summary>
        /// Specifies execution flow for the current method. The flow can be changed after any of the aspect functions call: 
        /// <see cref="BaseAspectAttribute.OnEntry"/>,
        /// <see cref="BaseAspectAttribute.OnSuccess"/>,
        /// <see cref="BaseAspectAttribute.OnException"/>,
        /// <see cref="BaseAspectAttribute.OnExit"/>.
        /// </summary>
        public ExecFlow ExecFlow { get; set; }


        /// <summary>
        /// Custom data. Can bu used to pass some object between <see cref="BaseAspectAttribute.OnEntry"/>, <see cref="BaseAspectAttribute.OnSuccess"/>, 
        /// <see cref="BaseAspectAttribute.OnException"/>, <see cref="BaseAspectAttribute.OnExit"/>
        /// </summary>
        public object Data { get; set; }
    }
}