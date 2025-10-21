using ARAS.Blazor.Components.Layout;
using ARAS.Blazor.Services.Interfaces;
using System.Collections.Concurrent;

namespace ARAS.Blazor.Services.Implementations
{
	public class ValidationService : IValidationService
	{
		private readonly ConcurrentDictionary<string, ComponentValidator> _components = new();

		public void ClearErrors()
		{
			foreach (var component in _components.Values)
				component.ClearErrors();
		}

		public void DisplayErrors(Dictionary<string, List<string>> errors)
		{
			foreach (var (key, value) in errors)
				if (_components.TryGetValue(key, out var component))
					component.SetErrors(value.FirstOrDefault() ?? "");
		}

		public void Register(string name, ComponentValidator component)
		{
			_components[name] = component;
		}

		public void Unregister(string name)
		{
			_components.TryRemove(name, out _);
		}
	}
}
