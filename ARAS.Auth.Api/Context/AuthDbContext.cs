using ARAS.Auth.Api.App_Code.Globals;
using ARAS.Auth.Api.Models.Entities;
using ARAS.Auth.Api.Models.SQLViews;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Auth.Api.Context
{
	public class AuthDbContext : IdentityDbContext<Account>
	{
		public AuthDbContext(DbContextOptions<AuthDbContext> options)
			: base(options) { }

		public virtual DbSet<Account> Accounts { get; set; }
		public virtual DbSet<AccountsV> AccountsVs { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// SQL VIEWS
			modelBuilder.Entity<AccountsV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("Accounts_v");

				entity.Property(e => e.AccountType).HasMaxLength(256);
				entity.Property(e => e.CreatorId).HasColumnName("CreatorID");
				entity.Property(e => e.Email).HasMaxLength(256);
				entity.Property(e => e.Id).HasMaxLength(250);
			});

			// MODELS
			modelBuilder.Entity<Account>()
				.Ignore(a => a.LockoutEnabled)
				.Ignore(a => a.LockoutEnd)
				.Ignore(a => a.TwoFactorEnabled)
				.Ignore(a => a.AccessFailedCount)
				.Ignore(a => a.PhoneNumber)
				.Ignore(a => a.PhoneNumberConfirmed)
				.Ignore(a => a.EmailConfirmed)
				.Ignore(a => a.SecurityStamp);

			modelBuilder.Entity<Account>().Property(u => u.Id).HasMaxLength(250);
			modelBuilder.Entity<IdentityRole>().Property(u => u.Id).HasMaxLength(250);

			// Pre-generated GUIDs (deterministic for migrations)
			var opsId = "ROL0348ba0dcd674a26816302bfb9ec35df";
			var requestorId = "ROL51f72eacb7c2411e84fcde08dd36cf21";
			var approverId = "ROLd43f5ae8518046d4804b91f0fc133eab";
			var validatorId = "ROLe80ca6b642b44e77b8bc7766df5d99d5";

			// Seed roles with GUIDs
			modelBuilder.Entity<IdentityRole>().HasData(
				new IdentityRole { Id = opsId, Name = "Ops", NormalizedName = "OPS" },
				new IdentityRole { Id = requestorId, Name = "Requestor", NormalizedName = "REQUESTOR" },
				new IdentityRole { Id = approverId, Name = "Approver", NormalizedName = "APPROVER" },
				new IdentityRole { Id = validatorId, Name = "Validator", NormalizedName = "VALIDATOR" }
			);

		}
	}
}
