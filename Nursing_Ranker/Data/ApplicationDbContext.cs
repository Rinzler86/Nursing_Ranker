using Microsoft.EntityFrameworkCore;
using Nursing_Ranker.Models.ClassModels;

namespace Nursing_Ranker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Existing Users table – keep this in place.
        public DbSet<User> Users { get; set; }

        // New DbSets for the nursing ranker entities
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<ApplicantCourse> ApplicantCourses { get; set; }
        public DbSet<ApplicantRequirement> ApplicantRequirements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure unique emails for users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Set default value for FavColor
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.FavColor)
                    .HasDefaultValue("white");
            });

            // Ensure precision where needed
            modelBuilder.Entity<ApplicantRequirement>()
                .Property(ar => ar.Score)
                .HasPrecision(5, 2); // Adjust as needed for decimal scores

            // Configure relationships
            modelBuilder.Entity<ApplicantCourse>()
                .HasOne(ac => ac.Applicant)
                .WithMany(a => a.ApplicantCourses)
                .HasForeignKey(ac => ac.ApplicantId);

            modelBuilder.Entity<ApplicantRequirement>()
                .HasOne(ar => ar.Applicant)
                .WithMany(a => a.ApplicantRequirements)
                .HasForeignKey(ar => ar.ApplicantId);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.ExtraCredits)
                .HasDefaultValue(0);

            // Seed Data Removed: No need for pre-defined global courses or requirements.
        }
    }
}






