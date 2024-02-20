using CommunicationService.Models.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CommunicationService.Models;
using static CommunicationService.Helpers.Enumerated;

namespace CommunicationService.Services;

/// <summary>
/// Service class responsible for managing chat-related functionalities, such as creating, retrieving, updating, and deleting chat data.
/// </summary>
public class ChatService
{
    /// <summary>
    /// MongoDB collection for storing chat documents.
    /// </summary>
    private readonly IMongoCollection<Chat> _chatCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatService"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging information.</param>
    /// <param name="settings">The configuration settings for MongoDB connection.</param>
    public ChatService(ILogger<ChatService> logger, IOptions<ConnectionConfigurations> settings)
    {
        try
        {
            // Make the connection with MongoDB using the provided connection string
            var mongoClient = new MongoClient(settings.Value.ConnectionString);

            // Set the database using the specified database name
            var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);

            // Set the collection using the specified collection name
            _chatCollection = mongoDatabase.GetCollection<Chat>(settings.Value.ChatCollectionName);
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception for higher-level error handling
            logger.LogError(ex, "Error while initializing ChatService.");
            throw;
        }
    }

    /// <summary>
    /// Creates a new chat in the MongoDB collection.
    /// </summary>
    /// <param name="chat">The chat object to be created.</param>
    /// <returns>The created chat object.</returns>
    public async Task<Chat> CreateChat(Chat chat)
    {
        try
        {
            // Insert the chat into the MongoDB collection asynchronously
            await _chatCollection.InsertOneAsync(chat);
            return chat;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Retrieves a list of all chats.
    /// </summary>
    /// <returns>A list of all chats.</returns>
    public async Task<List<Chat>> GetAllChats()
    {
        try
        {
            // Retrieve all chats from the MongoDB collection
            var chats = await _chatCollection.Find(_ => true).ToListAsync();
            return chats;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Retrieves a chat by ID.
    /// </summary>
    /// <param name="chatId">The ID of the chat to retrieve.</param>
    /// <returns>The chat with the specified ID.</returns>
    public async Task<Chat> GetChatById(string chatId)
    {
        try
        {
            // Find the chat by ID
            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            var chat = await _chatCollection.Find(filter).FirstOrDefaultAsync();

            return chat;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Adds a new message to the chat.
    /// </summary>
    /// <param name="chatId">The ID of the chat where the message should be added.</param>
    /// <param name="message">The message to be added.</param>
    /// <returns>The updated chat object with the new message.</returns>
    public async Task<Chat> AddMessage(string chatId, Message message)
    {
        try
        {
            // Find the chat by ID
            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            var chat = await _chatCollection.Find(filter).FirstOrDefaultAsync();

            if (chat == null)
            {
                // Handle the case where the chat is not found
                throw new InvalidOperationException($"Chat with ID {chatId} not found.");
            }

           

            // Add the new message to the chat
            chat.Messages.Add(message);

            // Update the chat in the MongoDB collection
            var update = Builders<Chat>.Update.Set(c => c.Messages, chat.Messages);
            await _chatCollection.UpdateOneAsync(filter, update);

            return chat;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Updates the status of a message in the chat.
    /// </summary>
    /// <param name="chatId">The ID of the chat containing the message.</param>
    /// <param name="messageId">The ID of the message to update.</param>
    /// <param name="newStatus">The new status for the message.</param>
    /// <returns>The updated chat object with the message status updated.</returns>
    public async Task<Chat> UpdateMessageStatus(string chatId, string messageId, MessageStatus newStatus)
    {
        try
        {
            // Find the chat by ID
            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            var chat = await _chatCollection.Find(filter).FirstOrDefaultAsync();

            if (chat == null)
            {
                // Handle the case where the chat is not found
                throw new InvalidOperationException($"Chat with ID {chatId} not found.");
            }

            // Find the message by ID in the chat's messages
            var message = chat.Messages.Find(m => m.Id == messageId);

            if (message == null)
            {
                // Handle the case where the message is not found
                throw new InvalidOperationException($"Message with ID {messageId} not found in chat.");
            }

            // Update the message status
            message.Status = newStatus;

            // Update the chat in the MongoDB collection
            var update = Builders<Chat>.Update.Set(c => c.Messages, chat.Messages);
            await _chatCollection.UpdateOneAsync(filter, update);

            return chat;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Adds a participant to a chat.
    /// </summary>
    /// <param name="chatId">The ID of the chat to which the participant should be added.</param>
    /// <param name="participantId">The ID of the participant to be added.</param>
    /// <returns>The updated chat object with the new participant added.</returns>
    public async Task<Chat> AddParticipantToChat(string chatId, string participantId)
    {
        try
        {
            // Find the chat by ID
            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            var chat = await _chatCollection.Find(filter).FirstOrDefaultAsync();

            if (chat == null)
            {
                // Handle the case where the chat is not found
                throw new InvalidOperationException($"Chat with ID {chatId} not found.");
            }

            // Check if the chat type is Group
            if (chat.Type != ChatType.Group)
            {
                // Handle the case where the chat type is not Group
                throw new InvalidOperationException($"Cannot add participant to a non-Group chat.");
            }

            // Check if the participant is already in the chat
            if (chat.Participants.Contains(participantId))
            {
                // Handle the case where the participant is already in the chat
                throw new InvalidOperationException($"Participant with ID {participantId} is already in the chat.");
            }

            // Add the participant to the chat
            chat.Participants.Add(participantId);

            // Update the chat in the MongoDB collection
            var update = Builders<Chat>.Update.Set(c => c.Participants, chat.Participants);
            await _chatCollection.UpdateOneAsync(filter, update);

            return chat;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Removes a participant from a chat.
    /// </summary>
    /// <param name="chatId">The ID of the chat from which the participant should be removed.</param>
    /// <param name="participantId">The ID of the participant to be removed.</param>
    /// <returns>The updated chat object with the participant removed.</returns>
    public async Task<Chat> RemoveParticipantFromChat(string chatId, string participantId)
    {
        try
        {
            // Find the chat by ID
            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            var chat = await _chatCollection.Find(filter).FirstOrDefaultAsync();

            if (chat == null)
            {
                // Handle the case where the chat is not found
                throw new InvalidOperationException($"Chat with ID {chatId} not found.");
            }

            // Remove the participant from the chat
            chat.Participants.Remove(participantId);

            // Update the chat in the MongoDB collection
            var update = Builders<Chat>.Update.Set(c => c.Participants, chat.Participants);
            await _chatCollection.UpdateOneAsync(filter, update);

            return chat;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Retrieves a list of chats in which a user is a participant.
    /// </summary>
    /// <param name="participantId">The ID of the participant for whom to retrieve chats.</param>
    /// <returns>A list of chats in which the participant is involved.</returns>
    public async Task<List<Chat>> GetChatsForParticipant(string participantId)
    {
        try
        {
            // Find chats where the participant is involved
            var filter = Builders<Chat>.Filter.AnyEq(c => c.Participants, participantId);
            var chats = await _chatCollection.Find(filter).ToListAsync();

            return chats;
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception for higher-level error handling
            throw ex;
        }
    }




}
