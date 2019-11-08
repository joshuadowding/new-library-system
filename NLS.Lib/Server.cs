using System;
using System.Diagnostics;
using System.Net;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace NLS.Lib
{
    public class Server
    {
        private const string APACHE_FUSEKI_DIR = "Resources/apache-jena-fuseki";
        private const string APACHE_BASE_URI = "http://localhost:3030/library-ontology/data";
        private const string ONTOLOGY_BASE_URI = "http://www.semanticweb.org/joshu/ontologies/2019/9/library-ontology#";

        private FusekiConnector fusekiConnector;
        private Process fusekiProcess;

        public void Launch()
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c fuseki-server --port 3030")
            {
                WorkingDirectory = APACHE_FUSEKI_DIR,
                CreateNoWindow = false,
                UseShellExecute = false
            };

            fusekiProcess = Process.Start(processInfo);
        }

        public bool Check()
        {
            bool result = false;

            for (int index = 0; index < 10; index++)
            {
                Console.WriteLine("Connection Attempt " + index + "...");

                if (TestServer())
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public void Close()
        {
            fusekiProcess.Close();
            fusekiProcess.Dispose();
        }

        public bool Connect()
        {
            Uri baseURI = new Uri(APACHE_BASE_URI);
            fusekiConnector = new FusekiConnector(baseURI);
            return fusekiConnector.IsReady;
        }

        public void Query()
        {
            string _subject = "subject";
            string _predicate = "predicate";
            string _object = "object";

            var queryBuilder = QueryBuilder.Select(new string[] { _subject, _predicate, _object }).Where(
                (triplePatturnBuilder) =>
                {
                    triplePatturnBuilder
                        .Subject(_subject)
                        .Predicate(_predicate)
                        .Object(_object);
                }).Limit(25);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(queryBuilder.BuildQuery().ToString());
            INodeFormatter resultFormatter = new SparqlFormatter();

            foreach (SparqlResult result in resultSet)
            {
                Console.WriteLine(result.ToString(resultFormatter));
            }
        }

        public void QueryCount()
        {
            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("lit", new Uri(ONTOLOGY_BASE_URI));
            queryString.CommandText = "SELECT ?series (COUNT(?novel) AS ?novels) ";
            queryString.CommandText += "WHERE { ?series a lit:Series. ?novel lit:isPartOf ?series } ";
            queryString.CommandText += "GROUP BY ?series ORDER BY ASC(?series)";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            INodeFormatter resultFormatter = new SparqlFormatter();

            foreach (SparqlResult result in resultSet)
            {
                Console.WriteLine(GetResultValue(result, "series"));
                Console.WriteLine(GetResultValue(result, "novels"));

                Console.WriteLine(result.ToString(resultFormatter));
                Console.WriteLine("");
            }
        }

        private string GetResultValue(SparqlResult result, string variable)
        {
            INode node;
            string value;

            if (result.TryGetValue(variable, out node))
            {
                switch (node.NodeType)
                {
                    case NodeType.Uri:
                        value = ((IUriNode)node).Uri.AbsoluteUri;
                        break;

                    case NodeType.Literal:
                        value = ((ILiteralNode)node).Value;
                        break;

                    default:
                        value = String.Empty;
                        break;
                }
            }
            else
            {
                value = String.Empty;
            }

            return value;
        }

        private bool TestServer()
        {
            bool result = false;
            HttpWebResponse response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:3030/");
                request.Method = "HEAD";
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException exception)
            {
                Console.WriteLine("Connection failure: " + exception);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    result = true;
                }
            }

            return result;
        }
    }
}
