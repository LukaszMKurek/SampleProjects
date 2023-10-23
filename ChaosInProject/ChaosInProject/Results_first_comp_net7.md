**No threads - 1**
* Sum: 80000000; Elapsed: 24,7974 ms
* Sum: 80000000; Elapsed: 23,923 ms
* Sum: 80000000; Elapsed: 25,0134 ms
* Sum: 80000000; Elapsed: 22,595 ms
* Sum: 80000000; Elapsed: 21,9836 ms

**Thread without any control - 2**
* Sum: 15509478; Elapsed: 88,5941 ms
* Sum: 13844262; Elapsed: 88,677 ms
* Sum: 22829832; Elapsed: 87,2124 ms
* Sum: 22443652; Elapsed: 85,7872 ms
* Sum: 23831500; Elapsed: 84,0527 ms

**Lock - 3**
* Sum: 80000000; Elapsed: 2998,309 ms
* Sum: 80000000; Elapsed: 2852,3862 ms
* Sum: 80000000; Elapsed: 4265,0675 ms
* Sum: 80000000; Elapsed: 4396,9089 ms
* Sum: 80000000; Elapsed: 3713,6452 ms

**Interlocked.Increment - 4**
* Sum: 80000000; Elapsed: 1472,2404 ms
* Sum: 80000000; Elapsed: 1758,7061 ms
* Sum: 80000000; Elapsed: 1490,478 ms
* Sum: 80000000; Elapsed: 1490,1689 ms
* Sum: 80000000; Elapsed: 1739,7679 ms

**Universal lock free with CAS - 5**
* Sum: 80000000; Elapsed: 8404,7768 ms
* Sum: 80000000; Elapsed: 9484,5249 ms
* Sum: 80000000; Elapsed: 10128,4752 ms
* Sum: 80000000; Elapsed: 8300,7643 ms
* Sum: 80000000; Elapsed: 8200,5612 ms

**Minimize false sharing with Interlocked.Increment - 6**
* Sum: 80000000; Elapsed: 8,3985 ms
* Sum: 80000000; Elapsed: 7,5415 ms
* Sum: 80000000; Elapsed: 7,6062 ms
* Sum: 80000000; Elapsed: 6,0712 ms
* Sum: 80000000; Elapsed: 6,0202 ms

**ThreadLocal first case - 7**
* Sum: 80000000; Elapsed: 334,399 ms
* Sum: 80000000; Elapsed: 334,3126 ms
* Sum: 80000000; Elapsed: 304,2373 ms
* Sum: 80000000; Elapsed: 306,4299 ms
* Sum: 80000000; Elapsed: 337,3837 ms

**ThreadLocal second case - 8**
* Sum: 80000000; Elapsed: 338,8986 ms
* Sum: 80000000; Elapsed: 321,41 ms
* Sum: 80000000; Elapsed: 312,1135 ms
* Sum: 80000000; Elapsed: 326,7487 ms
* Sum: 80000000; Elapsed: 321,4868 ms

**Reduce lock contention by Thread.GetCurrentProcessorId() and reduce false sharing - 9**
* Sum: 80000000; Elapsed: 1574,1986 ms
* Sum: 80000000; Elapsed: 1599,1996 ms
* Sum: 80000000; Elapsed: 1506,383 ms
* Sum: 80000000; Elapsed: 1440,2662 ms
* Sum: 80000000; Elapsed: 1416,2116 ms