using Mono.Cecil;


namespace Shaspect.Builder.Tools
{
    internal class AssemblyResolver : DefaultAssemblyResolver
    {
        public AssemblyResolver (string references)
        {
            foreach (var reference in references.Split (';'))
            {
                var assembly = ModuleDefinition.ReadModule (reference, new ReaderParameters {AssemblyResolver = this}).Assembly;
                RegisterAssembly (assembly);
            }
        }
    }
}