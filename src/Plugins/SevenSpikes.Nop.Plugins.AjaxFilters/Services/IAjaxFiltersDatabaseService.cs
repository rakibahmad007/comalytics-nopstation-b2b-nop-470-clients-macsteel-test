using System.Threading.Tasks;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public interface IAjaxFiltersDatabaseService
{
	Task CreateDatabaseScriptsAsync();

	Task UpdateDatabaseScriptsAsync();

	Task RemoveDatabaseScriptsAsync();
}
