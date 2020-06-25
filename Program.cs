namespace project
{
    class Program
    {
        static void Main(string[] args)
        {
            var empire = new Empire(21817);
            var rebels = new Rebels(31817);

            // Performs all communication until the Rebels respond "Success" or until receives a "Game over!".
            do
            {
                byte[] message = empire.communicate(Empire.Option.TELL_ME_MORE);
                message = empire.verify(message);

                if (empire.findCoordinates(message))
                {
                    rebels.write(message);

                    switch (rebels.read())
                    {
                        case Rebels.Response.OK:
                            continue;
                        case Rebels.Response.SUCESS:
                            empire.communicate(Empire.Option.STOP);
                            return;
                        case Rebels.Response.GAME_OVER:
                            empire.close();
                            return;
                    }
                }
            } while (true);
        }
    }
}
