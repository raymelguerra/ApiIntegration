using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.PostgreSQL;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class SyncDbContext(DbContextOptions<SyncDbContext> opts) : DbContext(opts)
    {

        public DbSet<SyncSchedule> SyncSchedules { get; set; }
        public DbSet<ExecutionHistory> ExecutionHistories { get; set; }
        public DbSet<FailedItem> FailedItems { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            
            mb.HasDefaultSchema("main_app");
            
            mb.Entity<SyncSchedule>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.JobKey).IsRequired();
                b.Property(x => x.CronExpression).IsRequired();
                Seeds.SyncScheduleSeed.Seed(b);
            });
            mb.Entity<ExecutionHistory>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.JobKey).IsRequired();
            });
            mb.Entity<FailedItem>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Payload).HasColumnType("jsonb");
            });
            
            mb.Entity<FailedItem>()
                .HasOne<ExecutionHistory>()
                .WithMany()
                .HasForeignKey(fi => fi.ExecutionHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.AddQuartz(builder => builder.UsePostgreSql());
        }
    }
}