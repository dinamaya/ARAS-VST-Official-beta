namespace ARAS.Main.SSMS.Api.Models.Dtos
{
	public class NoteCreateDto
	{
		public long RequestId { get; set; }
		public IEnumerable<NoteRowDto> Notes { get; set; }

		public NoteCreateDto() { }

		public NoteCreateDto(long requestId, IEnumerable<NoteRowDto> notes)
		{
			RequestId = requestId;
			Notes = notes;
		}
	}
}
