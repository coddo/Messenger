using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Messenger.Config;
using Messenger.Core;
using Messenger.Enums;

namespace Messenger
{
    /// <summary>
    /// Class that contains the entry point for the application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point of the application. Creates the <see cref="Server"/> and <see cref="Client"/>s and runs a simulation
        /// for the real-time messenger app
        /// </summary>
        public static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            Console.WriteLine($"Press any key to stop the entire simulation at any moment{Environment.NewLine}{Environment.NewLine}");

            Task.Delay(Settings.Instance.DelayBeforeSimulationStarts, cancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();

            StartSimulation(cancellationTokenSource).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task StartSimulation(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                // Create clients with their own topics of interest
                var client1 = new Client(Guid.NewGuid(), cancellationTokenSource.Token, TopicMessageType.Msdn, TopicMessageType.TheCodeProject);
                var client2 = new Client(Guid.NewGuid(), cancellationTokenSource.Token, TopicMessageType.TheCodeProject);
                var client3 = new Client(Guid.NewGuid(), cancellationTokenSource.Token, TopicMessageType.StackOverFlow, TopicMessageType.Msdn, TopicMessageType.TheCodeProject);

                // Create server
                var server = new Server(cancellationTokenSource);

                // Register clients into the server
                await server.RegisterClientAsync(client1).ConfigureAwait(false);
                await server.RegisterClientAsync(client2).ConfigureAwait(false);
                await server.RegisterClientAsync(client3).ConfigureAwait(false);

                // Start the server and the clients
                server.Start();
                client1.Start();
                client2.Start();
                client3.Start();

                // Leave some space to separate the initialization messages from the actual simulation messages
                Console.WriteLine();
                Console.WriteLine();

                // Start the simulation
                Simulate(cancellationTokenSource.Token, server, client1, client2, client3);

                // Wait for a key press in order to stop the simulation
                Console.ReadKey();

                // Stop all the threads and processes
                cancellationTokenSource.Cancel(false);

                // Confirm simulation end
                await Task.Delay(100).ConfigureAwait(false);
                Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}The simulation ended. Press any key to exit...");
            }
            finally
            {
                // Wait for a key press to exit the application
                Console.ReadKey();
            }
        }

        private static void Simulate(CancellationToken cancellationToken, Server server, Client client1, Client client2, Client client3)
        {
            var clients = new[]
            {
                client1,
                client2,
                client3
            };

            // Tasks for generating some topic messages
            var topicsTask1 = new Action(async () =>
            {
                try
                {
                    DateTime expireDate;
                    for (var i = 0; i < 350; i++)
                    {
                        expireDate = i % 3 == 0 ? DateTime.Now.AddSeconds(3) : DateTime.Now.AddSeconds(50);

                        try
                        {
                            await server.AddTopicMessageAsync(clients[i % 3].Id, TopicMessageType.Msdn, $"Topic hello world [{i}]", expireDate).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SERVER ERROR RESPONSE] {ex.Message}");
                        }

                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Simulation task canceled");
                }
            });

            var topicsTask2 = new Action(async () =>
            {
                try
                {
                    DateTime expireDate;
                    for (var i = 0; i < 350; i++)
                    {
                        expireDate = i % 3 == 0 ? DateTime.Now.AddSeconds(3) : DateTime.Now.AddSeconds(50);

                        try
                        {
                            await server.AddTopicMessageAsync(clients[i % 3].Id, TopicMessageType.StackOverFlow, $"Topic hello world [{i}]", expireDate).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SERVER ERROR RESPONSE] {ex.Message}");
                        }

                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Simulation task canceled");
                }

            });

            var topicsTask3 = new Action(async () =>
            {
                try
                {
                    DateTime expireDate;
                    for (var i = 0; i < 350; i++)
                    {
                        expireDate = i % 3 == 0 ? DateTime.Now.AddSeconds(3) : DateTime.Now.AddSeconds(50);

                        try
                        {
                            await server.AddTopicMessageAsync(clients[i % 3].Id, TopicMessageType.TheCodeProject, $"Topic hello world [{i}]", expireDate).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SERVER ERROR RESPONSE] {ex.Message}");
                        }

                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Simulation task canceled");
                }
            });

            // Tasks for generating some private messages
            var messageTask12 = new Action(async () =>
            {
                try
                {
                    for (var i = 0; i < 350; i++)
                    {
                        try
                        {
                            await server.AddQueueMessageAsync(client1.Id, client2.Id, $"Direct message from {client1.Id} to {client2.Id} Hello world!").ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SERVER ERROR RESPONSE] {ex.Message}");
                        }

                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Simulation task canceled");
                }
            });

            var messageTask13 = new Action(async () =>
            {
                try
                {
                    for (var i = 0; i < 350; i++)
                    {
                        try
                        {
                            await server.AddQueueMessageAsync(client1.Id, client3.Id, $"Direct message from {client1.Id} to {client3.Id} Hello world!").ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SERVER ERROR RESPONSE] {ex.Message}");
                        }

                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Simulation task canceled");
                }
            });

            var messageTask21 = new Action(async () =>
            {
                try
                {
                    for (var i = 0; i < 350; i++)
                    {
                        try
                        {
                            await server.AddQueueMessageAsync(client2.Id, client1.Id, $"Direct message from {client2.Id} to {client1.Id} Hello world!").ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SERVER ERROR RESPONSE] {ex.Message}");
                        }

                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Simulation task canceled");
                }
            });

            var messageTask23 = new Action(async () =>
            {
                try
                {
                    for (var i = 0; i < 350; i++)
                    {
                        try
                        {
                            await server.AddQueueMessageAsync(client2.Id, client3.Id, $"Direct message from {client2.Id} to {client3.Id} Hello world!").ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SERVER ERROR RESPONSE] {ex.Message}");
                        }

                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Simulation task canceled");
                }
            });

            var messageTask31 = new Action(async () =>
            {
                try
                {
                    for (var i = 0; i < 350; i++)
                    {
                        try
                        {
                            await server.AddQueueMessageAsync(client3.Id, client1.Id, $"Direct message from {client3.Id} to {client1.Id} Hello world!").ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SERVER ERROR RESPONSE] {ex.Message}");
                        }

                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Simulation task canceled");
                }
            });

            var messageTask32 = new Action(async () =>
            {
                try
                {
                    for (var i = 0; i < 350; i++)
                    {
                        try
                        {
                            await server.AddQueueMessageAsync(client3.Id, client2.Id, $"Direct message from {client3.Id} to {client2.Id} Hello world!").ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[SERVER ERROR RESPONSE] {ex.Message}");
                        }

                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine($"Simulation task canceled");
                }
            });

            // Start the simulation Tasks
            try
            {
                Parallel.Invoke(
                    () => topicsTask1.Invoke(),
                    () => topicsTask2.Invoke(),
                    () => topicsTask3.Invoke(),
                    () => messageTask12.Invoke(),
                    () => messageTask13.Invoke(),
                    () => messageTask21.Invoke(),
                    () => messageTask23.Invoke(),
                    () => messageTask31.Invoke(),
                    () => messageTask32.Invoke()
                );
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("Simulation was interrupted");
            }
        }
    }
}
