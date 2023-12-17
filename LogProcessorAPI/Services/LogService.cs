using LogProcessorAPI.Helpers;
using LogProcessorAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Diagnostics;
using System.Globalization;
using static MicroservicesHelpers.Enumerated;

namespace LogProcessorAPI.Services;

public class LogService
{
    private readonly IMongoCollection<LogInformation> _logsCollection;

    public LogService(ILogger<LogInformation> logger, IOptions<ConnectionConfigurations> settings)
    {
        //Make the connection with the MongoDB
        var mongoClient = new MongoClient(
        settings.Value.ConnectionString);

        //Set the database 
        var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);

        //Set the collection
        _logsCollection = mongoDatabase.GetCollection<LogInformation>(settings.Value.LogProcessorCollectionName);
    }


    /// <summary>
    /// Inserts a log information document into the logs collection asynchronously.
    /// </summary>
    /// <param name="log">The log information to insert into the collection.</param>
    /// <returns>A task representing the asynchronous operation, returning the inserted log information.</returns>
    public async Task<LogInformation> InsertsLogs(LogInformation log)
    {                                                                 
        try
        {
            // Insert the log information into the logs collection asynchronously
            await _logsCollection.InsertOneAsync(log);

            // Return the inserted log information
            return log;
        }
        catch (Exception ex)
        {
            // Rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    public async Task<List<LogInformation>> ListLogs(MicroservicesName? microservicesName = null, DateTime? dFrom = null, DateTime? dTo = null)
    {
        try
        {
            /// Create a filter to match documents based on the provided criteria
            var filter = Builders<LogInformation>.Filter.Empty;

            if (microservicesName != null)
            {
                // If microservicesName is not null, add a filter to match documents with the specified microservicesName
                filter &= Builders<LogInformation>.Filter.Eq(e => e.WebAPI, microservicesName.ToString());
            }

            // Add date filters if provided
            if (dFrom != null && dTo != null)
            {
                filter &= Builders<LogInformation>.Filter.Gte(e => e.TimeStanp, dFrom) &
                          Builders<LogInformation>.Filter.Lte(e => e.TimeStanp, dTo);
            }

            // Execute the query to retrieve all documents that match the filter
            var result = await _logsCollection.Find(filter).ToListAsync();

            // Return the list of documents
            return result;
        }
        catch (Exception ex)
        {
            // In case of an exception, rethrow it for higher-level error handling
            throw ex;
        }
    }
}
