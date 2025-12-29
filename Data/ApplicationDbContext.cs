using Microsoft.EntityFrameworkCore;
using Elearning.Models;

namespace Elearning.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Parameterless constructor for tools that may need it
        public ApplicationDbContext()
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<Answer> Answers { get; set; } = null!;
        public DbSet<Result> Results { get; set; } = null!;
        public DbSet<Certificate> Certificates { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback connection string for tools/CLI when DI isn't available
                optionsBuilder.UseMySql("server=localhost;port=3306;database=elearning;user=root;password=;", ServerVersion.AutoDetect("server=localhost;port=3306;database=elearning;user=root;password=;"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // Map Lesson.Order to OrderIndex column to match existing DB
            modelBuilder.Entity<Lesson>().Property(l => l.Order).HasColumnName("OrderIndex");

            // If the existing Certificates table uses column name PdfPath, map the model property FilePath to it
            modelBuilder.Entity<Certificate>().Property(c => c.FilePath).HasColumnName("PdfPath");

            // Relationships
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(qt => qt.Quiz)
                .HasForeignKey(qt => qt.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Results)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Quizzes)
                .WithOne(q => q.Course)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}