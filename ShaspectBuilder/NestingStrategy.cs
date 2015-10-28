using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                if (!IsApplicableElementTarget (aspect, method) || !IsApplicableTypeTarget (aspect, method) || !IsApplicableMemberTarget (aspect, method))
                    continue;

                if (aspect.Exclude || aspect.Replace)
                    res.RemoveAll (a => a.Name == aspect.Name);

                if (!aspect.Exclude)
                    res.Add (aspect);
            }

            return res;
        }


        private static bool IsApplicableTypeTarget (AspectDeclaration aspect, MethodDefinition method)
        {
            if (String.IsNullOrEmpty (aspect.TypeTargets))
                return true;

            var re = BuildRegexFromSearchPattern (aspect.TypeTargets);

            bool searchInFullName = (re.ToString().Contains (@"\.") || re.ToString().Contains (@"/"));
            string typeName = searchInFullName ? method.DeclaringType.FullName : method.DeclaringType.Name;

            return re.IsMatch (typeName);
        }


        private static bool IsApplicableMemberTarget (AspectDeclaration aspect, MethodDefinition method)
        {
            if (String.IsNullOrEmpty (aspect.MemberTargets))
                return true;

            var re = BuildRegexFromSearchPattern (aspect.MemberTargets);
            if (method.IsPropertyMethod())
                return re.IsMatch (method.Name) || re.IsMatch (TypeTools.GetPropertyNameByMethod (method));

            return re.IsMatch (method.Name);
        }


        private static Regex BuildRegexFromSearchPattern (string pattern)
        {
            var options = RegexOptions.None;

            if (pattern.StartsWith ("/"))
            {
                int p = pattern.LastIndexOf ('/');
                if (p == 0)
                    throw new ApplicationException ("Invalid RegEx notation: " + pattern);

                if (pattern.IndexOf ('i', p + 1) != -1)
                    options |= RegexOptions.IgnoreCase;

                pattern = pattern.Substring (1, p - 1);
            }
            else
            {
                pattern = '^' + Regex.Escape (pattern).Replace (@"\*", ".*") + '$';
            }

            return new Regex (pattern, options);
        }


        private static bool IsApplicableElementTarget (AspectDeclaration aspect, MethodDefinition method)
        {
            var elemetTargets = aspect.ElementTargets;
            if (elemetTargets == ElementTargets.Default)
                return true;

            bool isCCtor = method.IsStatic && method.IsConstructor;
            bool isCtor = !method.IsStatic && method.IsConstructor;
            bool isProperty = method.IsPropertyMethod();

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