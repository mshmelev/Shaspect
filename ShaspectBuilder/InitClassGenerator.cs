using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using Shaspect.Builder.Tools;
using CustomAttributeNamedArgument = Mono.Cecil.CustomAttributeNamedArgument;
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
    ///     ...
    ///
    ///     static AspectsCollection()
    ///     {
    ///       Aspect_0 = new SimpleAspect (param1, param2, param3);
    ///       Aspect_0.Prop1 = val1;
    ///       Aspect_0.Prop2 = val2;
    /// 
    ///       Aspect_1 = new ComplexAspect (param1, param2);
    ///       ...
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
        }



        public FieldDefinition BuildAspectInitCode (AspectDeclaration aspect)
        {
            var ctor = initCtor.Body.Instructions;

            // The best option to create attribute instance is to call MethodInfo.GetCustomAttribute(). But this approach has some difficulties with
            // generics and marrying Mono.Cecil with .NET Reflection.
            // So, 2-step initializing is used:
            // 1. create aspect class instance with regular class intantiating
            // 2. initialize all properties

            var aspectInstanceVar = CreateAspectInstance (aspect);
            InitAspectFields (aspect, aspectInstanceVar);
            InitAspectProperties (aspect, aspectInstanceVar);

            // Store created aspect in a global variable
            var aspectInstanceField = new FieldDefinition ("Aspect_" + EmittedAspects,
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, aspect.Aspect.AttributeType);
            initClass.Fields.Add (aspectInstanceField);
            ctor.Add (OpCodes.Ldloc, aspectInstanceVar);
            ctor.Add (OpCodes.Stsfld, aspectInstanceField);

            ++EmittedAspects;

            return aspectInstanceField;
        }


        private VariableDefinition CreateAspectInstance (AspectDeclaration aspect)
        {
            var ctor = initCtor.Body.Instructions;
    
            // init all array-type parameters as local variables first
            var arrayVars = new Dictionary<CustomAttributeArgument, VariableDefinition>();
            foreach (var arg in aspect.Aspect.ConstructorArguments.Where (a => a.Type.IsArray))
                arrayVars.Add (arg, AddInitArrayCode (arg.Type, (Array) arg.Value));

            // call aspect's contructor
            // var aspect = new AspectClass (param1, param2, ...)
            foreach (var arg in aspect.Aspect.ConstructorArguments)
            {
                if (arg.Type.IsArray)
                    ctor.Add (OpCodes.Ldloc, arrayVars[arg]);
                else
                    ctor.Add (ILTools.GetLdcOpCode (arg.Type, arg.Value));
            }
            ctor.Add (OpCodes.Newobj, aspect.Aspect.Constructor);
            
            var aspectInstanceVar = new VariableDefinition (aspect.Aspect.AttributeType);
            ctor.Add (OpCodes.Stloc, aspectInstanceVar);
            initCtor.Body.Variables.Add (aspectInstanceVar);

            return aspectInstanceVar;
        }


        private void InitAspectFields (AspectDeclaration aspect, VariableDefinition aspectInstanceVar)
        {
            var ctor = initCtor.Body.Instructions;

            // init all array-type fields as local variables first
            var arrayVars = new Dictionary<CustomAttributeNamedArgument, VariableDefinition>();
            foreach (var field in aspect.Aspect.Fields.Where (a => a.Argument.Type.IsArray))
                arrayVars.Add (field, AddInitArrayCode (field.Argument.Type, (Array) field.Argument.Value));

            // set field values
            // aspectVar.field1 = const1;
            foreach (var field in aspect.Aspect.Fields)
            {
                ctor.Add (OpCodes.Ldloc, aspectInstanceVar);
                if (field.Argument.Type.IsArray)
                    ctor.Add (OpCodes.Ldloc, arrayVars[field]);
                else
                    ctor.Add (ILTools.GetLdcOpCode (field.Argument.Type, field.Argument.Value));

                var fieldDef = aspectInstanceVar.VariableType.FindField (field.Name);
                ctor.Add (OpCodes.Stfld, initCtor.Module.Import (fieldDef));
            }
        }


        private void InitAspectProperties (AspectDeclaration aspect, VariableDefinition aspectInstanceVar)
        {
            var ctor = initCtor.Body.Instructions;

            // init all array-type properties as local variables first
            var arrayVars = new Dictionary<CustomAttributeNamedArgument, VariableDefinition>();
            foreach (var prop in aspect.Aspect.Properties.Where (a => a.Argument.Type.IsArray))
                arrayVars.Add (prop, AddInitArrayCode (prop.Argument.Type, (Array) prop.Argument.Value));

            // set field values
            // aspectVar.field1 = const1;
            foreach (var prop in aspect.Aspect.Properties)
            {
                Console.WriteLine("---------- {0}.{1}", aspect.Declarator, prop.Name);
                ctor.Add (OpCodes.Ldloc, aspectInstanceVar);
                if (prop.Argument.Type.IsArray)
                    ctor.Add (OpCodes.Ldloc, arrayVars[prop]);
                else
                    ctor.Add (ILTools.GetLdcOpCode (prop.Argument.Type, prop.Argument.Value));

                var propSetMethod = aspectInstanceVar.VariableType.FindProperty (prop.Name).SetMethod;
                ctor.Add (OpCodes.Callvirt, initCtor.Module.Import (propSetMethod));
            }
        }


        private VariableDefinition AddInitArrayCode (TypeReference type, Array value)
        {
            if (value.Rank != 1)
                throw new ArgumentException ("Only 1-dimension arrays are supported", "value");

            var ctor = initCtor.Body.Instructions;
            var elemType = type.GetElementType();

            // Generates code:
            // var arrayVar = new ArrayType[n];
            // arrayVar[0] = const1;
            // arrayVar[1] = const2;
            // ...

            var arrayVar = new VariableDefinition (type);
            initCtor.Body.Variables.Add (arrayVar);

            int n= value.GetLength (0);
            ctor.Add (OpCodes.Ldc_I4, n);
            ctor.Add (OpCodes.Newarr, elemType);
            ctor.Add (OpCodes.Stloc, arrayVar);

            int startIndex = value.GetLowerBound (0);
            for (int i = 0; i < n; ++i)
            {
                ctor.Add (OpCodes.Ldloc, arrayVar);
                ctor.Add (OpCodes.Ldc_I4, i + startIndex);
                ctor.Add (ILTools.GetLdcOpCode (elemType, ((CustomAttributeArgument)value.GetValue (i+startIndex)).Value));
                ctor.Add (ILTools.GetStelemOpCode (elemType));
            }

            return arrayVar;
        }


        public void FinalizeBuilding()
        {
            initCtor.Body.Instructions.Add (OpCodes.Ret);
            initCtor.Body.OptimizeMacros();
        }

    }
}