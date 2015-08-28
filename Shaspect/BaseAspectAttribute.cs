using System;


namespace Shaspect
{
    [AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor |
                     AttributeTargets.Method | AttributeTargets.Property)]
    public abstract class BaseAspectAttribute : Attribute
    {
        public virtual void OnEntry(MethodExecInfo methodExecInfo)
        {
        }


        public virtual void OnSuccess()
        {
        }


        public virtual void OnException()
        {
        }


        public virtual void OnExit()
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