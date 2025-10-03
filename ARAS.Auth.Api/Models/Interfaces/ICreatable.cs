using System.ComponentModel.DataAnnotations;

namespace ARAS.Auth.Api.Models.Interfaces
{
    public interface ICreatable
    {
        [DataType(DataType.DateTime)] public DateTime DateCreated { get; set; }
    }
}
