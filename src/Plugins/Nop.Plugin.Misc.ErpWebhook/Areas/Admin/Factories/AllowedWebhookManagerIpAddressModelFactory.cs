using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Models.AllowedWebhookManagerIpAddress;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Factories;

public class AllowedWebhookManagerIpAddressModelFactory
    : IAllowedWebhookManagerIpAddressModelFactory
{
    private readonly IAllowedWebhookManagerIpAddressesService _ipAddressService;

    public AllowedWebhookManagerIpAddressModelFactory(
        IAllowedWebhookManagerIpAddressesService ipAddressService
    )
    {
        _ipAddressService = ipAddressService;
    }

    public async Task<AllowedWebhookManagerIpAddressListModel> PrepareAllowedWebhookManagerIpAddressListModelAsync(
        AllowedWebhookManagerIpAddressSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        // Get IP addresses asynchronously
        var ipAddresses = await _ipAddressService.GetAllIpAddressesAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize
        );

        // Prepare list model
        var model = new AllowedWebhookManagerIpAddressListModel();

        model = await model.PrepareToGridAsync(
            searchModel,
            ipAddresses,
            () =>
            {
                var tasks = ipAddresses.SelectAwait(async ipAddress =>
                {
                    var ipAddressModel = ipAddress.ToModel<AllowedWebhookManagerIpAddressModel>();
                    return ipAddressModel;
                });

                return tasks;
            }
        );

        return model;
    }

    public async Task<AllowedWebhookManagerIpAddressSearchModel> PrepareAllowedWebhookManagerIpAddressSearchModelAsync(
        AllowedWebhookManagerIpAddressSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.SetGridPageSize();

        return searchModel;
    }
}
