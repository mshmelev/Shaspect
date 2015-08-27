using Mono.Cecil;


namespace Shaspect.Builder
{
    internal class AspectDeclaration
    {
        private bool? exclude;
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
    }
}