﻿using MainDB.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_Schedule;
using XTI_TempLog.Fakes;

namespace XTI_ConsoleApp.Fakes
{
    public static class FakeExtensions
    {
        public static void AddFakeConsoleAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDataProtection();
            services.AddAppDbContextForInMemory();
            services.AddSingleton<Clock, FakeClock>();
            services.Configure<AppActionOptions>(configuration.GetSection(AppActionOptions.AppAction));
            services.AddFakeTempLogServices();
            services.AddHostedService<ConsoleAppWorker>();
        }
    }
}