using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace HW_NETWORK_CLIENT_3
{
    internal class Client
    {
        private readonly string ip;
        private readonly int port;
        TcpClient client;
        public Client(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            client = new TcpClient();
        }
        public async Task ConnectAsync()
        {
            await client.ConnectAsync(IPAddress.Parse(ip), port);
            await HandleAsync();
        }
        public async Task HandleAsync()
        {
            try
            {
                await using NetworkStream stream = client.GetStream();
                while (true)
                {
                    Console.Write("Enter conversion currencies (FROM -> TO)\nFROM('Exit' to exit the program): ");
                    string? fromCurrency = Console.ReadLine()?.ToUpper().Trim();
                    if (fromCurrency == "EXIT") break;
                    Console.Write("TO: ");
                    string? toCurrency = Console.ReadLine()?.ToUpper().Trim();

                    if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Currencies cannot be empty.");
                        Console.ResetColor();
                        continue;
                    }

                    string? message = $"{fromCurrency} {toCurrency}";

                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    if (message.ToLower().Trim() == "exit") break;

                    byte[] buffer1 = new byte[1024];
                    int size = await stream.ReadAsync(buffer1, 0, buffer1.Length);
                    string message1 = Encoding.UTF8.GetString(buffer1, 0, size);

                    if (message1.Contains("Server is overloaded"))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(message1);
                        Console.ResetColor();
                        break;
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{message1}");
                    Console.ResetColor();
                    if (message1[message1.Length - 1] == '0')
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("No more attempts remaining");
                        Console.ResetColor();

                        client.Close();
                        return;
                    }
                }
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
        }
        
    }
}
