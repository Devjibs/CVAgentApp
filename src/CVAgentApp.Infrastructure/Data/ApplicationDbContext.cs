using CVAgentApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CVAgentApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<WorkExperience> WorkExperiences { get; set; }
    public DbSet<Education> Education { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Certification> Certifications { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<RequiredSkill> RequiredSkills { get; set; }
    public DbSet<RequiredQualification> RequiredQualifications { get; set; }
    public DbSet<CompanyInfo> CompanyInfos { get; set; }
    public DbSet<GeneratedDocument> GeneratedDocuments { get; set; }
    public DbSet<Session> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Candidate configuration
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // WorkExperience configuration
        modelBuilder.Entity<WorkExperience>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Company).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Position).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Candidate)
                .WithMany(c => c.WorkExperiences)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Education configuration
        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Institution).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Degree).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FieldOfStudy).HasMaxLength(100);
            entity.HasOne(e => e.Candidate)
                .WithMany(c => c.Education)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Skill configuration
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Candidate)
                .WithMany(c => c.Skills)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Certification configuration
        modelBuilder.Entity<Certification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IssuingOrganization).HasMaxLength(200);
            entity.Property(e => e.CredentialId).HasMaxLength(100);
            entity.HasOne(e => e.Candidate)
                .WithMany(c => c.Certifications)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Url).HasMaxLength(200);
            entity.HasOne(e => e.Candidate)
                .WithMany(c => c.Projects)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JobPosting configuration
        modelBuilder.Entity<JobPosting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Company).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // RequiredSkill configuration
        modelBuilder.Entity<RequiredSkill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.JobPosting)
                .WithMany(j => j.RequiredSkills)
                .HasForeignKey(e => e.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RequiredQualification configuration
        modelBuilder.Entity<RequiredQualification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.JobPosting)
                .WithMany(j => j.RequiredQualifications)
                .HasForeignKey(e => e.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CompanyInfo configuration
        modelBuilder.Entity<CompanyInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Mission).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Industry).HasMaxLength(200);
            entity.Property(e => e.Size).HasMaxLength(200);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.HasOne(e => e.JobPosting)
                .WithOne(j => j.CompanyInfo)
                .HasForeignKey<CompanyInfo>(e => e.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GeneratedDocument configuration
        modelBuilder.Entity<GeneratedDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BlobUrl).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Candidate)
                .WithMany()
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.JobPosting)
                .WithMany()
                .HasForeignKey(e => e.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Session)
                .WithMany(s => s.GeneratedDocuments)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionToken).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Candidate)
                .WithMany()
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.JobPosting)
                .WithMany()
                .HasForeignKey(e => e.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure JSON columns for collections
        modelBuilder.Entity<WorkExperience>()
            .Property(e => e.Achievements)
            .HasConversion(
                v => string.Join("|", v),
                v => v.Split("|", StringSplitOptions.RemoveEmptyEntries).ToList()
            );

        modelBuilder.Entity<Project>()
            .Property(e => e.Technologies)
            .HasConversion(
                v => string.Join("|", v),
                v => v.Split("|", StringSplitOptions.RemoveEmptyEntries).ToList()
            );

        modelBuilder.Entity<CompanyInfo>()
            .Property(e => e.Values)
            .HasConversion(
                v => string.Join("|", v),
                v => v.Split("|", StringSplitOptions.RemoveEmptyEntries).ToList()
            );

        modelBuilder.Entity<CompanyInfo>()
            .Property(e => e.RecentNews)
            .HasConversion(
                v => string.Join("|", v),
                v => v.Split("|", StringSplitOptions.RemoveEmptyEntries).ToList()
            );
    }
}
