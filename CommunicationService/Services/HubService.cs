﻿using MicroservicesHelpers.Models;
using CommunicationService.Models.MongoDB;
using CommunicationService.Models.Requests;
using CommunicationService.Models.SignalR;
using static CommunicationService.Helpers.Enumerated;
using static MicroservicesHelpers.Enumerated;
using CommunicationService.Models.Responses;
using DeepL;
using DeepL.Model;
using System.Data.Common;
using System.Security;

namespace CommunicationService.Services;

public class HubService
{
    /// <summary>
    /// Represents a static, instance of the Connection class.
    /// This class holds information associated with a user's connection.
    /// </summary>
    private static List<Connection> _connections;

    /// <summary>
    /// Represents a static, instance of the ConversationRoom class.
    /// This class encapsulates information related to a conversation room.
    /// </summary>
    private static List<ConversationRoom> _conversationRoom;

    /// <summary>
    /// 
    /// </summary>
    private readonly ChatService _chatService;

    private string _apiKey;


    public HubService(ChatService chatService)
    {
        _connections = new List<Connection>();
        _conversationRoom = new List<ConversationRoom>();
        _chatService = chatService;
        _apiKey = "577134a5-dd84-488e-b59c-c4fa196dacaf:fx";
    }

    /// <summary>
    /// Creates a new connection or updates an existing one with the given connectionId.
    /// </summary>
    /// <param name="connection">The connection information to be added or updated.</param>
    /// <param name="connectionId">The unique identifier of the connection.</param>
    /// <exception cref="Exception">Thrown if the operation fails.</exception>
    public async Task CreateConnection(Connection connection, string connectionId)
    {
        try
        {
            Connection existingConnection = null;

            // Check if there are any existing connections
            if (_connections.Any(c => c.UserID == connection.UserID))
            {
                // If a connection with the same UserID exists, add the connectionId to it
                existingConnection = _connections.First(c => c.UserID == connection.UserID);
                existingConnection.ConnectionIDs.Add(connectionId);
            }
            else
            {
                // If no connection with the same UserID exists, add the new connection
                connection.ConnectionIDs.Add(connectionId);
                _connections.Add(connection);

                existingConnection = connection;
            }

            List<Chat> lstChat = await GetChats(connection.UserID);

            if(lstChat.Count > 0)
            {
                List<string> lstChatGroup = lstChat.FindAll(x => x.Type == ChatType.Group)
                                                   .Select(chat => chat.Id)
                                                   .ToList();

                if (existingConnection.Rooms.Count > 0)
                {
                    foreach (string chat in lstChatGroup)
                    {
                        if(!existingConnection.Rooms.Contains(chat))
                            existingConnection.Rooms.Add(chat);
                    }
                }
                else
                {
                    existingConnection.Rooms.AddRange(lstChatGroup);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create connection.", ex);
        }
    }

    /// <summary>
    /// Disconnects a connection based on the given connectionId.
    /// </summary>
    /// <param name="connectionId">The unique identifier of the connection to be disconnected.</param>
    /// <exception cref="Exception">Thrown if the operation fails.</exception>
    public void DisconnectConnection(string connectionId)
    {
        try
        {
            // Find the connection that contains the specified connectionId
            var connectionToRemove = _connections.FirstOrDefault(c => c.ConnectionIDs.Contains(connectionId));

            if (connectionToRemove != null)
            {
                // Remove the connectionId from the list of ConnectionIDs
                connectionToRemove.ConnectionIDs.Remove(connectionId);

                // If there are no more associated ConnectionIDs, remove the entire connection
                if (connectionToRemove.ConnectionIDs.Count == 0)
                {
                    _connections.Remove(connectionToRemove);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to disconnect connection.", ex);
        }
    }

    public async Task<List<Chat>> GetChats(string userID)
    {
        try
        {
            return await _chatService.GetChatsForParticipant(userID);
        }
        catch (Exception ex)
        {
            // Log and handle the exception according to your application's error handling strategy
            throw ex;
        }
    }

    /// <summary>
    /// Creates a new chat based on the provided request.
    /// </summary>
    /// <param name="request">The request containing information to create a chat.</param>
    /// <returns>A response indicating the result of the operation along with the created chat.</returns>
    public async Task<MicroservicesResponse> CreateChat(ChatRequests request)
    {
        try
        {
            // Create a new Chat instance and populate it with data from the request
            Chat chat = new Chat
            {
                Participants = request.Participants,
                Name = request.Name
            };

            // Create the chat using the ChatService
            chat = await _chatService.CreateChat(chat);

            // Add messages to the created chat
            foreach (var message in request.Messages)
            {
                // Add each message to the chat and get the updated chat
                MicroservicesResponse res = await AddMessage(chat.Id, message);
                chat = (Chat)res.Data;
            }

            // Return a response indicating successful chat creation
            return new MicroservicesResponse(MicroservicesCode.OK, "Chat Created", "The chat was created successfully.", chat);
        }
        catch (InvalidOperationException ex)
        {
            return new MicroservicesResponse(MicroservicesCode.Validation, "Validation Error", ex.Message, null);
        }
        catch (Exception ex)
        {
            // Rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Adds a new message to the specified chat.
    /// </summary>
    /// <param name="chatID">The ID of the chat to which the message will be added.</param>
    /// <param name="requests">The request containing information to create a message.</param>
    /// <returns>A response indicating the result of the operation along with the updated chat.</returns>
    public async Task<MicroservicesResponse> AddMessage(string chatID, MessageRequests requests)
    {
        try
        {
            // Create a new Message instance and populate it with data from the request
            Message message = new Message
            {
                SenderId = requests.SenderId,
                ReceiverId = requests.ReceiverId,
                Content = requests.Content
            };

            // Add the message to the specified chat using the ChatService
            var updatedChat = await _chatService.AddMessage(chatID, message);

            // Return a response indicating successful message addition
            return new MicroservicesResponse(MicroservicesCode.OK, "", "", updatedChat);
        }
        catch (InvalidOperationException ex)
        {
            return new MicroservicesResponse(MicroservicesCode.Validation, "Validation Error", ex.Message, null);
        }
        catch (Exception ex)
        {
            // Rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Updates the status of a specific message in a chat.
    /// </summary>
    /// <param name="chatId">The ID of the chat containing the message.</param>
    /// <param name="messageId">The ID of the message to update.</param>
    /// <param name="newStatus">The new status for the message.</param>
    /// <returns>A response indicating the result of the operation along with the updated chat.</returns>
    public async Task<MicroservicesResponse> UpdateMessageStatus(string chatId, string messageId, MessageStatus newStatus)
    {
        try
        {
            // Update the message status in the specified chat using the ChatService
            var updatedChat = await _chatService.UpdateMessageStatus(chatId, messageId, newStatus);

            // Return a response indicating successful message status update
            return new MicroservicesResponse(MicroservicesCode.OK, "Status Updated", "Message status updated successfully", updatedChat);
        }
        catch (InvalidOperationException ex)
        {
            return new MicroservicesResponse(MicroservicesCode.Validation, "Validation Error", ex.Message, null);
        }
        catch (Exception ex)
        {
            // Rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Adds a participant to the chat.
    /// </summary>
    /// <param name="chatId">The ID of the chat where the participant should be added.</param>
    /// <param name="participantId">The ID of the participant to be added.</param>
    /// <returns>A MicroservicesResponse indicating the result of the operation.</returns>
    public async Task<MicroservicesResponse> AddParticipant(string chatId, string participantId)
    {
        try
        {
            await _chatService.AddParticipantToChat(chatId, participantId);

            return new MicroservicesResponse(MicroservicesCode.OK, "Participant Added", "Participant added successfully.", null);
        }
        catch (InvalidOperationException ex)
        {
            return new MicroservicesResponse(MicroservicesCode.Validation, "Validation Error", ex.Message, null);
        }
        catch (Exception ex)
        {
            // Log the exception for debugging purposes
            // Log.Error($"Error adding participant to chat. ChatId: {chatId}, ParticipantId: {participantId}", ex);

            // Rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    /// <summary>
    /// Removes a participant from the chat.
    /// </summary>
    /// <param name="chatId">The ID of the chat from which the participant should be removed.</param>
    /// <param name="participantId">The ID of the participant to be removed.</param>
    /// <returns>A MicroservicesResponse indicating the result of the operation.</returns>
    public async Task<MicroservicesResponse> RemoveParticipant(string chatId, string participantId)
    {
        try
        {
            await _chatService.RemoveParticipantFromChat(chatId, participantId);

            return new MicroservicesResponse(MicroservicesCode.OK, "Participant Removed", "Participant removed successfully.", null);
        }
        catch (InvalidOperationException ex)
        {
            return new MicroservicesResponse(MicroservicesCode.Validation, "Validation Error", ex.Message, null);
        }
        catch (Exception ex)
        {
            // Log the exception for debugging purposes
            // Log.Error($"Error removing participant from chat. ChatId: {chatId}, ParticipantId: {participantId}", ex);

            // Rethrow the exception for higher-level error handling
            throw ex;
        }
    }

    public async Task<(List<string>, MessageResponse)> SendMessage(MessageSignalR message)
    {
        try
        {
            MessageRequests msgR = new MessageRequests();
            msgR.SenderId = message.SenderId;
            msgR.Content = message.Content;
            msgR.ReceiverId = message.ReceiverId;

            MicroservicesResponse res = await AddMessage(message.ReceiverId, msgR);

            MessageResponse msgRes = new MessageResponse();
            List<string> connectionIDs = new List<string>();

            Connection conn = _connections.FirstOrDefault(u => u.UserID == message.SenderId);
            msgRes.SenderId = message.SenderId;
            msgRes.Content = message.Content;
            msgRes.ChatID = message.ReceiverId;
            msgRes.ReceiverId = message.ReceiverId;

            if (message.Type == ChatType.Individual)
            {
                Chat chat = (Chat)res.Data;

                foreach (string userid in chat.Participants)
                {
                    Connection receiverId = _connections.FirstOrDefault(u => u.UserID == userid);
                    connectionIDs.AddRange(receiverId.ConnectionIDs);
                }
            }
            else
            {
                // Encontrar todas as conexões que têm msgR.ReceiverId na lista de Rooms
                List<Connection> matchingConnections = _connections.FindAll(r => r.Rooms.Contains(msgR.ReceiverId)).ToList();

                // Obter todos os IDs das conexões encontradas em uma lista única
                List<string> receiverIds = matchingConnections.SelectMany(connection => connection.ConnectionIDs).ToList();

                connectionIDs.AddRange(receiverIds);
            }

            return (connectionIDs, msgRes);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    public async Task<List<string>> SendNotifications(Notifications notifications)
    {
        try
        {
            List<string> connectionIDs = new List<string>();

            foreach (string user in notifications.UserID)
            {
                List<string> conn = _connections.FirstOrDefault(u => u.UserID == user).ConnectionIDs;

                if(conn != null && conn.Count > 0)
                {
                    connectionIDs.AddRange(conn);
                }
            }

            return connectionIDs;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<TextResult> TranslateDeeplAsync(string text, string languageCode)
    {
        try
        {
            var translator = new DeepL.Translator(_apiKey);

            TextResult translatedText = await translator.TranslateTextAsync(text, null, languageCode);

            return translatedText;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

}
