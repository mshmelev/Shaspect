﻿using System;
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


        public static bool IsCompilerGenerated (this MethodDefinition method)
        {
            string s = method.Module.Import (typeof (CompilerGeneratedAttribute)).FullName;
            return method.CustomAttributes.Any (a => a.AttributeType.FullName == s);
        }


        public static bool IsInheritedFrom (this TypeReference childType, TypeReference baseType)
        {
            try
            {
                do
                {
                    if (childType.FullName == baseType.FullName)
                        return true;
                    childType = childType.Resolve().BaseType;
                } while (childType != null);
            }
            catch (AssemblyResolutionException)
            {
            }

            return false;
        }


        public static FieldDefinition FindField (this TypeReference type, string name)
        {
            while (type != null)
            {
                var typeDef = type.Resolve();
                var field = typeDef.Fields.FirstOrDefault (f => f.Name == name);
                if (field != null)
                    return field;

                type = typeDef.BaseType;
            }

            return null;
        }


        public static PropertyDefinition FindProperty (this TypeReference type, string name)
        {
            while (type != null)
            {
                var typeDef = type.Resolve();
                var prop = typeDef.Properties.FirstOrDefault (f => f.Name == name);
                if (prop != null)
                    return prop;

                type = typeDef.BaseType;
            }

            return null;
        }


        public static bool IsPropertyMethod (this MethodDefinition method)
        {
            return method.IsSpecialName && method.IsCompilerGenerated() && (method.Name.StartsWith ("get_") || method.Name.StartsWith ("set_"));
        }


        public static string GetPropertyNameByMethod (MethodDefinition method)
        {
            if (!method.IsPropertyMethod())
                throw new ArgumentException("Not a property method", "method");

            return method.Name.Substring (4);
        }

    }
}