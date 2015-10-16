using System.Collections.Generic;
using System.Text;

namespace Expressions_Demo.ScenarioGeneration
{
    /// <summary>
    /// Represents an action to update the team of an employee
    /// </summary>
    class UpdateTeamAction : WorkloadAction
    {
        /// <summary>
        /// The new team for the chosen employee
        /// </summary>
        public string NewTeam { get; set; }

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
            employee.Team = NewTeam;
            log.AppendFormat("Assigning employee {0} to team {1}", employee.Name, NewTeam);
            log.AppendLine();
        }
    }
}
