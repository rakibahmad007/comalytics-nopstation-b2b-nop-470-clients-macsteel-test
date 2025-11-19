using System.Text;
using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Comalytics.PictureAndSEOExportImport;

public class PictureAndSEOExportImportPlugin : BasePlugin, IAdminMenuPlugin
{
    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly INopFileProvider _nopFileProvider;
    private readonly ILogger _logger;
    private readonly ISettingService _settingService;

    public PictureAndSEOExportImportPlugin(IWebHelper webHelper, ILocalizationService localizationService, INopFileProvider nopFileProvider, ILogger logger, ISettingService settingService)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
        _nopFileProvider = nopFileProvider;
        _logger = logger;
        _settingService = settingService;
    }
    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/PictureAndSEOExportImport/Configure";
    }

    public override async Task InstallAsync()
    {
        await AddLocalResourcesAsync();

        var setting = new PictureAndSEOExportImportSettings();

        await _settingService.SaveSettingAsync(setting);

        await RunInstallDbScriptsAsync();

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await RunUninstallDbScriptsAsync();
        await DeleteLocalResourcesAsync();
        await _settingService.DeleteSettingAsync<PictureAndSEOExportImportSettings>();
        await base.UninstallAsync();
    }

    private async Task DeleteLocalResourcesAsync()
    {
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Import");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Export");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.Success");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.Failed");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.ErrorOccured");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.ProductId");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.Name");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.Sku");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.SeName");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.Picture1Id");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.Picture2Id");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.Picture3Id");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.ErrorMessage");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.IsOccupied");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportFromExcelTip");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Name");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Configure");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Admin.Configure.PageTitle");
    }

    private async Task RunUninstallDbScriptsAsync()
    {
        try
        {
            var sqlScrpts = new List<string>();
            var sqlFile = _nopFileProvider.MapPath(ScriptPath.ImportTablesDropSqlFilePath);
            var sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
            sqlScrpts.Add(sqlScript);

            sqlFile = _nopFileProvider.MapPath(ScriptPath.ImportStoredProceduresDropSqlFilePath);
            sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
            sqlScrpts.Add(sqlScript);

            sqlScrpts.Add(sqlScript);

            await RunSqls(sqlScrpts);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Error while parsing script from file", ex);
        }
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var comalyticsNode = rootNode.ChildNodes.Where(x => x.SystemName == "Comalytics").FirstOrDefault();
        if (comalyticsNode == null)
        {
            comalyticsNode = new SiteMapNode()
            {
                Title = "Comalytics",
                SystemName = "Comalytics",
                Visible = true,
                IconClass = "nav-icon fas fa-cube",
            };
            rootNode.ChildNodes.Add(comalyticsNode);
        }

        var pictureAndSEOExportImportNode = new SiteMapNode()
        {
            SystemName = "Comalytics.PictureAndSEOExportImport",
            Title = await _localizationService.GetResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Name"),
            ControllerName = "PictureAndSEOExportImport",
            ActionName = "Configure",
            IconClass = "nav-icon fas fa-cogs",
            Visible = true
        };

        comalyticsNode.ChildNodes.Add(pictureAndSEOExportImportNode);
    }

    private async Task AddLocalResourcesAsync()
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Import", "Import Excel File");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Export", "Export to Excel File");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.Success", "Successfully Imported");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.Failed", "Import failed");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.ErrorOccured", "Erorr occured while updating products. Check log for details.");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.ProductId", "Product Id");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.Name", "Product Name");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.Sku", "Sku");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.SeName", "Se Name");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.Picture1Id", "Picture-1 Id");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.Picture2Id", "Picture-2 Id");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.Picture3Id", "Picture-3 Id");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Field.ErrorMessage", "Error Message");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.IsOccupied", "Service is already in use. Try again after some time.");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportFromExcelTip", "Imported products are distinguished by Product Id. Check system log for any error occured.");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Name", "Product Picture and SEO Export Import");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Configure", "Configure");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.Admin.Configure.PageTitle", "Product Picture and SEO Export Import");

    }

    private async Task RunInstallDbScriptsAsync()
    {
        try
        {
            var sqlScrpts = new List<string>();
            var sqlFile = _nopFileProvider.MapPath(ScriptPath.ImportTablesCreateSqlFilePath);
            var sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
            sqlScrpts.Add(sqlScript);

            sqlFile = _nopFileProvider.MapPath(ScriptPath.ImportStoredProceduresCreateSqlFilePath);
            sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
            sqlScrpts.Add(sqlScript);

            sqlScrpts.Add(sqlScript);

            await RunSqls(sqlScrpts);
        }
        catch (Exception ex)
        {
            _logger.Error("Error while parsing script from file", ex);
        }
    }

    private async Task RunSqls(IList<string> sqlScripts)
    {
        var connString = DataSettingsManager.LoadSettings().ConnectionString;
        using (var connection = new SqlConnection(connString))
        {
            connection.Open();
            try
            {
                foreach (var script in sqlScripts)
                {
                    var query = script;
                    var cmd = new SqlCommand(query, connection);
                    var rowChanged = cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Error while runnig sql script", ex.Message);
                connection.Close();
            }
        }

    }
}
