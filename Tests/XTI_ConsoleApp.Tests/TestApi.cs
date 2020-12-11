using System.Threading.Tasks;
using XTI_App;
using XTI_App.Api;

namespace XTI_ConsoleApp.Tests
{
    public sealed class Counter
    {
        public int Value { get; private set; }

        public void Increment()
        {
            Value++;
        }
    }
    public sealed class TestAppKey
    {
        public static readonly AppKey Key = new AppKey("Test", AppType.Values.Service);
    }
    public sealed class TestApi : AppApi
    {
        public TestApi(IAppApiUser user, Counter counter)
            : base(TestAppKey.Key, AppVersionKey.Current, user, ResourceAccess.AllowAuthenticated())
        {
            Test = AddGroup(u => new TestGroup(this, u, counter));
        }

        public TestGroup Test { get; }
    }

    public sealed class TestGroup : AppApiGroup
    {
        public TestGroup(AppApi api, IAppApiUser user, Counter counter)
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
                () => new RunAction(counter)
            );
        }

        public AppApiAction<EmptyRequest, EmptyActionResult> Run { get; }
    }

    public sealed class RunAction : AppAction<EmptyRequest, EmptyActionResult>
    {
        private readonly Counter counter;

        public RunAction(Counter counter)
        {
            this.counter = counter;
        }

        public Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            counter.Increment();
            return Task.FromResult(new EmptyActionResult());
        }
    }

}
