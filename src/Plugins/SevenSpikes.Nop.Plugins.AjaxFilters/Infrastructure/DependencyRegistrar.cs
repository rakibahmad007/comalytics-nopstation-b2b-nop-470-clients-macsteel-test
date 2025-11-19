using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Configuration;
using SevenSpikes.Nop.Core.Helpers;
using SevenSpikes.Nop.Framework.AutoMapper;
using SevenSpikes.Nop.Framework.DependancyRegistrar;
using SevenSpikes.Nop.Plugins.AjaxFilters.Areas.Admin.Models;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;
using SevenSpikes.Nop.Plugins.AjaxFilters.Extensions;
using SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.ManufacturerFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.SpecificationFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.VendorFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.QueryStringManipulation;
using SevenSpikes.Nop.Plugins.AjaxFilters.Services;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Catalog.DTO;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure;

public class DependencyRegistrar : BaseDependancyRegistrar7Spikes
{
    protected override void CreateModelMappings()
    {
        ((Profile)AutoMapperConfiguration7Spikes.MapperConfigurationExpression)
            .CreateMap<SpecificationFilterModel7Spikes, SpecificationFilterModelDTO>()
            .ForMember<IList<SpecificationFilterDTO>>(
                (Expression<Func<SpecificationFilterModelDTO, IList<SpecificationFilterDTO>>>)(
                    (SpecificationFilterModelDTO dest) => dest.SpecificationFilterDTOs
                ),
                (Action<
                    IMemberConfigurationExpression<
                        SpecificationFilterModel7Spikes,
                        SpecificationFilterModelDTO,
                        IList<SpecificationFilterDTO>
                    >
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            SpecificationFilterModel7Spikes,
                            SpecificationFilterModelDTO,
                            IList<SpecificationFilterDTO>
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                SpecificationFilterModel7Spikes,
                                SpecificationFilterModelDTO,
                                IList<SpecificationFilterDTO>
                            >)
                                (object)opt
                        ).MapFrom<IList<SpecificationFilterDTO>>(
                            (Expression<
                                Func<SpecificationFilterModel7Spikes, IList<SpecificationFilterDTO>>
                            >)(
                                (SpecificationFilterModel7Spikes x) =>
                                    x
                                        .SpecificationFilterGroups.Select(
                                            (SpecificationFilterGroup group) => group.ToDTO()
                                        )
                                        .ToList()
                            )
                        );
                    }
            );
        ((Profile)AutoMapperConfiguration7Spikes.MapperConfigurationExpression)
            .CreateMap<SpecificationFilterGroup, SpecificationFilterDTO>()
            .ForMember<int>(
                (Expression<Func<SpecificationFilterDTO, int>>)(
                    (SpecificationFilterDTO dest) => dest.Id
                ),
                (Action<
                    IMemberConfigurationExpression<
                        SpecificationFilterGroup,
                        SpecificationFilterDTO,
                        int
                    >
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            SpecificationFilterGroup,
                            SpecificationFilterDTO,
                            int
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                SpecificationFilterGroup,
                                SpecificationFilterDTO,
                                int
                            >)
                                (object)opt
                        ).MapFrom<int>(
                            (Expression<Func<SpecificationFilterGroup, int>>)(
                                (SpecificationFilterGroup x) => x.Id
                            )
                        );
                    }
            )
            .ForMember<bool>(
                (Expression<Func<SpecificationFilterDTO, bool>>)(
                    (SpecificationFilterDTO dest) => dest.IsMain
                ),
                (Action<
                    IMemberConfigurationExpression<
                        SpecificationFilterGroup,
                        SpecificationFilterDTO,
                        bool
                    >
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            SpecificationFilterGroup,
                            SpecificationFilterDTO,
                            bool
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                SpecificationFilterGroup,
                                SpecificationFilterDTO,
                                bool
                            >)
                                (object)opt
                        ).MapFrom<bool>(
                            (Expression<Func<SpecificationFilterGroup, bool>>)(
                                (SpecificationFilterGroup x) => x.IsMain
                            )
                        );
                    }
            )
            .ForMember<IList<int>>(
                (Expression<Func<SpecificationFilterDTO, IList<int>>>)(
                    (SpecificationFilterDTO dest) => dest.SelectedFilterIds
                ),
                (Action<
                    IMemberConfigurationExpression<
                        SpecificationFilterGroup,
                        SpecificationFilterDTO,
                        IList<int>
                    >
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            SpecificationFilterGroup,
                            SpecificationFilterDTO,
                            IList<int>
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                SpecificationFilterGroup,
                                SpecificationFilterDTO,
                                IList<int>
                            >)
                                (object)opt
                        ).MapFrom<IList<int>>(
                            (Expression<Func<SpecificationFilterGroup, IList<int>>>)(
                                (SpecificationFilterGroup x) =>
                                    (
                                        from filterItem in x.FilterItems
                                        where
                                            (int)filterItem.FilterItemState == 1
                                            || (int)filterItem.FilterItemState == 2
                                        select filterItem into item
                                        select item.Id
                                    ).ToList()
                            )
                        );
                    }
            );
        ((Profile)AutoMapperConfiguration7Spikes.MapperConfigurationExpression)
            .CreateMap<AttributeFilterModel7Spikes, AttributeFilterModelDTO>()
            .ForMember<IList<AttributeFilterDTO>>(
                (Expression<Func<AttributeFilterModelDTO, IList<AttributeFilterDTO>>>)(
                    (AttributeFilterModelDTO dest) => dest.AttributeFilterDTOs
                ),
                (Action<
                    IMemberConfigurationExpression<
                        AttributeFilterModel7Spikes,
                        AttributeFilterModelDTO,
                        IList<AttributeFilterDTO>
                    >
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            AttributeFilterModel7Spikes,
                            AttributeFilterModelDTO,
                            IList<AttributeFilterDTO>
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                AttributeFilterModel7Spikes,
                                AttributeFilterModelDTO,
                                IList<AttributeFilterDTO>
                            >)
                                (object)opt
                        ).MapFrom<IList<AttributeFilterDTO>>(
                            (Expression<
                                Func<AttributeFilterModel7Spikes, IList<AttributeFilterDTO>>
                            >)(
                                (AttributeFilterModel7Spikes x) =>
                                    x
                                        .AttributeFilterGroups.Select(
                                            (AttributeFilterGroup group) => group.ToDTO()
                                        )
                                        .ToList()
                            )
                        );
                    }
            );
        ((Profile)AutoMapperConfiguration7Spikes.MapperConfigurationExpression)
            .CreateMap<AttributeFilterGroup, AttributeFilterDTO>()
            .ForMember<int>(
                (Expression<Func<AttributeFilterDTO, int>>)((AttributeFilterDTO dest) => dest.Id),
                (Action<
                    IMemberConfigurationExpression<AttributeFilterGroup, AttributeFilterDTO, int>
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            AttributeFilterGroup,
                            AttributeFilterDTO,
                            int
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                AttributeFilterGroup,
                                AttributeFilterDTO,
                                int
                            >)
                                (object)opt
                        ).MapFrom<int>(
                            (Expression<Func<AttributeFilterGroup, int>>)(
                                (AttributeFilterGroup x) => x.Id
                            )
                        );
                    }
            )
            .ForMember<bool>(
                (Expression<Func<AttributeFilterDTO, bool>>)(
                    (AttributeFilterDTO dest) => dest.IsMain
                ),
                (Action<
                    IMemberConfigurationExpression<AttributeFilterGroup, AttributeFilterDTO, bool>
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            AttributeFilterGroup,
                            AttributeFilterDTO,
                            bool
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                AttributeFilterGroup,
                                AttributeFilterDTO,
                                bool
                            >)
                                (object)opt
                        ).MapFrom<bool>(
                            (Expression<Func<AttributeFilterGroup, bool>>)(
                                (AttributeFilterGroup x) => x.IsMain
                            )
                        );
                    }
            )
            .ForMember<IList<int>>(
                (Expression<Func<AttributeFilterDTO, IList<int>>>)(
                    (AttributeFilterDTO dest) => dest.AllProductVariantIds
                ),
                (Action<
                    IMemberConfigurationExpression<
                        AttributeFilterGroup,
                        AttributeFilterDTO,
                        IList<int>
                    >
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            AttributeFilterGroup,
                            AttributeFilterDTO,
                            IList<int>
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                AttributeFilterGroup,
                                AttributeFilterDTO,
                                IList<int>
                            >)
                                (object)opt
                        ).MapFrom<IList<int>>(
                            (Expression<Func<AttributeFilterGroup, IList<int>>>)(
                                (AttributeFilterGroup x) =>
                                    x
                                        .FilterItems.SelectMany(
                                            (AttributeFilterItem item) =>
                                                item.ProductVariantAttributeIds
                                        )
                                        .ToList()
                            )
                        );
                    }
            )
            .ForMember<IList<int>>(
                (Expression<Func<AttributeFilterDTO, IList<int>>>)(
                    (AttributeFilterDTO dest) => dest.SelectedProductVariantIds
                ),
                (Action<
                    IMemberConfigurationExpression<
                        AttributeFilterGroup,
                        AttributeFilterDTO,
                        IList<int>
                    >
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            AttributeFilterGroup,
                            AttributeFilterDTO,
                            IList<int>
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                AttributeFilterGroup,
                                AttributeFilterDTO,
                                IList<int>
                            >)
                                (object)opt
                        ).MapFrom<IList<int>>(
                            (Expression<Func<AttributeFilterGroup, IList<int>>>)(
                                (AttributeFilterGroup x) =>
                                    x
                                        .FilterItems.Where(
                                            (AttributeFilterItem filterItem) =>
                                                (int)filterItem.FilterItemState == 1
                                                || (int)filterItem.FilterItemState == 2
                                        )
                                        .SelectMany(
                                            (AttributeFilterItem item) =>
                                                item.ProductVariantAttributeIds
                                        )
                                        .ToList()
                            )
                        );
                    }
            );
        ((Profile)AutoMapperConfiguration7Spikes.MapperConfigurationExpression)
            .CreateMap<ManufacturerFilterModel7Spikes, ManufacturerFilterModelDTO>()
            .ForMember<IList<int>>(
                (Expression<Func<ManufacturerFilterModelDTO, IList<int>>>)(
                    (ManufacturerFilterModelDTO dest) => dest.SelectedFilterIds
                ),
                (Action<
                    IMemberConfigurationExpression<
                        ManufacturerFilterModel7Spikes,
                        ManufacturerFilterModelDTO,
                        IList<int>
                    >
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            ManufacturerFilterModel7Spikes,
                            ManufacturerFilterModelDTO,
                            IList<int>
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                ManufacturerFilterModel7Spikes,
                                ManufacturerFilterModelDTO,
                                IList<int>
                            >)
                                (object)opt
                        ).MapFrom<List<int>>(
                            (Expression<Func<ManufacturerFilterModel7Spikes, List<int>>>)(
                                (ManufacturerFilterModel7Spikes x) =>
                                    (
                                        from filterItem in x.ManufacturerFilterItems
                                        where
                                            (int)filterItem.FilterItemState == 1
                                            || (int)filterItem.FilterItemState == 2
                                        select filterItem into item
                                        select item.Id
                                    ).ToList()
                            )
                        );
                    }
            );
        ((Profile)AutoMapperConfiguration7Spikes.MapperConfigurationExpression)
            .CreateMap<VendorFilterModel7Spikes, VendorFilterModelDTO>()
            .ForMember<IList<int>>(
                (Expression<Func<VendorFilterModelDTO, IList<int>>>)(
                    (VendorFilterModelDTO dest) => dest.SelectedFilterIds
                ),
                (Action<
                    IMemberConfigurationExpression<
                        VendorFilterModel7Spikes,
                        VendorFilterModelDTO,
                        IList<int>
                    >
                >)
                    delegate(
                        IMemberConfigurationExpression<
                            VendorFilterModel7Spikes,
                            VendorFilterModelDTO,
                            IList<int>
                        > opt
                    )
                    {
                        (
                            (IProjectionMemberConfiguration<
                                VendorFilterModel7Spikes,
                                VendorFilterModelDTO,
                                IList<int>
                            >)
                                (object)opt
                        ).MapFrom<List<int>>(
                            (Expression<Func<VendorFilterModel7Spikes, List<int>>>)(
                                (VendorFilterModel7Spikes x) =>
                                    (
                                        from filterItem in x.VendorFilterItems
                                        where
                                            (int)filterItem.FilterItemState == 1
                                            || (int)filterItem.FilterItemState == 2
                                        select filterItem into item
                                        select item.Id
                                    ).ToList()
                            )
                        );
                    }
            );
        CreateMvcModelMap<NopAjaxFiltersSettingsModel, NopAjaxFiltersSettings>();
    }

    protected override void RegisterPluginServices(IServiceCollection services)
    {
        //IL_0064: Unknown result type (might be due to invalid IL or missing references)
        //IL_006a: Invalid comparison between Unknown and I4
        //IL_0076: Unknown result type (might be due to invalid IL or missing references)
        //IL_007c: Invalid comparison between Unknown and I4
        services.AddTransient<IProductServiceNopAjaxFilters, ProductServiceNopAjaxFilters>();
        services.AddTransient<IProductAttributeService7Spikes, ProductAttributeService7Spikes>();
        services.AddTransient<
            ISpecificationAttributeService7Spikes,
            SpecificationAttributeService7Spikes
        >();
        services.AddTransient<IQueryStringBuilder, QueryStringBuilder>();
        services.AddTransient<
            IProductAttributeServiceAjaxFilters,
            ProductAttributeServiceAjaxFilters
        >();
        services.AddTransient<
            IPriceCalculationServiceNopAjaxFilters,
            PriceCalculationServiceNopAjaxFilters
        >();
        services.AddTransient<
            ISpecificationFilterOptionsHelper,
            SpecificationFilterOptionsHelper
        >();
        services.AddTransient<IAttributeFilterOptionsHelper, AttributeFilterOptionsHelper>();
        services.AddScoped<IFiltersPageHelper, FiltersPageHelper>();
        services.AddTransient<IQueryStringToModelUpdater, QueryStringToModelUpdater>();
        services.AddTransient<ITaxServiceNopAjaxFilters, TaxServiceNopAjaxFilters>();
        services.AddTransient<ISearchQueryStringHelper, SearchQueryStringHelper>();
        services.AddTransient<IProductServiceNopAjaxFilters, ProductServiceNopAjaxFilters>();
        services.AddTransient<ICustomAclHelper, CustomAclHelper>();
        DataConfig val = DataSettingsManager.LoadSettings((INopFileProvider)null, false);
        if ((int)val.DataProvider == 2)
        {
            services.AddTransient<IAjaxFiltersDatabaseService, AjaxFiltersDatabaseServiceMySQL>();
        }
        else if ((int)val.DataProvider == 3)
        {
            services.AddTransient<
                IAjaxFiltersDatabaseService,
                AjaxFiltersDatabaseServicePostgreSQL
            >();
        }
        else
        {
            services.AddTransient<IAjaxFiltersDatabaseService, AjaxFiltersDatabaseService>();
        }
        services.AddTransient<IConvertToDictionaryHelper, ConvertToDictionaryHelper>();
    }
}
