using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MotionAlarm.PageModels;

public abstract class PageModel : INotifyPropertyChanged
{
    #region States

    public const string STATE_IDLE = nameof(STATE_IDLE);
    public const string STATE_ERROR = nameof(STATE_ERROR);
    public const string STATE_LOADING = nameof(STATE_LOADING);
    public const string STATE_NO_ITEMS = nameof(STATE_NO_ITEMS);
    public const string STATE_CONTENT = nameof(STATE_CONTENT);

    #endregion

    private string _currentState = STATE_IDLE;
    private string _errorMsg = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentState
    {
        get => _currentState;
        set
        {
            if (SetProperty(ref _currentState, value))
            {
                // Important: bindings such as [STATE_LOADING]
                // depend on CurrentState.
                OnPropertyChanged("Item");
            }
        }
    }

    public string ErrorMsg
    {
        get => _errorMsg;
        set => SetProperty(ref _errorMsg, value);
    }

    public bool this[string states]
    {
        get
        {
            var stateList = states
                .Split('-', ';', StringSplitOptions.RemoveEmptyEntries);

            return stateList.Contains(
                CurrentState,
                StringComparer.InvariantCultureIgnoreCase);
        }
    }

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