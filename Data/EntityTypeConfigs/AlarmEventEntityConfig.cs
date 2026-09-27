using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotionAlarm.Data.Entities;

namespace MotionAlarm.Data.EntityTypeConfigs;

internal class AlarmEventEntityConfig : IEntityTypeConfiguration<AlarmEventEntity>
{
    public void Configure(EntityTypeBuilder<AlarmEventEntity> entity)
    {
        entity.HasKey(e => e.Id);

        entity.HasIndex(x => x.TriggeredAtUtc);

        entity.Property(x => x.ConcurrencyToken)
            .IsConcurrencyToken();
    }
}