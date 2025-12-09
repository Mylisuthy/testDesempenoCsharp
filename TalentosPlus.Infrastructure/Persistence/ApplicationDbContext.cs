using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TalentosPlus.Domain.Entities;

namespace TalentosPlus.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<EducationLevel> EducationLevels { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Employee - Department Relationship
        builder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Employee - Position Relationship
        builder.Entity<Employee>()
            .HasOne(e => e.Position)
            .WithMany(p => p.Employees)
            .HasForeignKey(e => e.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Employee - EducationLevel Relationship
        builder.Entity<Employee>()
            .HasOne(e => e.EducationLevel)
            .WithMany(el => el.Employees)
            .HasForeignKey(e => e.EducationLevelId)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique constraints
        builder.Entity<Employee>()
            .HasIndex(e => e.DocumentNumber)
            .IsUnique();
        
        builder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        builder.Entity<Department>()
            .HasIndex(d => d.Name)
            .IsUnique();

        builder.Entity<Position>()
            .HasIndex(p => p.Name)
            .IsUnique();

        builder.Entity<EducationLevel>()
            .HasIndex(el => el.Name)
            .IsUnique();
    }
}
