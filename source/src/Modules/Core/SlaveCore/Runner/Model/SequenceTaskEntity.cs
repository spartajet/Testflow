﻿using System;
using System.Reflection;
using System.Threading;
using Testflow.Usr;
using Testflow.CoreCommon.Data;
using Testflow.CoreCommon.Messages;
using Testflow.Data.Sequence;
using Testflow.Runtime;
using Testflow.Runtime.Data;
using Testflow.SlaveCore.Common;
using Testflow.SlaveCore.Data;

namespace Testflow.SlaveCore.Runner.Model
{
    internal class SequenceTaskEntity
    {
        private readonly ISequence _sequence;
        private readonly SlaveContext _context;

        private StepTaskEntityBase _stepEntityRoot;

        public SequenceTaskEntity(ISequence sequence, SlaveContext context)
        {
            this._sequence = sequence;
            this._context = context;
            this.State = RuntimeState.Idle;
        }

        public int Index => _sequence.Index;

        private int _runtimeState;

        /// <summary>
        /// 全局状态。配置规则：哪里最早获知全局状态变更就在哪里更新。
        /// </summary>
        public RuntimeState State
        {
            get { return (RuntimeState)_runtimeState; }
            set
            {
                // 如果当前状态大于等于待更新状态则不执行。因为在一次运行的实例中，状态的迁移是单向的。
                if ((int)value <= _runtimeState)
                {
                    return;
                }
                Thread.VolatileWrite(ref _runtimeState, (int)value);
            }
        }

        public void Generate()
        {
            this.State = RuntimeState.TestGen;

            _stepEntityRoot = ModuleUtils.CreateStepModelChain(_sequence.Steps, _context, _sequence.Index);
            if (null == _stepEntityRoot)
            {
                return;
            }
            _stepEntityRoot.GenerateInvokeInfo();
            _stepEntityRoot.InitializeParamsValues();

            this.State = RuntimeState.StartIdle;
            // 添加当前根节点到stepModel管理中
            StepTaskEntityBase.AddSequenceEntrance(_stepEntityRoot);
        }

        public void Invoke()
        {
            SequenceFailedInfo failedInfo = null;
            StepResult lastStepResult = StepResult.NotAvailable;
            StatusReportType finalReportType = StatusReportType.Failed;
            try
            {
                this.State = RuntimeState.Running;
                SequenceStatusInfo startStatusInfo = new SequenceStatusInfo(Index, _stepEntityRoot.GetStack(),
                    StatusReportType.Start, StepResult.NotAvailable);
                _context.StatusQueue.Enqueue(startStatusInfo);

                _stepEntityRoot.Invoke();

                this.State = RuntimeState.Success;
                finalReportType = StatusReportType.Over;
                lastStepResult = StepResult.Pass;
            }
            catch (TaskFailedException ex)
            {
                this.State = RuntimeState.Failed;
                finalReportType = StatusReportType.Failed;
                lastStepResult = StepResult.Failed;
                failedInfo = new SequenceFailedInfo(ex, ex.FailedType);
                _context.LogSession.Print(LogLevel.Info, Index, "Step force failed.");
            }
            catch (TestflowAssertException ex)
            {
                this.State = RuntimeState.Failed;
                finalReportType = StatusReportType.Failed;
                lastStepResult = StepResult.Failed;
                failedInfo = new SequenceFailedInfo(ex, FailedType.AssertionFailed);
                _context.LogSession.Print(LogLevel.Fatal, Index, "Assert exception catched.");
            }
            catch (ThreadAbortException ex)
            {
                this.State = RuntimeState.Abort;
                finalReportType = StatusReportType.Error;
                lastStepResult = StepResult.Abort;
                failedInfo = new SequenceFailedInfo(ex, FailedType.Abort);
                _context.LogSession.Print(LogLevel.Warn, Index, $"Sequence {Index} execution aborted");
            }
            catch (TestflowException ex)
            {
                this.State = RuntimeState.Error;
                finalReportType = StatusReportType.Error;
                lastStepResult = StepResult.Error;
                failedInfo = new SequenceFailedInfo(ex, FailedType.RuntimeError);
                _context.LogSession.Print(LogLevel.Error, Index, ex, "Inner exception catched.");
            }
            catch (TargetInvocationException ex)
            {
                this.State = RuntimeState.Failed;
                finalReportType = StatusReportType.Failed;
                lastStepResult = StepResult.Failed;
                failedInfo = new SequenceFailedInfo(ex.InnerException, FailedType.TargetError);
                _context.LogSession.Print(LogLevel.Error, Index, ex, "Invocation exception catched.");
            }
            catch (Exception ex)
            {
                this.State = RuntimeState.Error;
                finalReportType = StatusReportType.Error;
                lastStepResult = StepResult.Error;
                failedInfo = new SequenceFailedInfo(ex, FailedType.RuntimeError);
                _context.LogSession.Print(LogLevel.Error, Index, ex, "Runtime exception catched.");
            }
            finally
            {
                StepTaskEntityBase currentStep = StepTaskEntityBase.GetCurrentStep(Index);
                // 如果序列未成功则发送失败事件
                if (this.State != RuntimeState.Success)
                {
                    currentStep.SetStatusAndSendErrorEvent(lastStepResult);
                }
                // 发送结束事件，包括所有的ReturnData信息
                SequenceStatusInfo overStatusInfo = new SequenceStatusInfo(Index,
                    currentStep.GetStack(), finalReportType, StepResult.Over, failedInfo);
                overStatusInfo.WatchDatas = _context.VariableMapper.GetReturnDataValues(_sequence);
                this._context.StatusQueue.Enqueue(overStatusInfo);

                _context.VariableMapper.ClearSequenceVariables(_sequence);
                this._stepEntityRoot = null;
                // 将失败步骤职责链以后的step标记为null
                currentStep.NextStep = null;
            }
        }

        public void FillStatusInfo(StatusMessage message)
        {
            // 如果是外部调用且该序列已经执行结束或者未开始或者message中已经有了当前序列的信息，则说明该序列在前面的消息中已经标记结束，直接返回。
            if (message.InterestedSequence.Contains(this.Index) || this.State > RuntimeState.AbortRequested ||
                this.State == RuntimeState.StartIdle)
            {
                return;
            }
            message.SequenceStates.Add(this.State);
            StepTaskEntityBase currentStep = StepTaskEntityBase.GetCurrentStep(Index);
            currentStep.FillStatusInfo(message);
        }

        public void FillStatusInfo(StatusMessage message, string errorInfo)
        {
            // 如果是外部调用且该序列已经执行结束或者message中已经有了当前序列的信息，则说明该序列在前面的消息中已经标记结束，直接返回。
            if (message.InterestedSequence.Contains(this.Index) || this.State > RuntimeState.AbortRequested)
            {
                return;
            }
            message.Stacks.Add(StepTaskEntityBase.GetCurrentStep(Index).GetStack());
            message.SequenceStates.Add(this.State);
            message.Results.Add(StepResult.NotAvailable);
        }
    }
}