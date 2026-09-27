using CommunityToolkit.Mvvm.Input;
using MotionAlarm.Models;

namespace MotionAlarm.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}