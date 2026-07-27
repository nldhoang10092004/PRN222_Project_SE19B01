using CoreLibrary.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreLibrary.Data
{
    public partial class AppDbContext
    {
        public const string DefaultConnectionName = "DefaultConnection";

        public virtual DbSet<StudentExerciseResult> StudentExerciseResults { get; set; }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentExerciseResult>(entity =>
            {
                entity.HasKey(e => e.ResultId);
                entity.ToTable("StudentExerciseResults");

                entity.HasOne(d => d.Student)
                    .WithMany()
                    .HasForeignKey(d => d.StudentId);

                entity.HasOne(d => d.Exercise)
                    .WithMany()
                    .HasForeignKey(d => d.ExerciseId);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer($"Name={DefaultConnectionName}");
            }
        }
    }
}
