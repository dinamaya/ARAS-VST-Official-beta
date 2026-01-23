using ARAS.Blazor.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System;

namespace ARAS.Blazor.Services.Implementations
{
	public class QueryService : IQueryService
	{
		private readonly NavigationManager _navManager;
		private readonly ILogger<QueryService> _logger;

		private Uri CurrentUri => _navManager.ToAbsoluteUri(_navManager.Uri);


		public QueryService(NavigationManager navManager, ILogger<QueryService> logger)
		{
			_navManager = navManager;
			_logger = logger;
		}

		public string GetCurrentUrl(bool isEncoded)
		{
			string uri = CurrentUri.GetLeftPart(UriPartial.Path);
			return isEncoded ? Uri.EscapeDataString(uri) : uri;
		}

		public string GetValue(string key)
		{
			_logger.LogDebug("Fetching query param '{Key}' from {Uri}", key, _navManager.Uri);

			return QueryHelpers.ParseQuery(CurrentUri.Query).TryGetValue(key, out var qValue) ? qValue : string.Empty;
		}

		public bool KeyHasValueOf(string key, string targetValue)
		{
			_logger.LogDebug("Fetching query param '{Key}' from {Uri}", key, _navManager.Uri);

			return QueryHelpers.ParseQuery(CurrentUri.Query).TryGetValue(key, out var qValue) ?
			string.Equals(qValue, targetValue, StringComparison.OrdinalIgnoreCase) : false;
		}
	}
}
