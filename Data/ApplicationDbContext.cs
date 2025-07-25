using Microsoft.EntityFrameworkCore;
using YopoAPI.Modules.Authentication.Models;
using YopoAPI.Modules.UserManagement.Models;
using YopoAPI.Modules.RoleManagement.Models;
using YopoAPI.Modules.PolicyManagement.Models;

namespace YopoAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity.GetType().GetProperties()
                    .Any(p => p.Name == "CreatedAt" || p.Name == "UpdatedAt" || p.Name == "AssignedAt"))
                .ToList();

            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;
                var entityType = entry.Entity.GetType();

                if (entry.State == EntityState.Added)
                {
                    var createdAtProperty = entityType.GetProperty("CreatedAt");
                    if (createdAtProperty != null)
                    {
                        createdAtProperty.SetValue(entry.Entity, now);
                    }

                    var assignedAtProperty = entityType.GetProperty("AssignedAt");
                    if (assignedAtProperty != null)
                    {
                        assignedAtProperty.SetValue(entry.Entity, now);
                    }
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var updatedAtProperty = entityType.GetProperty("UpdatedAt");
                    if (updatedAtProperty != null)
                    {
                        updatedAtProperty.SetValue(entry.Entity, now);
                    }
                }
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Privilege> Privileges { get; set; }
        public DbSet<RolePrivilege> RolePrivileges { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Policy> Policies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.PhoneNumber).IsUnique();
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(255);
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
                entity.Property(u => u.PasswordHash).HasMaxLength(255);
                // CreatedAt and UpdatedAt will be set in application code

                // Configure relationship with Role
                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.Name).IsUnique();
                entity.Property(r => r.Name).HasMaxLength(50);
                entity.Property(r => r.Description).HasMaxLength(200);
                // CreatedAt will be set in application code

                // Configure self-referencing relationship for hierarchy
                entity.HasOne(r => r.ParentRole)
                      .WithMany(r => r.SubRoles)
                      .HasForeignKey(r => r.ParentRoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Invitation entity
            modelBuilder.Entity<Invitation>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Email).HasMaxLength(255);
                entity.Property(i => i.PhoneNumber).HasMaxLength(20);
                // CreatedAt will be set in application code

                entity.HasOne(i => i.InvitedBy)
                      .WithMany()
                      .HasForeignKey(i => i.InvitedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Role)
                      .WithMany()
                      .HasForeignKey(i => i.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Privilege entity
            modelBuilder.Entity<Privilege>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).HasMaxLength(100);
                entity.Property(p => p.Description).HasMaxLength(200);
                entity.Property(p => p.Category).HasMaxLength(50);
                // CreatedAt will be set in application code
            });

            // Configure RolePrivilege entity
            modelBuilder.Entity<RolePrivilege>(entity =>
            {
                entity.HasKey(rp => rp.Id);
                // AssignedAt will be set in application code

                entity.HasOne(rp => rp.Role)
                      .WithMany(r => r.RolePrivileges)
                      .HasForeignKey(rp => rp.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Privilege)
                      .WithMany(p => p.RolePrivileges)
                      .HasForeignKey(rp => rp.PrivilegeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PasswordResetToken entity
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(prt => prt.Id);
                entity.Property(prt => prt.Email).HasMaxLength(255);
                entity.Property(prt => prt.Token).HasMaxLength(6);
                // CreatedAt will be set in application code
            });

            // Configure Policy entity
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Type).HasMaxLength(50);
                entity.Property(p => p.Version).HasMaxLength(20);
                // CreatedAt and UpdatedAt will be set in application code
            });

            // Seed initial roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Super Admin", Description = "Super Administrator with full system access", HierarchyLevel = 0, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { Id = 2, Name = "Normal User", Description = "Regular user with limited access", HierarchyLevel = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { Id = 3, Name = "Property Admin", Description = "Property Administrator with property management privileges", HierarchyLevel = 1, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { Id = 4, Name = "Security Admin", Description = "Security Administrator with security management privileges", HierarchyLevel = 2, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed initial privileges
            modelBuilder.Entity<Privilege>().HasData(
                new Privilege { Id = 1, Name = "User Management", Description = "Create, read, update, delete users", Category = "User", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Privilege { Id = 2, Name = "Role Management", Description = "Create, read, update, delete roles", Category = "Role", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Privilege { Id = 3, Name = "Invitation Management", Description = "Create and manage invitations", Category = "Invitation", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Privilege { Id = 4, Name = "System Configuration", Description = "Configure system settings", Category = "System", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Privilege { Id = 5, Name = "Property Management", Description = "Manage properties", Category = "Property", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Privilege { Id = 6, Name = "Security Management", Description = "Manage security settings", Category = "Security", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Privilege { Id = 7, Name = "View Profile", Description = "View own profile", Category = "User", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Privilege { Id = 8, Name = "Edit Profile", Description = "Edit own profile", Category = "User", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed initial policies
            modelBuilder.Entity<Policy>().HasData(
                new Policy { Id = 1, Type = "terms", Content = "Terms and Conditions content will be added here.", Version = "1.0", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Policy { Id = 2, Type = "privacy", Content = "Privacy Policy content will be added here.", Version = "1.0", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}


