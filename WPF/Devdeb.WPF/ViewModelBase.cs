using System;
using System.ComponentModel;

namespace Devdeb.WPF
{
    public abstract class ViewModelBase<TModelVM> : INotifyPropertyChanged where TModelVM : ModelBase
    {
        private TModelVM _model;

        public ViewModelBase(TModelVM model) => _model = model;

        public TModelVM Model
        {
            get => _model;
            set
            {
                _model = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
                ModelChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ModelChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}