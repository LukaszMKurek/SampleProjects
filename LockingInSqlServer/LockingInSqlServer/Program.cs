using LockingInSqlServer.Scenarios;

namespace LockingInSqlServer
{
    internal static class Program
    {
        static async Task Main()
        {
            //await LockingProblems.Case1();
            //await LockingProblems.Case2();

            await LockingCosts.Run();

            //await DeadlocksScenarios.Scenario1();
            //await DeadlocksScenarios.Scenario2();
            //await DeadlocksScenarios.Scenario3();
        }
    }
}