using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpProductsImage;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces
{
    public interface IWebhookProductsImageService
    {
        Task ProcessProductsImageAsync(List<ErpProductsImageModel> erpProductsImage);
        Parallel_CustomPictureBinaryForERP GetCustomPictureBinaryForERPByNopPictureId(int nopPictureId);
        void DeleteCustomPictureBinaryForERP(Parallel_CustomPictureBinaryForERP customPictureBinary);
    }
}
