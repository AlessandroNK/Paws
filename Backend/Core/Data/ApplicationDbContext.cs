using System.Reflection;
using Backend.Core.Models.Appointments;
using Backend.Core.Models.CustomAttributes;
using Backend.Core.Models.Interfaces;
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

    /// <summary>
    /// User's session token, this is used to authenticate the user in the frontend and to authorize the user to access
    /// certain endpoints in the backend.
    /// </summary>
    public DbSet<SessionToken> SessionTokens { get; set; }

    //                                                                                                         Operators
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                            Events
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                      Constructors
    // -----------------------------------------------------------------------------------------------------------------


    //                                                                                                   Private Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This method processes the entities that are being added or modified in the context. It checks if the entity
    /// implements the <see cref="IEncryptable"/> interface and if its state is either Added or Modified. If both
    /// conditions are met, it calls the Hash method of the entity to encrypt its data before saving it to the database.
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void ProcessEntities()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IEncryptable encryptable) continue;
            if (entry is not ({ State: EntityState.Added } or { State: EntityState.Modified })) continue;

            // Here, we encrypt some entity's pieces of data
            // Hash
            var hashResult = encryptable.Hash();
            if (!hashResult)
                throw new Exception(
                    $"{hashResult.Code} Failed to hash entity of type {entry.Entity.GetType().Name}: {hashResult.Title}");
        }
    }


    //                                                                                                    Public Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This method is called when the model is being created. It is used to configure the model and its properties. In
    /// this case, we are configuring the model to use the "Users" table for the <see cref="User"/> entity and the "Vets"
    /// table for the <see cref="Vet"/> entity. We are also configuring the model to use a value converter for the
    /// properties that have the <see cref="EncryptProperty"/> attribute. The value converter will encrypt the data
    /// before saving it to the database and decrypt it when reading it from the database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Vet>().ToTable("Vets");

        var encrypter = new ValueConverter<string, string>(
            plain => SecurityService.EncryptString(plain),
            cipher => SecurityService.DecryptString(cipher)
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.ClrType.GetProperties())
            {
                if (property.GetCustomAttribute<EncryptProperty>() is not null)
                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion(encrypter);
            }
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This method is called when the changes are being saved to the database. It is used to process the entities that
    /// are being added or modified in the context. It calls the ProcessEntities method to encrypt the data before saving
    /// it to the database. Then, it calls the base SaveChanges method to save the changes to the database.
    /// </summary>
    /// <returns></returns>
    public override int SaveChanges()
    {
        // Encrypt things so information can be safely stored
        ProcessEntities();
        return base.SaveChanges();
    }

    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This method is called when the changes are being asynchronously saved to the database. It is used to process
    /// the entities that are being added or modified in the context. It calls the ProcessEntities method to encrypt the
    /// data before saving it to the database. Then, it calls the base SaveChanges method to save the changes to the
    /// database.
    /// </summary>
    /// <returns></returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Encrypt things so information can be safely stored
        ProcessEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }
}