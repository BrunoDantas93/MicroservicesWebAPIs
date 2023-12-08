using EventsAPI.Controllers;
using EventsAPI.Models;
using EventsAPI.Models.MongoDB;
using Google.Apis.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventsAPI.Services;

public class EventsService
{
    private readonly IMongoCollection<Event> _eventsCollection;

    public EventsService(ILogger<EventsTypeController> logger, IOptions<ConnectionConfigurations> settings)
    {
        //Make the connection with the MongoDB
        var mongoClient = new MongoClient(
        settings.Value.ConnectionString);

        //Set the database 
        var mongoDatabase = mongoClient.GetDatabase(
        settings.Value.DatabaseName);

        //Set the collection
        _eventsCollection = mongoDatabase.GetCollection<Event>(settings.Value.EventsCollectionName);
    }

    /// <summary>
    /// Registers an event by inserting it into the events collection asynchronously.
    /// </summary>
    /// <param name="ev">The event to be registered.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RegisterEvent(Event ev)
    {
        // Insert the event into the events collection asynchronously
        await _eventsCollection.InsertOneAsync(ev);

        // No need to return anything explicitly, as it's an asynchronous operation
        return;
    }

    /// <summary>
    /// Lists events based on the specified criteria.
    /// </summary>
    /// <param name="uid">Optional. The unique identifier of the event to filter by.</param>
    /// <returns>A task representing the asynchronous operation, containing a list of events.</returns>
    public async Task<List<Event>> ListEvents(string? uid = null)
    {
        try
        {
            // Create a filter to match documents based on the provided criteria
            var filter = Builders<Event>.Filter.Empty;

            if (uid != null)
            {
                // If uid is not null, add a filter to match documents with the specified unique identifier
                filter = Builders<Event>.Filter.Eq(e => e.Id, uid);
            }

            // Execute the query to retrieve all documents that match the filter
            var result = await _eventsCollection.Find(filter).ToListAsync();

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
    /// Updates an event in the events collection asynchronously.
    /// </summary>
    /// <param name="ev">The event with updated information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateEvent(Event ev)
    {
        try
        {
            // Create a filter to find the event with the specified unique identifier
            var filterDefinition = Builders<Event>.Filter.Eq(e => e.Id, ev.Id);

            // Retrieve the existing event to be updated
            var eventToUpdate = await _eventsCollection.Find(filterDefinition).FirstOrDefaultAsync();

            if (eventToUpdate != null)
            {
                // Create an update definition to set the entire event to the provided event
                var updateDefinition = Builders<Event>.Update.Set(e => e, ev);

                // Update the event in the events collection asynchronously
                await _eventsCollection.UpdateOneAsync(filterDefinition, updateDefinition);
            }
            else
            {
                // Throw an exception if the event with the specified ID is not found
                throw new Exception($"Event with ID {ev.Id} not found.");
            }
        }
        catch (Exception ex)
        {
            // Rethrow the exception for higher-level error handling
            throw ex;
        }
    }


    /// <summary>
    /// Saves a participant by adding them to the participants array of an event identified by the specified unique ID.
    /// </summary>
    /// <param name="uID">The unique identifier of the event.</param>
    /// <param name="participant">The participant to be saved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SaveParticipant(string uID, Participant participant)
    {
        // Create a filter to identify the event based on its unique ID
        var filter = Builders<Event>.Filter.Eq(x => x.Id, uID);

        // Create an update operation to push (add) the new participant to the event's Participants array
        var update = Builders<Event>.Update.Push(x => x.Participants, participant);

        // Perform an asynchronous update operation on the Events collection
        await _eventsCollection.UpdateOneAsync(filter, update);

        // Operation completed successfully, return from the method
        return;
    }

    /// <summary>
    /// Gets a participant within the participants array of an event identified by the specified unique ID.
    /// </summary>
    /// <param name="uID">The unique identifier of the event.</param>
    /// <param name="participantId">The unique identifier of the participant to retrieve.</param>
    /// <param name="participantCode">The code of the participant to retrieve (optional).</param>
    /// <returns>The participant information.</returns>
    public async Task<Participant?> GetParticipant(string uID, string participantId, string? participantCode = null)
    {
        try
        {
            // Create a filter to identify the event based on its unique ID and the participant to retrieve
            var filter = Builders<Event>.Filter.And(
                Builders<Event>.Filter.Eq(x => x.Id, uID),
                participantCode != null
                    ? Builders<Event>.Filter.ElemMatch(x => x.Participants, p => p.Id == participantId && p.Codigo == participantCode)
                    : Builders<Event>.Filter.ElemMatch(x => x.Participants, p => p.Id == participantId)
            );

            // Perform an asynchronous find operation on the Events collection
            var result = await _eventsCollection.Find(filter).FirstOrDefaultAsync();

            // Return the retrieved participant (or null if not found)
            return result?.Participants.FirstOrDefault(p => p.Id == participantId && (participantCode == null || p.Codigo == participantCode));
        }
        catch (Exception ex)
        {
            // Handle the exception as needed (log it, rethrow it, or return a default value)
            Console.WriteLine($"An error occurred while retrieving the participant: {ex.Message}");
            throw;
        }
    }


    /// <summary>
    /// Updates a participant within the participants array of an event identified by the specified unique ID.
    /// </summary>
    /// <param name="uID">The unique identifier of the event.</param>
    /// <param name="participantId">The unique identifier of the participant to update.</param>
    /// <param name="updatedParticipant">The updated participant information.</param>
    /// <param name="participantCode">The code of the participant to update (optional).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateParticipant(string uID, string participantId, Participant updatedParticipant, string? participantCode = null)
    {
        try
        {
            // Create a filter to identify the event based on its unique ID and the participant to update
            var filter = Builders<Event>.Filter.And(
                Builders<Event>.Filter.Eq(x => x.Id, uID),
                participantCode != null
                    ? Builders<Event>.Filter.ElemMatch(x => x.Participants, p => p.Id == participantId && p.Codigo == participantCode)
                    : Builders<Event>.Filter.ElemMatch(x => x.Participants, p => p.Id == participantId)
            );

            // Create an update operation to set the entire participant to the updated participant information
            var update = Builders<Event>.Update.Set(x => x.Participants[-1], updatedParticipant);

            // Perform an asynchronous update operation on the Events collection
            await _eventsCollection.UpdateOneAsync(filter, update);

            // Operation completed successfully, return from the method
            return;
        }
        catch (Exception ex)
        {
            // Handle the exception as needed (log it, rethrow it, or return a default value)
            Console.WriteLine($"An error occurred while updating the participant: {ex.Message}");
            throw;
        }
    }



}
