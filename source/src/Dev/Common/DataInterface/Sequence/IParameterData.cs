﻿using System;

namespace Testflow.DataInterface.Sequence
{
    public interface IParameterData
    {
        /// <summary>
        /// 当前ParameterData在ParametorCollection中的索引
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// 参数的值，如果是变量，则值为IVariableData
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// 参数所在配置集
        /// </summary>
        IAssemblyDescription Assembly { get; set; }

        /// <summary>
        /// 当前的值是变量还是值
        /// </summary>
        ParameterType ParameterType { get; set; }

        /// <summary>
        /// 该参数的Type类型
        /// </summary>
        Type Type { get; set; }
    }
}