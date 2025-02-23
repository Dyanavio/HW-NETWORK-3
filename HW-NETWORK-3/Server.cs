using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HW_NETWORK_3
{
    internal class Server
    {
        private TcpListener listener;
        private bool isRunning;
        private int clients = 2;
        SortedDictionary<string, double> converters = new SortedDictionary<string, double>()
        {
            ["UAH USD"] = 0.024042,
            ["USD UAH"] = 41.5943,
            ["USD EUR"] = 0.953,
            ["EUR USD"] = 1.0493,
            ["EUR GBP"] = 0.8337,
            ["GBP EUR"] = 1.1995,
            ["SEK UAH"] = 3.889984,
            ["UAH SEK"] = 0.25707
        };

        public Server(string ip, int port)
        {
            listener = new(IPAddress.Parse(ip), port);
        }
        public async Task StartAsync()
        {
            try
            {
                listener.Start();
                isRunning = true;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Server launched . . .");
                Console.ResetColor();

                while(isRunning)
                {

                    Console.WriteLine("Clients: " + clients);
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    if (clients != 0)
                    {
                        clients--;
                        HandleAsync(client);
                    }
                    else
                    {
                        WarnAsync(client);
                    }
                }
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Starting:" + e.Message);
                Console.ResetColor();
            }
        }
        public async void HandleAsync(TcpClient client)
        {
            string log = "";
            int attempts = 2;
            try
            {
                await using NetworkStream stream = client.GetStream();
                log += $"Connection time: {DateTime.Now}\n";

                for (int i = 0; i <= attempts; i++)
                {
                    byte[] buffer = new byte[1024];
                    int size = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (size == 0) break;

                    string input = Encoding.UTF8.GetString(buffer, 0, size);
                    log += input + '\n';
                    Console.WriteLine($"Received: {input}");
                    if (input.ToLower().Trim() == "exit")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Exiting");
                        break;
                    }
                    if (converters.TryGetValue(input, out double rate))
                    {
                        string? reply = $"{input}\n1 -> {converters[input.ToString()]}\nAttempts remaining: {attempts - i}";
                        buffer = Encoding.UTF8.GetBytes(reply);
                        await stream.WriteAsync(buffer, 0, buffer.Length);

                        if (attempts - i == 0)
                        {
                            log += $"Disconnect time: {DateTime.Now}\n";
                            break;
                        }
                    }
                    else
                    {
                        string reply = "Conversion not found!";
                        buffer = Encoding.UTF8.GetBytes(reply);
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Handling: " + e.Message);
                Console.ResetColor();
            }
            finally
            {
                clients++;
                client.Close();
                Console.ForegroundColor= ConsoleColor.Red;
                Console.WriteLine("Client disconnected");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(log);
                Console.ResetColor();
            }
        }
        private async void WarnAsync(TcpClient client)
        {
            await using NetworkStream stream = client.GetStream();

            byte[] buff = new byte[1024];
            int size = await stream.ReadAsync(buff, 0, buff.Length);

            string warning = "Server is overloaded. Forced disconnection";
            byte[] buffer = Encoding.UTF8.GetBytes(warning);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            
            client.Close();
        }
        public void Stop()
        {
            isRunning = false;
            listener.Stop();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Server stopped . . .");
            Console.ResetColor();
        }
    }
}
