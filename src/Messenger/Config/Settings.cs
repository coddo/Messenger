using System;
using Messenger.Core;

namespace Messenger.Config
{
    /// <summary>
    /// Class for general environment configuration used by <see cref="Server"/> and <see cref="Client"/> class instances
    /// </summary>
    public class Settings
    {
        #region Singleton declaration

        private static Settings mInstance;

        /// <summary>
        /// Gets the instance for the <see cref="Settings"/> class
        /// </summary>
        public static Settings Instance => mInstance ?? (mInstance = new Settings());

        #endregion

        /// <summary>
        /// Private empty constructor used to block instantiation outside of the <see cref="Settings"/> class context
        /// </summary>
        private Settings()
        {
        }

        /// <summary>
        /// The time to wait before starting the actual simulation
        /// </summary>
        public TimeSpan DelayBeforeSimulationStarts { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Maximum number of messages that the queue can contains
        /// </summary>
        public int MaxQueueMessages { get; set; } = 1000;

        /// <summary>
        /// The maximum time a topic stays active before it gets invalidated
        /// </summary>
        public TimeSpan MaxTopicLifeSpan { get; set; } = TimeSpan.FromSeconds(5);
    }
}
