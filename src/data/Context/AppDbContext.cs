using domain.Contracts.Enums;
using domain.Interfaces.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace data.Context;

/*
 * migrations: dotnet ef migrations add initial --project ../data --context AppDbContext --output-dir ../data/Migrations
 * 
 */

public class AppDbContext : DbContext, IUnitOfWork
{
    private readonly IOutboxMessageBuilderService _outboxMessageBuilderService;
    public DbContextOptions<AppDbContext> Options { get; init; }

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IOutboxMessageBuilderService outboxMessageBuilderService) : base(options)
    {
        Options = options;
        _outboxMessageBuilderService = outboxMessageBuilderService;
    }

    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobStage> JobStages => Set<JobStage>();
    public DbSet<JobTask> JobTasks => Set<JobTask>();
    public DbSet<Command> Commands => Set<Command>();
    public DbSet<AllowedFileType> AllowedFileTypes => Set<AllowedFileType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BaseName).HasMaxLength(800).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BaseName).IsUnique();
            entity.HasMany(e => e.Jobs)
                .WithOne(j => j.MediaAsset)
                .HasForeignKey(j => j.MediaAssetId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Filename).HasMaxLength(800);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.State).HasConversion(new EnumToStringConverter<State>());


            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Stages)
                .WithOne(s => s.Job)
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobStage>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.State).HasConversion(new EnumToStringConverter<State>());

            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Tasks)
                .WithOne(t => t.JobStage)
                .HasForeignKey(t => t.JobStageId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<JobTask>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.State).HasConversion(new EnumToStringConverter<State>());

            entity.HasOne(e => e.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(t => t.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Command>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status).HasConversion(new EnumToStringConverter<CommandStatus>());
        });

        modelBuilder.Entity<AllowedFileType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Extension).HasMaxLength(5).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Comment).HasMaxLength(800);

            entity.HasKey(e => e.Id);
        });
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        OutboxPopulate();
        await base.SaveChangesAsync(cancellationToken);
        CleanPendings();
    }

    private void OutboxPopulate()
    {
        var pendingEventsEntities = this.ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.Events != null && x.Entity.Events.Any());

        if (pendingEventsEntities == null)
            return;

        var pendingEvents = pendingEventsEntities.SelectMany(x => x.Entity.Events).ToList();

        var messages = _outboxMessageBuilderService.Build(pendingEvents);

        foreach (var message in messages)
            Commands.Add(message);
    }

    private void CleanPendings()
    {
        var pendingEventsEntities = this.ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.Events != null && x.Entity.Events.Any());
        pendingEventsEntities.ToList().ForEach(e
            => e.Entity.RemoveAllEvents());
    }
}
