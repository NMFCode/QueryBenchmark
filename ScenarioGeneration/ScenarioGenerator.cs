using System;
using System.Collections.Generic;

namespace Expressions_Demo.ScenarioGeneration
{
    /// <summary>
    /// Generates scenarios for the example query
    /// </summary>
    class ScenarioGenerator
    {
        /// <summary>
        /// Generate a scenario with the given set of parameters
        /// </summary>
        /// <param name="ratio">The ratio of model queries to model manipulation elements</param>
        /// <param name="teamCount">The amount of generated teams</param>
        /// <param name="actions">The amount of model manipulation actions</param>
        /// <param name="employees">The amount of employees generated</param>
        /// <param name="fastCount">Determines whether fast count is allowed (default true)</param>
        /// <returns>The generated workload</returns>
        public static List<WorkloadAction> GenerateScenario(float ratio, int teamCount, int actions, out int employees, bool fastCount = true)
        {
            var workload = new List<WorkloadAction>();
            var rand = new Random();
            var idx = 1;
            for (int i = 1; i <= teamCount; i++)
            {
                for (int j = 0; j < rand.Next(5, 100); j++)
                {
                    workload.Add(new AddEmployeeAction()
                    {
                        Name = "Employee" + idx.ToString(),
                        Team = "Team" + i.ToString(),
                        WorkItems = rand.Next(3, 100)
                    });
                    idx++;
                }
            }
            employees = idx;
            idx--;
            var read = new EvaluateQueryAction() { FastCount = fastCount };
            var counter = 0f;
            for (int i = 0; i < actions; i++)
            {
                counter += ratio;
                while (counter > 1.0f)
                {
                    counter -= 1.0f;
                    workload.Add(read);
                }
                var result = rand.NextDouble();
                if (result < 0.4)
                {
                    workload.Add(new UpdateWorkItemsAction()
                    {
                        Index = rand.Next(idx),
                        NewWorkItems = rand.Next(4, 10)
                    });
                }
                else if (result < 0.6)
                {
                    workload.Add(new UpdateWorkItemsAction()
                    {
                        Index = rand.Next(idx),
                        NewWorkItems = rand.Next(80, 200)
                    });
                }
                else if (result < 0.8)
                {
                    workload.Add(new UpdateTeamAction()
                    {
                        Index = rand.Next(idx),
                        NewTeam = "Team" + rand.Next(1, teamCount + 1).ToString()
                    });
                }
                else if (result < 0.9)
                {
                    var empIdx = rand.Next(idx);
                    idx--;
                    workload.Add(new DeleteEmployeeAction()
                    {
                        Index = rand.Next(empIdx)
                    });
                }
                else
                {
                    workload.Add(new AddEmployeeAction()
                    {
                        Name = "Employee" + idx.ToString(),
                        Team = "Team" + i.ToString(),
                        WorkItems = rand.Next(3, 100)
                    });
                    idx++;
                }
            }
            return workload;
        }

    }
}
