using Backend.Core.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Data;

/// <summary>
/// provides access to the database for the application.
/// </summary>
public class ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The set to handle <see cref="EncryptedUsers"/>
    /// </summary>
    public DbSet<EncryptedUser> EncryptedUsers { get; set; }


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
}