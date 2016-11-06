using System;
using Messenger.Core;

namespace Messenger.Models
{
    /// <summary>
    /// Base class for different types of messages
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Creates a new message that contains details about its sender and the content it carries
        /// </summary>
        /// <param name="senderId">The Id of the <see cref="Client"/> that has created this message</param>
        /// <param name="content">The content that the current message carries</param>
        protected Message(Guid senderId, string content)
        {
            SenderId = senderId;
            Content = content;

            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// The Id of the <see cref="Client"/> sender
        /// </summary>
        public Guid SenderId { get; }

        /// <summary>
        /// The content of the message
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// The exact time at which is was created. This is set automatically when creating a new <see cref="Message"/> instance
        /// </summary>
        public DateTime CreatedAt { get; }
    }
}
