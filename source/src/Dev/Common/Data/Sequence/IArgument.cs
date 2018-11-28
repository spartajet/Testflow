﻿using System;
using System.Xml.Serialization;
using Testflow.Data.Description;

namespace Testflow.Data.Sequence
{
    /// <summary>
    /// 参数类，描述FunctionData的接口
    /// </summary>
    public interface IArgument
    {
        /// <summary>
        /// 参数类
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 参数类的Type对象的索引号
        /// </summary>
        int TypeIndex { get; set; }

        /// <summary>
        /// 参数的修饰符
        /// </summary>
        ArgumentModifier Modifier { get; set; }

        /// <summary>
        /// 参数类的类型
        /// </summary>
        [XmlIgnore]
        VariableType VariableType { get; set; }
    }
}