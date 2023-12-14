using LogProcessorAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
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

    
    public async Task<LogInformation> InsertsLogs(LogInformation log)
    {
        try
        {
            await _logsCollection.InsertOneAsync(log);
            return log;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    ///// <summary>
    ///// This method is used to check if the user exist on the database and if the password is correct
    ///// </summary>
    ///// <param name="username"> The username represent the username of the user</param>
    ///// <param name="password"> The password represent the password of the user</param>
    ///// <returns> This method return the user if the user exist and the password is correct, if not return null</returns>
    //public async Task<User> GetLogin(string username, string password)
    //{
    //    return await _usersCollection.Find<User>(user => user.Username.ToLower() == username.ToLower() && user.Password == password).FirstOrDefaultAsync();
    //}

    ///// <summary>
    ///// Validates the existence of a user account based on the provided username and email.
    ///// </summary>
    ///// <param name="username">The username to be validated.</param>
    ///// <param name="email">The email address to be validated.</param>
    ///// <returns>Returns true if a user account with the specified username and email exists; otherwise, returns false.</returns>
    //public async Task<bool> ValidateAccount(string username, string email)
    //{
    //    // Query the user collection to find a user with the specified username and email
    //    User user = await _usersCollection.Find<User>(user => user.Username.ToLower() == username.ToLower() && user.Email == email).FirstOrDefaultAsync();

    //    // If no user is found, return false; otherwise, return true
    //    return user != null;
    //}


    ///// <summary>
    ///// This method is used to check if the username exist on the database
    ///// </summary>
    ///// <param name="username"> The username represent the username of the user</param>
    ///// <returns> This method return true if the user if the user exist if not return false</returns>
    //public async Task<bool> GetUsername(string username)
    //{
    //    User User = await _usersCollection.Find<User>(user => user.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

    //    // If no user is found, return false; otherwise, return true
    //    return User != null;
    //}

    ///// <summary>
    ///// Retrieves a user by their username.
    ///// </summary>
    ///// <param name="username">The username of the user to retrieve.</param>
    ///// <returns>Returns a User object if found; otherwise, returns null.</returns>
    //public async Task<User> GetByUsername(string username)
    //{
    //    // Perform a case-insensitive search for the user by username
    //    User user = await _usersCollection
    //        .Find(u => u.Username.ToLower() == username.ToLower())
    //        .FirstOrDefaultAsync();

    //    return user;
    //}


    ///// <summary>
    ///// Checks if the provided email already exists in the database.
    ///// </summary>
    ///// <param name="email">The email to check for existence.</param>
    ///// <returns>Returns a boolean indicating whether the email already exists (true) or not (false).</returns>
    //public async Task<bool> GetEmail(string email)
    //{
    //    // Retrieve a user with the specified email from the database
    //    User user = await _usersCollection.Find<User>(u => u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();

    //    // If a user with the provided email exists, return true; otherwise, return false
    //    return user != null;
    //}

    ///// <summary>
    ///// Retrieves a user based on their unique identifier.
    ///// </summary>
    ///// <param name="uID">The unique identifier of the user to retrieve.</param>
    ///// <returns>Returns a Task containing the User object if found; otherwise, returns null.</returns>
    //public async Task<User> GetUser(string uID)
    //{
    //    User user = await _usersCollection.Find<User>(user => user.Id == uID).FirstOrDefaultAsync();

    //    return user;
    //}

    ///// <summary>
    ///// Changes the user type for a user with the specified unique identifier.
    ///// </summary>
    ///// <param name="uID">The unique identifier of the user whose type is to be changed.</param>
    ///// <param name="newUserType">The new user type to assign to the user.</param>
    ///// <returns>A Task representing the asynchronous operation.</returns>
    //public async Task ChangeUserType(string uID, UserType newUserType)
    //{
    //    // Create a filter to identify the user based on their unique ID
    //    var filter = Builders<User>.Filter.Eq(x => x.Id, uID);

    //    // Create an update operation to set the user's UserType to the specified new user type
    //    var update = Builders<User>.Update.Set(x => x.UserType, newUserType);

    //    // Perform an asynchronous update operation on the Users collection
    //    await _usersCollection.UpdateOneAsync(filter, update);
    //}


    ///// <summary>
    ///// Saves a refresh token for a user identified by their unique ID.
    ///// </summary>
    ///// <param name="uID">The unique identifier of the user.</param>
    ///// <param name="refreshToken">The refresh token to be saved.</param>
    ///// <returns>A Task representing the asynchronous operation.</returns>
    //public async Task SaveRefreshToken(string uID, string refreshToken)
    //{
    //    // Create a filter to identify the user based on their unique ID
    //    var filter = Builders<User>.Filter.Eq(x => x.Id, uID);

    //    // Create an update operation to push (add) the new refresh token to the user's RefreshToken array
    //    var update = Builders<User>.Update.Push(x => x.RefreshToken, refreshToken);

    //    // Perform an asynchronous update operation on the Users collection
    //    await _usersCollection.UpdateOneAsync(filter, update);

    //    // Operation completed successfully, return from the method
    //    return;
    //}

    ///// <summary>
    ///// Deletes a specific refresh token associated with a user.
    ///// </summary>
    ///// <param name="uID">The unique identifier of the user.</param>
    ///// <param name="refreshToken">The refresh token to be removed.</param>
    ///// <returns>A Task representing the asynchronous operation.</returns>
    //public async Task DeleteRefreshToken(string uID, string refreshToken)
    //{
    //    // Create a filter to identify the user based on their unique ID
    //    var filter = Builders<User>.Filter.Eq(x => x.Id, uID);

    //    // Create an update operation to remove the specified refresh token from the user's RefreshToken array
    //    var update = Builders<User>.Update.Pull(x => x.RefreshToken, refreshToken);

    //    // Perform an asynchronous update operation on the Users collection
    //    await _usersCollection.UpdateOneAsync(filter, update);
    //}

    ///// <summary>
    ///// Saves a password recovery entry for a user identified by the specified username and email.
    ///// </summary>
    ///// <param name="username">The username associated with the account.</param>
    ///// <param name="email">The email address associated with the account.</param>
    ///// <param name="recovery">The password recovery entry to be saved.</param>
    ///// <returns>A Task representing the asynchronous operation.</returns>
    //public async Task SaveRecoveryCode(string username, string email, PasswordRecovery recovery)
    //{
    //    // Create a filter to find the user with the specified username and email
    //    var filter = Builders<User>.Filter.And(
    //        Builders<User>.Filter.Eq(x => x.Username, username),
    //        Builders<User>.Filter.Eq(x => x.Email, email)
    //    );

    //    // Retrieve the user based on the filter
    //    var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

    //    // Check if the user is found
    //    if (user != null)
    //    {
    //        // Initialize the PasswordRecoveries collection if it is null
    //        if (user.PasswordRecoveries == null)
    //        {
    //            user.PasswordRecoveries = new List<PasswordRecovery>();
    //        }

    //        // Add the new recovery entry to the user's PasswordRecoveries collection
    //        user.PasswordRecoveries.Add(recovery);

    //        // Create an update definition to set the PasswordRecoveries property of the user
    //        var update = Builders<User>.Update.Set(x => x.PasswordRecoveries, user.PasswordRecoveries);

    //        // Update the user in the collection with the new PasswordRecoveries collection
    //        await _usersCollection.UpdateOneAsync(filter, update);
    //    }
    //}

    ///// <summary>
    ///// Checks the validity of a recovery code for a user identified by the specified username and email.
    ///// </summary>
    ///// <param name="username">The username associated with the account.</param>
    ///// <param name="email">The email address associated with the account.</param>
    ///// <param name="recoveryCode">The recovery code to be validated.</param>
    ///// <returns>Returns true if the recovery code is valid for the specified user; otherwise, returns false.</returns>
    //public async Task<bool> CheckRecoveryCode(string username, string email, string recoveryCode)
    //{
    //    // Create a filter to find the user with the specified username and email
    //    var filter = Builders<User>.Filter.And(
    //        Builders<User>.Filter.Eq(x => x.Username, username),
    //        Builders<User>.Filter.Eq(x => x.Email, email)
    //    );

    //    // Retrieve the user based on the filter
    //    var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

    //    // Check if the user is found
    //    if (user != null)
    //    {
    //        // Find the recovery entry with the specified recovery code
    //        var recovery = user.PasswordRecoveries.FirstOrDefault(r => r.RecoveryCode == recoveryCode);

    //        // If a matching recovery entry is found, the recovery code is valid
    //        if (recovery != null)
    //        {
    //            return true;
    //        }
    //    }

    //    // If no user or matching recovery entry is found, the recovery code is invalid
    //    return false;
    //}

    ///// <summary>
    ///// Checks the validity of a recovery code for a user identified by the specified username and email.
    ///// The recovery code must match an entry in the user's password recovery records, and it must not be expired.
    ///// </summary>
    ///// <param name="username">The username associated with the account.</param>
    ///// <param name="recoveryCode">The recovery code to be validated.</param>
    ///// <returns>Returns true if the recovery code is valid and not expired for the specified user; otherwise, returns false.</returns>
    //public async Task<bool> ValidRecoveryCode(string username, string recoveryCode)
    //{
    //    // Create a filter to find the user with the specified username and email
    //    var filter = Builders<User>.Filter.And(
    //        Builders<User>.Filter.Eq(x => x.Username, username)
    //    );

    //    // Retrieve the user based on the filter
    //    var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

    //    // Check if the user is found
    //    if (user != null)
    //    {
    //        // Find the recovery entry with the specified recovery code and not expired
    //        var recovery = user.PasswordRecoveries.FirstOrDefault(r => r.RecoveryCode == recoveryCode && r.ExpirationTime > DateTime.Now);

    //        // If a matching and not expired recovery entry is found, the recovery code is valid
    //        if (recovery != null)
    //        {
    //            return true;
    //        }
    //    }

    //    // If no user or matching recovery entry is found, or the recovery is expired, the recovery code is invalid
    //    return false;
    //}

    ///// <summary>
    ///// Updates the password of a user identified by their user ID.
    ///// </summary>
    ///// <param name="userId">The unique identifier of the user.</param>
    ///// <param name="newPassword">The new password to set for the user.</param>
    ///// <returns>Returns a Task representing the asynchronous operation.</returns>
    //public async Task UpdatePassword(string userId, string newPassword)
    //{
    //    // Define a filter to identify the user by their user ID
    //    var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

    //    // Define an update to set the new password
    //    var update = Builders<User>.Update.Set(x => x.Password, newPassword);

    //    // Perform the update operation asynchronously
    //    await _usersCollection.UpdateOneAsync(filter, update);
    //}


}
