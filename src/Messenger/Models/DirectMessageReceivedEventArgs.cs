namespace Messenger.Models
{
    /// <summary>
    /// Event arguments used by the clients to wrap received messages in an async manner
    /// </summary>
    public class DirectMessageReceivedEventArgs
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public DirectMessageReceivedEventArgs()
        {
        }

        /// <summary>
        /// Create a new <see cref="DirectMessageReceivedEventArgs"/> instance that contains a given message
        /// </summary>
        /// <param name="message">The message that this instance should transport</param>
        public DirectMessageReceivedEventArgs(Message message)
        {
            Message = message;
        }

        /// <summary>
        /// The message carried
        /// </summary>
        public Message Message { get; }
    }
}
