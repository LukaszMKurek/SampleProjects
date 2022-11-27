using LockingInSqlServer.Utils;

namespace LockingInSqlServer.Scenarios;

public static class DeadlocksScenarios
{
    public static async Task Scenario1()
    {
        await ExampleDatabase1.CreateDatabase(enableReadCommittedSnapshot: false);

        var task1 = Benchmark.Test("update", sql: $$"""
                UPDATE [dbo].[Tab1] SET [Value] = @Value WHERE Id = @Id;
                """,
            parameters: i => new { Id = Random.Shared.Next(0, 2), Value = $"Val_updated_{i}" });

        var task2 = Benchmark.Test("select", sql: $$"""
                SELECT * FROM [dbo].[Tab1] x WHERE x.Value = @Value
                """,
            parameters: i => new { Value = $"Val_updated_{Random.Shared.Next(0, 2)}" });


        await Task.WhenAll(task1, task2);
    }

    public static async Task Scenario2()
    {
        await ExampleDatabase1.CreateDatabase(enableReadCommittedSnapshot: true);

        var task1 = Benchmark.Test("updates 1", sql: $$"""
                begin tran;
                UPDATE [dbo].[Tab2] SET [Value] = @Value WHERE Id = @Id2;
                UPDATE [dbo].[Tab1] SET [Value] = @Value WHERE Id = @Id1;
                commit tran;
                """,
            parameters: i => new { Id1 = Random.Shared.Next(0, 2), Id2 = Random.Shared.Next(2, 4), Value = $"Val_updated_{i}" });

        var task2 = Benchmark.Test("updates 2", sql: $$"""
                begin tran;
                UPDATE [dbo].[Tab1] SET [Value] = @Value WHERE Id = @Id1;
                UPDATE [dbo].[Tab2] SET [Value] = @Value WHERE Id = @Id2;
                commit tran;
                """,
            parameters: i => new { Id1 = Random.Shared.Next(0, 2), Id2 = Random.Shared.Next(2, 4), Value = $"Val_updated_{i}" });


        await Task.WhenAll(task1, task2);
    }

    public static async Task Scenario3()
    {
        await ExampleDatabase1.CreateDatabase(enableReadCommittedSnapshot: true);

        await ConnectionHelper.Execute($$"""
            CREATE VIEW [dbo].[View_X] WITH SCHEMABINDING AS
            SELECT t1.[Id], t1.[Value], t1.[Tab2Id], t2.[Value] as [Value2]
            FROM [dbo].[Tab1] t1
            INNER JOIN [dbo].[Tab2] t2 on t1.[Tab2Id] = t2.[Id]
            """, new {}, 10);

        await ConnectionHelper.Execute($$"""
            CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [dbo].[View_X] 
            (
                [Value2] ASC
            ) 
            """, new { }, 10);
      
        var task1 = Benchmark.Test("update", sql: $$"""
                UPDATE [dbo].[Tab1] SET [Value] = @Value WHERE Id = @Id;
                """,
            parameters: i => new { Id = Random.Shared.Next(0, 4), Value = $"Val_updated_{i}" });

        var task2 = Benchmark.Test("select", sql: $$"""
                UPDATE [dbo].[Tab2] SET [Value] = @Value WHERE Id = @Id;
                """,
            parameters: i => new { Id = Random.Shared.Next(0, 4), Value = $"Val_updated_{Random.Shared.Next(0, Int32.MaxValue)}" });


        await Task.WhenAll(task1, task2);
    }
}