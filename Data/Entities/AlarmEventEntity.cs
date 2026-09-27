namespace MotionAlarm.Data.Entities;

public class AlarmEventEntity
{
    public int Id { get; set; }
    public DateTime TriggeredAtUtc { get; set; }
    public int TriggerCount { get; set; } = 1;
    public bool SirenEnabled { get; set; }
    public int Sensitivity { get; set; }
    public Guid ConcurrencyToken { get; set; }
}
