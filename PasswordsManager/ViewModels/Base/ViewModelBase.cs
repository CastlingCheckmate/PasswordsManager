using System.ComponentModel;

namespace PasswordsManager.UI.ViewModels.Base
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