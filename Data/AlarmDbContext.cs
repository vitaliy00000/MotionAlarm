using Microsoft.EntityFrameworkCore;
using MotionAlarm.Data.Entities;
using MotionAlarm.Data.EntityTypeConfigs;

namespace MotionAlarm.Data;

public class AlarmDbContext : DbContext
{
    public AlarmDbContext(DbContextOptions<AlarmDbContext> options) 
        : base(options) 
    {
    }

    public DbSet<AlarmEventEntity> AlarmEvents => Set<AlarmEventEntity>();
    public DbSet<SettingsEntity> Settings => Set<SettingsEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AlarmEventEntityConfig());
        modelBuilder.ApplyConfiguration(new SettingsEntityConfig());
    }
}
