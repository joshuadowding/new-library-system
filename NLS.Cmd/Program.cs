using NLS.Lib;
using System;

namespace NLS.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Launch();

            if (server.Check())
            {
                if (server.Connect()) // TODO: Currently doesn't reflect the actual status of the Fuseki server.
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
