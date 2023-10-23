using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LockingAlgorithms
{
    internal sealed class Program
    {
        private static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

        static void Main(string[] args)
        {
            Console.WriteLine($"Machine ProcessorCount: {Environment.ProcessorCount}");

            Action action;
            switch (int.Parse(args[0]))
            {
                case 1:
                    Console.WriteLine("Only one thread");
                    action = Sum_1;
                    break;
                case 2:
                    Console.WriteLine("Many threads without any protect");
                    action = Sum_2;
                    break;
                case 3:
                    Console.WriteLine("Many threads with lock");
                    action = Sum_3;
                    break;
                case 4:
                    Console.WriteLine("Many threads with Interlocked.Increment");
                    action = Sum_4;
                    break;
                case 5:
                    Console.WriteLine("Many threads with optimistic locking");
                    action = Sum_5;
                    break;
                case 6:
                    Console.WriteLine("Many threads with reduced Interlocked.Increment usage");
                    action = Sum_6;
                    break;
                case 7:
                    Console.WriteLine("Many threads with ThreadLocal case 1");
                    action = Sum_7;
                    break;
                case 8:
                    Console.WriteLine("Many threads with ThreadLocal case 2");
                    action = Sum_8;
                    break;
                case 9:
                    Console.WriteLine("Many threads with Thread.GetCurrentProcessorId(), reduced lock contention and reduced false sharing");
                    action = Sum_9;
                    break;
                default:
                    throw new InvalidOperationException("Unknown parameter. Insert  1 - 9");
            }

            for (int i = 0; i < 100; i++)
                action();
        }

        private static void Sum_1()
        {
            var t = Stopwatch.GetTimestamp();

            long sum = 0;
            for (int i = 0; i < 8 * 10_000_000; i++)
            {
                sum = sum + 1;
            }

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.WriteLine($"Sum: {sum}; Elapsed: {elapsed.TotalMilliseconds} ms");
        }

        private static void Sum_2()
        {
            var t = Stopwatch.GetTimestamp();

            long sum = 0;
            Parallel.For(0, 8, parallelOptions: ParallelOptions,_ => 
            {
                for (int i = 0; i < 10_000_000; i++)
                {
                    sum = sum + 1;
                }

                // Implementacja jest błędna i produkuje losowe wyniki
                // dodawanie liczb nie jest bezpieczne wielowątkowo
                // gdy mamy 1 wątek sekwencja operacji jest prosta
                // read x from memory
                // x = x + 1
                // write x to memory
                // read x from memory
                // x = x + 1
                // write x to memory
                // jeśli jednak kilka wątków pracuje nad jedna zmienną pojawiają się dodatkowe nie pożądane sekwencje np:
                // T1: read x from memory
                // T2: read x from memory
                // T1: x = x + 1
                // T2: x = x + 1
                // T1: write x to memory
                // T2: write x to memory
                // efekt jest taki że x w pamięci nie powiększył się o 2 ale tylko o 1
                // są możliwe sytuacje że x w pamięci się powiększy o kilka ale powolny jeden wątek w rzuci do pamięci mocno nie aktualną wersję i cofniemy się w efekcie w czasie
            });

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.WriteLine($"Sum: {sum}; Elapsed: {elapsed.TotalMilliseconds} ms");
        }

        // Słowo kluczowe volatile nie jest rozwiązaniem problemu
        // Kiedyś było rozwiązaniem ale komputery się pokomplikowały i już nie jest to rozwiązanie
        // Część prawd ma swój termin ważności i jak on minie stają się tylko historycznymi faktami - ludziom bardzo często to umyka
        // Kiedyś gdy kompilatory nie umiały robić niektórych optymalizacji volatile nie był potrzebny, kiedy kompilatory się rozwinęły volatile był potrzebny by
        // rozwiązać problemy które te optymalizacje zrobiły, działało to do momentu aż pojawiły się komputery które mogły wykonywać kilka wątków jednocześnie
        // wtedy volatile już nie było w stanie rozwiązać problemów które wynikły z tych optymalizacji. W międzyczasie same procesory bardzo mocno skomplikowały swoją 
        // wewnętrzną strukturę by osiągać ogromną wydajność co jeszcze bardziej utrudniło pisanie programów wielowątkowych.
        // Kiedyś wystarczyła wiedza że wystarczy użyć volatile, teraz trzeba przeczytać i zrozumieć kilka grubych książek by zrozumieć co robić
        // Następcą volatile jako prostego rozwiązania pierwszego wyboru jest lock
        // Poniżej niedawny przykład że sprzęt nadal się zmienia
        // https://github.com/dotnet/runtime/issues/93624
        // to co kiedyś było dobre nie koniecznie musi być dobre w przyszłości
        // to co obecnie potęguje problemy wielowątkowe to dynamiczna zmiana prędkości rdzeni, rdzenie heterogeniczne, stopniowe wycofywanie się z 32 bitowych programów
        // popularne stają się także architektury z luźniejszymi zasadami pracy, oferują większą wydajność kosztem koszmarnych problemów z wielowątkowością
        // im dalej w las tym bardziej trzeba będzie się trzymać standardowych rozwiązań

        private static void Sum_3()
        {
            var t = Stopwatch.GetTimestamp();

            long sum = 0;
            var locker = new object();
            Parallel.For(0, 8, parallelOptions: ParallelOptions, _ =>
            {
                for (int i = 0; i < 10_000_000; i++)
                {
                    lock (locker)
                    {
                        sum = sum + 1;
                    }

                    // Zalety:
                    // przyzwoicie szybkie nawet w pesymistycznych warunkach
                    // jedno z najprostszych rozwiązań

                    // Wady:
                    // ryzyko deadlocków - można się jednak przed nimi zabezpieczyć w 100% jeśli przestrzega się kilku prostych reguł
                    // ryzyko opóźnień przez lock contention
                    
                    // trzeba stosować maksymalnie krótkie sekcje krytyczne na maksymalnie niezależnych obiektach
                }
            });

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.WriteLine($"Sum: {sum}; Elapsed: {elapsed.TotalMilliseconds} ms");
        }

        private static void Sum_4()
        {
            var t = Stopwatch.GetTimestamp();

            long sum = 0;
            Parallel.For(0, 8, parallelOptions: ParallelOptions, _ =>
            {
                for (int i = 0; i < 10_000_000; i++)
                {
                    Interlocked.Increment(ref sum);

                    // Zalety:
                    // brak deadlocków
                    // brak czekania na locka
                    // jedno z szybszych rozwiązań

                    // Wady:
                    // nadaje się tylko dla prostych scenariuszy
                    // narażamy się na split locka https://lwn.net/Articles/790464/
                    // cierpimy z powodu false sharing https://en.wikipedia.org/wiki/False_sharing
                }
            });

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.WriteLine($"Sum: {sum}; Elapsed: {elapsed.TotalMilliseconds} ms");

            /*
               Jak działa Interlocked.Increment?
               https://godbolt.org/

               using System;
               using System.Threading;
               
               class Program
               {
               static void Increment(ref long num) => num++;
               static void IncrementThreadSafe(ref long num) => Interlocked.Increment(ref num);
               }

                Interlocked.Increment powoduje że zwykła instrukcja inc [mem] ma dodatkowy prefix 'lock' powoduje on że CPU gwarantuje prawdziwą atomowość wykonywanej instrukcji
                lock z poziomu języka C# potrzebuje wykonać 2 instrukcje atomowe dlatego jest trochę wolniejszy od Interlocked.Increment 
             */
        }

        private static void Sum_5()
        {
            var t = Stopwatch.GetTimestamp();

            long sum = 0;
            Parallel.For(0, 8, parallelOptions: ParallelOptions, _ =>
            {
                for (int i = 0; i < 10_000_000; i++)
                {
                    while (true)
                    {
                        var sum_local = sum;
                        var sum_local_incremented = sum_local + 1;

                        if (Interlocked.CompareExchange(ref sum, sum_local_incremented, sum_local) == sum_local)
                            break;
                    }

                    // Zalety:
                    // brak deadlocków
                    // brak czekania na locka

                    // Wady:
                    // w ogólnym przypadku narażamy się na problem: https://pl.wikipedia.org/wiki/Problem_ABA
                    // narażamy się na https://en.wikipedia.org/wiki/Deadlock#Livelock skutki livelocka są często gorsze niż wysokiego lock contention
                    // działa dobrze tylko w optymistycznych warunkach, w pesymistycznych warunkach livelock masakruje wydajność i wprowadza ogromne opóźniania
                    // narażamy się na split locka https://lwn.net/Articles/790464/
                    // cierpimy z powodu https://en.wikipedia.org/wiki/False_sharing
                    // ponowienia powodują większe zużycie CPU, niestety nie ma górnego limitu i CPU może osiągnąć 100% gdy pojawią się pesymistyczne warunki
                }
            });

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.WriteLine($"Sum: {sum}; Elapsed: {elapsed.TotalMilliseconds} ms");
        }

        private static void Sum_6()
        {
            var t = Stopwatch.GetTimestamp();

            long sum = 0;
            Parallel.For(0, 8, parallelOptions: ParallelOptions, _ =>
            {
                long sum_local = 0;
                for (int i = 0; i < 10_000_000; i++)
                {
                    sum_local = sum_local + 1;
                }

                Interlocked.Add(ref sum, sum_local);

                // Najszybsze możliwe podejście bo maksymalnie zredukowałem wielowątkowy dostęp do danych ale nadal mamy procesowanie współbieżne
            });

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.WriteLine($"Sum: {sum}; Elapsed: {elapsed.TotalMilliseconds} ms");
        }

        private static void Sum_7()
        {
            var t = Stopwatch.GetTimestamp();

            long sum = 0;
            using ThreadLocal<long> sum_local = new ThreadLocal<long>();
            Parallel.For(0, 8, parallelOptions: ParallelOptions, _ =>
            {
                for (int i = 0; i < 10_000_000; i++)
                {
                    sum_local.Value = sum_local.Value + 1;
                }

                Interlocked.Add(ref sum, sum_local.Value);
                sum_local.Value = 0; // ta linijka jest krytyczna!!!
            });

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.WriteLine($"Sum: {sum}; Elapsed: {elapsed.TotalMilliseconds} ms");
        }

        private static void Sum_8()
        {
            var t = Stopwatch.GetTimestamp();

            using ThreadLocal<long> sum_local = new ThreadLocal<long>(trackAllValues: true); // A co jeśli ktoś sobie natworzy miliony wątków? (przez ich regularne tworzenie i ubijanie)
            Parallel.For(0, 8, parallelOptions: ParallelOptions, _ =>
            {
                for (int i = 0; i < 10_000_000; i++)
                {
                    sum_local.Value = sum_local.Value + 1;
                }

                // Maksymalnie zredukowałem wielowątkowy dostęp do danych
                // ThreadLocal daje jednak pewien narzut wydajnościowy
            });

            Thread.MemoryBarrier();
            var sum = sum_local.Values.Sum();

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.WriteLine($"Sum: {sum}; Elapsed: {elapsed.TotalMilliseconds} ms");
        }

        [StructLayout(LayoutKind.Auto, Size = 64)] // rozwiązuje problem false sharing. Rozmiar lini cache to 64B
        private struct PaddedLong
        {
            public long Value;
            public object locker;
        }

        private static void Sum_9()
        {
            var t = Stopwatch.GetTimestamp();

            var sums = new PaddedLong[Environment.ProcessorCount];
            for (var i = 0; i < sums.Length; i++)
                sums[i].locker = new object();

            Parallel.For(0, 8, parallelOptions: ParallelOptions, _ =>
            {
                for (int i = 0; i < 10_000_000; i++)
                {
                    // UWAGA! Thread.GetCurrentProcessorId() będzie działać dobrze tylko na niektórych maszynach. Czasami może się w ogóle wywrócić, zwrócić -1 albo inną dziwną liczbę
                    // Thread.GetCurrentProcessorId() nie zwraca dokładnej wartości tylko przybliżoną!
                    // Tu trzeba dopisać duża ilość kodu i testować rozwiązanie na różnych procesorach i platformach
                    // Może i jest szybciej ale jest to koszmar w testowaniu i utrzymaniu
                    ref var obj = ref sums[Thread.GetCurrentProcessorId()];
                    lock (obj.locker)
                    {
                        obj.Value = obj.Value + 1;
                    }
                }
            });

            Thread.MemoryBarrier();
            var sum = sums.Sum(i => i.Value);

            var elapsed = Stopwatch.GetElapsedTime(t);
            Console.WriteLine($"Sum: {sum}; Elapsed: {elapsed.TotalMilliseconds} ms");
        }

        // Jeśli komuś wydaje się że podałem abstrakcyjny przykład to proszę przeglądnąć poniższe commity w których autorzy szarpią się z problemem inkrementacji liczb:
        // https://github.com/dotnet/runtime/pull/84427/files
        // https://github.com/dotnet/runtime/pull/91566/files
    }
}