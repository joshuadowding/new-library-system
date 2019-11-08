using NLS.Lib;
using System;

namespace NLS.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            //GraphLoader graphLoader = new GraphLoader();
            //graphLoader.Load();

            Server server = new Server();

            server.Launch(); // TODO: Make it so that the server doesn't attempt a connection before Fuseki has launched.

            if (server.Check())
            {
                if (server.Connect("http://localhost:3030/library-ontology/data")) // TODO: Currently doesn't reflect the actual status of the Fuseki server.
                {
                    Console.WriteLine("Connected.");
                    server.Query();

                    Console.WriteLine("\n");
                    server.QueryCount();

                    server.Close();
                }
            }
            else
            {
                server.Close();
            }

            Console.ReadLine();
        }
    }
}
