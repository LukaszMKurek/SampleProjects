**No threads - 1**
* Sum: 80000000; Elapsed: 19,1073 ms
* Sum: 80000000; Elapsed: 19,1022 ms
* Sum: 80000000; Elapsed: 19,1102 ms
* Sum: 80000000; Elapsed: 19,117 ms
* Sum: 80000000; Elapsed: 19,1004 ms

**Thread without any control - 2**
* Sum: 13823607; Elapsed: 70,2844 ms
* Sum: 14042519; Elapsed: 69,5656 ms
* Sum: 18096374; Elapsed: 71,0232 ms
* Sum: 16977119; Elapsed: 73,9687 ms
* Sum: 19139077; Elapsed: 73,0463 ms

**Lock - 3**
* Sum: 80000000; Elapsed: 1776,3506 ms
* Sum: 80000000; Elapsed: 2087,2548 ms
* Sum: 80000000; Elapsed: 2084,1999 ms
* Sum: 80000000; Elapsed: 2189,7279 ms
* Sum: 80000000; Elapsed: 1957,4469 ms

**Interlocked.Increment - 4**
* Sum: 80000000; Elapsed: 1304,1936 ms
* Sum: 80000000; Elapsed: 1401,0177 ms
* Sum: 80000000; Elapsed: 1223,3569 ms
* Sum: 80000000; Elapsed: 1406,5137 ms
* Sum: 80000000; Elapsed: 1397,9341 ms

**Universal lock free with CAS - 5**
* Sum: 80000000; Elapsed: 7553,4688 ms
* Sum: 80000000; Elapsed: 7946,5248 ms
* Sum: 80000000; Elapsed: 7897,6542 mss
* Sum: 80000000; Elapsed: 7780,8393 ms
* Sum: 80000000; Elapsed: 7741,0001 ms

**Minimize false sharing with Interlocked.Increment - 6**
* Sum: 80000000; Elapsed: 4,9855 ms
* Sum: 80000000; Elapsed: 5,2979 ms
* Sum: 80000000; Elapsed: 5,0019 ms
* Sum: 80000000; Elapsed: 4,9886 ms
* Sum: 80000000; Elapsed: 5,0498 ms

**ThreadLocal first case - 7**
* Sum: 80000000; Elapsed: 166,6048 ms
* Sum: 80000000; Elapsed: 167,7125 ms
* Sum: 80000000; Elapsed: 188,6876 ms
* Sum: 80000000; Elapsed: 177,5627 ms
* Sum: 80000000; Elapsed: 173,8507 ms

**ThreadLocal second case - 8**
* Sum: 80000000; Elapsed: 156,3993 ms
* Sum: 80000000; Elapsed: 158,9913 ms
* Sum: 80000000; Elapsed: 159,0986 ms
* Sum: 80000000; Elapsed: 168,3807 ms
* Sum: 80000000; Elapsed: 162,9245 ms

**Reduce lock contention by Thread.GetCurrentProcessorId() and reduce false sharing - 9**
* Sum: 80000000; Elapsed: 1118,514 ms
* Sum: 80000000; Elapsed: 1531,298 ms
* Sum: 80000000; Elapsed: 1369,4964 ms
* Sum: 80000000; Elapsed: 1391,5955 ms
* Sum: 80000000; Elapsed: 1316,6439 ms