using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

namespace Server
{
    class Program
    {
        static void Main()
        {

            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            Console.Title = "QueueServer";

            var endpointConfiguration = new EndpointConfiguration("ClientUI");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            var routing = transport.Routing();

            //TODO: Добавть нормальные messages
            routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            await RunLoop(endpointInstance);


            await endpointInstance.Stop()
                .ConfigureAwait(false);

        }

        private static ILog log = LogManager.GetLogger<Program>();

        static async Task RunLoop(IEndpointInstance endpointInstance)
        {
            while (true)
            {
                {
                    log.Info("Press 'P' to place an order, or 'Q' to quit.");
                    var key = Console.ReadKey();
                    Console.WriteLine();

                    switch (key.Key)
                    {
                        case ConsoleKey.P:
                            var command = new PlaceOrder
                            {
                                OrderId = Guid.NewGuid().ToString()
                            };

                            log.Info("Press 'P' to place an order, or 'Q' to quit.");
                            await endpointInstance.Send(command)
                                .ConfigureAwait(false);
                            break;
                        case ConsoleKey.Q:
                            return;
                        default:
                            log.Info("Unknown input. Please try again.");
                            break;

                    }
                }
            }
        }
    }
}
