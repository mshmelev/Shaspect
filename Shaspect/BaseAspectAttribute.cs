using System;
using System.Reflection;


namespace Shaspect
{
    [AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor |
                     AttributeTargets.Method | AttributeTargets.Property)]
    public abstract class BaseAspectAttribute : Attribute
    {
        /// <summary>
        /// Called once at application startup for every method the aspect is applied for.
        /// </summary>
        /// <param name="method"></param>
        public virtual void Initialize (MethodBase method)
        {
        }


        /// <summary>
        /// Called on target method is starting execution
        /// </summary>
        /// <param name="methodExecInfo"></param>
        public virtual void OnEntry (MethodExecInfo methodExecInfo)
        {
        }


        /// <summary>
        /// Called on target method successfully finished execution.
        /// </summary>
        /// <param name="methodExecInfo"></param>
        public virtual void OnSuccess (MethodExecInfo methodExecInfo)
        {
        }


        /// <summary>
        /// Called on unhandled exception occured in target method.
        /// </summary>
        /// <param name="methodExecInfo"></param>
        public virtual void OnException (MethodExecInfo methodExecInfo)
        {
        }


        /// <summary>
        /// Called on target method finished execution either successfully or with exception.
        /// Called after <see cref="OnSuccess"/> and <see cref="OnException"/>
        /// </summary>
        /// <param name="methodExecInfo"></param>
        public virtual void OnExit (MethodExecInfo methodExecInfo)
        {
        }


        /// <summary>
        /// Specifies element types on which the aspect should be applied.
        /// </summary>
        public ElementTargets ElementTargets { get; set; }


        /// <summary>
        /// Whether to exclude this aspect from an element.
        /// </summary>
        public bool Exclude { get; set; }
    }
}