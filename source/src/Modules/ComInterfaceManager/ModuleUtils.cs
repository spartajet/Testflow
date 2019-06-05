﻿using System.Collections.Generic;
using System.Text;
using Testflow.ComInterfaceManager.Data;
using Testflow.Data;
using Testflow.Data.Description;
using Testflow.Modules;

namespace Testflow.ComInterfaceManager
{
    internal static class ModuleUtils
    {
        // 使用TypeDescription信息更新VariableTypes和Class中的ClassType信息
        public static void ValidateComDescription(ISequenceManager sequenceManager, ComInterfaceDescription description, 
            DescriptionCollections descriptionCollection)
        {
            int componentId = description.ComponentId;
            foreach (ITypeDescription typeDescription in description.TypeDescriptions)
            {
                ITypeData classType = GetTypeDataByDescription(sequenceManager, descriptionCollection, typeDescription);
                description.VariableTypes.Add(classType);
            }
            description.TypeDescriptions.Clear();
            description.TypeDescriptions = null;
            ((List<ITypeData>)description.VariableTypes).TrimExcess();

            foreach (ClassInterfaceDescription classDescription in description.Classes)
            {
                ITypeData classType = GetTypeDataByDescription(sequenceManager, descriptionCollection,
                    classDescription.ClassTypeDescription);
                classDescription.ClassType = classType;
                classDescription.ClassTypeDescription = null;
            }
            ((List<IClassInterfaceDescription>)description.Classes).TrimExcess();

            foreach (IClassInterfaceDescription classDescription in description.Classes)
            {
                foreach (IFuncInterfaceDescription functionDescription in classDescription.Functions)
                {
                    foreach (IArgumentDescription argumentDescription in functionDescription.Arguments)
                    {
                        InitializeArgumentType(sequenceManager, descriptionCollection, argumentDescription);
                    }
                    ((List<IArgumentDescription>) functionDescription.Arguments).TrimExcess();
                    if (null != functionDescription.Properties)
                    {
                        foreach (IArgumentDescription propertyDescription in functionDescription.Properties)
                        {
                            InitializeArgumentType(sequenceManager, descriptionCollection, propertyDescription);
                        }
                        ((List<IArgumentDescription>) functionDescription.Properties).TrimExcess();
                    }
                    functionDescription.ClassType = classDescription.ClassType;
                    if (null != functionDescription.Return)
                    {
                        InitializeArgumentType(sequenceManager, descriptionCollection, functionDescription.Return);
                    }
                }
            }
        }

        private static ITypeData GetTypeDataByDescription(ISequenceManager sequenceManager,
            DescriptionCollections descriptionCollection, ITypeDescription typeDescription)
        {
            string classFullName = GetFullName(typeDescription);
            ITypeData classType;
            if (!descriptionCollection.ContainsType(classFullName))
            {
                classType = sequenceManager.CreateTypeData(typeDescription);
                descriptionCollection.AddTypeData(classFullName, classType);
            }
            else
            {
                classType = descriptionCollection.GetTypeData(classFullName);
            }
            return classType;
        }

        private static void InitializeArgumentType(ISequenceManager sequenceManager,
            DescriptionCollections descriptionCollection, IArgumentDescription argumentDescription)
        {
            ArgumentDescription argDescription = (ArgumentDescription) argumentDescription;
            string fullName = GetFullName(argDescription.TypeDescription);
            if (descriptionCollection.ContainsType(fullName))
            {
                argDescription.Type = descriptionCollection.GetTypeData(fullName);
            }
            else
            {
                ITypeData typeData = sequenceManager.CreateTypeData(argDescription.TypeDescription);
                descriptionCollection.AddTypeData(fullName, typeData);
                argDescription.Type = typeData;
            }
            argDescription.TypeDescription = null;
        }

        public static string GetFullName(ITypeData typeData)
        {
            const string fullNameFormat = "{0}.{1}";
            return string.Format(fullNameFormat, typeData.Namespace, typeData.Name);
        }

        public static string GetFullName(ITypeDescription typeDescription)
        {
            const string fullNameFormat = "{0}.{1}";
            return string.Format(fullNameFormat, typeDescription.Namespace, typeDescription.Name);
        }

        public static string GetSignature(string className, FunctionInterfaceDescription funcDescription)
        {
            const string signatureFormat = "{0}.{1}({2})";
            StringBuilder paramStr = new StringBuilder(20);
            const string delim = ",";
            foreach (ArgumentDescription argument in funcDescription.Arguments)
            {
                paramStr.Append(argument.TypeDescription.Name).Append(delim);
            }
            if (paramStr.Length > 0)
            {
                paramStr.Remove(paramStr.Length - 1, 1);
            }
            return string.Format(signatureFormat, className, funcDescription.Name, paramStr);
        }
    }
}