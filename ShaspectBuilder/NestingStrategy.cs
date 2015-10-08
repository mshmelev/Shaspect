using System.Collections.Generic;
using System.Linq;


namespace Shaspect.Builder
{
    /// <summary>
    /// Determines how aspects from different levels are merged. E.g. how aspects on class level are merged with method aspects.
    /// </summary>
    internal static class NestingStrategy
    {
        public static List<AspectDeclaration> NestWith (this IEnumerable<AspectDeclaration> aspects, IEnumerable<AspectDeclaration> nestedAspects)
        {
            var replacingAspects = new List<AspectDeclaration>();
            var excludingAspects = new List<AspectDeclaration>();
            var appendingAspects = new List<AspectDeclaration>();
            foreach (var aspect in nestedAspects)
            {
                if (aspect.Replace)
                    replacingAspects.Add (aspect);
                if (aspect.Exclude)
                    excludingAspects.Add (aspect);
                if (!aspect.Replace && !aspect.Exclude)
                    appendingAspects.Add (aspect);
            }

            var res = aspects.ToList();

            // merge considering the Replace property
            res.RemoveAll (a => replacingAspects.Any (ra => ra.Aspect.AttributeType.FullName == a.Aspect.AttributeType.FullName));
            res.AddRange (replacingAspects);

            // merge considering the Exclude property
            res.RemoveAll (a => excludingAspects.Any (ea => ea.Aspect.AttributeType.FullName == a.Aspect.AttributeType.FullName));

            // add the rest
            res.AddRange (appendingAspects);

            return res;
        }

    }
}