using Microsoft.EntityFrameworkCore;
using Wtrfll.Server.Slices.Sessions.Domain;

namespace Wtrfll.Server.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionParticipant> SessionParticipants => Set<SessionParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(session => session.Id);
            entity.Property(session => session.ShortCode)
                .HasMaxLength(12)
                .IsRequired();
            entity.Property(session => session.Name)
                .HasMaxLength(160)
                .IsRequired();
            entity.HasIndex(session => session.ShortCode).IsUnique();
            entity.Property(session => session.ControllerJoinCode)
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(session => session.DisplayJoinCode)
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(session => session.Status)
                .HasConversion<string>()
                .HasMaxLength(32);
            entity.Property(session => session.ScheduledAt);
        });

        modelBuilder.Entity<SessionParticipant>(entity =>
        {
            entity.HasKey(participant => participant.Id);
            entity.Property(participant => participant.Role)
                .HasConversion<string>()
                .HasMaxLength(32);
            entity.HasIndex(participant => new { participant.SessionId, participant.Role });
            entity.HasOne(participant => participant.Session)
                .WithMany(session => session.Participants)
                .HasForeignKey(participant => participant.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

