using System;
using System.Collections.Generic;
using System.Text;
using Execom.IOG.TypeVisual;

namespace Execom.IOG.TypeVisual
{
    public class TypeVisualUtilities
    {

        /// <summary>
        /// Checks if the string name type is an collection (ICollection or IScalarSet or IOrderedCollection or IIndexedCollection) or if it is a Dictionary (IDictionary)
        /// <para>If it is a collection or a dictionary, the out parameter 'collectionValue' will be the name of the type used as the value in the collection or dictionary. Otherwise it will be assigned to 'null'.</para>
        /// <para>If it is a dictionary, the out parameter 'collectionKey' will be the name of the type used as a key in the dictionary. Otherwise it will be assigned to 'null'.</para>
        /// <para>If the name type is neither a collection nor a dictonary, the method will return 'NotACollection', and the out parameters will be assigned to 'null'.</para>
        /// </summary>
        /// <param name="typeName">Name of the type that is being checked.</param>
        /// <param name="collectionKey">If the typeName is a dictionary, returns the name of the type used as the key in the dictionary</param>
        /// <param name="collectionValue">If the typeName is a collection or a dictionary, returns the name of the type used as the value</param>
        /// <returns>Type of collection or dictionary, or NotACollection if it's neither.</returns>
        public static PropertyCollectionType CheckIfCollectionOrDictionary(string typeName, out string collectionKey, out string collectionValue)
        {
            collectionKey = null;
            collectionValue = null;
            if (typeName.StartsWith("ICollection<") || typeName.StartsWith("IIndexedCollection<")
                                || typeName.StartsWith("IOrderedCollection<") || typeName.StartsWith("IScalarSet<"))
            {
                collectionValue = typeName.Substring(typeName.IndexOf("<") + 1,
                    typeName.LastIndexOf(">") - typeName.IndexOf("<") - 1);
                string collectionName = typeName.Substring(0,typeName.IndexOf("<"));
                foreach (PropertyCollectionType enumValue in Enum.GetValues(typeof(PropertyCollectionType)))
                {
                    if (enumValue.ToString().Equals(collectionName))
                    {
                        return enumValue;
                    }
                }
                throw new Exception("No Collection name found in enumerations. This should be a dead code.");
            }
            else if (typeName.StartsWith("IDictionary<"))
            {
                collectionKey = typeName.Substring(typeName.IndexOf("<") + 1,
                    typeName.IndexOf(",") - typeName.IndexOf("<") - 1);
                collectionValue = typeName.Substring(typeName.IndexOf(",") + 1,
                    typeName.IndexOf(">") - typeName.IndexOf(",") - 1);
                return PropertyCollectionType.IDictionary;
            }

            return PropertyCollectionType.NotACollection;
        }
        

        /// <summary>
        /// Returns a collection of children and parent types of each type. This information is stored in IDictionaries, where the key is a Type, and the value is a collection of either children or parents types for the type that is the key.
        /// <para>Children of a type are all types that are referenced by the type in question.</para>
        /// <para>Parents of a type are all types that reference the type in question</para>
        /// </summary>
        /// <param name="typeUnits">Collection of types from which the information about child and parent types are built.</param>
        /// <param name="childrenDictionary">Dictionary of children types for the type represented by the key.</param>
        /// <param name="parentsDictionary">Dictionary of parent types for the type represented by the key.</param>
        public static void GetChildrenAndParentsDictonaryOfTypes(ICollection<TypeVisualUnit> typeUnits,
                            out IDictionary<TypeVisualUnit, ICollection<TypeVisualUnit>> childrenDictionary,
                                out IDictionary<TypeVisualUnit, ICollection<TypeVisualUnit>> parentsDictionary)
        {
            childrenDictionary = new Dictionary<TypeVisualUnit, ICollection<TypeVisualUnit>>();
            parentsDictionary = new Dictionary<TypeVisualUnit, ICollection<TypeVisualUnit>>();
            var typeUnitsDictionary = new Dictionary<string, TypeVisualUnit>();
            foreach (TypeVisualUnit type in typeUnits)
            {
                try
                {
                    typeUnitsDictionary.Add(type.Name, type);
                    childrenDictionary.Add(type, new List<TypeVisualUnit>());
                    parentsDictionary.Add(type, new List<TypeVisualUnit>());
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Duplicate types in the types collection.");
                }
            }
            foreach (TypeVisualUnit type in typeUnitsDictionary.Values)
            {
                foreach (TypeVisualProperty property in type.NonScalarProperties)
                {
                    string propertyType = property.Type;
                    TypeVisualUnit child = typeUnitsDictionary[propertyType];
                    if (!childrenDictionary[type].Contains(child))
                        childrenDictionary[type].Add(child);
                    if (!parentsDictionary[child].Contains(type))
                        parentsDictionary[child].Add(type);
                }

            }

        }

        /// <summary>
        /// Extracts and returns the type name from the Assembly Qualified Name of a type. The returned name consists of only the type name, not the namespaces it belongs to.
        /// <para>Successfully returns types with generic arguments by recursively calling itself.</para>
        /// <para>This method is used in cases where the assembly of certain types are not available.</para>
        /// </summary>
        /// <param name="assemblyName">The Assembly Qualified name</param>
        /// <returns>The type name consisting only of the name of the type, not the namespaces which it belongs to.</returns>
        public static string GetTypeNameFromAssemblyName(string assemblyName)
        {

            String typeName = "";
            if (assemblyName.Contains("`"))
            {
                int numberOfArguments = Int32.Parse(assemblyName.Substring(assemblyName.IndexOf('`') + 1, 1));
                int genericArgumentsStringLenght = assemblyName.LastIndexOf(']') - assemblyName.IndexOf('[');
                string genericArguments = assemblyName.Substring(assemblyName.IndexOf('[') + 1, genericArgumentsStringLenght - 1);
                List<int> openBracketsIndexes = new List<int>();
                List<int> closedBracketsIndexes = new List<int>();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    if (genericArguments[i] == '[')
                        openBracketsIndexes.Add(i);
                    if (genericArguments[i] == ']')
                        closedBracketsIndexes.Add(i);
                }
                string[] genericArgumentsArray = new string[numberOfArguments];
                int iGenericArgument = 0;
                int openBracket = openBracketsIndexes[0];
                for (int i = 0; i < openBracketsIndexes.Count; i++)
                {
                    if (i != openBracketsIndexes.Count - 1)
                    {
                        if (openBracketsIndexes[i + 1] > closedBracketsIndexes[i])
                        {
                            genericArgumentsArray[iGenericArgument] = genericArguments.Substring(openBracket + 1, closedBracketsIndexes[i] - openBracket - 1);
                            iGenericArgument++;
                            openBracket = openBracketsIndexes[i + 1];
                        }
                    }
                    else
                    {
                        genericArgumentsArray[iGenericArgument] = genericArguments.Substring(openBracket + 1, closedBracketsIndexes[i] - openBracket - 1);
                        iGenericArgument++;
                    }
                }
                if (iGenericArgument != numberOfArguments)
                    throw new Exception("Invalid number of found arguments.");


                String typeNameBeforeGenericArguments = assemblyName.Substring(0, assemblyName.IndexOf('`'));
                typeName = typeNameBeforeGenericArguments.Substring(typeNameBeforeGenericArguments.LastIndexOf('.') + 1);
                typeName += "<";
                for (int index = 0; index < genericArgumentsArray.Length; index++)
                {
                    if (index != 0)
                        typeName += ",";
                    typeName += GetTypeNameFromAssemblyName(genericArgumentsArray[index]);
                }
                typeName += ">";
            }
            else
            {
                int indexComma = assemblyName.IndexOf(',');
                String typeFullName = assemblyName.Substring(0, indexComma);
                int indexDot = typeFullName.LastIndexOf('.');
                typeName = typeFullName.Substring(indexDot + 1);
            }
            if (typeName.Contains("+"))
            {
                int indexPlus = typeName.LastIndexOf("+");
                typeName = typeName.Substring(indexPlus + 1);
            }
            return typeName;
        }

    }
}
