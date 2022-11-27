using Dapper;
using LockingInSqlServer.Utils;
using IsolationLevel = System.Data.IsolationLevel;

namespace LockingInSqlServer.Scenarios;

public static class LockingProblems
{
    public static async Task Case1()
    {
        await ExampleDatabase1.CreateDatabase(enableReadCommittedSnapshot: true);

        // number of connection in pool - default is only 100
        // max number of thread workers in sql server i limited
        // every waiting for lock release hold one worker thread and if wait to many locks then all thread can be used and any other operation cant be started at serwer

        await Benchmark.Test("selects before update", sql: $$"""
                SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id
                """,
            parameters: i => new { Id = Random.Shared.Next(0, 1024) });

        await Benchmark.Test("updates before update", sql: $$"""
                UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id
                """,
            parameters: i => new { Id = Random.Shared.Next(0, 1024), Tab2Id = Random.Shared.Next(0, 1024), Value = $"Val_updated_{i}" });


        // I want to avoid waiting for connection open during measure sql execution time
        await using var connection1 = await ConnectionHelper.GetSqlConnection();
        await using var connection2 = await ConnectionHelper.GetSqlConnection();
        await using var connection3 = await ConnectionHelper.GetSqlConnection();

        var updateTasks = new List<Task>();
        for (int i = 0; i < 700; i++)
        {
            var task = Task.Run(async () =>
            {
                var id = 1; // updating 1 row by hundreds threads will block all thread in sql server
                            //var id = Random.Shared.Next(0, 4048); // if lock is only acquired an any other thread do not wait then there is no problem
                await using var c = await ConnectionHelper.GetSqlConnection();
                await using var tran = await c.BeginTransactionAsync(IsolationLevel.ReadCommitted);

                await c.ExecuteAsync("UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id", new
                {
                    Id = id,
                    Tab2Id = Random.Shared.Next(0, 1024),
                    Value = $"Val_updated_{id}"
                }, tran, commandTimeout: 60);

                await Task.Delay(TimeSpan.FromSeconds(30));

                await tran.CommitAsync();
            });

            updateTasks.Add(task);
        }

        // wait to ensure that all threads are blocked
        await Task.Delay(TimeSpan.FromSeconds(10));

        // executing any sql is blocked
         await Parallel.ForEachAsync(new[] {connection1, connection2, connection3}, async (connection, ct) =>
         {
             await Benchmark.MeasureSingle("selects during update", connection, sql: $$"""
                 SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id
                 """,
                 parameters: new { Id = Random.Shared.Next(10, 1024) },
                 commandTimeout: 60);
         });


        await Task.WhenAll(updateTasks);
    }

    public static async Task Case2()
    {
        await ExampleDatabase1.CreateDatabase(enableReadCommittedSnapshot: true);

        // number of connection in pool - default is only 100
        // max number of thread workers in sql server i limited
        // every waiting for lock release hold one worker thread and if wait to many locks then all thread can be used and any other operation cant be started at serwer

        await Benchmark.Test("selects before update", sql: $$"""
                SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id
                """,
            parameters: i => new { Id = Random.Shared.Next(0, 1024) });

        await Benchmark.Test("updates before update", sql: $$"""
                UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id
                """,
            parameters: i => new { Id = Random.Shared.Next(0, 1024), Tab2Id = Random.Shared.Next(0, 1024), Value = $"Val_updated_{i}" });


        var updateTasks = new List<Task>();
        for (int i = 0; i < 700; i++)
        {
           var task = Task.Run(async () =>
           {
               var id = 1; // updating 1 row by hundreds threads will block all thread in sql server
               //var id = Random.Shared.Next(0, 4048); // if lock is only acquired an any other thread do not wait then there is no problem
               await using var c = await ConnectionHelper.GetSqlConnection();
               await using var tran = await c.BeginTransactionAsync(IsolationLevel.ReadCommitted);
               
               await c.ExecuteAsync("UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id", new
               {
                   Id = id,
                   Tab2Id = Random.Shared.Next(0, 1024),
                   Value = $"Val_updated_{id}"
               }, tran, commandTimeout: 60);

               await Task.Delay(TimeSpan.FromSeconds(30));

               await tran.CommitAsync();
           });

            updateTasks.Add(task);
        }

        // wait to ensure that all threads are blocked
        await Task.Delay(TimeSpan.FromSeconds(10));

        // opening connection is blocked
        await Benchmark.MeasureSingle("selects during update", sql: $$"""
                SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id
                """,
            parameters: i => new { Id = Random.Shared.Next(10, 1024) },
            commandTimeout: 60);


        await Task.WhenAll(updateTasks);
    }
}