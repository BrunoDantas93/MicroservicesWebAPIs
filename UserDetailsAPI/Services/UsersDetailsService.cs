using Google.Apis.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserDetailsAPI.Controllers;
using UserDetailsAPI.Models;
using UserDetailsAPI.Models.MongoDB;

namespace UserDetailsAPI.Services;

public class UsersDetailsService
{
    private readonly IMongoCollection<UserDetails> _userDetailsCollection;

    public UsersDetailsService(ILogger<UserDetailsController> logger, IOptions<ConnectionConfigurations> settings)
    {
        //Make the connection with the MongoDB
        var mongoClient = new MongoClient(
        settings.Value.ConnectionString);

        //Set the database 
        var mongoDatabase = mongoClient.GetDatabase(
        settings.Value.DatabaseName);

        //Set the collection
        _userDetailsCollection = mongoDatabase.GetCollection<UserDetails>(settings.Value.UserDetailsCollectionName);
    }

    /// <summary>
    /// Registers user details by inserting them into the UserDetails collection asynchronously.
    /// </summary>
    /// <param name="ud">The user details to be registered.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RegisterUserDetails(UserDetails ud)
    {
        // Insert the user details into the UserDetails collection asynchronously
        await _userDetailsCollection.InsertOneAsync(ud);

        // No need to return anything explicitly, as it's an asynchronous operation
        return;
    }


    /// <summary>
    /// Lists user details based on the specified criteria.
    /// </summary>
    /// <param name="uid">Optional. The unique identifier of the user details to filter by.</param>
    /// <returns>A task representing the asynchronous operation, containing a list of user details.</returns>
    public async Task<List<UserDetails>> ListUsersDetails(string? uid = null)
    {
        try
        {
            // Create a filter to match documents based on the provided criteria
            var filter = Builders<UserDetails>.Filter.Empty;

            if (uid != null)
            {
                // If uid is not null, add a filter to match documents with the specified unique identifier
                filter = Builders<UserDetails>.Filter.Eq(e => e.Id, uid);
            }

            // Execute the query to retrieve all documents that match the filter
            var result = await _userDetailsCollection.Find(filter).ToListAsync();

            // Return the list of documents
            return result;
        }
        catch (Exception ex)
        {
            // In case of an exception, rethrow it for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Updates user details in the UserDetails collection asynchronously.
    /// </summary>
    /// <param name="ud">The user details with updated information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateUsersDetails(UserDetails ud)
    {
        try
        {
            // Create a filter to find the user details with the specified unique identifier
            var filterDefinition = Builders<UserDetails>.Filter.Eq(e => e.Id, ud.Id);

            // Retrieve the existing user details to be updated
            var userDetailsToUpdate = await _userDetailsCollection.Find(filterDefinition).FirstOrDefaultAsync();

            if (userDetailsToUpdate != null)
            {
                // Create an update definition to set the entire user details to the provided user details
                var updateDefinition = Builders<UserDetails>.Update.Set(e => e.FirstName, ud.FirstName)
                                                                   .Set(e => e.LastName, ud.LastName)
                                                                   .Set(e => e.Address, ud.Address)
                                                                   .Set(e => e.Gender, ud.Gender)
                                                                   .Set(e => e.BirthDate, ud.BirthDate)
                                                                   .Set(e => e.Nationality, ud.Nationality);

                // Update the user details in the UserDetails collection asynchronously
                await _userDetailsCollection.UpdateOneAsync(filterDefinition, updateDefinition);
            }
            else
            {
                // Throw an exception if the user details with the specified ID is not found
                throw new Exception($"User Details with ID {ud.Id} not found.");
            }
        }
        catch (Exception ex)
        {
            // Rethrow the exception for higher-level error handling
            throw ex;
        }
    }

}
