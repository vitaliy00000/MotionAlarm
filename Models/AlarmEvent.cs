using MotionAlarm.Localization;

namespace MotionAlarm.Models;

public class AlarmEvent
{
    public int Id { get; set; }
    public DateTime TriggeredAtUtc { get; set; }
    public int TriggerCount { get; set; }
    public bool SirenEnabled { get; set; }
    public int Sensitivity { get; set; }
    public string SensitivityText => LocalizationResources.Instance.Format("AlarmLog_Sensitivity", Sensitivity); 
    public string SirenStatusText => SirenEnabled ? LocalizationResources.Instance["AlarmLog_SirenEnabled"] : LocalizationResources.Instance["AlarmLog_SirenDisabled"];
}
