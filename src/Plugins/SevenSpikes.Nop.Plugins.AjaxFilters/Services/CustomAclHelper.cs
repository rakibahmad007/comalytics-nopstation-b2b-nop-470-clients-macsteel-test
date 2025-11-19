using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Services.Customers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public class CustomAclHelper : ICustomAclHelper
{
    private readonly IWorkContext _workContext;

    private readonly IRepository<AclRecord> _aclRepository;

    private readonly IRepository<Product> _productRepository;

    private readonly IRepository<Manufacturer> _manufacturerRepository;

    private readonly IRepository<Category> _categoryRepository;

    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;

    private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;

    private readonly CatalogSettings _catalogSettings;
    private readonly ICustomerService _customerService;

    public CustomAclHelper(IWorkContext workContext, IRepository<AclRecord> aclRepository, IRepository<Product> productRepository,
        IRepository<Manufacturer> manufacturerRepository, IRepository<Category> categoryRepository, IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository, CatalogSettings catalogSettings, IErpCustomerFunctionalityService erpCustomerFunctionalityService, ICustomerService customerService)
    {
        _workContext = workContext;
        _aclRepository = aclRepository;
        _productRepository = productRepository;
        _manufacturerRepository = manufacturerRepository;
        _categoryRepository = categoryRepository;
        _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
        _catalogSettings = catalogSettings;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _customerService = customerService;
    }

    public async Task<IQueryable<Product>> GetAvailableProductsForCurrentErpCustomerAsync()
    {
        var filterInfoModel = await _erpCustomerFunctionalityService.GetErpFilterInfoModel();

        if (filterInfoModel.IsErpAccount && filterInfoModel.ErpFilterFacetReturnNoProduct)
        {
            var emptyProducts = new List<Product>().AsQueryable();
            return emptyProducts;
        }

        var result = _productRepository.Table;

        if (!_catalogSettings.IgnoreAcl)
        {
            //Access control list. Allowed customer roles
            var allowedCustomerRolesIds = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());

            result = from p in result
                     join acl in _aclRepository.Table
                    on new { c1 = p.Id, c2 = nameof(Product) } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into p_acl
                     from acl in p_acl.DefaultIfEmpty()
                     where !p.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                     select p;
        }

        #region B2B Account

        if (filterInfoModel.IsErpAccount && filterInfoModel.PreFilterFacetSpecIds != null && filterInfoModel.PreFilterFacetSpecIds.Count() > 0)
        {
            if (filterInfoModel.IsErpAccount && filterInfoModel.SpecialExcludeSpecIds != null && filterInfoModel.SpecialExcludeSpecIds.Count() > 0)
            {
                result = from p in result
                         join psa in _productSpecificationAttributeRepository.Table
                         on p.Id equals psa.ProductId
                         where filterInfoModel.PreFilterFacetSpecIds.Contains(psa.SpecificationAttributeOptionId) && !filterInfoModel.ExcludedProductIds.Contains(p.Id)
                         select p;
            }
            else
            {
                result = from p in result
                         join psa in _productSpecificationAttributeRepository.Table
                         on p.Id equals psa.ProductId
                         where filterInfoModel.PreFilterFacetSpecIds.Contains(psa.SpecificationAttributeOptionId)
                         select p;
            }
        }
        #endregion

        return result;
    }
}
