using System.ComponentModel;

namespace Devdeb.WPF
{
    public class NotifyingProperty<TValue> : INotifyPropertyChanged
    {
        private TValue _value;

        public NotifyingProperty(TValue value) => _value = value;
        public NotifyingProperty() => _value = default(TValue);

        public TValue Value { get => _value; set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); } }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
