using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Seeds
{
    public abstract class SyncScheduleSeed
    {
        public static void Seed(EntityTypeBuilder<SyncSchedule> builder)
        {
            builder.HasData(
                new SyncSchedule
                {
                    JobKey = "UpdateProviders",
                    CronExpression = "0 0 0 * * ?", // every day at midnight
                        Enabled = true,
                    LastModifiedUtc = DateTime.UtcNow
                },
                new SyncSchedule
                {
                    JobKey = "UpdateMaterials",
                    CronExpression = "0 0 0 * * ?", // every day at midnight
                    Enabled = true,
                    LastModifiedUtc = DateTime.UtcNow
                },
                new SyncSchedule
                {
                    JobKey = "UpdateMerchandiseEntry",
                    CronExpression = "0 0 0 * * ?", // every day at midnight
                    Enabled = true,
                    LastModifiedUtc = DateTime.UtcNow
                },
                new SyncSchedule
                {
                    JobKey = "UpdateStockPhotoValuations",
                    CronExpression = "0 0 0 * * ?", // every day at midnight
                    Enabled = true,
                    LastModifiedUtc = DateTime.UtcNow
                },
                new SyncSchedule
                {
                    JobKey = "UpdateWarehouses",
                    CronExpression = "0 0 0 * * ?", // every day at midnight
                    Enabled = true,
                    LastModifiedUtc = DateTime.UtcNow
                }
            );
        }
    }
}