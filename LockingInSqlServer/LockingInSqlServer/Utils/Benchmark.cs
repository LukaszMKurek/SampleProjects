using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Diagnostics;
using Dapper;

namespace LockingInSqlServer.Utils
{
    public static class Benchmark
    {
        public static async Task Test(string name, string sql, Func<int, object> parameters, int warmup = 16, int? commandTimeout = null, int taskCount = 64, int repeatCount = 512)
        {
            // warm up
            for (int i = 0; i < warmup; i++)
                await Execute(i, i);

            for (int n = 0; n < 3; n++)
            {
                var bag = new ConcurrentBag<double>();

                var total = Stopwatch.StartNew();
                await RunInParallel(taskCount, async n =>
                {
                    for (int i = 0; i < repeatCount; i++)
                    {
                        var t = Stopwatch.StartNew();
                        await Execute(i, n);

                        bag.Add(t.Elapsed.TotalMilliseconds);
                    }
                });

                Console.WriteLine($"{name.PadRight(55, '.')} Avg: {bag.Average():F4} ms; {taskCount * repeatCount / total.Elapsed.TotalMilliseconds * 1000:F2} Request/s");
            }

            Console.WriteLine();

            async Task Execute(int i, int n)
            {
                await using var connection = await ConnectionHelper.GetSqlConnection();
                await connection.ExecuteAsync(sql, parameters(i * n), commandTimeout: commandTimeout);
            }
        }

        public static async Task Test2(string name, Func<int, SqlConnection, Task> action, int taskCount = 64, int repeatCount = 512)
        {
            // warm up
            for (int i = 0; i < 16; i++)
                await Execute(i, i);

            var bag = new ConcurrentBag<double>();

            for (int n = 0; n < 3; n++)
            {
                var total = Stopwatch.StartNew();
                await RunInParallel(taskCount, async n =>
                {
                    for (int i = 0; i < repeatCount; i++)
                    {
                        var t = Stopwatch.StartNew();
                        await Execute(i, n);

                        bag.Add(t.Elapsed.TotalMilliseconds);
                    }
                });

                Console.WriteLine($"{name.PadRight(55, '.')} Avg: {bag.Average():F4} ms; {taskCount * repeatCount / total.Elapsed.TotalMilliseconds * 1000:F2} Request/s");
            }

            Console.WriteLine();

            async Task Execute(int i, int n)
            {
                await using var connection = await ConnectionHelper.GetSqlConnection();
                await action(i * n, connection);
            }
        }

        public static Task MeasureSingle(string name, string sql, Func<int, object> parameters, int parallelRun = 3, int? commandTimeout = null)
        {
            return RunInParallel(parallelRun, async i =>
            {
                var t = Stopwatch.StartNew();

                await using var connection = await ConnectionHelper.GetSqlConnection();

                var connectionOpening = t.Elapsed.TotalMilliseconds;
                t.Restart();

                await connection.ExecuteAsync(sql, parameters(i), commandTimeout: commandTimeout);

                Console.WriteLine($"{name} took {t.Elapsed.TotalMilliseconds:F1} ms. Connection opening took: {connectionOpening:F1} ms");
            });
        }

        public static async Task MeasureSingle(string name, SqlConnection connection, string sql, object parameters, int? commandTimeout = null)
        {
            var t = Stopwatch.StartNew();
            await connection.ExecuteAsync(sql, parameters, commandTimeout: commandTimeout);

            Console.WriteLine($"{name} took {t.Elapsed.TotalMilliseconds:F1} ms");
        }

        public static async Task RunInParallel(int tasksCount, Func<int, Task> action)
        {
            var tasks = new Task[tasksCount];
            for (var i = 0; i < tasksCount; i++)
            {
                var i1 = i;
                tasks[i] = Task.Run(async () =>
                {
                    await action(i1);
                });
            }

            await Task.WhenAll(tasks);
        }
    }
}
