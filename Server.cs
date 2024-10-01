using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace chatApp
{
    public class Server
    {
        private TcpListener _server; //Server to listen for connections
        private List<TcpClient> _clients = new List<TcpClient>(); //List to keep track of active clients
        private bool _isRunning;

        public Server(int port)
        {
            _server = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            _server.Start();
            _isRunning = true;
            Console.WriteLine("Server started..");

            new Thread(ListenForClients).Start(); //start listening for clients on a new thread

        }

        private void ListenForClients()
        {
            while (_isRunning)
            {
                var client = _server.AcceptTcpClient();
                _clients.Add(client);
                Console.WriteLine("Client connected!");

                Thread clientThread = new Thread(HandleClient); //handle client on a separate thread
                clientThread.Start();
            }
        }

        private void HandleClient(object clientObject)
        {
            TcpClient client = (TcpClient)clientObject;
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);

            while (_isRunning)
            {
                try
                {
                    string message = reader.ReadLine();
                    if (message != null)
                    {
                        BroadcastMessage(message);
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    break;
                }
            }

            client.Close();
            _clients.Remove(client); //clean up

        }

        private void BroadcastMessage(string message)
        {
            foreach (var client in _clients)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                    writer.WriteLine(message);
                    writer.Flush();
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to client: {ex}");
                }
            }
        }

    }
}
