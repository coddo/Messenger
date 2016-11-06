using System;
using Messenger.Core;
using Messenger.Enums;

namespace Messenger.Models
{
    /// <summary>
    /// Message type with general content that can be accessed by anyone
    /// </summary>
    public class TopicMessage : Message
    {
        /// <summary>
        /// Create a new topic message that has an exact topic and type
        /// </summary>
        /// <param name="senderId">The Id of the <see cref="Client"/> that created the message</param>
        /// <param name="type">The type of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="expiresAt">The time at which the message should expire</param>
        public TopicMessage(Guid senderId, TopicMessageType type, string content, DateTime expiresAt) : base(senderId, content)
        {
            Type = type;
            ExpiresAt = expiresAt;
        }


        /// <summary>
        /// The type of topic that the message has
        /// </summary>
        public TopicMessageType Type { get; }

        /// <summary>
        /// The time at which the topic message expires
        /// </summary>
        public DateTime ExpiresAt { get; }
    }
}
