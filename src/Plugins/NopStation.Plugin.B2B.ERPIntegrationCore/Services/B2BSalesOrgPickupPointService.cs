using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class B2BSalesOrgPickupPointService : IB2BSalesOrgPickupPointService
{
    private readonly IRepository<B2BSalesOrgPickupPoint> _b2BSalesOrgPickupPointRepository;

    public B2BSalesOrgPickupPointService(IRepository<B2BSalesOrgPickupPoint> b2BSalesOrgPickupPointRepository)
    {
        _b2BSalesOrgPickupPointRepository = b2BSalesOrgPickupPointRepository;
    }

    public async Task<IPagedList<B2BSalesOrgPickupPoint>> GetAllB2BSalesOrgPickupPointsAsync(int salesOrganisationId, 
        int pageIndex = 0, 
        int pageSize = int.MaxValue)
    {
        var b2BSalesOrgPickupPoints = await _b2BSalesOrgPickupPointRepository.GetAllPagedAsync(query =>
        {
            if (salesOrganisationId > 0)
                query = query.Where(b => b.B2BSalesOrgId == salesOrganisationId);

            query = query.OrderByDescending(b => b.Id);

            return query;
        }, pageIndex, pageSize);

        return b2BSalesOrgPickupPoints;
    }

    public async Task<B2BSalesOrgPickupPoint> GetB2BSalesOrgPickupPointByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _b2BSalesOrgPickupPointRepository.GetByIdAsync(id);
    }

    public async Task<bool> CheckAnyB2BSalesOrgPickupPointExistBySalesOrgIdAndPickupPointIdAsync(int salesOrgId, int pickupPointId)
    {
        if (salesOrgId == 0 || pickupPointId == 0)
            return false;

        var query = _b2BSalesOrgPickupPointRepository.Table;

        return query.Any(a => a.B2BSalesOrgId == salesOrgId && a.NopPickupPointId == pickupPointId);
    }

    public async Task InsertB2BSalesOrgPickupPointAsync(B2BSalesOrgPickupPoint b2BSalesOrgPickupPoint)
    {
        ArgumentNullException.ThrowIfNull(b2BSalesOrgPickupPoint);

        await _b2BSalesOrgPickupPointRepository.InsertAsync(b2BSalesOrgPickupPoint);
    }

    public async Task UpdateB2BSalesOrgPickupPointAsync(B2BSalesOrgPickupPoint b2BSalesOrgPickupPoint)
    {
        ArgumentNullException.ThrowIfNull(b2BSalesOrgPickupPoint);

        await _b2BSalesOrgPickupPointRepository.UpdateAsync(b2BSalesOrgPickupPoint);
    }

    public async Task DeleteB2BSalesOrgPickupPointAsync(B2BSalesOrgPickupPoint b2BSalesOrgPickupPoint)
    {
        ArgumentNullException.ThrowIfNull(b2BSalesOrgPickupPoint);

        await _b2BSalesOrgPickupPointRepository.DeleteAsync(b2BSalesOrgPickupPoint);
    }

    public async Task<IList<B2BSalesOrgPickupPoint>> GetAllB2BSalesOrgPickupPointsBySalesOrganisationIdAsync(int salesOrganisationId)
    {
        var query = _b2BSalesOrgPickupPointRepository.Table;
        query = query.Where(b => b.B2BSalesOrgId == salesOrganisationId);
        query = query.OrderByDescending(b => b.Id);
        return await query.ToListAsync();
    }
}
