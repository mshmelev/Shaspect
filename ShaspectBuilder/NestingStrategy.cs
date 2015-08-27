using System.Collections.Generic;
using System.Linq;


namespace Shaspect.Builder
{
    /// <summary>
    /// Determines how aspects from different levels are merged. E.g. how aspects on class level are merged with method aspects.
    /// </summary>
    internal static class NestingStrategy
    {
        public static List<AspectDeclaration> NestWith (this IEnumerable<AspectDeclaration> aspects, List<AspectDeclaration> nestedAspects)
        {
            // merge considering the Exclude property
            var res = new List<AspectDeclaration>();
            var excludingAspects = nestedAspects.Where (na => na.Exclude).ToList();

            foreach (var aspect in aspects)
            {
                if (excludingAspects.All (ea => ea.Aspect.AttributeType.FullName != aspect.Aspect.AttributeType.FullName))
                    res.Add (aspect);
            }

            res.AddRange (nestedAspects.Where (na => !na.Exclude));

            return res;
        }

    }
}