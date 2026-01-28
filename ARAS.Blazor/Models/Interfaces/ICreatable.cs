using System.ComponentModel.DataAnnotations;

namespace ARAS.Blazor.Models.Interfaces
{
    public interface ICreatable
    {
        [DataType(DataType.DateTime)] public DateTime DateCreated { get; set; }
    }
}
