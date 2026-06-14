using System;
using System.Collections.Generic;
using CoreLibrary.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreLibrary.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<AnswerOption> AnswerOptions { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseReview> CourseReviews { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<Flashcard> Flashcards { get; set; }

    public virtual DbSet<JlptLevel> JlptLevels { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonProgress> LessonProgresses { get; set; }

    public virtual DbSet<Mentor> Mentors { get; set; }

    public virtual DbSet<PlacementTest> PlacementTests { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    public virtual DbSet<ReviewResponse> ReviewResponses { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentMembership> StudentMemberships { get; set; }

    public virtual DbSet<StudentPlacementResult> StudentPlacementResults { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<VoucherUsage> VoucherUsages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Vietnamese_CI_AS");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Accounts__349DA5A60FC027CE");

            entity.HasIndex(e => e.Email, "IX_Accounts_Email");

            entity.HasIndex(e => e.Email, "UQ__Accounts__A9D105349211492E").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.Property(e => e.ResetToken).HasMaxLength(512);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Student");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admins__719FE48814375614");

            entity.Property(e => e.AdminId).ValueGeneratedNever();
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.FullName).HasMaxLength(150);

            entity.HasOne(d => d.AdminNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.AdminId)
                .HasConstraintName("FK_Admins_Account");
        });

        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__AnswerOp__92C7A1FFDDAEFD47");

            entity.Property(e => e.AnswerText).HasMaxLength(500);

            entity.HasOne(d => d.Question).WithMany(p => p.AnswerOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__AnswerOpt__Quest__73BA3083");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A77637D21D");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.CourseCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_CreatedBy_Mentor");

            entity.HasOne(d => d.Level).WithMany(p => p.Courses)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Courses__LevelId__00200768");

            entity.HasOne(d => d.Mentor).WithMany(p => p.CourseMentors)
                .HasForeignKey(d => d.MentorId)
                .HasConstraintName("FK__Courses__MentorI__01142BA1");
        });

        modelBuilder.Entity<CourseReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__CourseRe__74BC79CEF133F038");

            entity.HasIndex(e => e.CourseId, "IX_CourseReviews_Course");

            entity.HasIndex(e => new { e.StudentId, e.CourseId }, "UQ__CourseRe__5E57FC829E47B463").IsUnique();

            entity.Property(e => e.Comment).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsVisible).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseReviews)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseRev__Cours__3C34F16F");

            entity.HasOne(d => d.Student).WithMany(p => p.CourseReviews)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseRev__Stude__3B40CD36");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F68771B865FEFE0");

            entity.HasIndex(e => e.CourseId, "IX_Enrollments_Course");

            entity.HasIndex(e => e.StudentId, "IX_Enrollments_Student");

            entity.HasIndex(e => new { e.StudentId, e.CourseId }, "UQ__Enrollme__5E57FC82BD5BA24D").IsUnique();

            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK__Enrollmen__Assig__0B91BA14");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Cours__0A9D95DB");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Stude__09A971A2");
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.ExerciseId).HasName("PK__Exercise__A074AD2FB65940A2");

            entity.HasIndex(e => e.CourseId, "IX_Exercises_Course");

            entity.Property(e => e.AudioUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ExerciseType).HasMaxLength(20);
            entity.Property(e => e.StrokeOrderUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Course).WithMany(p => p.Exercises)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Exercises__Cours__1EA48E88");
        });

        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.HasKey(e => e.FlashcardId).HasName("PK__Flashcar__D36F8572FE5A7DA8");

            entity.HasIndex(e => new { e.StudentId, e.NextReviewAt }, "IX_Flashcards_Student");

            entity.Property(e => e.BackText).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Efactor)
                .HasDefaultValue(2.5m)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("EFactor");
            entity.Property(e => e.FrontText).HasMaxLength(300);

            entity.HasOne(d => d.Course).WithMany(p => p.Flashcards)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Flashcard__Cours__3493CFA7");

            entity.HasOne(d => d.Student).WithMany(p => p.Flashcards)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Flashcard__Stude__339FAB6E");
        });

        modelBuilder.Entity<JlptLevel>(entity =>
        {
            entity.HasKey(e => e.LevelId).HasName("PK__JlptLeve__09F03C263BEB838F");

            entity.HasIndex(e => e.LevelName, "UQ__JlptLeve__9EF3BE7B346BA885").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.LevelName).HasMaxLength(10);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lessons__B084ACD0D6A002C9");

            entity.HasIndex(e => e.CourseId, "IX_Lessons_Course");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.LessonType)
                .HasMaxLength(20)
                .HasDefaultValue("Video");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.VideoUrl).HasMaxLength(500);

            entity.HasOne(d => d.Course).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Lessons__CourseI__0F624AF8");
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasKey(e => e.ProgressId).HasName("PK__LessonPr__BAE29CA583C0EE0F");

            entity.ToTable("LessonProgress");

            entity.HasIndex(e => new { e.StudentId, e.LessonId }, "UQ__LessonPr__29CD615577E7458A").IsUnique();

            entity.Property(e => e.LastAccessedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonProgresses)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("FK__LessonPro__Lesso__18EBB532");

            entity.HasOne(d => d.Student).WithMany(p => p.LessonProgresses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LessonPro__Stude__17F790F9");
        });

        modelBuilder.Entity<Mentor>(entity =>
        {
            entity.HasKey(e => e.MentorId).HasName("PK__Mentors__053B7E988C2BF34E");

            entity.Property(e => e.MentorId).ValueGeneratedNever();
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Expertise).HasMaxLength(300);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.MentorNavigation).WithOne(p => p.Mentor)
                .HasForeignKey<Mentor>(d => d.MentorId)
                .HasConstraintName("FK_Mentors_Account");
        });

        modelBuilder.Entity<PlacementTest>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__Placemen__8CC33160000642CF");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Duration).HasDefaultValue(30);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PassScore).HasDefaultValue(60);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PlacementTests)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Placement__Creat__693CA210");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06FACBD081A4D");

            entity.HasIndex(e => e.ExerciseId, "IX_Questions_Exercise");

            entity.HasIndex(e => e.QuizId, "IX_Questions_Quiz");

            entity.Property(e => e.Points).HasDefaultValue(1);
            entity.Property(e => e.QuestionType)
                .HasMaxLength(30)
                .HasDefaultValue("MultipleChoice");

            entity.HasOne(d => d.Exercise).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Questions_Exercise");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Questions_Quiz");

            entity.HasOne(d => d.Test).WithMany(p => p.Questions)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Questions__TestI__6E01572D");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__Quizzes__8B42AE8E5A0B999C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.PassScore).HasDefaultValue(60);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Course).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quizzes__CourseI__25518C17");
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId).HasName("PK__QuizAtte__891A68E6E164C635");

            entity.HasIndex(e => e.StudentId, "IX_QuizAttempts_Student");

            entity.Property(e => e.StartedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuizAttem__QuizI__2CF2ADDF");

            entity.HasOne(d => d.Student).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuizAttem__Stude__2BFE89A6");
        });

        modelBuilder.Entity<ReviewResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__ReviewRe__1AAA646C64AA8ED8");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Response).HasMaxLength(1000);

            entity.HasOne(d => d.Responder).WithMany(p => p.ReviewResponses)
                .HasForeignKey(d => d.ResponderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReviewRes__Respo__43D61337");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewResponses)
                .HasForeignKey(d => d.ReviewId)
                .HasConstraintName("FK__ReviewRes__Revie__42E1EEFE");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B9972E4BBED");

            entity.Property(e => e.StudentId).ValueGeneratedNever();
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.StudentNavigation).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.StudentId)
                .HasConstraintName("FK_Students_Account");
        });

        modelBuilder.Entity<StudentMembership>(entity =>
        {
            entity.HasKey(e => e.MembershipId).HasName("PK__StudentM__92A78679428EA419");

            entity.HasIndex(e => new { e.StudentId, e.IsActive, e.EndDate }, "IX_Memberships_Active");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StartDate).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Plan).WithMany(p => p.StudentMemberships)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentMe__PlanI__60A75C0F");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentMemberships)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentMe__Stude__5FB337D6");

            entity.HasOne(d => d.Transaction).WithMany(p => p.StudentMemberships)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentMe__Trans__619B8048");
        });

        modelBuilder.Entity<StudentPlacementResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__StudentP__9769020859E0DBB1");

            entity.HasIndex(e => new { e.StudentId, e.TestId }, "UQ__StudentP__2A09188ED6B93402").IsUnique();

            entity.Property(e => e.StartedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.RecommendedLevel).WithMany(p => p.StudentPlacementResults)
                .HasForeignKey(d => d.RecommendedLevelId)
                .HasConstraintName("FK__StudentPl__Recom__7C4F7684");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentPlacementResults)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentPl__Stude__787EE5A0");

            entity.HasOne(d => d.Test).WithMany(p => p.StudentPlacementResults)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentPl__TestI__797309D9");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Subscrip__755C22B7DA59F4C5");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PlanName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__55433A6BE70AB901");

            entity.HasIndex(e => e.PaymentStatus, "IX_Transactions_Status");

            entity.HasIndex(e => e.StudentId, "IX_Transactions_Student");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.FinalAmount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.OriginalAmount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentRef).HasMaxLength(200);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Plan).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__PlanI__571DF1D5");

            entity.HasOne(d => d.Student).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Stude__5629CD9C");

            entity.HasOne(d => d.Voucher).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK__Transacti__Vouch__5812160E");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__Vouchers__3AEE792182ADE823");

            entity.HasIndex(e => new { e.IsActive, e.ExpiresAt }, "IX_Vouchers_ActiveExpiry");

            entity.HasIndex(e => e.Code, "IX_Vouchers_Code");

            entity.HasIndex(e => e.Code, "UQ__Vouchers__A25C5AA7CF97A689").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.DiscountType)
                .HasMaxLength(10)
                .HasDefaultValue("Percent");
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxDiscountCap).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.MaxUsesPerUser).HasDefaultValue(1);
            entity.Property(e => e.MinOrderValue).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.ApplicablePlan).WithMany(p => p.Vouchers)
                .HasForeignKey(d => d.ApplicablePlanId)
                .HasConstraintName("FK__Vouchers__Applic__46E78A0C");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Vouchers)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vouchers__Create__4AB81AF0");
        });

        modelBuilder.Entity<VoucherUsage>(entity =>
        {
            entity.HasKey(e => e.UsageId).HasName("PK__VoucherU__29B197202C14D958");

            entity.HasIndex(e => e.StudentId, "IX_VoucherUsages_Student");

            entity.HasIndex(e => e.VoucherId, "IX_VoucherUsages_Voucher");

            entity.HasIndex(e => new { e.VoucherId, e.StudentId }, "UQ_VoucherUsage").IsUnique();

            entity.Property(e => e.AppliedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DiscountApplied).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Student).WithMany(p => p.VoucherUsages)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VoucherUs__Stude__5165187F");

            entity.HasOne(d => d.Transaction).WithMany(p => p.VoucherUsages)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_VoucherUsages_Transaction");

            entity.HasOne(d => d.Voucher).WithMany(p => p.VoucherUsages)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VoucherUs__Vouch__5070F446");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
