﻿using Microsoft.Extensions.DependencyInjection;
using System;
using XTI_App.Api;
using XTI_TempLog;

namespace XTI_App.Hosting
{
    public sealed class ActionRunnerFactory : IActionRunnerFactory
    {
        private readonly IServiceProvider services;

        public ActionRunnerFactory(IServiceProvider services)
        {
            this.services = services;
        }

        public AppApi CreateAppApi() => services.GetService<AppApi>();

        public TempLogSession CreateTempLogSession() => services.GetService<TempLogSession>();

        public XtiPath CreateXtiPath() => services.GetService<XtiPath>();
    }
}
