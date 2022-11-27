using System.Data;
using Dapper;
using LockingInSqlServer.Utils;

namespace LockingInSqlServer.Scenarios;

public static class LockingCosts
{
    public static async Task Run()
    {
        await ExampleDatabase1.CreateDatabase(enableReadCommittedSnapshot: true);

        const int IdPool = 256;
        await Benchmark.Test("simple select", sql: $$"""
                SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id
                """,
            parameters: i => new
            { Id = Random.Shared.Next(0, IdPool), Tab2Id = Random.Shared.Next(0, IdPool), Value = $"Val_updated_{i}" });

        await Benchmark.Test("simple update", sql: $$"""
                UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id
                """,
            parameters: i => new
            { Id = Random.Shared.Next(0, IdPool), Tab2Id = Random.Shared.Next(0, IdPool), Value = $"Val_updated_{i}" });

        await Benchmark.Test("update in transaction", sql: $$"""
                begin tran;
                UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id
                commit tran;
                """,
            parameters: i => new
            { Id = Random.Shared.Next(0, IdPool), Tab2Id = Random.Shared.Next(0, IdPool), Value = $"Val_updated_{i}" });

        await Benchmark.Test("simple select and update in transaction", sql: $$"""
                begin tran;
                SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id
                UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id
                commit tran;
                """,
            parameters: i => new
            { Id = Random.Shared.Next(0, IdPool), Tab2Id = Random.Shared.Next(0, IdPool), Value = $"Val_updated_{i}" });

        await Benchmark.Test("select with lock and update in transaction", sql: $$"""
                begin tran;
                SELECT 1 FROM [dbo].[Tab1] x WITH (UPDLOCK, SERIALIZABLE) WHERE x.Id = @Id
                UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id
                commit tran;
                """,
            parameters: i => new
            { Id = Random.Shared.Next(0, IdPool), Tab2Id = Random.Shared.Next(0, IdPool), Value = $"Val_updated_{i}" });

        await Benchmark.Test("getapplock, select and update in transaction", sql: $$"""
                begin tran;
                EXEC sp_getapplock @resource=@LockKey, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=4000;
                SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id
                UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id
                commit tran;
                """,
            parameters: i =>
            {
                var id = Random.Shared.Next(0, IdPool);
                return new
                {
                    Id = id,
                    Tab2Id = Random.Shared.Next(0, IdPool),
                    Value = $"Val_updated_{i}",
                    LockKey = $"K_{id}"
                };
            });

        /*await Benchmark.Test("", sql: $$"""
                begin tran;
                SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id
                WAITFOR DELAY '00:00:00.005';
                UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id
                WAITFOR DELAY '00:00:00.005';
                commit tran;
                """,
                parameters: i => new { Id = Random.Shared.Next(0, IdPool), Tab2Id = Random.Shared.Next(0, IdPool), Value = $"Val_updated_{i}" });

            await Benchmark.Test("", sql: $$"""
                begin tran;
                SELECT 1 FROM [dbo].[Tab1] x WITH (UPDLOCK, SERIALIZABLE) WHERE x.Id = @Id
                WAITFOR DELAY '00:00:00.005';
                UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id
                WAITFOR DELAY '00:00:00.005';
                commit tran;
                """,
                parameters: i => new { Id = Random.Shared.Next(0, IdPool), Tab2Id = Random.Shared.Next(0, IdPool), Value = $"Val_updated_{i}" });

            await Benchmark.Test("", sql: $$"""
                begin tran;
                EXEC sp_getapplock @resource=@LockKey, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=4000;
                SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id
                WAITFOR DELAY '00:00:00.005';
                UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id
                WAITFOR DELAY '00:00:00.005';
                commit tran;
                """,
                parameters: i =>
                {
                    var id = Random.Shared.Next(0, IdPool);
                    return new
                    {
                        Id = id,
                        Tab2Id = Random.Shared.Next(0, IdPool),
                        Value = $"Val_updated_{i}",
                        LockKey = $"K_{id}"
                    };
                });
            */

        await Benchmark.Test2("many round trips, simple select and update", async (i, c) =>
        {
            var id = Random.Shared.Next(0, IdPool);
            await using var tran = await c.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            await c.QueryFirstAsync<int>("SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id", new { Id = id }, tran);
            await c.ExecuteAsync("UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id", new
            {
                Id = id,
                Tab2Id = Random.Shared.Next(0, IdPool),
                Value = $"Val_updated_{i}"
            }, tran);

            await tran.CommitAsync();
        });

        await Benchmark.Test2("many round trips, select with lock and update", async (i, c) =>
        {
            var id = Random.Shared.Next(0, IdPool);
            await using var tran = await c.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            await c.QueryFirstAsync<int>("SELECT 1 FROM [dbo].[Tab1] x WITH (UPDLOCK, SERIALIZABLE) WHERE x.Id = @Id",
                new { Id = id }, tran);
            await c.ExecuteAsync("UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id", new
            {
                Id = id,
                Tab2Id = Random.Shared.Next(0, IdPool),
                Value = $"Val_updated_{i}"
            }, tran);

            await tran.CommitAsync();
        });

        await Benchmark.Test2("many round trips, getapplock, select and update", async (i, c) =>
        {
            var id = Random.Shared.Next(0, IdPool);
            await using var tran = await c.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            await c.ExecuteAsync(
                "EXEC sp_getapplock @resource=@LockKey, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=4000;",
                new { LockKey = $"K_{id}" }, tran);
            await c.QueryFirstAsync<int>("SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id", new { Id = id }, tran);
            await c.ExecuteAsync("UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id", new
            {
                Id = id,
                Tab2Id = Random.Shared.Next(0, IdPool),
                Value = $"Val_updated_{i}"
            }, tran);

            await tran.CommitAsync();
        });

        await Benchmark.Test2("many round trips, getapplock (2), select and update", async (i, c) =>
        {
            var id = Random.Shared.Next(0, IdPool);
            await using var tran = await c.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            await c.QueryFirstAsync<int>(
                "EXEC sp_getapplock @resource=@LockKey, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=4000;" +
                "SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id", new { Id = id, LockKey = $"K_{id}" }, tran);
            await c.ExecuteAsync("UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id", new
            {
                Id = id,
                Tab2Id = Random.Shared.Next(0, IdPool),
                Value = $"Val_updated_{i}"
            }, tran);

            await tran.CommitAsync();
        });

        await Benchmark.Test2("many round trips, getapplock (3), select and update", async (i, c) =>
        {
            var id = Random.Shared.Next(0, IdPool);
            await using var tran = await c.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            await c.ExecuteAsync("sp_getapplock",
                new { @resource = $"K_{id}", @LockMode = "Exclusive", @LockOwner = "Transaction", @LockTimeout = 4000 }, tran,
                commandType: CommandType.StoredProcedure);
            await c.QueryFirstAsync<int>("SELECT 1 FROM [dbo].[Tab1] x WHERE x.Id = @Id", new { Id = id }, tran);
            await c.ExecuteAsync("UPDATE [dbo].[Tab1] SET [Value] = @Value, [Tab2Id] = @Tab2Id WHERE Id = @Id", new
            {
                Id = id,
                Tab2Id = Random.Shared.Next(0, IdPool),
                Value = $"Val_updated_{i}"
            }, tran);

            await tran.CommitAsync();
        });
    }
}