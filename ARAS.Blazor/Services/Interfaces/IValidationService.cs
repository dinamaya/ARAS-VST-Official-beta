using ARAS.Blazor.Components.Layout;

namespace ARAS.Blazor.Services.Interfaces
{
	public interface IValidationService
	{
		void Register(string name, ComponentValidator component);
		void Unregister(string name);
		void DisplayErrors(Dictionary<string, List<string>> errors);
		void ClearErrors();
	}
}
