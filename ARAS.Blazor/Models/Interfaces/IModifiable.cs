using System.ComponentModel.DataAnnotations;

namespace ARAS.Blazor.Models.Interfaces
{
    public interface IModifiable
    {
        [DataType(DataType.DateTime)] public DateTime DateModified { get; set; }
    }
}
