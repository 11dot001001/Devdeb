using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devdeb.WPF
{
    public class NotifyingProperty<TValue> : INotifyPropertyChanged
    {
        private TValue _value;

        public NotifyingProperty(TValue value) => _value = value;

        public TValue Value { get => _value; set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); } }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
