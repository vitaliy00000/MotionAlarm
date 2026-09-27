namespace MotionAlarm.Models;

public class AlarmSettings
{
    public bool SirenEnabled { get; set; } = true;
    public int Sensitivity { get; set; } = 5; // 1..5
    public int TriggerWindowMs { get; set; } = 200;
}
