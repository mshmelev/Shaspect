using System;


namespace Shaspect
{
    [Flags]
    public enum ElementTargets
    {
        Default = 0,

        Method = 1,
        Property = 2,
        InstanceConstructor = 4,
        StaticConstructor = 8,
        AnyMember = Method | Property | InstanceConstructor | StaticConstructor
    }
}