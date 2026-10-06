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

    public DbSet<MediaPackage> MediaPackages => Set<MediaPackage>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Command> Commands => Set<Command>();
    public DbSet<AllowedFileType> AllowedFileTypes => Set<AllowedFileType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MediaPackage>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Identifier).HasMaxLength(500).IsRequired();
            entity.Property(e => e.State).HasConversion(new EnumToStringConverter<HealthState>());

            entity.HasIndex(e => e.Identifier).IsUnique();
            entity.HasMany(e => e.Assets)
                .WithOne(j => j.MediaPackage)
                .HasForeignKey(j => j.MediaPackageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Filename).HasMaxLength(550).IsRequired();

            entity.HasIndex(e => new { e.MediaPackageId, e.Filename }).IsUnique();
            entity.HasMany(e => e.Jobs)
                .WithOne(j => j.MediaAsset)
                .HasForeignKey(j => j.MediaAssetId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.State).HasConversion(new EnumToStringConverter<State>());
            entity.Property(e => e.JobType).HasConversion(new EnumToStringConverter<JobType>());
            entity.Property(e => e.Task).HasConversion(new EnumToStringConverter<JobTask>());
        });

        modelBuilder.Entity<Command>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Status).HasConversion(new EnumToStringConverter<CommandStatus>());
            entity.Property(e => e.ErorMessage).HasMaxLength(800);
        });

        modelBuilder.Entity<AllowedFileType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Extension).HasMaxLength(5).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(30);
            entity.Property(e => e.Chain).HasMaxLength(200);
            entity.Property(e => e.Comment).HasMaxLength(800);

            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Extension).IsUnique();
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
