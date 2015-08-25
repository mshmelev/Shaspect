using System;


namespace Shaspect
{
    [AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor |
                     AttributeTargets.Method | AttributeTargets.Property)]
    // TODO: make abstract
    public class BaseAspectAttribute : Attribute
    {
        public virtual void OnEntry()
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
    }
}