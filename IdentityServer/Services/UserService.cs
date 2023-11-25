using IdentityServer.Controllers;
using IdentityServer.Models;
using IdentityServer.Models.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using static MicroservicesHelpers.Enumerated;

namespace IdentityServer.Services;

public class UserService
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserService(ILogger<UserController> logger, IOptions<ConnectionConfigurations> settings)
    {
        //Make the connection with the MongoDB
        var mongoClient = new MongoClient(
        settings.Value.ConnectionString);

        //Set the database 
        var mongoDatabase = mongoClient.GetDatabase(
        settings.Value.DatabaseName);

        //Set the collection
        _usersCollection = mongoDatabase.GetCollection<User>(settings.Value.UserAuthCollectionName);
    }

    /// <summary>
    /// This method is used to create a new user Auth
    /// </summary>
    /// <param name="user"> The users represent the user to be created</param>
    /// <returns> The users represent the user created</returns>
    public async Task<User> CreateUsers(User user)
    {
        try
        {
            await _usersCollection.InsertOneAsync(user);
            return user;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// This method is used to check if the user exist on the database and if the password is correct
    /// </summary>
    /// <param name="username"> The username represent the username of the user</param>
    /// <param name="password"> The password represent the password of the user</param>
    /// <returns> This method return the user if the user exist and the password is correct, if not return null</returns>
    public async Task<User> GetLogin(string username, string password)
    {
        return await _usersCollection.Find<User>(user => user.Username.ToLower() == username.ToLower() && user.Password == password).FirstOrDefaultAsync();
    }

    /// <summary>
    /// This method is used to check if the username exist on the database
    /// </summary>
    /// <param name="username"> The username represent the username of the user</param>
    /// <returns> This method return true if the user if the user exist if not return false</returns>
    public async Task<bool> GetUsername(string username)
    {
        User User = await _usersCollection.Find<User>(user => user.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();
    
        if(User != null)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Checks if the provided email already exists in the database.
    /// </summary>
    /// <param name="email">The email to check for existence.</param>
    /// <returns>Returns a boolean indicating whether the email already exists (true) or not (false).</returns>
    public async Task<bool> GetEmail(string email)
    {
        // Retrieve a user with the specified email from the database
        User user = await _usersCollection.Find<User>(u => u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();

        // If a user with the provided email exists, return true; otherwise, return false
        return user != null;
    }

    /// <summary>
    /// Retrieves a user based on their unique identifier.
    /// </summary>
    /// <param name="uID">The unique identifier of the user to retrieve.</param>
    /// <returns>Returns a Task containing the User object if found; otherwise, returns null.</returns>
    public async Task<User> GetUser(string uID)
    {
        User user = await _usersCollection.Find<User>(user => user.Id == uID).FirstOrDefaultAsync();

        return user;
    }

    /// <summary>
    /// Changes the user type for a user with the specified unique identifier.
    /// </summary>
    /// <param name="uID">The unique identifier of the user whose type is to be changed.</param>
    /// <param name="newUserType">The new user type to assign to the user.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task ChangeUserType(string uID, UserType newUserType)
    {
        // Create a filter to identify the user based on their unique ID
        var filter = Builders<User>.Filter.Eq(x => x.Id, uID);

        // Create an update operation to set the user's UserType to the specified new user type
        var update = Builders<User>.Update.Set(x => x.UserType, newUserType);

        // Perform an asynchronous update operation on the Users collection
        await _usersCollection.UpdateOneAsync(filter, update);
    }


    /// <summary>
    /// Saves a refresh token for a user identified by their unique ID.
    /// </summary>
    /// <param name="uID">The unique identifier of the user.</param>
    /// <param name="refreshToken">The refresh token to be saved.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task SaveRefreshToken(string uID, string refreshToken)
    {
        // Create a filter to identify the user based on their unique ID
        var filter = Builders<User>.Filter.Eq(x => x.Id, uID);

        // Create an update operation to push (add) the new refresh token to the user's RefreshToken array
        var update = Builders<User>.Update.Push(x => x.RefreshToken, refreshToken);

        // Perform an asynchronous update operation on the Users collection
        await _usersCollection.UpdateOneAsync(filter, update);

        // Operation completed successfully, return from the method
        return;
    }

    /// <summary>
    /// Deletes a specific refresh token associated with a user.
    /// </summary>
    /// <param name="uID">The unique identifier of the user.</param>
    /// <param name="refreshToken">The refresh token to be removed.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task DeleteRefreshToken(string uID, string refreshToken)
    {
        // Create a filter to identify the user based on their unique ID
        var filter = Builders<User>.Filter.Eq(x => x.Id, uID);

        // Create an update operation to remove the specified refresh token from the user's RefreshToken array
        var update = Builders<User>.Update.Pull(x => x.RefreshToken, refreshToken);

        // Perform an asynchronous update operation on the Users collection
        await _usersCollection.UpdateOneAsync(filter, update);
    }


}
