﻿namespace Testflow.Data.Sequence
{
    /// <summary>
    /// 参数类型，值/变量引用
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// 不可用
        /// </summary>
        NotAvailable = -1,

        /// <summary>
        /// 参数是直接传值
        /// </summary>
        Value = 0,

        /// <summary>
        /// 参数是传的变量
        /// </summary>
        Variable = 1,

        /// <summary>
        /// 表达式类型
        /// </summary>
        Expression = 2
    }
}