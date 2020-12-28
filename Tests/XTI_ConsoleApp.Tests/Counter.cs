namespace XTI_ConsoleApp.Tests
{
    public sealed class Counter
    {
        public int ContinuousValue { get; private set; }

        public void IncrementContinuous()
        {
            ContinuousValue++;
        }

        public int OptionalValue { get; private set; }

        public void IncrementOptional()
        {
            OptionalValue++;
        }
    }
}
