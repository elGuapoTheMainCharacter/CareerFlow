using CareerFlow.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace CareerFlow.Web.Data;
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
 public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
 public DbSet<Company> Companies => Set<Company>();
 public DbSet<JobOpportunity> JobOpportunities => Set<JobOpportunity>();
 public DbSet<JobApplication> JobApplications => Set<JobApplication>();
 public DbSet<ApplicationStatusChange> ApplicationStatusChanges => Set<ApplicationStatusChange>();
 public DbSet<ApplicationNote> ApplicationNotes => Set<ApplicationNote>();
 public DbSet<ResumeDocument> ResumeDocuments => Set<ResumeDocument>();
 public DbSet<Notification> Notifications => Set<Notification>();
public DbSet<JobInvitation> JobInvitations => Set<JobInvitation>();
 
 protected override void OnModelCreating(ModelBuilder builder)
 {
 base.OnModelCreating(builder);
 builder.Entity<Company>()
 .HasIndex(c => new { c.OwnerId, c.Name });
 builder.Entity<JobOpportunity>()
 .Property(j => j.SalaryMin).HasPrecision(12, 2);
 builder.Entity<JobOpportunity>()
 .Property(j => j.SalaryMax).HasPrecision(12, 2);
 builder.Entity<Company>()
 .HasOne(c => c.Owner)
 .WithMany()
 .HasForeignKey(c => c.OwnerId)
 .OnDelete(DeleteBehavior.Cascade);
 builder.Entity<JobOpportunity>()
 .HasOne(j => j.Company)
 .WithMany(c => c.Opportunities)
 .HasForeignKey(j => j.CompanyId)
 .OnDelete(DeleteBehavior.Restrict);
 builder.Entity<JobOpportunity>()
 .HasOne(j => j.Owner)
 .WithMany()
 .HasForeignKey(j => j.OwnerId)
 .OnDelete(DeleteBehavior.Cascade);
 builder.Entity<JobApplication>()
 .HasOne(a => a.JobOpportunity)
 .WithMany(j => j.Applications)
 .HasForeignKey(a => a.JobOpportunityId)
 .OnDelete(DeleteBehavior.Cascade);
 builder.Entity<JobApplication>()
 .HasOne(a => a.Owner)
 .WithMany()
 .HasForeignKey(a => a.OwnerId)
 .OnDelete(DeleteBehavior.Cascade);
 builder.Entity<JobApplication>()
    .HasOne(a => a.ResumeDocument)
    .WithMany()
    .HasForeignKey(a => a.ResumeDocumentId)
    .OnDelete(DeleteBehavior.SetNull);

// The database itself refuses a second application by the same person to the same job.
builder.Entity<JobApplication>()
    .HasIndex(a => new { a.OwnerId, a.JobOpportunityId })
    .IsUnique();

builder.Entity<Notification>()
    .HasOne(n => n.User)
    .WithMany()
    .HasForeignKey(n => n.UserId)
    .OnDelete(DeleteBehavior.Cascade);
builder.Entity<Notification>()
    .HasIndex(n => new { n.UserId, n.IsRead });

builder.Entity<JobInvitation>()
    .HasOne(i => i.Employer)
    .WithMany()
    .HasForeignKey(i => i.EmployerId)
    .OnDelete(DeleteBehavior.Cascade);
builder.Entity<JobInvitation>()
    .HasOne(i => i.Seeker)
    .WithMany()
    .HasForeignKey(i => i.SeekerId)
    .OnDelete(DeleteBehavior.Cascade);
builder.Entity<JobInvitation>()
    .HasOne(i => i.JobOpportunity)
    .WithMany()
    .HasForeignKey(i => i.JobOpportunityId)
    .OnDelete(DeleteBehavior.Cascade);
builder.Entity<JobInvitation>()
    .HasIndex(i => new { i.SeekerId, i.JobOpportunityId })
    .IsUnique();
 }
}