using System;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;
using Microsoft.Extensions.DependencyInjection;

namespace XTI_ConsoleApp.Tests
{
    public sealed class TestAppKey
    {
        public static readonly AppKey Key = new AppKey("Test", AppType.Values.Service);
    }
    public sealed class TestApiFactory : AppApiFactory
    {
        private readonly IServiceProvider services;

        public TestApiFactory(IServiceProvider services)
        {
            this.services = services;
        }

        protected override IAppApi _Create(IAppApiUser user)
            =>
                new TestApi
                (
                    user,
                    services.GetService<Counter>(),
                    services.GetService<TestOptions>()
                );
    }
    public sealed class TestApi : AppApiWrapper
    {
        public TestApi(IAppApiUser user, Counter counter, TestOptions options)
            : base
            (
                new AppApi
                (
                    TestAppKey.Key,
                    user,
                    ResourceAccess.AllowAuthenticated()
                )
            )
        {
            Test = new TestGroup(source.AddGroup(nameof(Test)), counter, options);
        }

        public TestGroup Test { get; }
    }

    public sealed class TestGroup : AppApiGroupWrapper
    {
        public TestGroup(AppApiGroup source, Counter counter, TestOptions options)
            : base(source)
        {
            var actions = new AppApiActionFactory(source);
            RunContinuously = source.AddAction
            (
                actions.Action
                (
                    nameof(RunContinuously),
                    () => new RunContinuouslyAction(counter)
                )
            );
            RunUntilSuccess = source.AddAction
            (
                actions.Action
                (
                    nameof(RunUntilSuccess),
                    () => new RunUntilSuccessAction(counter)
                )
            );
            OptionalRun = source.AddAction
            (
                actions.Action
                (
                    nameof(OptionalRun),
                    () => new OptionalRunAction(counter, options)
                )
            );
        }

        public AppApiAction<EmptyRequest, EmptyActionResult> RunContinuously { get; }
        public AppApiAction<EmptyRequest, EmptyActionResult> RunUntilSuccess { get; }
        public AppApiAction<EmptyRequest, EmptyActionResult> OptionalRun { get; }
    }

    public sealed class RunContinuouslyAction : AppAction<EmptyRequest, EmptyActionResult>
    {
        private readonly Counter counter;

        public RunContinuouslyAction(Counter counter)
        {
            this.counter = counter;
        }

        public Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            counter.IncrementContinuous();
            return Task.FromResult(new EmptyActionResult());
        }
    }

    public sealed class RunUntilSuccessAction : AppAction<EmptyRequest, EmptyActionResult>
    {
        private readonly Counter counter;

        public RunUntilSuccessAction(Counter counter)
        {
            this.counter = counter;
        }

        public Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            counter.IncrementUntilSuccess();
            return Task.FromResult(new EmptyActionResult());
        }
    }

    public sealed class TestOptions
    {
        public bool IsOptional { get; set; }
        public bool ThrowException { get; set; }
    }

    public sealed class OptionalRunAction : OptionalAction<EmptyRequest, EmptyActionResult>
    {
        private readonly Counter counter;
        private readonly TestOptions options;

        public OptionalRunAction(Counter counter, TestOptions options)
        {
            this.counter = counter;
            this.options = options;
        }

        public Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            counter.IncrementOptional();
            if (options.ThrowException)
            {
                throw new Exception("Testing");
            }
            return Task.FromResult(new EmptyActionResult());
        }

        public Task<bool> IsOptional()
        {
            return Task.FromResult(options.IsOptional);
        }
    }
}
