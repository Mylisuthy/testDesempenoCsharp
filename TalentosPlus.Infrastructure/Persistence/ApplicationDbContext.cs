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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Employee - Department Relationship
        builder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

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
    }
}
