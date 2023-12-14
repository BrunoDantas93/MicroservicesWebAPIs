using EventsAPI.Controllers;
using EventsAPI.Models;
using EventsAPI.Models.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserDetailsAPI.Models;

namespace EventsAPI.Services;

public class EventsTypeService
{
    private readonly IMongoCollection<EventsType> _eventsTypeCollection;

    public EventsTypeService(ILogger<EventsTypeController> logger, IOptions<ConnectionConfigurations> settings)
    {
        //Make the connection with the MongoDB
        var mongoClient = new MongoClient(
        settings.Value.ConnectionString);

        //Set the database 
        var mongoDatabase = mongoClient.GetDatabase(
        settings.Value.DatabaseName);

        //Set the collection
        _eventsTypeCollection = mongoDatabase.GetCollection<EventsType>(settings.Value.EventsTypeCollectionName);
    }

    /// <summary>
    /// Creates a new EventsType and inserts it into the associated collection.
    /// </summary>
    /// <param name="type">The EventsType to be created and inserted.</param>
    /// <returns>A task representing the asynchronous operation with the result being the created EventsType.</returns>
    public async Task<EventsType> CreateEventType(EventsType type)
    {
        try
        {
            // Insert the EventsType asynchronously into the associated collection
            await _eventsTypeCollection.InsertOneAsync(type);

            // Return the created EventsType
            return type;
        }
        catch (Exception ex)
        {
            // In case of an exception, rethrow it for higher-level error handling
            throw ex;
        }
    }


    /// <summary>
    /// Retrieves a list of EventsTypes based on the specified criteria.
    /// </summary>
    /// <param name="uid">Optional. The unique identifier of the event to filter by.</param>
    /// <param name="type">Optional. The type of events to filter by.</param>
    /// <returns>A task representing the asynchronous operation, containing a list of EventsType.</returns>
    public async Task<List<EventsType>> ListEventsTypes(string? uid = null, string? type = null)
    {
        try
        {
            // Create a filter to match documents based on the provided criteria
            var filter = Builders<EventsType>.Filter.Eq(e => e.Disabled, false);

            if (uid != null)
            {
                // If uid is not null, add a filter to match documents with the specified unique identifier
                filter = Builders<EventsType>.Filter.Eq(e => e.Id, uid);
            }
            else if (type != null)
            {
                // If type is not null, add a filter to match documents with the specified type
                filter = Builders<EventsType>.Filter.Eq(e => e.Type, type);
            }

            // Execute the query to retrieve all documents that match the filter
            var result = await _eventsTypeCollection.Find(filter).ToListAsync();

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
    /// Changes the user type by toggling the Disabled property of an EventsType document.
    /// </summary>
    /// <param name="uID">The unique identifier of the EventsType document.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ChangeEventType(string uID)
    {
        try
        {
            // Create a filter to find the EventsType document with the specified unique identifier
            var filterDefinition = Builders<EventsType>.Filter.Eq(e => e.Id, uID);

            // Retrieve the EventsType document to be updated
            var eventToUpdate = await _eventsTypeCollection.Find(filterDefinition).FirstOrDefaultAsync();

            if (eventToUpdate != null)
            {
                // Toggle the Disabled property
                var updateDefinition = Builders<EventsType>.Update.Set(e => e.Disabled, !eventToUpdate.Disabled);

                // Update the EventsType document with the new Disabled value
                await _eventsTypeCollection.UpdateOneAsync(filterDefinition, updateDefinition);
            }
            else
            {
                // Throw an exception if the EventsType document is not found
                throw new Exception($"EventsType with ID {uID} not found.");
            }
        }
        catch (Exception ex)
        {
            // Rethrow the exception for higher-level error handling
            throw ex;
        }
    }



}
