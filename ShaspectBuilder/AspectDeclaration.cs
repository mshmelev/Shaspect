using Mono.Cecil;


namespace Shaspect.Builder
{
    internal class AspectDeclaration
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
                        nestingLevel = 1000;
                    else if (Declarator is ModuleDefinition)
                        nestingLevel = 2000;
                    else if (Declarator is TypeDefinition)
                    {
                        nestingLevel = 3000;
                        for (var t = ((TypeDefinition) Declarator).DeclaringType; t!= null; t = t.DeclaringType)
                            ++nestingLevel;
                    }
                    else if (Declarator is PropertyDefinition)
                        nestingLevel = 4000;
                    else if (Declarator is MethodDefinition)
                        nestingLevel = 5000;
                    else
                        nestingLevel = 10000;
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


        public string Name
        {
            get { return Aspect.AttributeType.FullName; }
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
    }
}