using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MotionAlarm.PageModels;

public abstract class PageModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(
        ref T backingField,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
        {
            return false;
        }

        backingField = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    protected void OnPropertiesChanged(
        params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            OnPropertyChanged(propertyName);
        }
    }

    protected static void ReevaluateCommand(ICommand command) 
    {
        if (command is Command mauiCommand)
        {
            mauiCommand.ChangeCanExecute(); 
        } 
    }
}