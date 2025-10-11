using Domain.Entities;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Seeds
{
    public abstract class SyncScheduleSeed
    {
        public static void Seed(EntityTypeBuilder<SyncSchedule> builder)
        {
            // Use a static date instead of DateTime.UtcNow to avoid model changes
            DateTime seedDate = new(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.HasData(
            new SyncSchedule
            {
                JobKey = "UpdateProviders",
                CronExpression = "0 0 0 * * ?",// every day at midnight
                Enabled = true,
                LastModifiedUtc = seedDate
            },
            new SyncSchedule
            {
                JobKey = "UpdateMaterials",
                CronExpression = "0 0 0 * * ?",// every day at midnight
                Enabled = true,
                LastModifiedUtc = seedDate
            },
            new SyncSchedule
            {
                JobKey = "UpdateMerchandiseEntry",
                CronExpression = "0 0 0 * * ?",// every day at midnight
                Enabled = true,
                LastModifiedUtc = seedDate
            },
            new SyncSchedule
            {
                JobKey = "UpdateStockPhotoValuations",
                CronExpression = "0 0 0 * * ?",// every day at midnight
                Enabled = true,
                LastModifiedUtc = seedDate
            },
            new SyncSchedule
            {
                JobKey = "UpdateWarehouses",
                CronExpression = "0 0 0 * * ?",// every day at midnight
                Enabled = true,
                LastModifiedUtc = seedDate
            }
            );
        }
    }
}