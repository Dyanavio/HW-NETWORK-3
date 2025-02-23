namespace HW_NETWORK_CLIENT_3
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string ip = "127.0.0.1";
            const int port = 5050;

            Client client = new(ip, port);
            await client.ConnectAsync();

            Console.ReadKey();
        }
    }
}
