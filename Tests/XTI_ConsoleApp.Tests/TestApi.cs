using System;
using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_ConsoleApp.Tests
{
    public sealed class TestAppKey
    {
        public static readonly AppKey Key = new AppKey("Test", AppType.Values.Service);
    }
    public sealed class TestApi : AppApi
    {
        public TestApi(IAppApiUser user, Counter counter, TestOptions options)
            : base(TestAppKey.Key, user, ResourceAccess.AllowAuthenticated())
        {
            Test = AddGroup(u => new TestGroup(this, u, counter, options));
        }

        public TestGroup Test { get; }
    }

    public sealed class TestGroup : AppApiGroup
    {
        public TestGroup(AppApi api, IAppApiUser user, Counter counter, TestOptions options)
            : base
            (
                api,
                new NameFromGroupClassName(nameof(TestGroup)).Value,
                ModifierCategoryName.Default,
                api.Access,
                user,
                (n, a, u) => new AppApiActionCollection(n, a, u)
            )
        {
            var actions = Actions<AppApiActionCollection>();
            Run = actions.Add
            (
                nameof(Run),
                () => new ContinuousRunAction(counter)
            );
            OptionalRun = actions.Add
            (
                nameof(OptionalRun),
                () => new OptionalRunAction(counter, options)
            );
        }

        public AppApiAction<EmptyRequest, EmptyActionResult> Run { get; }
        public AppApiAction<EmptyRequest, EmptyActionResult> OptionalRun { get; }
    }

    public sealed class ContinuousRunAction : AppAction<EmptyRequest, EmptyActionResult>
    {
        private readonly Counter counter;

        public ContinuousRunAction(Counter counter)
        {
            this.counter = counter;
        }

        public Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            counter.IncrementContinuous();
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
