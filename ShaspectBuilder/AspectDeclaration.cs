using System;
using Mono.Cecil;


namespace Shaspect.Builder
{
    internal class AspectDeclaration : IComparable<AspectDeclaration>
    {
        private bool? exclude;
        private bool? replace;
        private int? order;
        private int? nestingLevel;
        private ElementTargets? elementTargets;


        public CustomAttribute Aspect { get; set; }


        public ICustomAttributeProvider Declarator { get; set; }


        public bool Exclude
        {
            get
            {
                if (exclude == null)
                    exclude = GetPropertyValue ("Exclude", false);

                return exclude.Value;
            }
        }


        public bool Replace
        {
            get
            {
                if (replace == null)
                    replace = GetPropertyValue ("Replace", false);

                return replace.Value;
            }
        }


        public int Order
        {
            get
            {
                if (order == null)
                    order= GetPropertyValue ("Order", 0);

                return order.Value;
            }
        }


        public int NestingLevel
        {
            get
            {
                if (nestingLevel == null)
                {
                    if (Declarator is AssemblyDefinition)
                        nestingLevel= 1;
                    else if (Declarator is ModuleDefinition)
                        nestingLevel=2;
                    else if (Declarator is TypeDefinition)
                        nestingLevel=3;
                    else if (Declarator is PropertyDefinition)
                        nestingLevel=4;
                    else if (Declarator is MethodDefinition)
                        nestingLevel=5;
                    else
                        nestingLevel = 100;
                }

                return nestingLevel.Value;
            }
        }


        public ElementTargets ElementTargets
        {
            get
            {
                if (elementTargets == null)
                    elementTargets = GetPropertyValue ("ElementTargets", ElementTargets.Default);

                return elementTargets.Value;
            }
        }


        private T GetPropertyValue<T> (string propertyName, T defaultValue = default(T))
        {
            foreach (var prop in Aspect.Properties)
            {
                if (prop.Name == propertyName)
                    return (T) prop.Argument.Value;
            }

            return defaultValue;
        }


        public int CompareTo (AspectDeclaration other)
        {
            if (other == null)
                throw new ArgumentNullException ("other");

            // order by Order property first
            int res = Order - other.Order;
            if (res != 0)
                return res;

            // then order by the place of declaration
            return (other.NestingLevel - NestingLevel);
        }
    }
}