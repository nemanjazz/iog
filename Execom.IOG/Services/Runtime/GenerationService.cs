// -----------------------------------------------------------------------
// <copyright file="GenerationService.cs" company="Execom">
// Copyright 2011 EXECOM d.o.o
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace Execom.IOG.Services.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Collections;
    using Execom.IOG.Services.Data;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Execom.IOG.Services.Facade;
    using Execom.IOG.Attributes;
    using Execom.IOG.Types;

    /// <summary>
    /// Class responsible for generation of proxy types and instances.
    /// Proxy types implement interfaces defined by entity models.
    /// They serve as application entity objects during runtime.
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class GenerationService
    {
        /// <summary>
        /// Service which allows access to type information
        /// </summary>
        private TypesService typesService;
        
        /// <summary>
        /// Creates new instance of GenerationService type
        /// </summary>
        /// <param name="typesService">Type service to use</param>
        public GenerationService(TypesService typesService)
        {
            this.typesService = typesService;
        }

        /// <summary>
        /// Generates Equals/GetHashCode methods in type 
        /// </summary>
        /// <param name="tb">Type builder to use</param>
        private static void AddEqualsHashcode(TypeBuilder tb)
        {
            MethodBuilder equals = tb.DefineMethod("Equals", MethodAttributes.Public | MethodAttributes.Virtual, typeof(bool), new Type[] { typeof(object) });
            ILGenerator equalsIL = equals.GetILGenerator();
            equalsIL.Emit(OpCodes.Nop);
            equalsIL.Emit(OpCodes.Ldarg_0);
            equalsIL.Emit(OpCodes.Ldarg_1);
            equalsIL.Emit(OpCodes.Call, typeof(StaticProxyFacade).GetMethod("AreEqual", new Type[] { typeof(object), typeof(object) }));
            equalsIL.Emit(OpCodes.Ret);


            MethodBuilder hashCode = tb.DefineMethod("GetHashCode", MethodAttributes.Public | MethodAttributes.Virtual, typeof(int), new Type[0]);
            ILGenerator hashCodeIL = hashCode.GetILGenerator();
            hashCodeIL.Emit(OpCodes.Nop);
            hashCodeIL.Emit(OpCodes.Ldarg_0);
            hashCodeIL.Emit(OpCodes.Call, typeof(StaticProxyFacade).GetMethod("GetProxyHashCode"));
            hashCodeIL.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Generates code for a property member
        /// </summary>
        /// <param name="staticConstructorIL">Builder of static constructor</param>
        /// <param name="tb">Builder of type</param>
        /// <param name="property">Reflected property</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="fbinstaceId">Builder of instance ID field</param>
        /// <param name="fbTypeId">Builder of type ID field</param>
        /// <param name="fbReadOnly">Builder of readOnly field</param>
        /// <param name="fbFacade">Builder of facade field</param>
        private static void GenerateDataProperty(ILGenerator staticConstructorIL, TypeBuilder tb, PropertyInfo property, String propertyName, FieldBuilder fbinstaceId, FieldBuilder fbTypeId, FieldBuilder fbReadOnly, FieldBuilder fbFacade)
        {
            FieldBuilder fbProperty = tb.DefineField(propertyName + Constants.PropertyMemberIdSufix, typeof(Guid), FieldAttributes.Private | FieldAttributes.Static);
            FieldBuilder fbIsScalar = tb.DefineField(propertyName + Constants.PropertyIsScalarSufix, typeof(bool), FieldAttributes.Private | FieldAttributes.Static);

            // Generate static constructor part for a property. 

            // Resolve member id for property
            staticConstructorIL.Emit(OpCodes.Ldloc_0);
            staticConstructorIL.Emit(OpCodes.Ldsfld, fbTypeId);
            staticConstructorIL.Emit(OpCodes.Ldstr, propertyName);
            staticConstructorIL.Emit(OpCodes.Callvirt, typeof(StaticProxyFacade).GetMethod("GetTypeMemberId"));
            staticConstructorIL.Emit(OpCodes.Stsfld, fbProperty);

            // Resolve if property is scalar
            staticConstructorIL.Emit(OpCodes.Ldloc_0);
            staticConstructorIL.Emit(OpCodes.Ldsfld, fbProperty);
            staticConstructorIL.Emit(OpCodes.Callvirt, typeof(StaticProxyFacade).GetMethod("IsScalarMember"));
            staticConstructorIL.Emit(OpCodes.Stsfld, fbIsScalar);



            // Define a property
            PropertyBuilder propertyBuilder = tb.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.Virtual;

            // Define the "get" accessor method a property Number. The method returns
            // value of RuntimeProxy.GetInstanceMemberValue method
            MethodBuilder propertyGetAccessor = tb.DefineMethod("get_" + property.Name, getSetAttr, property.PropertyType, Type.EmptyTypes);
            ILGenerator propertyGetIL = propertyGetAccessor.GetILGenerator();
            propertyGetIL.Emit(OpCodes.Ldarg_0);
            propertyGetIL.Emit(OpCodes.Ldfld, fbFacade);
            propertyGetIL.Emit(OpCodes.Ldarg_0);
            propertyGetIL.Emit(OpCodes.Ldfld, fbinstaceId);
            propertyGetIL.Emit(OpCodes.Ldsfld, fbProperty);
            propertyGetIL.Emit(OpCodes.Ldsfld, fbIsScalar);
            propertyGetIL.Emit(OpCodes.Ldarg_0);
            propertyGetIL.Emit(OpCodes.Ldfld, fbReadOnly);
            propertyGetIL.Emit(OpCodes.Callvirt, typeof(IRuntimeProxyFacade).GetMethod("GetInstanceMemberValue", new Type[] { typeof(Guid), typeof(Guid), typeof(bool), typeof(bool) }));
            propertyGetIL.Emit(OpCodes.Unbox_Any, property.PropertyType);
            propertyGetIL.Emit(OpCodes.Ret);

            // Define the "set" accessor method a property, which has no return
            // type and calls RuntimeProxy.SetInstanceMemberValue method
            MethodBuilder propertySetAccessor = tb.DefineMethod("set_" + property.Name, getSetAttr, null, new Type[] { property.PropertyType });
            ILGenerator propertySetIL = propertySetAccessor.GetILGenerator();
            propertySetIL.Emit(OpCodes.Ldarg_0);
            propertySetIL.Emit(OpCodes.Ldfld, fbFacade);
            propertySetIL.Emit(OpCodes.Ldarg_0);
            propertySetIL.Emit(OpCodes.Ldfld, fbinstaceId);
            propertySetIL.Emit(OpCodes.Ldsfld, fbProperty);
            propertySetIL.Emit(OpCodes.Ldarg_1);
            propertySetIL.Emit(OpCodes.Box, property.PropertyType);
            propertySetIL.Emit(OpCodes.Ldsfld, fbIsScalar);
            propertySetIL.Emit(OpCodes.Ldarg_0);
            propertySetIL.Emit(OpCodes.Ldfld, fbReadOnly);
            propertySetIL.Emit(OpCodes.Callvirt, typeof(IRuntimeProxyFacade).GetMethod("SetInstanceMemberValue", new Type[] { typeof(Guid), typeof(Guid), typeof(object), typeof(bool), typeof(bool) }));
            propertySetIL.Emit(OpCodes.Ret);

            // Map the "get" and "set" accessor methods to the PropertyBuilder. 
            propertyBuilder.SetGetMethod(propertyGetAccessor);
            propertyBuilder.SetSetMethod(propertySetAccessor);
        }

        private static void GenerateRevisionIdProperty(TypeBuilder tb, PropertyInfo property, FieldBuilder fbinstaceId)
        {
            // Define a property
            PropertyBuilder propertyBuilder = tb.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.Virtual;

            // Define the "get" accessor method a property Number. The method returns
            // value of RuntimeProxy.GetInstanceMemberValue method
            MethodBuilder propertyGetAccessor = tb.DefineMethod("get_" + property.Name, getSetAttr, property.PropertyType, Type.EmptyTypes);
            ILGenerator propertyGetIL = propertyGetAccessor.GetILGenerator();
            propertyGetIL.Emit(OpCodes.Ldarg_0);
            propertyGetIL.Emit(OpCodes.Ldfld, fbinstaceId);           
            propertyGetIL.Emit(OpCodes.Ret);
            
            // Map the "get" accessor methods to the PropertyBuilder. 
            propertyBuilder.SetGetMethod(propertyGetAccessor);

            propertyBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(RevisionIdAttribute).GetConstructor(new Type[] { }), new object[] { }));
        }

        /// <summary>
        /// Generates entity proxy type for given entity interface
        /// </summary>
        /// <param name="type">Interface type, it must be registered previously with the types service</param>
        /// <param name="saveAssemblyToDisk">Defines if assembly should be saved to disk</param>
        /// <param name="assemblyFileName">If assembly saving is on, defines path and file name of the generated assembly</param>
        /// <returns>Generated proxy type</returns>
        public Type GenerateProxyType(Type type, bool saveAssemblyToDisk, string assemblyFileName)
        {            
            Type dictionaryType = null;
            if (Utils.IsDictionaryType(type, ref dictionaryType))
            {
                return GenerateDictionaryProxyType(type);
            }

            Type collectionType = null;
            if (Utils.IsCollectionType(type, ref collectionType))
            {
                return GenerateCollectionProxyType(type);
            }

            if (!type.IsInterface)
            {
                throw new ArgumentException("Type should be an interface:" + type.AssemblyQualifiedName);
            }

            Guid typeId = typesService.GetTypeId(type);

            if (typeId == Guid.Empty)
            {
                throw new ArgumentException("Type not registered:" + type.AssemblyQualifiedName);
            }
            
            AssemblyName aName = new AssemblyName(Constants.GeneratedAssemblyName);

            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave);            
           
            // For a single-module assembly, the module name is usually
            // the assembly name plus an extension.
            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name);            

            // Create the type.
            TypeBuilder typeBuilder = mb.DefineType(type.Name + Constants.ProxyTypeSufix, TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(type);            
            FieldBuilder fbReadOnly = typeBuilder.DefineField(Constants.ReadOnlyFieldName, typeof(Boolean), FieldAttributes.Public);
            FieldBuilder fbinstaceId = typeBuilder.DefineField(Constants.InstanceIdFieldName, typeof(Guid), FieldAttributes.Public);
            FieldBuilder fbPrimaryKeyId = typeBuilder.DefineField(Constants.PrimaryKeyIdFieldName, typeof(Guid), FieldAttributes.Public | FieldAttributes.Static);            
            FieldBuilder fbTypeId = typeBuilder.DefineField(Constants.TypeIdFieldName, typeof(Guid), FieldAttributes.Private | FieldAttributes.Static);
            FieldBuilder fbFacade = typeBuilder.DefineField(Constants.FacadeFieldName, typeof(IRuntimeProxyFacade), FieldAttributes.Private);

            // Define the constructor for proxy (IRuntimeProxyFacade facade, Guid instanceId, Boolean readOnly)
            Type[] parameterTypes = { typeof(IRuntimeProxyFacade), typeof(Guid), typeof(Boolean) };
            ConstructorBuilder constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);
            ILGenerator constructorIL = constructor.GetILGenerator();
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_2);
            constructorIL.Emit(OpCodes.Stfld, fbinstaceId);
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_3);
            constructorIL.Emit(OpCodes.Stfld, fbReadOnly);
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, fbFacade);
            constructorIL.Emit(OpCodes.Ret);

            // Define the static constructor for type
            ConstructorBuilder staticConstructor = typeBuilder.DefineTypeInitializer();            
            ILGenerator staticConstructorIL = staticConstructor.GetILGenerator();
            staticConstructorIL.DeclareLocal(typeof(StaticProxyFacade));
            staticConstructorIL.Emit(OpCodes.Nop);
            staticConstructorIL.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod"));
            staticConstructorIL.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("get_DeclaringType"));
            staticConstructorIL.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("get_GUID"));
            staticConstructorIL.Emit(OpCodes.Stsfld, fbTypeId); // Set result to static field typeId

            staticConstructorIL.Emit(OpCodes.Call, typeof(StaticProxyFacade).GetMethod("get_Instance")); // Get instance of proxy facade
            staticConstructorIL.Emit(OpCodes.Stloc_0); // Set to local 0  

            staticConstructorIL.Emit(OpCodes.Ldloc_0);
            staticConstructorIL.Emit(OpCodes.Ldsfld, fbTypeId);
            staticConstructorIL.Emit(OpCodes.Callvirt, typeof(StaticProxyFacade).GetMethod("GetTypePrimaryKeyMemberId"));//Get id of primary key member
            staticConstructorIL.Emit(OpCodes.Stsfld, fbPrimaryKeyId);//Set to static field

            Collection<PropertyInfo> properties = new Collection<PropertyInfo>();

            // Get list of properties from the inherited interfaces also
            Utils.ExtractProperties(type, properties);

            // define all properties and related code in static constructor
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (IsRevisionIdProperty(propertyInfo))
                {
                    GenerateRevisionIdProperty(typeBuilder, propertyInfo, fbinstaceId);
                }
                else
                    if (IsDataProperty(propertyInfo))
                    {
                        GenerateDataProperty(staticConstructorIL, typeBuilder, propertyInfo, propertyInfo.Name, fbinstaceId, fbTypeId, fbReadOnly, fbFacade);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid property " + propertyInfo.ToString() + " in " + type.ToString());
                    }
            }

            //finish the static constructor.
            staticConstructorIL.Emit(OpCodes.Ret);

            // Generate destructor
            ILGenerator finalizerIL = typeBuilder.DefineMethod("Finalize", MethodAttributes.Virtual | MethodAttributes.Family | MethodAttributes.HideBySig).GetILGenerator();
            finalizerIL.BeginExceptionBlock();
            finalizerIL.Emit(OpCodes.Ldarg_0);
            finalizerIL.Emit(OpCodes.Ldfld, fbFacade);
            finalizerIL.Emit(OpCodes.Ldarg_0);            
            finalizerIL.Emit(OpCodes.Callvirt, typeof(IRuntimeProxyFacade).GetMethod("ProxyCollected"));
            finalizerIL.BeginFinallyBlock();
            finalizerIL.Emit(OpCodes.Ldarg_0);
            finalizerIL.Emit(OpCodes.Call, typeof(Object).GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance));
            finalizerIL.EndExceptionBlock();
            finalizerIL.Emit(OpCodes.Ret);

            AddEqualsHashcode(typeBuilder);

            //Add GUID attribute which corresponds to registered type ID
            typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(GuidAttribute).GetConstructor(new Type[] { typeof(string) }), new object[] { typeId.ToString() }));

            // Finish the type.
            Type generatedType = typeBuilder.CreateType();

            if (saveAssemblyToDisk)
            {
                ab.Save(assemblyFileName);
            }            

            return generatedType;
        }

        private bool IsRevisionIdProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanRead && !propertyInfo.CanWrite && propertyInfo.GetCustomAttributes(typeof(RevisionIdAttribute), false).Length == 1 && propertyInfo.PropertyType == typeof(Guid);
        }

        private bool IsDataProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanRead && propertyInfo.CanWrite;
        }

        private Type GenerateCollectionProxyType(Type collectionType)
        {
            Type[] elementType = collectionType.GetGenericArguments();
            if (elementType.Length != 1)
            {
                throw new ArgumentException("Collection type should specify element type.");
            }

            Type baseListType = null;

            bool isSet = collectionType.GetGenericTypeDefinition().Equals(typeof(IScalarSet<>));
            bool isOrdered = collectionType.GetGenericTypeDefinition().Equals(typeof(IOrderedCollection<>));

            if (!typesService.IsSealedType(elementType[0]))
            {
                if (!isSet)
                {
                    if (isOrdered)
                    {
                        baseListType = typeof(OrderedCollectionProxy<>);
                    }
                    else
                    {
                        baseListType = typeof(CollectionProxy<>);
                    }
                }
                else
                {
                    baseListType = typeof(SetCollectionProxy<>);
                }
            }
            else
            {
                if (!isSet)
                {
                    if (isOrdered)
                    {
                        baseListType = typeof(OrderedCollectionProxySealed<>);
                    }
                    else
                    {
                        baseListType = typeof(CollectionProxySealed<>);
                    }
                }
                else
                {
                    baseListType = typeof(SetCollectionProxySealed<>);
                }
            }

            return baseListType.MakeGenericType(elementType);
        }

        private Type GenerateDictionaryProxyType(Type dictionaryType)
        {
            Type[] elementType = dictionaryType.GetGenericArguments();
            if (elementType.Length != 2)
            {
                throw new ArgumentException("Dictionary type should specify key and element type.");
            }

            Type baseDictionaryType = null;

            if (!typesService.IsSealedType(elementType[0]))
            {
                baseDictionaryType = typeof(DictionaryProxy<,>);
            }
            else
            {
                baseDictionaryType = typeof(DictionaryProxySealed<,>);
            }

            return baseDictionaryType.MakeGenericType(elementType);
        }

        /// <summary>
        /// Generates entity proxy types for given entity interfaces
        /// </summary>
        /// <param name="types">Interface types, they must be registered previously with the types service</param>
        /// <param name="saveAssemblyToDisk">Defines if assembly should be saved to disk</param>
        /// <param name="assemblyFileName">If assembly saving is on, defines path and file name of the generated assembly</param>
        /// <returns>Mapping between interface types and generated proxy types</returns>
        public Dictionary<Type, Type> GenerateProxyTypes(IEnumerable<Type> types, bool saveAssemblyToDisk, string assemblyFileName)
        {
            Dictionary<Type, Type> mapping = new Dictionary<Type,Type>();

            foreach (var type in types)
            {
                if (type.IsInterface)
                {
                    Type generatedType = GenerateProxyType(type, saveAssemblyToDisk, assemblyFileName);
                    mapping.Add(type, generatedType);
                }
            }

            return mapping;
        }                
    }
}
