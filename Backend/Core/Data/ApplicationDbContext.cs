using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Data;

/// <summary>
/// provides access to the database for the application.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    //                                                                                                Private Properties
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                 Public Properties
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The set to handle <see cref="EncryptedUsers"/>
    /// </summary>
    public DbSet<EncryptedUser> EncryptedUsers { get; set; }

    /// <summary>
    /// The set to handle <see cref="EncryptedPets"/>
    /// </summary>
    public DbSet<EncryptedPet> EncryptedPets { get; set; }

    /// <summary>
    /// The set to handle <see cref="EncryptedUserPet"/>
    /// </summary>
    public DbSet<EncryptedUserPet> EncryptedUserPets { get; set; }

    /// <summary>
    /// OMG this is getting crazy
    /// </summary>
    public DbSet<EncryptedShareInvitation> EncryptedShareInvitations { get; set; }


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