using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Interfaces
{
    public interface ICreatable
    {
        [DataType(DataType.DateTime)] public DateTime DateCreated { get; set; }
    }
}
