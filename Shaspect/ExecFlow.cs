namespace Shaspect
{
    /// <summary>
    /// Specifies execution flow for a method with aspect.
    /// </summary>
    public enum ExecFlow
    {
        /// <summary>
        /// Default behavior, like if aspect is not applied to the method.
        /// </summary>
        Default,

        /// <summary>
        /// Immediate exit from the method returning the result in <see cref="MethodExecInfo.ReturnValue"/>
        /// </summary>
        Return,

        /// <summary>
        /// Immediately throw an exception in <see cref="MethodExecInfo.Exception"/>.
        /// To rethrow an exception catched in <see cref="BaseAspectAttribute.OnException"/>, <see cref="Default"/> execution flow should be applied.
        /// </summary>
        ThrowException
    }
}