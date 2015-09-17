using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;


namespace Shaspect.Builder.Tools
{
    internal static class TypeTools
    {
        public static bool IsCompilerGenerated (this TypeDefinition type)
        {
            string s = type.Module.Import (typeof (CompilerGeneratedAttribute)).FullName;
            return type.CustomAttributes.Any (a => a.AttributeType.FullName == s);
        }
    }
}