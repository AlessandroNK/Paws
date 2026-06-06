using Backend.Core.Data;
using Backend.Core.Internal;
using Backend.Core.Models.Enums;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Results;
using Backend.Core.Models.Users;
using Backend.Core.Models.Vets;
using Backend.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Repositories;

public class VetRepository(
    ApplicationDbContext dbContext,
    ILogger<PetRepository> logger
    ) : IVetRepository
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The database context used to interact with the database.
    /// This context is used to perform CRUD operations on the <see cref="User"/> and
    /// <see cref="DocumentType"/> entities.
    /// It is injected into the repository to provide access to the underlying database.
    /// </summary>
    private readonly ApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// We wanna log!!!
    /// </summary>
    private readonly ILogger<PetRepository> _logger = logger;


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                         Operators
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                            Events
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                      Constructors
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                   Private Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static IQueryable<Vet> ApplyStatusFilters(
        IQueryable<Vet> query,
        StatusFilters? filters = null
    )
    {
        filters ??= new StatusFilters();

        if (!filters.IncludeActive) query = query.Where(i => i.Status != EntityStatus.Active);
        if (!filters.IncludeInactive) query = query.Where(i => i.Status != EntityStatus.Inactive);
        if (!filters.IncludeDeleted) query = query.Where(i => i.Status != EntityStatus.Deleted);
        if (!filters.IncludeBanned) query = query.Where(i => i.Status != EntityStatus.Banned);
        if (!filters.IncludeArchived) query = query.Where(i => i.Status != EntityStatus.Archived);
        if (!filters.IncludeToDelete) query = query.Where(i => i.Status != EntityStatus.ToDelete);

        return query;
    }


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async Task<Result<List<Vet>>> GetVetsAsync(StatusFilters? filters = null)
    {
        var query = _dbContext.Vets.AsQueryable();

        // Apply status filters
        query = ApplyStatusFilters(query, filters);

        var vets = await query.ToListAsync();
        return vets.Count > 0
            ? new Result<List<Vet>>
            {
                Success = true,
                Status = 200,
                Code = "VETS_FOUND",
                Data = vets,
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            }
            : new Result<List<Vet>>
            {
                Success = false,
                Status = 404,
                Code = "VETS_NOT_FOUND",
                Message = "No vets found matching the specified filters",
                TraceCode = FileCodes.CallerIC(),
                Returnable = true
            };
    }
}