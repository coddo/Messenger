using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Messenger.Config;
using Messenger.Enums;
using Messenger.Models;

namespace Messenger.Core
{
    /// <summary>
    /// Class for the server that handles all types of messages and interacts with the clients
    /// </summary>
    public class Server
    {
        private readonly CancellationTokenSource mCancellationTokenSource;
        private readonly Task mTopicInvalidatorTask;
        private readonly Task mMessageSenderTask;
        private readonly IList<Client> mClients;
        private readonly IList<TopicMessage> mTopicMessages;
        private readonly Queue<QueueMessage> mQueueMessages;

        /// <summary>
        /// Creates a new instance of the <see cref="Server" /> class
        /// </summary>
        /// <param name="cancellationTokenSource">
        /// The source of the token that sends a cancel signal to all underlying threads and stops the server
        /// </param>
        public Server(CancellationTokenSource cancellationTokenSource)
        {
            mClients = new List<Client>();

            mQueueMessages = new Queue<QueueMessage>();
            mTopicMessages = new List<TopicMessage>();

            mCancellationTokenSource = cancellationTokenSource;

            mTopicInvalidatorTask = new Task(async () => await InvalidateTopicsAsync().ConfigureAwait(false), mCancellationTokenSource.Token);
            mMessageSenderTask = new Task(async () => await SendMessagesAsync().ConfigureAwait(false), mCancellationTokenSource.Token);
        }

        #region Server management

        /// <summary>
        /// Starts the server and all it's inner threads that handle messages
        /// </summary>
        public void Start()
        {
            mTopicInvalidatorTask.Start();
            mMessageSenderTask.Start();

            Console.WriteLine("Server started!");
        }

        #endregion

        #region Client management

        /// <summary>
        /// Register a new client into the server
        /// </summary>
        /// <param name="client">The client that will be registered</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the Id of the client is invalid or a client with the same Id is already registered
        /// </exception>
        public async Task RegisterClientAsync(Client client)
        {
            if (client.Id == Guid.Empty)
            {
                throw new ArgumentException($"Client registration failed: the entered client has an invalid Id: {client.Id}");
            }

            lock (mClients)
            {
                if (mClients.Any(cl => cl.Id == client.Id))
                {
                    throw new ArgumentException($"Client registration failed: There is already a client with the Id: {client.Id}");
                }
            }

            try
            {
                await Task.Run(() =>
                {
                    lock (mClients)
                    {
                        mClients.Add(client);
                    }

                    client.Server = this;
                }, mCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"Task {nameof(RegisterClientAsync)} cancelled");
            }
        }

        #endregion

        #region Messages

        /// <summary>
        /// Adds a new direct message to the messages queue
        /// </summary>
        /// <param name="senderId">The Id of the client that sent the message</param>
        /// <param name="targetId">The Id of the client that needs to receive the message</param>
        /// <param name="content">The content of the message</param>
        /// <exception cref="ArgumentNullException">Thrown when the content of the message is null or empty</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the Id of either the sender or the receiver is invalid or not registered in the server
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when the message queue has reached it's maximum capacity and cannot retain any more messages
        /// </exception>
        public async Task AddQueueMessageAsync(Guid senderId, Guid targetId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "Cannot add a message with null content to the queue");
            }

            lock (mClients)
            {
                if (mClients.All(client => client.Id != senderId))
                {
                    throw new ArgumentException($"Cannot send message from an inexistent client. There is no registered client with id: {senderId}", nameof(senderId));
                }

                if (mClients.All(client => client.Id != targetId))
                {
                    throw new ArgumentException($"Cannot send message to an inexistent client. There is no registered client with id: {targetId}", nameof(targetId));
                }
            }

            lock (mQueueMessages)
            {
                if (mQueueMessages.Count >= Settings.Instance.MaxQueueMessages)
                {
                    throw new OverflowException("The messages queue has reached its maximum capacity and cannot retain any more messages");
                }
            }

            try
            {
                await Task.Run(() =>
                {
                    var message = new QueueMessage(senderId, targetId, content);

                    lock (mQueueMessages)
                    {
                        mQueueMessages.Enqueue(message);
                    }
                }, mCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"Task {nameof(AddQueueMessageAsync)} cancelled");
            }
        }

        /// <summary>
        /// Adds a new topic message for everyone with a specific topic and type
        /// </summary>
        /// <param name="senderId">The Id of the client that sent the message</param>
        /// <param name="type">The topic type of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="expirationDate">The date at which the message should expire</param>
        /// <exception cref="ArgumentNullException">Thrown when the content of the message is null or empty</exception>
        /// <exception cref="ArgumentException">Thrown when the Id of the sender is invalid or not registered into the server</exception>
        public async Task AddTopicMessageAsync(Guid senderId, TopicMessageType type, string content, DateTime expirationDate)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "Cannot add a message with null content to the queue");
            }

            lock (mClients)
            {
                if (mClients.All(client => client.Id != senderId))
                {
                    throw new ArgumentException($"Cannot send message from an inexistent client. There is no registered client with id: {senderId}", nameof(senderId));
                }
            }

            try
            {
                await Task.Run(() =>
                {
                    var message = new TopicMessage(senderId, type, content, expirationDate);

                    lock (mTopicMessages)
                    {
                        mTopicMessages.Add(message);
                    }
                }, mCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"Task {nameof(AddTopicMessageAsync)} cancelled");
            }
        }

        /// <summary>
        /// Retrieve all the available topic messages that have a certain type
        /// </summary>
        /// <param name="topicMessageType">The type of the topic messages to be fetched</param>
        /// <returns>A collection of topic messages that are of the given type</returns>
        public async Task<IList<TopicMessage>> GetTopicMessagesAsync(TopicMessageType topicMessageType)
        {
            try
            {
                return await Task.Run(() =>
                {
                    lock (mTopicMessages)
                    {
                        return mTopicMessages.Where(message => message.Type == topicMessageType).ToList();
                    }
                }, mCancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"Task {nameof(GetTopicMessagesAsync)} cancelled");
                return new List<TopicMessage>();
            }
        }

        #endregion

        #region Task actions

        /// <summary>
        /// Method for a background task that invalidates expired topic messages
        /// </summary>
        private async Task InvalidateTopicsAsync()
        {
            try
            {
                while (!mCancellationTokenSource.Token.IsCancellationRequested)
                {
                    lock (mTopicMessages)
                    {
                        for (var i = 0; i < mTopicMessages.Count;)
                        {
                            var message = mTopicMessages[i];

                            if (DateTime.Now.CompareTo(message.ExpiresAt) >= 0 || 
                                Settings.Instance.MaxTopicLifeSpan.CompareTo((DateTime.Now - message.CreatedAt)) <= 0)
                            {
                                mTopicMessages.RemoveAt(i);
                            }
                            else
                            {
                                i++;
                            }
                        }
                    }

                    await Task.Delay(50, mCancellationTokenSource.Token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"Task {nameof(InvalidateTopicsAsync)} cancelled");
            }
        }

        /// <summary>
        /// Method for a background task that takes messages from a message queue and sends them to their respective targets
        /// </summary>
        private async Task SendMessagesAsync()
        {
            try
            {
                bool canContinue;
                while (!mCancellationTokenSource.Token.IsCancellationRequested)
                {
                    QueueMessage message = null;
                    lock (mQueueMessages)
                    {
                        if (mQueueMessages.Count > 0)
                        {
                            canContinue = true;
                            message = mQueueMessages.Dequeue();
                        }
                        else
                        {
                            canContinue = false;
                        }
                    }

                    if (!canContinue)
                    {
                        await Task.Delay(50, mCancellationTokenSource.Token).ConfigureAwait(false);
                        continue;
                    }

                    Client senderClient;
                    Client targetClient;
                    lock (mClients)
                    {
                        senderClient = mClients.FirstOrDefault(client => client.Id == message.SenderId);
                        targetClient = mClients.FirstOrDefault(client => client.Id == message.TargetId);
                    }
                    
                    if (senderClient != null && targetClient != null)
                    {
                        await targetClient.ReceiveMessageAsync(senderClient, message).ConfigureAwait(false);
                    }
                    else
                    {
                        Console.WriteLine($"No client with Id {message.TargetId} found to send the message to");
                    }

                    await Task.Delay(50, mCancellationTokenSource.Token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"Task {nameof(SendMessagesAsync)} cancelled");
            }
        }

        #endregion
    }
}
