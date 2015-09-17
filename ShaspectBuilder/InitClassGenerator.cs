using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Shaspect.Builder.Tools;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;


namespace Shaspect.Builder
{
    /// <summary>
    /// Generates Shaspect.Implementation.AspectsCollection class, which initializes all aspects in the target assembly.
    /// The code is generated using IL and is equivalent of:
    /// <code>
    /// namespace Shaspect.Implementation
    /// {
    ///   [CompilerGenerated]
    ///   [DebuggerNonUserCode]
    ///   internal static class AspectsCollection
    ///   {
    ///     public static readonly SimpleAspect Aspect_0;
    ///     public static readonly ComplexAspect Aspect_1;
    ///
    ///     static AspectsCollection()
    ///     {
    ///       Aspect_0 = (SimpleAspect) GetAttrInstance (typeof(Class1).GetMethod("Method1"), typeof (SimpleAspect));
    ///       Aspect_1 = (ComplexAspect) GetAttrInstance (typeof(Class1).GetMethod("Method2"), typeof (ComplexAspect));
    ///     }
    ///
    ///     private static Attribute GetAttrInstance(ICustomAttributeProvider element, Type attrType)
    ///     {
    ///       var attrs = memberInfo.GetCustomAttributes (attrType);
    ///       foreach (var attr in attrs)
    ///       {
    ///         if (attr.GetType().Equals (attrType))
    ///           return (Attribute)attr;
    ///       }
    ///       return null;
    ///     }
    ///   }
    /// }
    /// </code>
    /// </summary>
    internal class InitClassGenerator
    {
        private readonly ModuleDefinition mainModule;
        private TypeDefinition initClass;
        private MethodDefinition initCtor;
        private MethodDefinition getAttrInstanceMethod;

        public int EmittedAspects { get; private set; }



        public InitClassGenerator(AssemblyDefinition assembly)
        {
            mainModule = assembly.MainModule;

            CreateAspectsCollectionClass();
        }


        private void CreateAspectsCollectionClass()
        {
            initClass = new TypeDefinition (
                "Shaspect.Implementation_"+ mainModule.Assembly.FullName.GetHashCode().ToString("x"),
                "AspectsCollection",
                TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.AnsiClass | TypeAttributes.AutoClass |
                TypeAttributes.NotPublic | TypeAttributes.Sealed,
                mainModule.TypeSystem.Object
                );
            initClass.CustomAttributes.Add (
                new CustomAttribute (mainModule.Import (typeof (DebuggerNonUserCodeAttribute).GetConstructor (Type.EmptyTypes))));
            initClass.CustomAttributes.Add (
                new CustomAttribute (mainModule.Import (typeof (CompilerGeneratedAttribute).GetConstructor (Type.EmptyTypes))));
            mainModule.Types.Add (initClass);

            initCtor = new MethodDefinition (".cctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName | MethodAttributes.Static, mainModule.TypeSystem.Void);
            initCtor.Body.InitLocals = true;
            initCtor.Body.Variables.Add (new VariableDefinition (mainModule.Import (typeof(Type[]))));   // used in attributes instantiating
            initClass.Methods.Add (initCtor);

            BuildGetAttrInstance();
        }



        public FieldDefinition BuildAspectInitCode (AspectDeclaration aspect)
        {
            var ctor = initCtor.Body.Instructions;

            var aspectField = new FieldDefinition ("Aspect_" + EmittedAspects,
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, aspect.Aspect.AttributeType);
            initClass.Fields.Add (aspectField);

            // instantiating Attribute by calling GetCustomAttribute() for the declarator type
            if (aspect.Declarator is AssemblyDefinition)
                BuildGetAssemblyInfo (ctor);
            else if (aspect.Declarator is TypeDefinition)
                BuildGetTypeInfo ((TypeDefinition)aspect.Declarator, ctor);
            else if (aspect.Declarator is MethodDefinition)
                BuildGetMethodInfo ((MethodDefinition)aspect.Declarator, ctor);
            else if (aspect.Declarator is PropertyDefinition)
                BuildGetPropertyInfo ((PropertyDefinition)aspect.Declarator, ctor);

            // the IL code bellow is generated from:
/*
            Aspect_0= (Test1Attribute) GetAttrInstance (methodInfo, typeof (Test1Attribute));  // methodInfo - retrieved above
*/
            ctor.Add (OpCodes.Ldtoken, aspect.Aspect.AttributeType);
            ctor.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));
            ctor.Add (OpCodes.Call, getAttrInstanceMethod);

            ctor.Add (OpCodes.Castclass, aspect.Aspect.AttributeType);
            ctor.Add (OpCodes.Stsfld, aspectField);

            ++EmittedAspects;

            return aspectField;
        }


        public void FinalizeBuilding()
        {
            initCtor.Body.Instructions.Add (OpCodes.Ret);
        }


        private void BuildGetAssemblyInfo (Collection<Instruction> code)
        {
            // the IL code bellow is generated from:
            /*
            Assembly.GetExecutingAssembly();
            */
            code.Add (OpCodes.Call, mainModule.Import (typeof (Assembly).GetMethod ("GetExecutingAssembly")));
        }


        private void BuildGetTypeInfo (TypeDefinition typeDef, Collection<Instruction> code)
        {
            // the IL code bellow is generated from:
            /*
            typeof (Class1)
            */
            code.Add (OpCodes.Ldtoken, typeDef);
            code.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));
        }


        private void BuildGetMethodInfo (MethodDefinition method, Collection<Instruction> code)
        {
            // the IL code bellow is generated from:
/*
            var methodInfo= typeof (Class1)
                .GetMethod (
                    "F1",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                    null,
                    CallingConventions.Any,
                    new[] {typeof (int), typeof (string)},  // params
                    null
                );
*/
            code.Add (OpCodes.Ldtoken, method.DeclaringType);
            code.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));
            code.Add (OpCodes.Ldstr, method.Name);
            code.Add (OpCodes.Ldc_I4, (int) (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
            code.Add (OpCodes.Ldnull);
            code.Add (OpCodes.Ldc_I4, (int) CallingConventions.Any);

            code.Add (OpCodes.Ldc_I4, method.Parameters.Count);
            code.Add (OpCodes.Newarr, mainModule.Import (typeof (Type)));
            code.Add (OpCodes.Stloc_0);
            code.Add (OpCodes.Ldloc_0);

            for (int i = 0; i < method.Parameters.Count; ++i)
            {
                code.Add (OpCodes.Ldc_I4, i);
                code.Add (OpCodes.Ldtoken, method.Parameters[i].ParameterType);
                code.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));
                code.Add (OpCodes.Stelem_Ref);
                code.Add (OpCodes.Ldloc_0);
            }

            code.Add (OpCodes.Ldnull);
            code.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetMethod", new[]
            {
                typeof (string), typeof (BindingFlags), typeof (Binder), typeof (CallingConventions), typeof (Type[]),
                typeof (ParameterModifier[])
            })));
        }
        
        
        private void BuildGetPropertyInfo (PropertyDefinition property, Collection<Instruction> code)
        {
            // the IL code bellow is generated from:
/*
            var methodInfo= typeof (Class1)
                .GetProperty (
                    "P1",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                    null,
                    typeof(int),                            // return type
                    new[] {typeof (int), typeof (string)},  // params
                    null
                );
*/
            code.Add (OpCodes.Ldtoken, property.DeclaringType);
            code.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));
            code.Add (OpCodes.Ldstr, property.Name);
            code.Add (OpCodes.Ldc_I4, (int) (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
            code.Add (OpCodes.Ldnull);
            code.Add (OpCodes.Ldtoken, property.PropertyType);
            code.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));

            code.Add (OpCodes.Ldc_I4, property.Parameters.Count);
            code.Add (OpCodes.Newarr, mainModule.Import (typeof (Type)));
            code.Add (OpCodes.Stloc_0);
            code.Add (OpCodes.Ldloc_0);

            for (int i = 0; i < property.Parameters.Count; ++i)
            {
                code.Add (OpCodes.Ldc_I4, i);
                code.Add (OpCodes.Ldtoken, property.Parameters[i].ParameterType);
                code.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetTypeFromHandle")));
                code.Add (OpCodes.Stelem_Ref);
                code.Add (OpCodes.Ldloc_0);
            }

            code.Add (OpCodes.Ldnull);
            code.Add (OpCodes.Call, mainModule.Import (typeof (Type).GetMethod ("GetProperty", new[]
            {
                typeof (string), typeof (BindingFlags), typeof (Binder), typeof (Type), typeof (Type[]),
                typeof (ParameterModifier[])
            })));
        }




        /// <summary>
        /// Add GetAttrInstance() method to Shaspect.Implementation
        /// </summary>
        private void BuildGetAttrInstance()
        {
            // The IL code bellow is compiled from:
/*
        private static Attribute GetAttrInstance (ICustomAttributeProvider element, Type attrType)
        {
            var attrs = memberInfo.GetCustomAttributes (attrType);
            foreach (var attr in attrs)
            {
                if (attr.GetType().Equals (attrType))
                    return (Attribute)attr;
            }

            return null;
        }
*/

            getAttrInstanceMethod = new MethodDefinition ("GetAttrInstance", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, mainModule.Import (typeof (Attribute)));

            initClass.Methods.Add (getAttrInstanceMethod);

            getAttrInstanceMethod.Parameters.Add (new ParameterDefinition ("element", ParameterAttributes.None, mainModule.Import (typeof(System.Reflection.ICustomAttributeProvider))));
            getAttrInstanceMethod.Parameters.Add (new ParameterDefinition ("attrType", ParameterAttributes.None, mainModule.Import (typeof(Type))));

            getAttrInstanceMethod.Body.InitLocals = true;
            getAttrInstanceMethod.Body.Variables.Add (new VariableDefinition ("attrs", mainModule.Import (typeof(object[]))));
            getAttrInstanceMethod.Body.Variables.Add (new VariableDefinition ("attr", mainModule.Import (typeof(object))));
            getAttrInstanceMethod.Body.Variables.Add (new VariableDefinition (mainModule.Import (typeof(Attribute))));
            getAttrInstanceMethod.Body.Variables.Add (new VariableDefinition (mainModule.Import (typeof (object[]))));
            var intVar = new VariableDefinition (mainModule.Import (typeof (int)));
            getAttrInstanceMethod.Body.Variables.Add (intVar);

            var code = getAttrInstanceMethod.Body.Instructions;

            var retRes = Instruction.Create (OpCodes.Ldloc_2);
            var label1 = Instruction.Create (OpCodes.Ldloc_S, intVar);
            var label2 = Instruction.Create (OpCodes.Ldloc_S, intVar);

            code.Add (OpCodes.Ldarg_0);
            code.Add (OpCodes.Ldarg_1);
            code.Add (OpCodes.Ldc_I4_1);    // true
            code.Add (OpCodes.Callvirt, mainModule.Import (typeof (System.Reflection.ICustomAttributeProvider).GetMethod ("GetCustomAttributes", new []
                {
                    typeof(Type), typeof(bool)
                })));
            code.Add (OpCodes.Stloc_0);
            code.Add (OpCodes.Ldloc_0);
            code.Add (OpCodes.Stloc_3);
            code.Add (OpCodes.Ldc_I4_0);
            code.Add (OpCodes.Stloc_S, intVar);
            code.Add (OpCodes.Br_S, label2);
            var label3= Instruction.Create (OpCodes.Ldloc_3);
            code.Add (label3);
            code.Add (OpCodes.Ldloc_S, intVar);
            code.Add (OpCodes.Ldelem_Ref);
            code.Add (OpCodes.Stloc_1);
            code.Add (OpCodes.Ldloc_1);
            code.Add (OpCodes.Callvirt, mainModule.Import (typeof (object).GetMethod("GetType", Type.EmptyTypes)));
            code.Add (OpCodes.Ldarg_1);
            code.Add (OpCodes.Callvirt, mainModule.Import (typeof (Type).GetMethod("Equals", new [] {typeof(Type)})));
            code.Add (OpCodes.Brfalse_S, label1);
            code.Add (OpCodes.Ldloc_1);
            code.Add (OpCodes.Castclass, mainModule.Import (typeof (Attribute)));
            code.Add (OpCodes.Stloc_2);
            code.Add (OpCodes.Leave_S, retRes);
            code.Add (label1);      // OpCodes.Ldloc_S, intVar
            code.Add (OpCodes.Ldc_I4_1);
            code.Add (OpCodes.Add);
            code.Add (OpCodes.Stloc_S, intVar);
            code.Add (label2);      // OpCodes.Ldloc_S, intVar
            code.Add (OpCodes.Ldloc_3);
            code.Add (OpCodes.Ldlen);
            code.Add (OpCodes.Conv_I4);
            code.Add (OpCodes.Blt_S, label3);
            code.Add (OpCodes.Ldnull);
            code.Add (OpCodes.Ret);

            code.Add (retRes);      // OpCodes.Ldloc_2
            code.Add (OpCodes.Ret);
        }


    }
}