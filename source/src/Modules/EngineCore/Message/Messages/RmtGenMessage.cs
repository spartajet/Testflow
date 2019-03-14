﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Testflow.EngineCore.Common;

namespace Testflow.EngineCore.Message.Messages
{
    /// <summary>
    /// 远程运行器生成消息
    /// </summary>
    public class RmtGenMessage : MessageBase
    {
        public RmtGenMessage(string name, int id, RunnerType type, string sequence) : base(name, id, MessageType.RmtGen)
        {
            this.SequenceType = type;
            this.Sequence = sequence;
            this.Params = new Dictionary<string, string>(Constants.DefaultRuntimeSize);
        }

        /// <summary>
        /// 序列的类型
        /// </summary>
        public RunnerType SequenceType { get; set; }

        /// <summary>
        /// 测试序列数据序列化后的数据
        /// </summary>
        public string Sequence { get; set; }

        /// <summary>
        /// 额外参数配置
        /// </summary>
        public Dictionary<string, string> Params { get; }

        public RmtGenMessage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.SequenceType = (RunnerType) info.GetValue("SequenceType", typeof (RunnerType));
            this.Sequence = (string) info.GetValue("Sequence", typeof (string));
            this.Params = info.GetValue("Params", typeof (Dictionary<string, string>)) as Dictionary<string, string>;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SequenceType", SequenceType);
            info.AddValue("Sequence", Sequence);
            info.AddValue("Params", Params);
        }
    }
}