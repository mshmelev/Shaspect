using System;


namespace Shaspect
{
    /// <summary>
    /// Specifies element types on which the aspect can be applied
    /// </summary>
    [Flags]
    public enum ElementTargets
    {
        /// <summary>
        /// No filtering by default, aspect can be applied to any member
        /// </summary>
        Default = 0,

        /// <summary>
        /// Regular method
        /// </summary>
        Method = 1,

        /// <summary>
        /// Property
        /// </summary>
        Property = 2,

        /// <summary>
        /// Instance constructor
        /// </summary>
        InstanceConstructor = 4,

        /// <summary>
        /// Static constructor
        /// </summary>
        StaticConstructor = 8,

        /// <summary>
        /// Any member
        /// </summary>
        AnyMember = Method | Property | InstanceConstructor | StaticConstructor
    }
}