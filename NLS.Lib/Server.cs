using System;
using VDS.RDF.Storage;

namespace NLS.Lib
{
    public class Server
    {
        private const string APACHE_FUSEKI_URI = @"http://localhost:3030/library-ontology/data";

        private FusekiConnector fusekiConnector;

        public Query Query { get; set; }

        public bool Connect()
        {
            Uri baseURI = new Uri(APACHE_FUSEKI_URI);
            fusekiConnector = new FusekiConnector(baseURI);
            Query = new Query(fusekiConnector);
            return fusekiConnector.IsReady;
        }

        public void Disconnect()
        {
            fusekiConnector.Dispose();
        }
    }
}
