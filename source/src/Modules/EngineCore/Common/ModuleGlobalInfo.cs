﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testflow.Modules;
using Testflow.Utility.I18nUtil;

namespace Testflow.EngineCore.Common
{
    internal class ModuleGlobalInfo : IDisposable
    {
        public IModuleConfigData ConfigData { get; set; }

        public I18N I18N { get; }

        public ILogService LogService { get; }

        public ModuleGlobalInfo(IModuleConfigData configData)
        {
            TestflowRunner testflowRunner = TestflowRunner.GetInstance();
            this.I18N = I18N.GetInstance(Constants.I18nName);
            this.LogService = testflowRunner.LogService;
            this.ConfigData = configData;
        }

        public void Dispose()
        {
        }
    }
}