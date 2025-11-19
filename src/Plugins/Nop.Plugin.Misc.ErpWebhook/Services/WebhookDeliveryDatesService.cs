using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpDeliveryDates;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Misc.ErpWebhook.Services;

public class WebhookDeliveryDatesService : IWebhookDeliveryDatesService
{
    private readonly IRepository<ErpDeliveryDates> _erpDeliveryDatesRepo;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpWebhookService _erpWebhookService;

    public WebhookDeliveryDatesService(IRepository<ErpDeliveryDates> erpDeliveryDatesRepo,
        IErpLogsService erpLogsService,
        IErpWebhookService erpWebhookService)
    {
        _erpDeliveryDatesRepo = erpDeliveryDatesRepo;
        _erpLogsService = erpLogsService;
        _erpWebhookService = erpWebhookService;
    }

    public async Task ProcessDeliveryDatesAsync(List<DeliveryDatesModel> deliveryDates)
    {
        if (deliveryDates == null || !deliveryDates.Any())
            return;

        try
        {
            foreach (var item in deliveryDates)
            {
                item.SalesOrgOrPlant = item.SalesOrgOrPlant?.Trim().ToUpperInvariant();
                item.City = item.City?.Trim().ToUpperInvariant();
            }

            //2330
            var duplicateDeliveryDatesFor1032 = new List<DeliveryDatesModel>();

            duplicateDeliveryDatesFor1032 = deliveryDates
                .Where(x => x.SalesOrgOrPlant.Equals("1030"))
                .Select(y => y.Clone())
                .ToList();

            foreach (var item in duplicateDeliveryDatesFor1032)
            {
                item.SalesOrgOrPlant = "1032";
            }
            if (duplicateDeliveryDatesFor1032.Count != 0)
            {
                deliveryDates.AddRange(duplicateDeliveryDatesFor1032);
            }
            // Check if similar entities exist in the database
            var existingDeliveryDates = (from obj in deliveryDates
                                         join dbEntity in _erpDeliveryDatesRepo.Table
                                         on new { obj.SalesOrgOrPlant, obj.City } equals new { dbEntity.SalesOrgOrPlant, dbEntity.City }
                                         select dbEntity).ToList();


            foreach (var delDate in existingDeliveryDates)
            {
                var updatedDelDate = deliveryDates.FirstOrDefault(x => x.SalesOrgOrPlant == delDate.SalesOrgOrPlant && x.City == delDate.City);
                if (updatedDelDate == null)
                    continue;

                MapDeliveryDates(delDate, updatedDelDate);
                await _erpDeliveryDatesRepo.UpdateAsync(delDate);
            }

            // Find new entities not in the similar list but exist in the objectsList
            var newEntities = deliveryDates
                .Where(y => !existingDeliveryDates
                .Any(e => e.SalesOrgOrPlant == y.SalesOrgOrPlant && e.City == y.City));

            var newErpDeliveryDates = new List<ErpDeliveryDates>();

            foreach (var delDate in newEntities)
            {
                var newErpDeliveryDate = new ErpDeliveryDates
                {
                    CreatedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now
                };

                MapDeliveryDates(newErpDeliveryDate, delDate);
                newErpDeliveryDates.Add(newErpDeliveryDate);
            }
            if (newErpDeliveryDates.Count != 0)
            {
                await _erpDeliveryDatesRepo.InsertAsync(newErpDeliveryDates);
            }
        }
        catch (Exception e)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.DeliveryDates,
                "Exception happened on deliverDates webhook service. Click view to see details",
                e.Message + Environment.NewLine + e.StackTrace
            );
        }
    }

    void MapDeliveryDates(ErpDeliveryDates existingDelDate, DeliveryDatesModel updatedDelDate)
    {
        existingDelDate.SalesOrgOrPlant = updatedDelDate.SalesOrgOrPlant?.Trim().ToUpperInvariant();
        existingDelDate.City = updatedDelDate.City?.Trim().ToUpperInvariant();
        existingDelDate.CutOffTime = updatedDelDate.CutOffTime;
        existingDelDate.AllWeekIndicator = _erpWebhookService.StringToBool(updatedDelDate.AllWeekIndicator);
        existingDelDate.Monday = _erpWebhookService.StringToBool(updatedDelDate.Monday);
        existingDelDate.Tuesday = _erpWebhookService.StringToBool(updatedDelDate.Tuesday);
        existingDelDate.Wednesday = _erpWebhookService.StringToBool(updatedDelDate.Wednesday);
        existingDelDate.Thursday = _erpWebhookService.StringToBool(updatedDelDate.Thursday);
        existingDelDate.Friday = _erpWebhookService.StringToBool(updatedDelDate.Friday);
        existingDelDate.DelDate1 = updatedDelDate.DelDate1;
        existingDelDate.DelDate2 = updatedDelDate.DelDate2;
        existingDelDate.DelDate3 = updatedDelDate.DelDate3;
        existingDelDate.DelDate4 = updatedDelDate.DelDate4;
        existingDelDate.DelDate5 = updatedDelDate.DelDate5;
        existingDelDate.DelDate6 = updatedDelDate.DelDate6;
        existingDelDate.DelDate7 = updatedDelDate.DelDate7;
        existingDelDate.DelDate8 = updatedDelDate.DelDate8;
        existingDelDate.DelDate9 = updatedDelDate.DelDate9;
        existingDelDate.DelDate10 = updatedDelDate.DelDate10;
        existingDelDate.Deleted = _erpWebhookService.StringToBool(updatedDelDate.Deleted);
        existingDelDate.UpdatedOn = DateTime.Now;
        existingDelDate.IsFullLoadRequired = _erpWebhookService.StringToBool(updatedDelDate.IsFullLoadRequired);
    }
}
