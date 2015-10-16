using System.Collections.Generic;
using System.Text;

namespace Expressions_Demo.ScenarioGeneration
{
    /// <summary>
    /// Represents an action to update the work items of an employee
    /// </summary>
    class UpdateWorkItemsAction : WorkloadAction
    {
        /// <summary>
        /// The new amount of work items
        /// </summary>
        public int NewWorkItems { get; set; }

        /// <summary>
        /// The index of the employee
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Performs the change
        /// </summary>
        public override void Perform(IList<Employee> employees, IEnumerable<Employee> query, StringBuilder log)
        {
            var employee = employees[Index];
            employee.WorkItems = NewWorkItems;
            log.AppendFormat("Assigning employee {0} {1} work items", employee.Name, NewWorkItems);
            log.AppendLine();
        }
    }
}
