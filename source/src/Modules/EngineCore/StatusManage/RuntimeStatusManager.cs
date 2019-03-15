﻿using Testflow.EngineCore.Common;
using Testflow.Utility.MessageUtil;
using IMessageConsumer = Testflow.EngineCore.Message.IMessageConsumer;

namespace Testflow.EngineCore.StatusManage
{
    /// <summary>
    /// 运行时所有测试的状态管理
    /// </summary>
    internal class RuntimeStatusManager : IMessageConsumer
    {
        private readonly ModuleGlobalInfo _globalInfo;

        public RuntimeStatusManager(ModuleGlobalInfo globalInfo)
        {
            _globalInfo = globalInfo;
        }

        public void HandleMessage(IMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void AddToQueue(IMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}