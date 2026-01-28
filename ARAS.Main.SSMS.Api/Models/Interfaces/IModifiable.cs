using System.ComponentModel.DataAnnotations;

namespace ARAS.Main.SSMS.Api.Models.Interfaces
{
    public interface IModifiable
    {
        [DataType(DataType.DateTime)] public DateTime DateModified { get; set; }
    }
}
