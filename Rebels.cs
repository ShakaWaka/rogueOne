using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Numerics;

namespace project
{
    class Rebels : Connection
    {
        private int encryptExponent;
        private int modulus;

        public enum Response { OK, SUCESS, GAME_OVER, UNEXPECTED };

        /// <summary>
        /// Initializes the connection to the Rebels and recovers the BFF public key (Encrypt Exponent and Modulus).
        /// </summary>
        /// <param name="port">Port used to initialize the connection.</param>
        public Rebels(int port) : base(port)
        {
            byte[] key = read(tcpClient.ReceiveBufferSize);
            MatchCollection numbers = Regex.Matches(Encoding.ASCII.GetString(key), @"\d+");

            encryptExponent = Int32.Parse(numbers[0].Value);
            modulus = Int32.Parse(numbers[1].Value);

            Console.WriteLine("Encrypt exponent: " + encryptExponent);
            Console.WriteLine("Modulus: " + modulus);
        }

        /// <summary>
        /// Encrypts a message using modular exponentiation.
        /// </summary>
        /// <param name="message">Message to be encrypted</param>
        private void encrypt(byte[] message)
        {
            for (int i = 0; i < message.Length; i++)
            {
                BigInteger encryptedCharacter = BigInteger.ModPow(message[i], encryptExponent, modulus);
                var encryptedBytes = encryptedCharacter.ToByteArray();

                message[i] = encryptedBytes[encryptedBytes.Length - 1];
            }
        }

        /// <summary>
        /// Assembles the message to be sent to the Rebels following the pattern: 2 bytes for the size of the message + encrypted message without checksum.
        /// </summary>
        /// <param name="message">Encrypted message.</param>
        public void write(byte[] message)
        {
            if (networkStream.CanWrite)
            {
                encrypt(message);

                var size = BitConverter.GetBytes(message.Length);
                
                Buffer.BlockCopy(size, 0, message, 0, 1);
                Buffer.BlockCopy(size, size.Length - 1, message, 0, 1);
                
                networkStream.Write(message, 0, message.Length);
            }
            else
            {
                Console.WriteLine("Cannot write data to the Rebel's stream!");
                close();
            }
        }

        /// <summary>
        /// Reads Rebel's server response.
        /// </summary>
        /// <returns>Predefined message read.</returns>
        public Response read()
        {
            var message = Encoding.ASCII.GetString(read(tcpClient.ReceiveBufferSize));


            if (message.Contains("OK"))
            {
                Console.WriteLine("Rebels: OK");

                return Response.OK;
            }
            else if (message.Contains("Sucess"))
            {
                Console.WriteLine("Rebels: Sucess");

                return Response.SUCESS;
            }

            else if (message.Contains("Game over!"))
            {
                Console.WriteLine("Rebels: Game over!");
                // Closing the TCP connection does not close the stream.
                networkStream.Close();
                Console.WriteLine("Closing connection and stream with the Rebels...");

                return Response.GAME_OVER;
            }
            else
            {
                Console.WriteLine("Unexpected message from Rebels!");
                close();

                return Response.UNEXPECTED;
            }
        }
    }
}
