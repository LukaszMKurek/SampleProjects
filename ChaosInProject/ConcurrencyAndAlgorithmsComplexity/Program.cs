using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ConcurrencyAndAlgorithmsComplexity
{
    internal sealed class Program
    {
        private static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

        /// <summary>
        /// Przykład demonstruje jak ilość danych i złożoność algorytmu wpływa na wydajność
        /// oraz jak wielowątkowe przetwarzanie wpływa na wydajność
        /// Wnioski są takie że jak mamy dużo danych to redukcją złożoności algorytmu możemy dać sobie wzrosty dziesiątki, setki, tysiące razy większe
        /// Programowanie wielowątkowe zwykle daje kilkukrotne przyśpieszenie i to w optymistycznych warunkach
        /// Wniosek jest taki że redukcja złożoności algorytmu jest pierwszą rzeczą którą trzeba zbadać
        /// Drugi wniosek jest taki że musimy wiedzieć jak wygląda złożoność obliczeniowa naszego systemu, musimy wiedzieć w jakich granicach on poprawnie działa
        /// oraz musimy monitorować warunki na produkcji czy nie zbliżamy się do granic możliwości
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine($"Machine ProcessorCount: {Environment.ProcessorCount}");

            Action<Action> action;
            switch (int.Parse(args[0]))
            {
                case 1:
                    Console.WriteLine("Only one thread");
                    action = OneThread_1;
                    break;
                case 2:
                    Console.WriteLine("Many threads without locking");
                    action = ManyThreads_2;
                    break;
                case 3:
                    Console.WriteLine("Many threads with locking");
                    action = ManyThreadsWithLock_3;
                    break;
                default:
                    throw new InvalidOperationException("Unknown parameter. Insert 1 - 3");
            }

            for (int i = 0; i < 2000; i++)
            {
                Console.Write($"Input {i}: ");
                action(() => SimulateLoad(i));
                Console.Write("\t");
                action(() => SimulateLoad(i*i));
                Console.Write("\t");
                action(() => SimulateLoad(i*i*i));
                Console.WriteLine();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static long SimulateLoad(int i)
            {
                long x = 1;
                for (int j = 0; j < 100 * i; j++)
                    x += j;

                return x;
            }
        }

        private static void OneThread_1(Action algorithm)
        {
            var t = Stopwatch.GetTimestamp();

            for (int i = 0; i < 8 * 100; i++)
            {
                algorithm();
            }

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.Write($"Elapsed: {elapsed.TotalMilliseconds} ms");
        }

        private static void ManyThreads_2(Action algorithm)
        {
            var t = Stopwatch.GetTimestamp();

            Parallel.For(0, 8, parallelOptions: ParallelOptions,_ => 
            {
                for (int i = 0; i < 100; i++)
                {
                    algorithm();
                }
            });

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.Write($"Elapsed: {elapsed.TotalMilliseconds} ms");
        }
        
        private static void ManyThreadsWithLock_3(Action algorithm)
        {
            var t = Stopwatch.GetTimestamp();

            var locker = new object();
            Parallel.For(0, 8, parallelOptions: ParallelOptions, _ =>
            {
                for (int i = 0; i < 100; i++)
                {
                    lock (locker)
                    {
                        algorithm();
                    }
                }
            });

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.Write($"Elapsed: {elapsed.TotalMilliseconds} ms");
        }
    }
}