using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARAS.Main.Oracle.Api.Models.Entities
{
	[Table("AR_LOOKUPS")]
	public class ReasonCode
	{
		[Required, Column("LOOKUP_TYPE"), StringLength(30)] public string LookupType { get; set; } = string.Empty;

		[Key, Column("LOOKUP_CODE"), StringLength(30)] public string LookupCode { get; set; } = string.Empty;
		
		[Required, Column("LAST_UPDATE_DATE")] public DateTime LastUpdateDate { get; set; }

		[Required, Column("LAST_UPDATED_BY")] public long LastUpdatedBy { get; set; }

		[Required, Column("CREATION_DATE")] public DateTime CreationDate { get; set; }

		[Required, Column("CREATED_BY")] public long CreatedBy { get; set; }

		[Required, Column("MEANING"), StringLength(80)] public string Meaning { get; set; } = string.Empty;

		[Required, Column("ENABLED_FLAG"), StringLength(1)] public string EnabledFlag { get; set; } = string.Empty;

		[Column("LAST_UPDATE_LOGIN")] public long? LastUpdateLogin { get; set; }

		[Column("START_DATE_ACTIVE")] public DateTime? StartDateActive { get; set; }

		[Column("END_DATE_ACTIVE")] public DateTime? EndDateActive { get; set; }

		[Column("DESCRIPTION"), StringLength(240)] public string? Description { get; set; }

		[Column("ATTRIBUTE_CATEGORY"), StringLength(30)] public string? AttributeCategory { get; set; }

		[Column("ATTRIBUTE1")] public string? Attribute1 { get; set; }
		[Column("ATTRIBUTE2")] public string? Attribute2 { get; set; }
		[Column("ATTRIBUTE3")] public string? Attribute3 { get; set; }
		[Column("ATTRIBUTE4")] public string? Attribute4 { get; set; }
		[Column("ATTRIBUTE5")] public string? Attribute5 { get; set; }
		[Column("ATTRIBUTE6")] public string? Attribute6 { get; set; }
		[Column("ATTRIBUTE7")] public string? Attribute7 { get; set; }
		[Column("ATTRIBUTE8")] public string? Attribute8 { get; set; }
		[Column("ATTRIBUTE9")] public string? Attribute9 { get; set; }
		[Column("ATTRIBUTE10")] public string? Attribute10 { get; set; }
		[Column("ATTRIBUTE11")] public string? Attribute11 { get; set; }
		[Column("ATTRIBUTE12")] public string? Attribute12 { get; set; }
		[Column("ATTRIBUTE13")] public string? Attribute13 { get; set; }
		[Column("ATTRIBUTE14")] public string? Attribute14 { get; set; }
		[Column("ATTRIBUTE15")] public string? Attribute15 { get; set; }

		[Column("EXTERNALLY_VISIBLE_FLAG"), StringLength(150)] public string? ExternallyVisibleFlag { get; set; }
	}
}
