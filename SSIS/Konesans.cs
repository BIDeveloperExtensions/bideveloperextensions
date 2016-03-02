using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
//using Konesans.Dts.Design.Controls;
//using Konesans.Dts.Design.PropertyHelp;

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;

namespace BIDSHelper.SSIS
{
    public class Konesans
    {
        private static System.Reflection.Assembly konesansAssembly = null;
        private static Type typePropertyVariables = null;

        public static Form CreateKonesansExpressionEditorForm(Variables variables, VariableDispenser variableDispenser, Type propertyType, PropertyDescriptor property, string expression)
        {
            EnsurePropertyVariablesTypeLoaded();

            System.Reflection.BindingFlags setpropflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance;
            object oPropertyVariables = typePropertyVariables.GetConstructors()[0].Invoke(new object[] { });
            oPropertyVariables.GetType().InvokeMember("Variables", setpropflags, null, oPropertyVariables, new object[] { variables });
            oPropertyVariables.GetType().InvokeMember("VariableDispenser", setpropflags, null, oPropertyVariables, new object[] { variableDispenser });
            oPropertyVariables.GetType().InvokeMember("Type", setpropflags, null, oPropertyVariables, new object[] { propertyType });

            Type typeExpressionEditorPublic = konesansAssembly.GetType("Konesans.Dts.Design.Controls.ExpressionEditorPublic");
            Form editor = (Form)typeExpressionEditorPublic.GetConstructors()[0].Invoke(new object[] { expression, oPropertyVariables, property });
            return editor;
        }

        #region Late Binding to Konesans Assembly
        private static void EnsureKonesansAssemblyLoaded()
        {
            if (konesansAssembly == null)
            {
                konesansAssembly = System.Reflection.Assembly.Load(BIDSHelper.Properties.Resources.Konesans_Dts_CommonLibrary);
            }
        }

        private static void EnsurePropertyVariablesTypeLoaded()
        {
            EnsureKonesansAssemblyLoaded();
            if (typePropertyVariables != null) return;

            Type typeInterface = konesansAssembly.GetType("Konesans.Dts.Design.PropertyHelp.IPropertyRuntimeVariables");

            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "BIDSHelperKonesans";

            AssemblyBuilder newAssembly = System.Threading.Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder newModule = newAssembly.DefineDynamicModule("KonesansInterfaces");
            TypeBuilder myTypeBuilder = newModule.DefineType("PropertyVariables", TypeAttributes.Public);
            myTypeBuilder.AddInterfaceImplementation(typeInterface);

            FieldBuilder fieldVariables = myTypeBuilder.DefineField("_variables", typeof(Variables), FieldAttributes.Private);
            FieldBuilder fieldVariableDispenser = myTypeBuilder.DefineField("_variableDispenser", typeof(VariableDispenser), FieldAttributes.Private);
            FieldBuilder fieldType = myTypeBuilder.DefineField("_type", typeof(Type), FieldAttributes.Private);



            PropertyBuilder propertyBuilderVariables = myTypeBuilder.DefineProperty("Variables",
                                     PropertyAttributes.HasDefault,
                                     typeof(Variables),
                                     new Type[] { typeof(Variables) });

            //define the behavior of the "get" property
            MethodBuilder methodBuilderGetVariables = myTypeBuilder.DefineMethod("GetVariables",
                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                    typeof(Variables),
                                    new Type[] { });

            ILGenerator ilGetVariables = methodBuilderGetVariables.GetILGenerator();
            ilGetVariables.Emit(OpCodes.Ldarg_0);
            ilGetVariables.Emit(OpCodes.Ldfld, fieldVariables);
            ilGetVariables.Emit(OpCodes.Ret);

            //define the behavior of the "set" property
            MethodBuilder methodBuilderSetVariables = myTypeBuilder.DefineMethod("SetVariables",
                                    MethodAttributes.Public,
                                    null,
                                    new Type[] { typeof(Variables) });

            ILGenerator ilSetVariables = methodBuilderSetVariables.GetILGenerator();
            ilSetVariables.Emit(OpCodes.Ldarg_0);
            ilSetVariables.Emit(OpCodes.Ldarg_1);
            ilSetVariables.Emit(OpCodes.Stfld, fieldVariables);
            ilSetVariables.Emit(OpCodes.Ret);

            //Map the two methods created above to our PropertyBuilder to 
            //their corresponding behaviors, "get" and "set" respectively. 
            propertyBuilderVariables.SetGetMethod(methodBuilderGetVariables);
            propertyBuilderVariables.SetSetMethod(methodBuilderSetVariables);

            MethodInfo methodInfoGetVariables = typeInterface.GetProperty("Variables").GetGetMethod();
            myTypeBuilder.DefineMethodOverride(methodBuilderGetVariables, methodInfoGetVariables);



            PropertyBuilder propertyBuilderVariableDispenser = myTypeBuilder.DefineProperty("VariableDispenser",
                         PropertyAttributes.HasDefault,
                         typeof(VariableDispenser),
                         new Type[] { typeof(VariableDispenser) });

            //define the behavior of the "get" property
            MethodBuilder methodBuilderGetVariableDispenser = myTypeBuilder.DefineMethod("GetVariableDispenser",
                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                    typeof(VariableDispenser),
                                    new Type[] { });

            ILGenerator ilGetVariableDispenser = methodBuilderGetVariableDispenser.GetILGenerator();
            ilGetVariableDispenser.Emit(OpCodes.Ldarg_0);
            ilGetVariableDispenser.Emit(OpCodes.Ldfld, fieldVariableDispenser);
            ilGetVariableDispenser.Emit(OpCodes.Ret);

            //define the behavior of the "set" property
            MethodBuilder methodBuilderSetVariableDispenser = myTypeBuilder.DefineMethod("SetVariableDispenser",
                                    MethodAttributes.Public,
                                    null,
                                    new Type[] { typeof(VariableDispenser) });

            ILGenerator ilSetVariableDispenser = methodBuilderSetVariableDispenser.GetILGenerator();
            ilSetVariableDispenser.Emit(OpCodes.Ldarg_0);
            ilSetVariableDispenser.Emit(OpCodes.Ldarg_1);
            ilSetVariableDispenser.Emit(OpCodes.Stfld, fieldVariableDispenser);
            ilSetVariableDispenser.Emit(OpCodes.Ret);

            //Map the two methods created above to our PropertyBuilder to 
            //their corresponding behaviors, "get" and "set" respectively. 
            propertyBuilderVariableDispenser.SetGetMethod(methodBuilderGetVariableDispenser);
            propertyBuilderVariableDispenser.SetSetMethod(methodBuilderSetVariableDispenser);

            MethodInfo methodInfoGetVariableDispenser = typeInterface.GetProperty("VariableDispenser").GetGetMethod();
            myTypeBuilder.DefineMethodOverride(methodBuilderGetVariableDispenser, methodInfoGetVariableDispenser);



            PropertyBuilder propertyBuilderType = myTypeBuilder.DefineProperty("Type",
                                     PropertyAttributes.HasDefault,
                                     typeof(Type),
                                     new Type[] { typeof(Type) });

            //define the behavior of the "get" property
            MethodBuilder methodBuilderGetType = myTypeBuilder.DefineMethod("GetType",
                                    MethodAttributes.Public,
                                    typeof(Type),
                                    new Type[] { });

            ILGenerator ilGetType = methodBuilderGetType.GetILGenerator();
            ilGetType.Emit(OpCodes.Ldarg_0);
            ilGetType.Emit(OpCodes.Ldfld, fieldType);
            ilGetType.Emit(OpCodes.Ret);

            //define the behavior of the "set" property
            MethodBuilder methodBuilderSetType = myTypeBuilder.DefineMethod("SetType",
                                    MethodAttributes.Public,
                                    null,
                                    new Type[] { typeof(Type) });

            ILGenerator ilSetType = methodBuilderSetType.GetILGenerator();
            ilSetType.Emit(OpCodes.Ldarg_0);
            ilSetType.Emit(OpCodes.Ldarg_1);
            ilSetType.Emit(OpCodes.Stfld, fieldType);
            ilSetType.Emit(OpCodes.Ret);

            //Map the two methods created above to our PropertyBuilder to 
            //their corresponding behaviors, "get" and "set" respectively. 
            propertyBuilderType.SetGetMethod(methodBuilderGetType);
            propertyBuilderType.SetSetMethod(methodBuilderSetType);



            MethodBuilder methodBuilderGetPropertyType =
               myTypeBuilder.DefineMethod(
               "GetPropertyType",
               MethodAttributes.Public | MethodAttributes.Virtual,
               typeof(Type),
               new Type[] { typeof(string) });

            ILGenerator ilGetPropertyType = methodBuilderGetPropertyType.GetILGenerator();
            ilGetPropertyType.Emit(OpCodes.Ldarg_0);
            ilGetPropertyType.Emit(OpCodes.Ldfld, fieldType);
            ilGetPropertyType.Emit(OpCodes.Ret);

            MethodInfo methodInfoGetPropertyType = typeInterface.GetMethod(methodBuilderGetPropertyType.Name);
            myTypeBuilder.DefineMethodOverride(methodBuilderGetPropertyType, methodInfoGetPropertyType);



            typePropertyVariables = myTypeBuilder.CreateType();
        }
        #endregion
    }
}
