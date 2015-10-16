using System.Collections.Generic;
using System.Text;

namespace Expressions_Demo.ScenarioGeneration
{
    /// <summary>
    /// Represents the deletion of an employee
    /// </summary>
    class DeleteEmployeeAction : WorkloadAction
    {
        /// <summary>
        /// The index of the empoloyee that should be removed
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Deletes the employee
        /// </summary>
        public override void Perform(IList<Employee> employees, IEnumerable<Employee> query, StringBuilder log)
        {
            var employee = employees[Index];
            employees.RemoveAt(Index);
            log.AppendFormat("Delete Employee {0}", employee.Name);
            log.AppendLine();
        }
    }
}
