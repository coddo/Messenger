using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Messenger.Enums;
using Messenger.Models;

namespace Messenger.Core
{
    /// <summary>
    /// Class used to describe clients that listen on the server for messages
    /// </summary>
    public class Client
    {
        #region Delegates
        
        /// <summary>
        /// Handler type for message received events
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e"></param>
        public delegate void MessageReceivedEventHandler(object sender, DirectMessageReceivedEventArgs e);

        #endregion

        #region Events

        /// <summary>
        /// Event for receiving messages
        /// </summary>
        public event MessageReceivedEventHandler OnMessageReceived;

        #endregion

        private readonly CancellationToken mCancellationToken;

        private readonly TopicMessageType[] mTopicsOfInterest;

        private readonly Task mTopicSearcherTask;

        /// <summary>
        /// Creates a new <see cref="Client"/> instance
        /// </summary>
        /// <param name="id">The Id that it will have</param>
        /// <param name="cancellationToken">The token used for cancelling tasks and stopping the client</param>
        /// <param name="topicsOfInterest">The topics that this client is interested in</param>
        public Client(Guid id, CancellationToken cancellationToken, params TopicMessageType[] topicsOfInterest)
        {
            OnMessageReceived += Client_OnDirectMessageReceived;

            Id = id;
            mCancellationToken = cancellationToken;
            mTopicsOfInterest = topicsOfInterest;

            mTopicSearcherTask = new Task(async () => await SearchTopicsAsync().ConfigureAwait(false), mCancellationToken);
        }

        /// <summary>
        /// The Id of the client
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The server under which it runs
        /// </summary>
        public Server Server { private get; set; }

        /// <summary>
        /// Starts the client and its background thread for dealing with topic messages from the server
        /// </summary>
        public void Start()
        {
            mTopicSearcherTask.Start();

            Console.WriteLine($"Client ({Id}) started!");
        }

        /// <summary>
        /// Receive a direct message from another client through the server and notify all listeners through the message received event 
        /// </summary>
        /// <param name="sender">The <see cref="Client"/> that has sent the message</param>
        /// <param name="message">The message that is sent</param>
        public async Task ReceiveMessageAsync(Client sender, Message message)
        {
            try
            {
                await Task.Run(() => OnMessageReceived?.Invoke(sender, new DirectMessageReceivedEventArgs(message)), mCancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"Task {nameof(ReceiveMessageAsync)} cancelled");
            }
        }

        /// <summary>
        /// Method for a background thread that checks for topic messages of interest present on the server
        /// </summary>
        private async Task SearchTopicsAsync()
        {
            try
            {
                while (!mCancellationToken.IsCancellationRequested)
                {
                    foreach (var topic in mTopicsOfInterest)
                    {
                        var topicMessages = await Server.GetTopicMessagesAsync(topic).ConfigureAwait(false);

                        foreach (var message in topicMessages)
                        {
                            Console.WriteLine($"[Client {Id}] (Topic Message) Received message of interest {message.Type} from [Client {message.SenderId}] with content: {message.Content}");
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2), mCancellationToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"Task {nameof(SearchTopicsAsync)} cancelled");
            }
        }

        /// <summary>
        /// Event handling for when a direct message is received
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="e">The event args that contain the received message</param>
        private void Client_OnDirectMessageReceived(object sender, DirectMessageReceivedEventArgs e)
        {
            var message = e.Message;
            if (message == null)
            {
                Debug.WriteLine($"[Client {Id}] (Queue Message) Received a null message");
                return;
            }

            Console.WriteLine($"[Client {Id}] (Queue Message) Received message from [Client {message.SenderId}] with content: {message.Content}");
        }
    }
}
