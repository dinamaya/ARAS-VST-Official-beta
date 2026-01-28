namespace ARAS.Main.SSMS.Api.Models.Interfaces
{
    public interface ICreatableByUser : ICreatable
    {
        public string CreatedBy { get; set; }
    }
}
