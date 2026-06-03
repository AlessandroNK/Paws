using System.Reflection;
using Backend.Core.Models.Appointments;
using Backend.Core.Models.CustomAttributes;
using Backend.Core.Models.Intern;
using Backend.Core.Models.Pets;
using Backend.Core.Models.Relationships;
using Backend.Core.Models.Users;
using Backend.Core.Models.Vets;
using Backend.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
    /// The set to handle <see cref="User"/>
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// The set to handle <see cref="Pets"/>
    /// </summary>
    public DbSet<Pet> Pets { get; set; }

    /// <summary>
    /// The set to handle <see cref="UserPet"/>
    /// </summary>
    public DbSet<UserPet> UserPets { get; set; }

    /// <summary>
    /// OMG this is getting crazy
    /// </summary>
    public DbSet<OwnershipInvitation> OwnershipInvitations { get; set; }

    /// <summary>
    /// The set to return the Great Roman Empire to its greatness
    /// </summary>
    public DbSet<Vet> Vets { get; set; }

    /// <summary>
    /// The set to handle <see cref="Appointments"/>
    /// </summary>
    public DbSet<Appointment> Appointments { get; set; }

    /// <summary>
    /// The set to handle app's configurations
    /// </summary>
    public DbSet<AppConfig> AppConfigs { get; set; }


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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Vet>().ToTable("Vets");

        var encrypter = new ValueConverter<string, string>(
            plain => SecurityService.EncryptString(plain),
            cipher => SecurityService.DecryptString(cipher)
        );

        var hasher = new ValueConverter<string, string>(
            plain => SecurityService.HashString(plain),
            stored => stored
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.ClrType.GetProperties())
            {
                if (property.GetCustomAttribute<EncryptProperty>() is not null)
                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion(hasher);

                else if (property.GetCustomAttribute<HashProperty>() is not null)
                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion(encrypter);
            }
        }
    }
}