using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Shaspect.Builder.Tools;


namespace Shaspect.Builder
{
    /// <summary>
    /// Determines how aspects from different levels are merged. E.g. how aspects on class level are merged with method aspects.
    /// </summary>
    internal static class NestingStrategy
    {
        public static IEnumerable<AspectDeclaration> GetApplicableAspects (IEnumerable<AspectDeclaration> aspects, MethodDefinition method)
        {
            aspects = aspects.OrderBy (a => a.NestingLevel);
            var res = new List<AspectDeclaration>();

            foreach (var aspect in aspects)
            {
                if (!IsApplicableElementTarget (aspect, method))
                    continue;

                if (aspect.Exclude || aspect.Replace)
                    res.RemoveAll (a => a.Name == aspect.Name);

                if (!aspect.Exclude)
                    res.Add (aspect);
            }

            return res;
        }



        private static bool IsApplicableElementTarget (AspectDeclaration aspect, MethodDefinition method)
        {
            var elemetTargets = aspect.ElementTargets;
            if (elemetTargets == ElementTargets.Default)
                return true;

            bool isCCtor = method.IsStatic && method.IsConstructor;
            bool isCtor = !method.IsStatic && method.IsConstructor;
            bool isProperty = method.IsSpecialName && method.IsCompilerGenerated() && (method.Name.StartsWith ("get_") || method.Name.StartsWith ("set_"));

            if ((elemetTargets & ElementTargets.StaticConstructor) == ElementTargets.StaticConstructor && isCCtor)
                return true;

            if ((elemetTargets & ElementTargets.InstanceConstructor) == ElementTargets.InstanceConstructor && isCtor)
                return true;

            if ((elemetTargets & ElementTargets.Property) == ElementTargets.Property && isProperty)
                return true;

            if ((elemetTargets & ElementTargets.Method) == ElementTargets.Method && !(isCtor || isCCtor || isProperty))
                return true;

            return false;
        }

    }
}