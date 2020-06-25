using System;
using System.Text;
using System.Text.RegularExpressions;

namespace project
{
    class Empire : Connection
    {
        private byte[] size;
        private byte[] receivedChecksum;
        private byte calculatedChecksum;

        public enum Option { TELL_ME_MORE, SEND_AGAIN, STOP };

        /// <summary>
        /// Initializes the connection to the Empire and verifies that the token is correct.
        /// </summary>
        /// <param name="port">Port used to initialize the connection.</param>
        public Empire(int port) : base(port)
        {
            var message = Encoding.ASCII.GetString(verify(read(false)));

            if (message.Contains("User accepted."))
            {
                Console.WriteLine("Empire: User accepted.");
            }
            else if (message.Contains("Game over!"))
            {
                Console.WriteLine("Empire: Game over!");
                Console.WriteLine("Failed to send token to Empire!");
                // Closing the TCP connection does not close the stream.
                networkStream.Close();
                Console.WriteLine("Closing connection and stream with the Empire...");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("Unexpected message from Empire!");
                close();
            }
        }

        /// <summary>
        /// Read the size bytes and use them to read the payload.
        /// </summary>
        /// <returns>Message read (size bytes + payload).</returns>
        private byte[] read(bool sendAgain)
        {
            if (!sendAgain)
            {
                size = read(2);
            }

            byte[] message = read(BitConverter.ToInt16(size, 0));

            receivedChecksum = read(1);

            return message;
        }

        /// <summary>
        /// Sends a predefined message to the server.
        /// </summary>
        /// <param name="option">Option chosen to communicate with the server.</param>
        /// <returns>Message received from server.</returns>
        public byte[] communicate(Option option)
        {
            switch (option)
            {
                case Option.TELL_ME_MORE:
                    write("tell me more");
                    Console.WriteLine("Me: tell me more");
                    return read(false);
                // Uses base.read() instead of read() because size bytes will not be received.
                case Option.SEND_AGAIN:
                    write("send again");
                    Console.WriteLine("Me: send again");
                    return read(true);
                case Option.STOP:
                    write("stop");
                    Console.WriteLine("Me: stop");
                    // Closing the TCP connection does not close the stream.
                    networkStream.Close();
                    Console.WriteLine("Closing connection and stream with the Empire...");
                    return new byte[0];
                default:
                    Console.WriteLine("Invalid option to communicate with the Empire!");
                    close();
                    return new byte[0];
            }
        }

        /// <summary>
        /// Discover the encryption key knowing the word that is present in every message.
        /// </summary>
        /// <param name="message">Encrypted message without size and checksum bytes.</param>
        /// <returns>Decrypted message.</returns>
        private byte[] decrypt(byte[] message)
        {
            var word = Encoding.ASCII.GetBytes("Vader");
            // Initialize key to avoid warnings.
            var key = new byte();

            // Runs through only the part of the message to which you can apply bitwise XOR with the whole word.
            for (int i = 0; i < message.Length - word.Length; i++)
            {
                var results = new byte[word.Length];

                // Calculates the bitwise XOR between the word and the part in question of the message.
                for (int j = 0; j < word.Length; j++)
                {
                    results[j] = (byte)(message[i + j] ^ word[j]);

                    // From the second byte, check if it is the same as the previous one.
                    if ((j > 0) && (results[j] != results[j - 1]))
                    {
                        break;
                    }
                }

                // Verifies that all bitwise XOR results between the word and the part in question of the message are equal.
                if (Array.TrueForAll(results, result => result == results[0]))
                {
                    key = results[0];
                    break;
                }
            }

            // Applies the found key to get the decrypted message.
            for (int i = 0; i < message.Length; i++)
            {
                message[i] = (byte)(message[i] ^ key);
            }

            Console.WriteLine("Empire message decrypted: " + Encoding.ASCII.GetString(message));

            return message;
        }

        private void calculateChecksum(byte[] message)
        {
            var sum = size[0] + size[1];

            // Sums all bytes of message except checksum
            for (int i = 0; i < message.Length; i++)
            {
                sum += message[i];
            }

            var sumBytes = BitConverter.GetBytes(sum);

            calculatedChecksum = sumBytes[0];
        }

        /// <summary>
        /// Calculates and checks checksum, requesting the payload again if needed, and calls decrypt.
        /// </summary>
        /// <param name="message">Encrypted message with size and checksum bytes.</param>
        /// <returns>Decrypted message.</returns>
        public byte[] verify(byte[] message)
        {
            calculateChecksum(message);

            while (calculatedChecksum != receivedChecksum[0])
            {
                Console.WriteLine("Incorrect Empire message checksum!");
                message = communicate(Option.SEND_AGAIN);
                calculateChecksum(message);
            }

            var messageString = Encoding.ASCII.GetString(message);

            if ((messageString.Contains("User accepted.")) || (messageString.Contains("Game over!")))
            {
                return message;
            }
            else
            {
                return decrypt(message);
            }
        }

        /// <summary>
        /// Checks if a message contains the character 'x' followed by a number with unknown amount of
        /// digits followed by the character 'y' followed by a number with unknown amount of digits
        /// </summary>
        /// <param name="message">Decrypted message.</param>
        /// <returns>True if the message contains coordinates, false otherwise.</returns>
        public bool findCoordinates(byte[] message)
        {
            return Regex.IsMatch(Encoding.ASCII.GetString(message), @"[x]\d+[y]\d+") ? true : false;
        }
    }
}
