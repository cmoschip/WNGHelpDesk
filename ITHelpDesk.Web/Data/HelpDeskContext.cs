using ITHelpDesk.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Web.Data
{
    public class HelpDeskContext : DbContext
    {
        public HelpDeskContext(DbContextOptions<HelpDeskContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistory> TicketHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Ticket entity
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Tickets");
                entity.HasKey(e => e.TicketId);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Status).HasDefaultValue("New");
                entity.Property(e => e.Priority).HasDefaultValue("Medium");

                // Configure relationships
                entity.HasMany(e => e.Comments)
                    .WithOne(e => e.Ticket)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.History)
                    .WithOne(e => e.Ticket)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TicketComment entity
            modelBuilder.Entity<TicketComment>(entity =>
            {
                entity.ToTable("TicketComments");
                entity.HasKey(e => e.CommentId);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.CommentType).HasDefaultValue("Comment");
                entity.Property(e => e.IsInternal).HasDefaultValue(false);
            });

            // Configure TicketHistory entity
            modelBuilder.Entity<TicketHistory>(entity =>
            {
                entity.ToTable("TicketHistory");
                entity.HasKey(e => e.HistoryId);
                entity.Property(e => e.ChangedDate).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}
