﻿using System.Collections.Generic;

namespace Testflow.DataInterface.Sequence
{
    /// <summary>
    /// 每个Sequence的参数配置
    /// </summary>
    public interface ISequenceParameter
    {
        /// <summary>
        /// 该参数Sequence在当前SequenceGroup的索引号
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// 所有Step的Parameters
        /// </summary>
        IList<ISequenceStepParameter> StepParameters { get; set; }

    }
    
}