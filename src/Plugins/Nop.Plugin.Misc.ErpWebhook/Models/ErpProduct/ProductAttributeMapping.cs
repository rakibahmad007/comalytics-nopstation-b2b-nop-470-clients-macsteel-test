using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.ErpWebhook.Models.ErpProduct
{
    public class ProductAttributeMapping
    {
        public int ProductId { get; set; } = 1;
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
    }
}
