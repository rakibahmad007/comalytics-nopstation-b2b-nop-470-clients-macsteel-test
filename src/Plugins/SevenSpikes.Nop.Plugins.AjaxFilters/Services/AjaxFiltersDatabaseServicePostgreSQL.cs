using System.Threading.Tasks;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public class AjaxFiltersDatabaseServicePostgreSQL : IAjaxFiltersDatabaseService
{
	public Task CreateDatabaseScriptsAsync()
	{
		return Task.CompletedTask;
	}

	public Task UpdateDatabaseScriptsAsync()
	{
		return Task.CompletedTask;
	}

	public Task RemoveDatabaseScriptsAsync()
	{
		return Task.CompletedTask;
	}
}
