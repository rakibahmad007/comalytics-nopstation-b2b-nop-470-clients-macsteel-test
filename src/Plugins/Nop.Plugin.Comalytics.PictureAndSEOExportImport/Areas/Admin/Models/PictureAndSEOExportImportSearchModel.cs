using Nop.Web.Framework.Models;

namespace Nop.Plugin.Comalytics.PictureAndSEOExportImport.Areas.Admin.Models
{
    public record PictureAndSEOExportImportSearchModel : BaseSearchModel
    {
        public int IsUploaded { get; set; }
        public int LogId { get; set; }
        public string Message { get; set; }
    }
}
