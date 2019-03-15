﻿using System.Threading;
using Testflow.Common;
using Testflow.EngineCore.Common;
using Testflow.EngineCore.Message.Messages;
using Testflow.Utility.MessageUtil;

namespace Testflow.EngineCore.Message
{
    internal class SyncMsgTransceiver : MessageTransceiver
    {
        private Thread _receiveThread;
        private CancellationTokenSource _cancellation;

        public SyncMsgTransceiver(ModuleGlobalInfo globalInfo) : base(globalInfo)
        {
        }

        protected override void Start()
        {
            _receiveThread = new Thread(SynchronousReceive)
            {
                IsBackground = true
            };
            _cancellation = new CancellationTokenSource();
            _receiveThread.Start();
        }

        protected override void Stop()
        {
            _cancellation.Cancel();
            if (null != _receiveThread && ThreadState.Running == _receiveThread.ThreadState && !_receiveThread.Join(Constants.OperationTimeout))
            {
                _receiveThread.Abort();
                GlobalInfo.LogService.Print(LogLevel.Warn, CommonConst.PlatformLogSession,
                    "Message receive thread stopped abnormally");
            }
        }

        protected override void SendMessage(MessageBase message)
        {
            DownLinkMessenger.Send(message, FormatterType);
        }

        private void SynchronousReceive(object state)
        {
            while (!_cancellation.IsCancellationRequested)
            {
                IMessage message = UpLinkMessenger.Receive();
                IMessageConsumer consumer = GetConsumer(message);
                consumer.HandleMessage(message);
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            Stop();
        }
    }
}