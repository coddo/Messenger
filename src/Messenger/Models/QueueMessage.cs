using System;
using Messenger.Core;

namespace Messenger.Models
{
    /// <summary>
    /// Message type that is sent as a 1-1 conversation between to clients
    /// </summary>
    public class QueueMessage : Message
    {
        /// <summary>
        /// Create a new direct message between clients
        /// </summary>
        /// <param name="senderId">The Id of the <see cref="Client"/> that created the message</param>
        /// <param name="targetId">The Id of the <see cref="Client"/> that should receive the message</param>
        /// <param name="content">The content of the message</param>
        public QueueMessage(Guid senderId, Guid targetId, string content) : base(senderId, content)
        {
            TargetId = targetId;
        }

        /// <summary>
        /// The Id of the <see cref="Client"/> to whom the message is intended for
        /// </summary>
        public Guid TargetId { get; }
    }
}
