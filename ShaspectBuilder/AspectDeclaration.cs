using Mono.Cecil;


namespace Shaspect.Builder
{
    internal class AspectDeclaration
    {
        public CustomAttribute Aspect { get; set; }
        public ICustomAttributeProvider Declarator { get; set; }
    }
}