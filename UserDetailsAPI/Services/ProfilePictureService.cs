using Google.Apis.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserDetailsAPI.Controllers;
using UserDetailsAPI.Models;
using UserDetailsAPI.Models.MongoDB;

namespace UserDetailsAPI.Services;

public class ProfilePictureService
{
    private readonly IMongoCollection<ProfilePictures> _profilePictureCollection;

    public ProfilePictureService(ILogger<UserDetailsController> logger, IOptions<ConnectionConfigurations> settings)
    {
        //Make the connection with the MongoDB
        var mongoClient = new MongoClient(
        settings.Value.ConnectionString);

        //Set the database 
        var mongoDatabase = mongoClient.GetDatabase(
        settings.Value.DatabaseName);

        //Set the collection
        _profilePictureCollection = mongoDatabase.GetCollection<ProfilePictures>(settings.Value.ProfilePicturesCollectionName);
    }

    /// <summary>
    /// Registers a profile picture by inserting it into the ProfilePictures collection asynchronously.
    /// </summary>
    /// <param name="pp">The profile picture to be registered.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RegisterProfilePicture(ProfilePictures pp)
    {
        // Insert the profile picture into the ProfilePictures collection asynchronously
        await _profilePictureCollection.InsertOneAsync(pp);

        // No need to return anything explicitly, as it's an asynchronous operation
        return;
    }

    /// <summary>
    /// Lists profile pictures based on the specified criteria.
    /// </summary>
    /// <param name="uid">Optional. The unique identifier of the profile pictures to filter by.</param>
    /// <returns>A task representing the asynchronous operation, containing a list of profile pictures.</returns>
    public async Task<List<ProfilePictures>> ListProfilePictures(string? uid = null)
    {
        try
        {
            // Create a filter to match documents based on the provided criteria
            var filter = Builders<ProfilePictures>.Filter.Empty;

            if (uid != null)
            {
                // If uid is not null, add a filter to match documents with the specified unique identifier
                filter = Builders<ProfilePictures>.Filter.Eq(e => e.Id, uid);
            }

            // Execute the query to retrieve all documents that match the filter
            var result = await _profilePictureCollection.Find(filter).ToListAsync();

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
    /// Updates a profile picture in the ProfilePictures collection asynchronously.
    /// </summary>
    /// <param name="ud">The profile picture with updated information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateProfilePicture(ProfilePictures ud)
    {
        try
        {
            // Create a filter to find the profile picture with the specified unique identifier
            var filterDefinition = Builders<ProfilePictures>.Filter.Eq(e => e.Id, ud.Id);

            // Retrieve the existing profile picture to be updated
            var profilePictureToUpdate = await _profilePictureCollection.Find(filterDefinition).FirstOrDefaultAsync();

            if (profilePictureToUpdate != null)
            {
                // Create an update definition to set the entire profile picture to the provided profile picture
                var updateDefinition = Builders<ProfilePictures>.Update.Set(e => e, ud);

                // Update the profile picture in the ProfilePictures collection asynchronously
                await _profilePictureCollection.UpdateOneAsync(filterDefinition, updateDefinition);
            }
            else
            {
                // Throw an exception if the profile picture with the specified ID is not found
                throw new Exception($"Profile Pictures with ID {ud.Id} not found.");
            }
        }
        catch (Exception ex)
        {
            // Rethrow the exception for higher-level error handling
            throw ex;
        }
    }
}
