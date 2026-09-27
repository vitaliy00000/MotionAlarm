using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotionAlarm.Data.Entities;

namespace MotionAlarm.Data.EntityTypeConfigs;

internal class SettingsEntityConfig : IEntityTypeConfiguration<SettingsEntity>
{
    public void Configure(EntityTypeBuilder<SettingsEntity> entity)
    {
        entity.HasKey(x => x.Id);
    }
}