﻿using System;
using System.Threading;
using Testflow.Usr;
using Testflow.CoreCommon.Common;
using Testflow.CoreCommon.Data;
using Testflow.CoreCommon.Messages;
using Testflow.Runtime;
using Testflow.Runtime.Data;
using Testflow.SlaveCore.Common;
using Testflow.SlaveCore.Data;
using Testflow.SlaveCore.Runner.Model;

namespace Testflow.SlaveCore.SlaveFlowControl
{
    internal class RunSingleSequenceFlowTask : SlaveFlowTaskBase
    {
        private readonly int _sequenceIndex;
        public RunSingleSequenceFlowTask(int sequenceIndex,SlaveContext context) : base(context)
        {
            _sequenceIndex = sequenceIndex;
        }

        protected override void FlowTaskAction()
        {
            SendStartMessage();

            // 打印状态日志
            Context.LogSession.Print(LogLevel.Info, Context.SessionId, $"Start run sequence {_sequenceIndex}.");

            Context.State = RuntimeState.Running;

            SessionTaskEntity sessionTaskEntity = Context.SessionTaskEntity;

            try
            {
                sessionTaskEntity.InvokeSetUp();
                RuntimeState setUpState = sessionTaskEntity.GetSequenceTaskState(CommonConst.SetupIndex);
                // 如果SetUp执行失败，则执行TearDown，且配置所有序列为失败状态，并发送所有序列都失败的信息
                if (CoreUtils.IsFailed(setUpState))
                {
                    // 打印状态日志
                    Context.LogSession.Print(LogLevel.Error, Context.SessionId, "Run setup failed.");
                    for (int i = 0; i < sessionTaskEntity.SequenceCount; i++)
                    {
                        SequenceTaskEntity sequenceTaskEntity = sessionTaskEntity.GetSequenceTaskEntity(i);
                        sequenceTaskEntity.State = RuntimeState.Failed;

                        FailedInfo failedInfo = new FailedInfo(Context.I18N.GetStr("SetUpFailed"), FailedType.SetUpFailed);
                        CallStack sequenceStack = ModuleUtils.GetSequenceStack(i, sequenceTaskEntity.RootCoroutineId);
                        SequenceStatusInfo statusInfo = new SequenceStatusInfo(i, sequenceStack, 
                            StatusReportType.Failed, setUpState, StepResult.NotAvailable, failedInfo)
                        {
                            ExecutionTime = DateTime.Now,
                            ExecutionTicks = -1,
                            CoroutineId = sequenceTaskEntity.RootCoroutineId
                        };
                        Context.StatusQueue.Enqueue(statusInfo);
                    }
                }
                else
                {
                    sessionTaskEntity.InvokeSequence(_sequenceIndex);
                }
            }
            finally
            {
                sessionTaskEntity.InvokeTearDown();

                Context.State = RuntimeState.Over;
                this.Next = null;

                SendOverMessage();

                // 打印状态日志
                Context.LogSession.Print(LogLevel.Info, Context.SessionId, "Run single sequence over.");
            }
        }

        protected override void TaskErrorAction(Exception ex)
        {
            StatusMessage errorMessage = new StatusMessage(MessageNames.ErrorStatusName, Context.State, Context.SessionId)
            {
                ExceptionInfo = new FailedInfo(ex, FailedType.RuntimeError),
                Index = Context.MsgIndex
            };
            Context.SessionTaskEntity.FillSequenceInfo(errorMessage, Context.I18N.GetStr("RuntimeError"));
            if (Context.GetProperty<bool>("EnablePerformanceMonitor"))
            {
                ModuleUtils.FillPerformance(errorMessage);
            }
            errorMessage.WatchData = Context.VariableMapper.GetReturnDataValues();
            Context.UplinkMsgProcessor.SendMessage(errorMessage, true);
        }

        public override MessageBase GetHeartBeatMessage()
        {
            StatusMessage statusMessage = new StatusMessage(MessageNames.HeartBeatStatusName, Context.State, Context.SessionId)
            {
                Index = Context.MsgIndex
            };
            SessionTaskEntity sessionTaskEntity = Context.SessionTaskEntity;
            sessionTaskEntity.FillSequenceInfo(statusMessage);
            if (Context.GetProperty<bool>("EnablePerformanceMonitor"))
            {
                ModuleUtils.FillPerformance(statusMessage);
            }
            return statusMessage;
        }

        public override SlaveFlowTaskBase Next { get; protected set; }
        public override void Dispose()
        {
            // ignore
        }
    }
}