using System.Collections.Generic;
using System.Text;

namespace Expressions_Demo.ScenarioGeneration
{
    /// <summary>
    /// The abstract base class for elements of a generated workload
    /// </summary>
    abstract class WorkloadAction
    {
        /// <summary>
        /// Rund the workload on the given parameters
        /// </summary>
        /// <param name="employees">The employee collection</param>
        /// <param name="query">The query</param>
        /// <param name="log">A log</param>
        public abstract void Perform(IList<Employee> employees, IEnumerable<Employee> query, StringBuilder log);
    }
}
