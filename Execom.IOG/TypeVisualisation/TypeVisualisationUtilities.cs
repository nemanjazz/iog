using System;
using System.Collections.Generic;
using System.Text;

namespace Execom.IOG.TypeVisualisation
{
    public class TypeVisualisationUtilities
    {
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
