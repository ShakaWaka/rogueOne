using System;
using System.Net.Sockets;
using System.Text;

namespace project
{
    class Connection
    {
        private const string hostname = "galaxy.ddns.net";
        private const string token = "3c8e4efe-1458-41f9-a8de-dd5959e4120a";
        protected TcpClient tcpClient;
        protected NetworkStream networkStream;

        /// <summary>
        /// Initializes a TcpClient and NetworkStream instances to send and receive data and sends the token to the server.
        /// </summary>
        /// <param name="port">Port used to initialize the connection.</param>
        public Connection(int port)
        {
            tcpClient = new TcpClient(hostname, port);
            networkStream = tcpClient.GetStream();
            Console.WriteLine($"Connected to the host {hostname} through the port {port}");

            write(token);
            Console.WriteLine("Token sent: " + token);
        }

        /// <summary>
        /// Converts a message to ASCII byte stream and sends it to the server.
        /// </summary>
        /// <param name="message">Message to be sent.</param>
        protected void write(string message)
        {
            if (networkStream.CanWrite)
            {
                var sendBytes = Encoding.ASCII.GetBytes(message);

                networkStream.Write(sendBytes, 0, sendBytes.Length);
            }
            else
            {
                Console.WriteLine("Cannot write data to the stream!");
                close();
            }
        }

        /// <summary>
        /// Reads server response.
        /// </summary>
        /// <returns>Message read.</returns>
        public byte[] read(int bufferSize)
        {
            var readBuffer = new byte[bufferSize];

            if (networkStream.CanRead)
            {
                int number = networkStream.Read(readBuffer, 0, readBuffer.Length);
            }   
            else
            {
                Console.WriteLine("Cannot read from the stream!");
                close();
            }

            return readBuffer;
        }

        /// <summary>
        /// Closes the connection and terminates the process.
        /// </summary>
        public void close()
        {
            tcpClient.Close();
            networkStream.Close();
            Console.WriteLine("Closing connection and stream...");
            Environment.Exit(0);
        }
    }
}
