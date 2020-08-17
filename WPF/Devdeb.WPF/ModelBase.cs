using System.Collections.Generic;
using System.ComponentModel;

namespace Devdeb.WPF
{
    public abstract class ModelBase
    {
        private readonly List<INotifyPropertyChanged> _propetries;

        protected ModelBase()
        {
            _propetries = new List<INotifyPropertyChanged>();
            Configure();
        }
        protected abstract void Configure();

        protected void AddProperty(object property) => _propetries.Add(new NotifyingProperty<object>(property));
    }
}