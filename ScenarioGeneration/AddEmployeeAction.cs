using System.Collections.Generic;
using System.Text;

namespace Expressions_Demo.ScenarioGeneration
{
    /// <summary>
    /// Represents a class to add an employee to the employees list
    /// </summary>
    class AddEmployeeAction : WorkloadAction
    {
        /// <summary>
        /// The name for the new employee
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The team for the new employee
        /// </summary>
        public string Team { get; set; }

        /// <summary>
        /// The work items for the new employee
        /// </summary>
        public int WorkItems { get; set; }

        /// <summary>
        /// Adds the employee to the employees collection
        /// </summary>
        public override void Perform(IList<Employee> employees, IEnumerable<Employee> query, StringBuilder log)
        {
            var employee = new Employee()
            {
                Name = Name,
                Team = Team,
                WorkItems = WorkItems
            };
            employees.Add(employee);
            log.AppendFormat("Appended Employee {{Name={0},Team={1},WorkItems={2}}}", Name, Team, WorkItems);
            log.AppendLine();
        }
    }
}
