using System;
using System.Collections.Generic;
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
        private const string APACHE_FUSEKI_DIR = "~/Resources/apache-jena-fuseki";
        private const string APACHE_SERVER_URL = "http://localhost:3030";
        private const string APACHE_FUSEKI_URI = "http://localhost:3030/library-ontology/data";
        private const string ONTOLOGY_BASE_URI = "http://www.semanticweb.org/joshu/ontologies/2019/9/library-ontology#";
        private const string RDFS_BASE_URI = "http://www.w3.org/2000/01/rdf-schema#";

        private static FusekiConnector fusekiConnector;

        public static bool Connect()
        {
            Uri baseURI = new Uri(APACHE_FUSEKI_URI);
            fusekiConnector = new FusekiConnector(baseURI);
            return fusekiConnector.IsReady;
        }

        public static void Disconnect()
        {
            fusekiConnector.Dispose();
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

        /// <summary>
        /// Gets the total number of publications per series.
        /// </summary>
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

                //Console.WriteLine(result.ToString(resultFormatter));
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// Gets all of the individual publications per series.
        /// </summary>
        public void QueryPublicationsPerSeries()
        {
            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("lit", new Uri(ONTOLOGY_BASE_URI));
            queryString.CommandText = "SELECT ?series ?novel ";
            queryString.CommandText += "WHERE { ?series a lit:Series. ?novel lit:isPartOf ?series } ";
            queryString.CommandText += "ORDER BY ASC(?series)";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            INodeFormatter resultFormatter = new SparqlFormatter();

            foreach (SparqlResult result in resultSet)
            {
                Console.WriteLine(GetResultValue(result, "series"));
                Console.WriteLine(GetResultValue(result, "novel"));

                //Console.WriteLine(result.ToString(resultFormatter));
                Console.WriteLine("");
            }
        }

        public static List<string> QueryClassesForSelectors(string className)
        {
            List<string> selectOptions = new List<string>();

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("rdfs", new Uri(RDFS_BASE_URI));
            queryString.Namespaces.AddNamespace("lib", new Uri(ONTOLOGY_BASE_URI));

            queryString.CommandText = "SELECT DISTINCT ?class ";
            queryString.CommandText += "WHERE { ?class rdfs:subClassOf lib:" + className + " } ";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            foreach (SparqlResult result in resultSet)
            {
                string classURI = GetResultValue(result, "class");
                string[] classSplit = classURI.Split('#');
                string classOption = classSplit[1].Trim();

                selectOptions.Add(classOption);
            }

            return selectOptions;
        }

        public static List<string> QueryIndividualsForSelectors(string className)
        {
            List<string> selectOptions = new List<string>();

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("rdfs", new Uri(RDFS_BASE_URI));
            queryString.Namespaces.AddNamespace("lib", new Uri(ONTOLOGY_BASE_URI));

            queryString.CommandText = "SELECT DISTINCT ?class ";
            queryString.CommandText += "WHERE { ?class a lib:" + className + " } ";
            queryString.CommandText += "ORDER BY ASC(?class)";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            foreach (SparqlResult result in resultSet)
            {
                string classURI = GetResultValue(result, "class");
                string[] classSplit = classURI.Split('#');
                string classOption = classSplit[1].Trim();

                selectOptions.Add(classOption);
            }

            return selectOptions;
        }

        private static string GetResultValue(SparqlResult result, string variable)
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
    }
}
