namespace MotionAlarm.Models;

public class AlarmEvent
{
    public int Id { get; set; }
    public DateTime TriggeredAtUtc { get; set; }
    public int TriggerCount { get; set; }
    public bool SirenEnabled { get; set; }
    public int Sensitivity { get; set; }
    public string SirenStatusText => SirenEnabled ? "Сирена: Увімкнена" : "Сирена: Вимкнена";
}
