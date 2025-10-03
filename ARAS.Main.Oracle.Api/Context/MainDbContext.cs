using ARAS.Main.Oracle.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.Oracle.Api.Context
{
	public class MainDbContext : DbContext
	{
		public virtual DbSet<AccEndUser> TestAccEndUser { get; set; }
		public virtual DbSet<InvoiceDetails> InvoiceDetails { get; set; }

		public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<AccEndUser>(entity =>
			{
				entity.ToTable("ACC_END_USER", "APPS");

				entity.HasKey(e => e.AccEndUserId);

				entity.Property(e => e.AccEndUserId)
					  .HasColumnName("ACC_END_USER_ID");

				entity.Property(e => e.AccName)
					  .HasColumnName("ACC_NAME")
					  .HasMaxLength(200);

				entity.Property(e => e.AccAccountName)
					  .HasColumnName("ACC_ACCOUNT_NAME")
					  .HasMaxLength(200);

				entity.Property(e => e.AccContactFname)
					  .HasColumnName("ACC_CONTACT_FNAME")
					  .HasMaxLength(200);

				entity.Property(e => e.AccContactLname)
					  .HasColumnName("ACC_CONTACT_LNAME")
					  .HasMaxLength(200);

				entity.Property(e => e.AccContactEmail)
					  .HasColumnName("ACC_CONTACT_EMAIL")
					  .HasMaxLength(200);

				entity.Property(e => e.AccAddressLine1)
					  .HasColumnName("ACC_ADDRESS_LINE1")
					  .HasMaxLength(200);

				entity.Property(e => e.AccCity)
					  .HasColumnName("ACC_CITY")
					  .HasMaxLength(200);

				entity.Property(e => e.AccState)
					  .HasColumnName("ACC_STATE")
					  .HasMaxLength(200);

				entity.Property(e => e.AccPostalCode)
					  .HasColumnName("ACC_POSTAL_CODE")
					  .HasMaxLength(200);

				entity.Property(e => e.AccAttribute1).HasColumnName("ACC_ATTRIBUTE1").HasMaxLength(200);
				entity.Property(e => e.AccAttribute2).HasColumnName("ACC_ATTRIBUTE2").HasMaxLength(200);
				entity.Property(e => e.AccAttribute3).HasColumnName("ACC_ATTRIBUTE3").HasMaxLength(200);
				entity.Property(e => e.AccAttribute4).HasColumnName("ACC_ATTRIBUTE4").HasMaxLength(200);
				entity.Property(e => e.AccAttribute5).HasColumnName("ACC_ATTRIBUTE5").HasMaxLength(200);

				entity.Property(e => e.AccAddressLine2)
					  .HasColumnName("ACC_ADDRESS_LINE2")
					  .HasMaxLength(200);

				entity.Property(e => e.SiteId)
					  .HasColumnName("SITE_ID");
			});


			modelBuilder.Entity<AccEndUser>(entity =>
			{
				entity.ToTable("ACC_END_USER", "APPS");
			});
		}
	}
}
