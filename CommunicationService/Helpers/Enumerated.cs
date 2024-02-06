namespace CommunicationService.Helpers;

/// <summary>
/// Class that contains the ChatType enumeration.
/// </summary>
public class Enumerated
{
    /// <summary>
    /// Enumeration representing the type of chat.
    /// </summary>
    public enum ChatType
    {
        /// <summary>
        /// Represents an individual chat.
        /// </summary>
        Individual,

        /// <summary>
        /// Represents a group chat.
        /// </summary>
        Group
    }


    /// <summary>
    /// Enumeration that represents the status of a message.
    /// </summary>
    public enum MessageStatus
    {
        /// <summary>
        /// The message has been sent.
        /// </summary>
        Sent,

        /// <summary>
        /// The message has been delivered.
        /// </summary>
        Delivered,

        /// <summary>
        /// The message has been read.
        /// </summary>
        Read
    }

}
