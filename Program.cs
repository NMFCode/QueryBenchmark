using Expressions_Demo.ScenarioGeneration;
using NMF.Collections.ObjectModel;
using NMF.Expressions;
using NMF.Expressions.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Expressions_Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Measure(new [] { 10, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 }, 1.0f, 0.1f, 1.0f, 5);
        }

        /// <summary>
        /// This method encapsulates the query. It is used both for the incremental version as well as the batch version
        /// </summary>
        /// <param name="employees">The collection of employees</param>
        /// <returns>A query of employees</returns>
        private static IEnumerableExpression<Employee> CreateQuery(IEnumerableExpression<Employee> employees)
        {
            return from employee in employees
                   group employee by employee.Team into team
                   let teamMax = team.Max(member => member.WorkItems)
                   from collegue in team
                   where 4 * collegue.WorkItems < teamMax
                   select collegue;
        }

        /// <summary>
        /// This method encapsulates the query without NMF Expressions at all. It is used for the classic version
        /// </summary>
        /// <param name="employees">The collection of employees</param>
        /// <returns>A query of employees</returns>
        private static IEnumerable<Employee> CreateClassicQuery(IEnumerable<Employee> employees)
        {
            return from employee in employees
                   group employee by employee.Team into team
                   let teamMax = team.Max(member => member.WorkItems)
                   from collegue in team
                   where 4 * collegue.WorkItems < teamMax
                   select collegue;
        }

        /// <summary>
        /// Measures the polling algorithm versus the incremental algorithm for the given parameters
        /// </summary>
        /// <param name="sizes">The values for n</param>
        /// <param name="minRatio">The minimum ratio</param>
        /// <param name="step">The ratio step</param>
        /// <param name="maxRatio">The maximum ratio</param>
        /// <param name="iterations">The amount of iterations</param>
        private static void Measure(int[] sizes, float minRatio, float step, float maxRatio, int iterations)
        {
            var maxRatioIdx = Math.Min(0, (int)Math.Ceiling((maxRatio - minRatio) / step));
            var times = new long[sizes.Length, maxRatioIdx + 1, iterations, 9];
            var workloadCounts = new int[sizes.Length, maxRatioIdx + 1, iterations];
            var elementCounts = new int[sizes.Length, maxRatioIdx + 1, iterations];

            for (int sizeIdx = 0; sizeIdx < sizes.Length; sizeIdx++)
            {
                var n = sizes[sizeIdx];

                for (int ratioIdx = 0; ratioIdx <= maxRatioIdx; ratioIdx++)
                {
                    var sumPoll = 0L;
                    var sumInc = 0L;
                    var ratio = minRatio + ratioIdx * step;

                    for (int iteration = 0; iteration < iterations; iteration++)
                    {
                        RunIteration(times, workloadCounts, elementCounts, sizeIdx, n, ratioIdx, ref sumPoll, ref sumInc, ratio, iteration);
                    }

                    Console.WriteLine("Average speedup for ratio={0}: {1:0.###}", ratio, ((double)sumPoll) / sumInc);
                }
            }

            WriteResultsToCsv(sizes, minRatio, step, iterations, maxRatioIdx, times, workloadCounts, elementCounts);
        }

        private static void RunIteration(long[, , ,] times, int[, ,] workloadCounts, int[, ,] elementCounts, int sizeIdx, int n, int ratioIdx, ref long sumPoll, ref long sumInc, float ratio, int iteration)
        {
            Console.WriteLine("Generating workload for n={0},ratio={1},iteration={2}...", n, ratio, iteration);

            int employeesCount;
            var workload = ScenarioGenerator.GenerateScenario(ratio, n, 1000, out employeesCount);
            workloadCounts[sizeIdx, ratioIdx, iteration] = workload.Count;
            elementCounts[sizeIdx, ratioIdx, iteration] = employeesCount;

            Console.WriteLine("Generated workload of {0} items.", workload.Count);

            var classicTime = PlayWorkloadClassic(workload, employeesCount, "classic.log");
            var batchTime = PlayWorkloadBatch(workload, employeesCount, "batch.log");
            var incTime = PlayWorkloadIncremental(workload, employeesCount, "inc.log");

            times[sizeIdx, ratioIdx, iteration, 0] = classicTime.Initialization;
            times[sizeIdx, ratioIdx, iteration, 1] = batchTime.Initialization;
            times[sizeIdx, ratioIdx, iteration, 2] = incTime.Initialization;
            times[sizeIdx, ratioIdx, iteration, 3] = classicTime.MainWorkload;
            times[sizeIdx, ratioIdx, iteration, 4] = batchTime.MainWorkload;
            times[sizeIdx, ratioIdx, iteration, 5] = incTime.MainWorkload;
            times[sizeIdx, ratioIdx, iteration, 6] = classicTime.MemoryConsumption;
            times[sizeIdx, ratioIdx, iteration, 7] = batchTime.MemoryConsumption;
            times[sizeIdx, ratioIdx, iteration, 8] = incTime.MemoryConsumption;

            if (!FileContentsMatch("classic.log", "inc.log") || !FileContentsMatch("classic.log", "batch.log"))
            {
                var consoleColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("!!! The output files do not match !!!");
                Console.ForegroundColor = consoleColor;
                Debugger.Break();
            }

            if (iteration > 0)
            {
                sumPoll += batchTime.MainWorkload;
                sumInc += incTime.MainWorkload;
            }
        }

        /// <summary>
        /// Determines whether the file contents in the given files match bytewise
        /// </summary>
        /// <param name="path1">The path of the first file</param>
        /// <param name="path2">The path of the second file</param>
        /// <returns>True, if the file contents match, otherwise false</returns>
        private static bool FileContentsMatch(string path1, string path2)
        {
            using (var fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read))
            {
                using (var fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read))
                {
                    var buffer1 = new byte[sizeof(long)];
                    var buffer2 = new byte[sizeof(long)];

                    int read1, read2;

                    do
                    {
                        read1 = fs1.Read(buffer1, 0, sizeof(long));
                        read2 = fs2.Read(buffer2, 0, sizeof(long));

                        if (BitConverter.ToInt64(buffer1, 0) != BitConverter.ToInt64(buffer2, 0))
                        {
                            return false;
                        }
                    } while (read1 > 0 && read2 > 0);

                    return true;
                }
            }
        }

        /// <summary>
        /// Writes the results to a file
        /// </summary>
        private static void WriteResultsToCsv(int[] sizes, float minRatio, float step, int iterations, int maxRatioIdx, long[, , ,] times, int[, ,] workloadCounts, int[, ,] elementCounts)
        {
            using (var sw = new StreamWriter("results.csv"))
            {
                sw.WriteLine("Size;Workload;Employees;Ratio;Iteration;Classic.Init;Batch.Init;Incremental.Init;Classic.Main;Batch.Main;Incremental.Main;Classic.Memory;Batch.Memory;Incremental.Memory;");
                for (int sizeIdx = 0; sizeIdx < sizes.Length; sizeIdx++)
                {
                    var n = sizes[sizeIdx];
                    for (int ratioIdx = 0; ratioIdx <= maxRatioIdx; ratioIdx++)
                    {
                        for (int iteration = 0; iteration < iterations; iteration++)
                        {
                            sw.Write(n);
                            sw.Write(";");
                            sw.Write(workloadCounts[sizeIdx, ratioIdx, iteration]);
                            sw.Write(";");
                            sw.Write(elementCounts[sizeIdx, ratioIdx, iteration]);
                            sw.Write(";");
                            sw.Write(minRatio + (ratioIdx * step));
                            sw.Write(";");
                            sw.Write(iteration);
                            sw.Write(";");
                            for (int i = 0; i < 9; i++)
                            {
                                sw.Write(times[sizeIdx, ratioIdx, iteration, i]);
                                sw.Write(";");
                            }
                            sw.WriteLine();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A small struct to hold the results
        /// </summary>
        private struct Results
        {
            /// <summary>
            /// The milliseconds taken for the initialization
            /// </summary>
            public long Initialization { get; set; }

            /// <summary>
            /// The miliiseconds taken for the main workload
            /// </summary>
            public long MainWorkload { get; set; }

            /// <summary>
            /// The consumed memory in bytes
            /// </summary>
            public long MemoryConsumption { get; set; }
        }

        /// <summary>
        /// Use a common stopwatch for measurements
        /// Stopwatch internally uses a QueryPerformanceCounter which is more precise than DateTime.Now
        /// </summary>
        private static Stopwatch watch = new Stopwatch();

        /// <summary>
        /// Play the given workload in a classic version and store the results in the given file
        /// </summary>
        /// <param name="workload">The workload</param>
        /// <param name="employees">The amount of employees generated for the workload</param>
        /// <param name="path">The path for the results</param>
        /// <returns>The measurement results</returns>
        private static Results PlayWorkloadClassic(List<WorkloadAction> workload, int employees, string path)
        {
            var sb = new StringBuilder();
            watch.Restart();

            var employeesList = new List<Employee>(employees);

            var pollQuery = CreateClassicQuery(employeesList);

            Console.WriteLine("Running workload on classic interface");

            for (int i = 0; i < employees; i++)
            {
                workload[i].Perform(employeesList, pollQuery, sb);
            }
            watch.Stop();
            var initTime = watch.ElapsedMilliseconds;

            watch.Restart();
            for (int i = employees; i < workload.Count; i++)
            {
                workload[i].Perform(employeesList, pollQuery, sb);
            }

            watch.Stop();
            var main = watch.ElapsedMilliseconds;

            File.WriteAllText(path, sb.ToString());

            Console.WriteLine("Completed. Classic took {0}ms", watch.ElapsedMilliseconds);

            var memory = MemoryMeter.ComputeMemoryConsumption(pollQuery);

            return new Results() { Initialization = initTime, MainWorkload = main, MemoryConsumption = memory };
        }

        /// <summary>
        /// Play the given workload in a batch version and store the results in the given file
        /// </summary>
        /// <param name="workload">The workload</param>
        /// <param name="employees">The amount of employees generated for the workload</param>
        /// <param name="path">The path for the results</param>
        /// <returns>The measurement results</returns>
        private static Results PlayWorkloadBatch(List<WorkloadAction> workload, int employees, string path)
        {
            var sb = new StringBuilder();
            watch.Restart();

            var employeesList = new ObservableList<Employee>();

            var pollQuery = CreateQuery(employeesList);

            Console.WriteLine("Running workload on batch interface");

            for (int i = 0; i < employees; i++)
            {
                workload[i].Perform(employeesList, pollQuery, sb);
            }
            watch.Stop();
            var initTime = watch.ElapsedMilliseconds;

            watch.Restart();
            for (int i = employees; i < workload.Count; i++)
            {
                workload[i].Perform(employeesList, pollQuery, sb);
            }

            watch.Stop();
            var main = watch.ElapsedMilliseconds;

            File.WriteAllText(path, sb.ToString());

            Console.WriteLine("Completed. Batch took {0}ms", watch.ElapsedMilliseconds);
            
            var memory = MemoryMeter.ComputeMemoryConsumption(pollQuery);

            return new Results() { Initialization = initTime, MainWorkload = main, MemoryConsumption = memory };
        }


        /// <summary>
        /// Play the given workload in an incremental manner and store the results in the given file
        /// </summary>
        /// <param name="workload">The workload</param>
        /// <param name="employees">The amount of employees generated for the workload</param>
        /// <param name="path">The path for the results</param>
        /// <returns>The measurement results</returns>
        private static Results PlayWorkloadIncremental(List<WorkloadAction> workload, int employees, string path)
        {
            var sb = new StringBuilder();
            watch.Restart();

            var employeesCollection = new ObservableList<Employee>();

            var incQuery = CreateQuery(employeesCollection).AsNotifiable();

            Console.WriteLine("Running workload on incremental interface");

            for (int i = 0; i < employees; i++)
            {
                workload[i].Perform(employeesCollection, incQuery, sb);
            }
            watch.Stop();
            var initTime = watch.ElapsedMilliseconds;

            watch.Restart();
            for (int i = employees; i < workload.Count; i++)
            {
                workload[i].Perform(employeesCollection, incQuery, sb);
            }

            watch.Stop();
            var main = watch.ElapsedMilliseconds;

            File.WriteAllText(path, sb.ToString());

            Console.WriteLine("Completed. Incremental took {0}ms", watch.ElapsedMilliseconds);

            // Memory meter takes a lot of time for the incremental algorithm
            // Comment this line, if you wish to make runtime measurements, only
            var memory = MemoryMeter.ComputeMemoryConsumption(incQuery);

            return new Results() { Initialization = initTime, MainWorkload = main, MemoryConsumption = memory };
        }
    }
}
