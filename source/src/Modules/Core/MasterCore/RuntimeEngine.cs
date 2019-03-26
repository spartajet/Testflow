﻿using System;
using System.Threading;
using Testflow.Common;
using Testflow.CoreCommon.Common;
using Testflow.CoreCommon.Messages;
using Testflow.Data.Sequence;
using Testflow.MasterCore.TestMaintain;
using Testflow.MasterCore.Common;
using Testflow.MasterCore.Core;
using Testflow.MasterCore.Message;
using Testflow.MasterCore.StatusManage;
using Testflow.MasterCore.SyncManage;
using Testflow.Modules;
using Testflow.Runtime;

namespace Testflow.MasterCore
{
    /// <summary>
    /// 运行时引擎的实例
    /// </summary>
    internal class RuntimeEngine : IDisposable
    {
        private readonly ModuleGlobalInfo _globalInfo;


        private readonly RuntimeStatusManager _statusManager;
        private readonly SynchronousManager _syncManager;
        private readonly EngineFlowController _controller;

        public RuntimeEngine(IModuleConfigData configData)
        {
            _globalInfo = new ModuleGlobalInfo(configData);
            bool isSyncMessenger = _globalInfo.ConfigData.GetProperty<bool>("EngineSyncMessenger");
            // TODO 暂时写死使用LocalTestMaintainer
            // 初始化消息收发器
            MessageTransceiver messageTransceiver = MessageTransceiver.GetTransceiver(_globalInfo, isSyncMessenger);
            _globalInfo.RuntimeInitialize(messageTransceiver);

            _controller = new EngineFlowController(_globalInfo);
            _statusManager = new RuntimeStatusManager(_globalInfo);
            _syncManager = new SynchronousManager(_globalInfo);

            RuntimeStateMachine stateMachine = new RuntimeStateMachine();
            _globalInfo.StateMachine = stateMachine;
            
            RegisterMessageHandler();
        }

        public RuntimeStatusManager StatusManager => _statusManager;
        public SynchronousManager SyncManager => _syncManager;
        public EngineFlowController Controller => _controller;

        public RuntimeType RuntimeType => _globalInfo.ConfigData.GetProperty<RuntimeType>("RuntimeType");

        private void RegisterMessageHandler()
        {
            //RmtGen消息由远端接收，所以无需分发
            _controller.RegisterMessageHandler();
            _globalInfo.MessageTransceiver.AddConsumer(MessageType.Status.ToString(), _statusManager);
            _globalInfo.MessageTransceiver.AddConsumer(MessageType.TestGen.ToString(), _statusManager);
            _globalInfo.MessageTransceiver.AddConsumer(MessageType.Sync.ToString(), _syncManager);
        }

        public void Initialize(ISequenceFlowContainer sequenceContainer)
        {
            _globalInfo.StateMachine.State = RuntimeState.Idle;
            _controller.Initialize(sequenceContainer);

            _statusManager.Initialize(sequenceContainer);

            _globalInfo.StateMachine.StateAbort += Stop;
            _globalInfo.StateMachine.StateError += Stop;
            _globalInfo.StateMachine.StateCollapsed += Stop;
            _globalInfo.StateMachine.StateOver += Stop;
        }

        public void Start()
        {
            _statusManager.Start();
            _syncManager.Start();
            _controller.Start();
        }

        public void Stop()
        {
            _syncManager.Stop();
            _controller.Stop();
            _statusManager.Stop();
        }

        public void FreeTests()
        {
            _controller.TestMaintainer.FreeHosts();
        }

        public void Dispose()
        {
            _controller.Dispose();
            _syncManager.Dispose();
            _statusManager.Dispose();
        }

    }
}