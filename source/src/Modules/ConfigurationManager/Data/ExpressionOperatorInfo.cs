﻿using System.Xml.Serialization;
using Testflow.Data.Expression;

namespace Testflow.ConfigurationManager.Data
{
    /// <summary>
    /// 表达式操作符配置项
    /// </summary>
    public class ExpressionOperatorInfo : IExpressionOperatorInfo
    {
        /// <summary>
        /// 操作符名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 操作符描述信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 操作符样式
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// 操作符格式化字符串，0对应Source，1对应Target
        /// </summary>
        public string FormatString { get; set; }

        /// <summary>
        /// 运算符计算所在的程序集
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// 运算法的计算类，该类必须继承自Testflow.Usr.Expression.IExpressionFunction
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 计算的方法
        /// </summary>
        [XmlIgnore]
        public IExpressionCalculator CalculationClass { get; set; }
    }
}