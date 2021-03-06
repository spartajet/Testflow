﻿using Testflow.CoreCommon.Messages;
using Testflow.Utility.MessageUtil;

namespace Testflow.MasterCore.Message
{
    internal interface IMessageHandler
    {
        /// <summary>
        /// 同步处理消息
        /// </summary>
        bool HandleMessage(MessageBase message);

        /// <summary>
        /// 异步添加消息到待处理队列
        /// </summary>
        void AddToQueue(MessageBase message);
    }
}