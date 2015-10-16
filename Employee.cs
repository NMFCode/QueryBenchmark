using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Expressions_Demo
{
    /// <summary>
    /// Represents an employee
    /// </summary>
    class Employee : INotifyPropertyChanged
    {
        private string team;
        private string name;
        private int workItems;

        /// <summary>
        /// The team the employee is assigned to
        /// </summary>
        public string Team
        {
            get { return team; }
            set { SetProperty(ref team, value); }
        }

        /// <summary>
        /// The employees name
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        /// <summary>
        /// The work items assigned to the employee
        /// </summary>
        public int WorkItems
        {
            get { return workItems; }
            set { SetProperty(ref workItems, value); }
        }

        /// <summary>
        /// Sets the given property
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <param name="field">The backing field for the property</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The name of the property</param>
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event for the given property
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Is fired whenever a property changes its value
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
