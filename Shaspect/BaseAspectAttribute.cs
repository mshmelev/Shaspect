using System;
using System.Reflection;


namespace Shaspect
{
    /// <summary>
    /// Base abstract class for any aspect.
    /// </summary>
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
        /// Whether to exclude/remove aspect of this type (which could be set higher in the hieararchy) from an element
        /// </summary>
        public bool Exclude { get; set; }


        /// <summary>
        /// Whether to reoplace aspect of this type (which could be set higher in the hieararchy) with this one on an element.
        /// </summary>
        public bool Replace { get; set; }


        /// <summary>
        /// Specifies order in which the aspect is called. The lower the number the earlier the aspect is called.
        /// <para>
        /// Should be used if multiple aspects are applied for a method (or if they are nested from class or assembly level).
        /// The order number is global for the whole assembly and all the aspect types.
        /// The default aspect execution order is from bottom to top (method-level aspects, then class-level, then assembly-level).
        /// Doesn't affect <see cref="Exclude"/> or <see cref="Replace"/> behavior.
        /// </para>
        /// </summary>
        public int Order { get; set; }


        /// <summary>
        /// Specifies type name where the aspect is applicable.
        /// <para>
        /// Can be exact type name, pattern or regex. Regex syntax: "/pattern/i" or "/pattern/"; 'i' flag means ignore case.
        /// <example>
        ///     Examples: "MyClassName", "Namespace.MyClass*", "/MyClass[0-9]+/i"
        /// </example>
        /// </para>
        /// </summary>
        public string TypeTargets { get; set; }


        /// <summary>
        /// Specifies member name for which the aspect is applicable.
        /// <para>
        /// Can be exact type name, pattern or regex. Regex syntax: "/pattern/i" or "/pattern/"; 'i' flag means ignore case.
        /// <example>
        ///     Examples: "MyMethodName", "MyMethod*", "/MyMethod[0-9]+/i"
        /// </example>
        /// </para>
        /// </summary>
        public string MemberTargets { get; set; }
    }
}