using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expressions_Demo.ScenarioGeneration
{
    /// <summary>
    /// Represents a workload action to evaluate the query and measure its count
    /// </summary>
    class EvaluateQueryAction : WorkloadAction
    {
        /// <summary>
        /// Determines whether the count may be determined through ICollection interface
        /// </summary>
        public bool FastCount { get; set; }

        /// <summary>
        /// Evaluate the query count and write the result to the log
        /// </summary>
        public override void Perform(IList<Employee> employees, IEnumerable<Employee> query, StringBuilder log)
        {
            var matches = 0;
            if (FastCount)
            {
                matches = query.Count();
            }
            else
            {
                foreach (var item in query)
                {
                    matches++;
                }
            }
            log.AppendFormat("Query has {0} matches", matches);
            log.AppendLine();
        }
    }
}
