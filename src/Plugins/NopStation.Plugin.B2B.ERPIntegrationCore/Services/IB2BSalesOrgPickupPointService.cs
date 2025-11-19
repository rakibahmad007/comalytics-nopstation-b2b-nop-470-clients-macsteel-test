using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;
public interface IB2BSalesOrgPickupPointService
{
    Task<IPagedList<B2BSalesOrgPickupPoint>> GetAllB2BSalesOrgPickupPointsAsync(int salesOrganisationId, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<B2BSalesOrgPickupPoint> GetB2BSalesOrgPickupPointByIdAsync(int id);

    Task<bool> CheckAnyB2BSalesOrgPickupPointExistBySalesOrgIdAndPickupPointIdAsync(int salesOrgId, int pickupPointId);

    Task InsertB2BSalesOrgPickupPointAsync(B2BSalesOrgPickupPoint b2BSalesOrgPickupPoint);

    Task UpdateB2BSalesOrgPickupPointAsync(B2BSalesOrgPickupPoint b2BSalesOrgPickupPoint);

    Task DeleteB2BSalesOrgPickupPointAsync(B2BSalesOrgPickupPoint b2BSalesOrgPickupPoint);

    Task<IList<B2BSalesOrgPickupPoint>> GetAllB2BSalesOrgPickupPointsBySalesOrganisationIdAsync(int salesOrganisationId);
}