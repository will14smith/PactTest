using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PactTest.CommandLine
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            Configure(services);
            using var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            await Run(scope.ServiceProvider);
        }

        private static void Configure(IServiceCollection services)
        {
            services.AddHttpClient<IClient, Client>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001/");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                // ignore SSL for local...
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) => true
                };
            });
        }
        
        private static async Task Run(IServiceProvider services)
        {
            while (true)
            {
                var command = Prompt("Enter a command");

                try
                {
                    if (await RunCommand(services, command))
                    {
                        return;
                    }
                }
                catch(Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
        }

        private static async Task<bool> RunCommand(IServiceProvider services, string command)
        {
            switch (command)
            {
                case "L":
                case "l":
                    await ListOrders(services.GetRequiredService<IClient>());
                    break;
                case "G":
                case "g":
                    await GetOrder(services.GetRequiredService<IClient>());
                    break;
                case "A":
                case "a":
                    await AddOrder(services.GetRequiredService<IClient>());
                    break;
                case "U":
                case "u":
                    await UpdateOrder(services.GetRequiredService<IClient>());
                    break;
                case "D":
                case "d":
                    await DeleteOrder(services.GetRequiredService<IClient>());
                    break;

                case "H":
                case "h":
                case "?":
                    PrintHelp();
                    break;
                case "Q":
                case "q": return true;
                
                default: throw new ArgumentOutOfRangeException($"Unknown command: {command}");
            }

            return false;
        }

        private static async Task ListOrders(IClient client)
        {
            var orders = await client.GetAllAsync();
            foreach (var order in orders)
            {
                PrintOrder(order);
            }
        }

        private static async Task GetOrder(IClient client)
        {
            var id = int.Parse(Prompt("Enter order id"));
            
            var order = await client.GetByIdAsync(id);
            PrintOrder(order);
        }       
        private static async Task AddOrder(IClient client)
        {
            var person = Prompt("Enter person");
            var item = Prompt("Enter item  ");
            
            var order = await client.AddAsync(new OrderAdd { Person = person, Item = item });
            PrintOrder(order);
        }
        private static async Task UpdateOrder(IClient client)
        {
            var id = int.Parse(Prompt("Enter order id"));
            var person = Prompt("Enter person  ");
            var item = Prompt("Enter item    ");
            var shipped = bool.Parse(Prompt("Enter shipped "));
            
            var order = await client.UpdateAsync(id, new OrderUpdate { Person = person, Item = item, Shipped = shipped });
            PrintOrder(order);
        }   
        private static async Task DeleteOrder(IClient client)
        {
            var id = int.Parse(Prompt("Enter order id"));

            if (!await client.DeleteAsync(id))
            {
                throw new Exception($"Failed to delete order {id}");
            }
        }

        private static void PrintOrder(Order order)
        {
            Console.WriteLine($"Id: {order.Id.ToString().PadLeft(5)} Person: {order.Person.PadRight(20)} Item: {order.Item.PadRight(20)} Shipped: {order.Shipped}");
        }
        
        private static void PrintHelp()
        {
            Console.ForegroundColor = ConsoleColor.White;
            
            Console.WriteLine("Help:");
            Console.WriteLine();
            
            Console.WriteLine("L/l   - list all the orders");
            Console.WriteLine("G/g   - get a specific order by id");
            Console.WriteLine("A/a   - add a new order");
            Console.WriteLine("U/u   - update an order");
            Console.WriteLine("D/d   - delete an order");
            
            Console.WriteLine("H/h/? - print this help message");
            Console.WriteLine("Q/q   - exit the program");
            
            Console.ResetColor();
        }

        private static string Prompt(string prompt)
        {
            Console.Write($"{prompt}: ");
            return Console.ReadLine();
        }
    }
}