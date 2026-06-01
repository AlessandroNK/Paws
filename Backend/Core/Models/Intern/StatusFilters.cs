namespace Backend.Core.Models.Intern;

public class StatusFilters
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------

    #region GenericStatus

    public bool IncludeActive { get; set; } = true;
    public bool IncludeInactive { get; set; }
    public bool IncludeDeleted { get; set; }
    public bool IncludeBanned { get; set; }
    public bool IncludeArchived { get; set; }
    public bool IncludeToDelete { get; set; }

    #endregion

    #region UserSpecificStatus

    public bool IncludeUnverified { get; set; }

    #endregion


    //                                                                                                         Operators
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                            Events
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                      Constructors
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                   Private Methods
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Initializes a new instance of the <see cref="StatusFilters"/> class whit all filters set to true, meaning that
    /// all statuses will be excluded.
    /// </summary>
    /// <returns></returns>
    public static StatusFilters IncludeAll() => new()
    {
        IncludeActive = true,
        IncludeInactive = true,
        IncludeDeleted = true,
        IncludeBanned = true,
        IncludeArchived = true,
        IncludeToDelete = true,
        IncludeUnverified = true
    };

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Initializes a new instance of the <see cref="StatusFilters"/> class with all filters set to false, meaning that
    /// no status will be excluded.
    /// </summary>
    /// <returns></returns>
    public static StatusFilters ExcludeAll() => new()
    {
        IncludeActive = false,
        IncludeInactive = false,
        IncludeDeleted = false,
        IncludeBanned = false,
        IncludeArchived = false,
        IncludeToDelete = false,
        IncludeUnverified = false
    };

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Initializes a new instance of the <see cref="StatusFilters"/> class with only the active status included, meaning
    /// that only active entities will be included in the query results.
    /// </summary>
    /// <returns></returns>
    public static StatusFilters Create() => new()
    {
        IncludeActive = true,
        IncludeInactive = false,
        IncludeDeleted = false,
        IncludeBanned = false,
        IncludeArchived = false,
        IncludeToDelete = false,
        IncludeUnverified = false
    };

    #region Include

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenIncludeActive()
    {
        IncludeActive = true;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenIncludeInactive()
    {
        IncludeInactive = true;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenIncludeDeleted()
    {
        IncludeDeleted = true;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenIncludeBanned()
    {
        IncludeBanned = true;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenIncludeArchived()
    {
        IncludeArchived = true;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenIncludeToDelete()
    {
        IncludeToDelete = true;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenIncludeUnverified()
    {
        IncludeUnverified = true;
        return this;
    }

    #endregion

    #region Exclude

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenExcludeActive()
    {
        IncludeActive = false;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenExcludeInactive()
    {
        IncludeInactive = false;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenExcludeDeleted()
    {
        IncludeDeleted = false;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenExcludeBanned()
    {
        IncludeBanned = false;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenExcludeArchived()
    {
        IncludeArchived = false;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenExcludeToDelete()
    {
        IncludeToDelete = false;
        return this;
    }

    // -----------------------------------------------------------------------------------------------------------------
    public StatusFilters ThenExcludeUnverified()
    {
        IncludeUnverified = false;
        return this;
    }

    #endregion
}