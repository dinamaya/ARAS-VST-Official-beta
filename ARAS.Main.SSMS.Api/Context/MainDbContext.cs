using ARAS.Main.SSMS.Api.Models.Entities;
using ARAS.Main.SSMS.Api.Models.SQLVIews;
using ARAS.Main.SSMS.Api.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace ARAS.Main.SSMS.Api.Context
{
	public class MainDbContext : DbContext
	{
		public virtual DbSet<Adjustment> Adjustments { get; set; }
		public virtual DbSet<AdjustmentType> AdjustmentTypes { get; set; }
        public virtual DbSet<APAROffset> APAROffsets { get; set; }
        public virtual DbSet<CNDetails> CNDetails { get; set; }
		public virtual DbSet<Invoice> Invoices { get; set; }
		public virtual DbSet<Note> Notes { get; set; }
		public virtual DbSet<Request> Requests { get; set; }
		public virtual DbSet<Status> Statuses { get; set; }
		public virtual DbSet<Transaction> Transactions { get; set; }
		public virtual DbSet<TransactionRemarks> TransactionRemarks { get; set; }

        // SQL VIEWS
        public virtual DbSet<AparoffsetRowV> VwAparoffsetRows { get; set; }
		public virtual DbSet<InvoiceNumbersV> VwInvoiceNumbers { get; set; }
		public virtual DbSet<NotesV> VwNotes { get; set; }
		public virtual DbSet<TransactionsHistoryV> VwTransactionsHistory { get; set; }
		public virtual DbSet<RequestAdjustmentsV> VwRequestAdjustments { get; set; }
		public virtual DbSet<RequestsNumberSourceV> VwRequestsNumberSources { get; set; }

		// NEW SQL VIEWS
		public virtual DbSet<LatestReceiptAdjustmentDetailsV> VwLatestReceiptAdjustmentDetails { get; set; }

		public MainDbContext(DbContextOptions<MainDbContext> options) : base(options) { }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<CNDetails>()
			  .HasOne(a => a.Invoice)
			  .WithMany()
			  .HasForeignKey(a => a.InvoiceId);

			modelBuilder.Entity<Transaction>()
			  .HasOne(a => a.Request)
			  .WithMany()
			  .HasForeignKey(a => a.RequestId);

			modelBuilder.Entity<TransactionRemarks>()
			  .HasOne(a => a.Transaction)
			  .WithMany()
			  .HasForeignKey(a => a.TransactionId);

			modelBuilder.Entity<Request>(entity =>
			{
				entity
			  .HasOne(a => a.AdjustmentType)
				.WithMany()
				.HasForeignKey(a => a.AdjustmentTypeId);
			});

			modelBuilder.Entity<Adjustment>(entity =>
			{
				entity
			  .HasOne(a => a.Invoice)
				.WithMany()
				.HasForeignKey(a => a.InvoiceId);

				entity
			  .HasOne(a => a.Request)
				.WithMany()
				.HasForeignKey(a => a.RequestId);

				entity
			  .HasOne(a => a.AdjustmentType)
				.WithMany()
				.HasForeignKey(a => a.AdjustmentTypeId);
			});

			modelBuilder.Entity<Note>()
			  .HasOne(a => a.Request)
			  .WithMany()
			  .HasForeignKey(a => a.RequestId);


			// SQL VIEWS
			modelBuilder.Entity<LatestReceiptAdjustmentDetailsV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("LatestReceiptAdjustmentDetails_v");

				entity.Property(e => e.ApproverId).HasMaxLength(250);
				entity.Property(e => e.UpdaterId).HasMaxLength(250);
			});



			// OLD SQL VIEWS Can Be Remove
			modelBuilder.Entity<AparoffsetRowV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("APAROffsetRow_v");
			});

			modelBuilder.Entity<InvoiceNumbersV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("InvoiceNumbers_v");
			});

			modelBuilder.Entity<LatestRequestTransactionV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("LatestRequestTransaction_v");

				entity.Property(e => e.ApproverId).HasMaxLength(250);
				entity.Property(e => e.ValidatorId).HasMaxLength(250);
			});

			modelBuilder.Entity<NotesV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("Notes_v");

				entity.Property(e => e.AccountType).HasMaxLength(256);
				entity.Property(e => e.Id).HasMaxLength(450);
			});

			modelBuilder.Entity<RequestAdjustmentsV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("RequestAdjustments_v");

				entity.Property(e => e.AdjustmentTypeId).HasMaxLength(450);
			});

			modelBuilder.Entity<RequestsNumberSourceV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("RequestsNumberSource_v");
			});

			modelBuilder.Entity<TransactionsHistoryV>(entity =>
			{
				entity
					.HasNoKey()
					.ToView("TransactionsHistory_v");

				entity.Property(e => e.AccountRole).HasMaxLength(256);
				entity.Property(e => e.StatusId).HasMaxLength(450);
				entity.Property(e => e.TransactionRemarksId).HasMaxLength(450);
			});
		}
	}
}
