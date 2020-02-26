using System.ComponentModel;

namespace WPF.UI.ViewModels
{

    public class ViewModelBase : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(params string[] propertiesNames)
        {
            foreach (var propertyName in propertiesNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

}