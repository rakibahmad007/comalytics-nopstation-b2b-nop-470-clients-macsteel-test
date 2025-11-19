using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.ErpWebhook.Infrastructure;

public class ProductPictureEventConsumer : IConsumer<EntityDeletedEvent<ProductPicture>>
{
    private readonly IWebhookProductsImageService _webhookProductsImageService;

    public ProductPictureEventConsumer(IWebhookProductsImageService webhookProductsImageService)
    {
        _webhookProductsImageService = webhookProductsImageService;
    }

    public async Task HandleEventAsync(EntityDeletedEvent<ProductPicture> eventMessage)
    {
        if (eventMessage.Entity == null)
            return;

        var productPicture = eventMessage.Entity;

        var customPictureBinary = _webhookProductsImageService
            .GetCustomPictureBinaryForERPByNopPictureId(productPicture.PictureId);

        if (customPictureBinary != null)
        {
            _webhookProductsImageService.DeleteCustomPictureBinaryForERP(customPictureBinary);
        }

        await Task.CompletedTask;
    }
}
