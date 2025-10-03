using System.ComponentModel.DataAnnotations;

namespace ARAS.Auth.Api.Models.Interfaces
{
    public interface IModifiable
    {
        [DataType(DataType.DateTime)] public DateTime DateModified { get; set; }
    }
}
