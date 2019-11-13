using NLS.Lib.Models;
using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;

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

        public static List<string> QueryFilterOptions(string filterName, QueryFilterType filterType)
        {
            List<string> filterOptions = new List<string>();

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("rdfs", new Uri(RDFS_BASE_URI));
            queryString.Namespaces.AddNamespace("lib", new Uri(ONTOLOGY_BASE_URI));

            switch(filterType)
            {
                case QueryFilterType.Class:
                    queryString.CommandText = "SELECT DISTINCT ?class ";
                    queryString.CommandText += "WHERE { ?class rdfs:subClassOf lib:" + filterName + " } ";
                    queryString.CommandText += "ORDER BY ASC(?class)";
                    break;

                case QueryFilterType.Individual:
                    queryString.CommandText = "SELECT DISTINCT ?class ";
                    queryString.CommandText += "WHERE { ?class a lib:" + filterName + " } ";
                    queryString.CommandText += "ORDER BY ASC(?class)";
                    break;
            }

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            foreach (SparqlResult result in resultSet)
            {
                string resultValue = GetResultValue(result, "class");
                string[] resultSplit = resultValue.Split('#');
                string filterOption = resultSplit[1].Trim().Replace('_', ' ');

                filterOptions.Add(filterOption);
            }

            return filterOptions;
        }

        public static List<string> QueryWithSearchModel(SearchModel searchModel)
        {
            List<string> queryResults = new List<string>();

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("rdfs", new Uri(RDFS_BASE_URI));
            queryString.Namespaces.AddNamespace("lib", new Uri(ONTOLOGY_BASE_URI));

            queryString.CommandText = "SELECT DISTINCT ?class ";
            queryString.CommandText += "WHERE { ";

            if (searchModel.Genre != null)
            {
                queryString.CommandText += "?class a lib:" + searchModel.Genre + ". ";
            }

            if (searchModel.Form != null)
            {
                queryString.CommandText += "?class a lib:" + searchModel.Form + " ";
            }

            if (searchModel.Author != null)
            {
                queryString.CommandText += "{?class lib:hasAuthor ?author}";
            }

            if (searchModel.Author != null)
            {
                queryString.CommandText += "FILTER(regex(str(?author), '" + searchModel.Author.Replace(' ', '_') + "')) ";
            }

            queryString.CommandText += " } ";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            foreach (SparqlResult result in resultSet)
            {
                string classURI = GetResultValue(result, "class");
                string[] classSplit = classURI.Split('#');
                string classOption = classSplit[1].Trim().Replace('_', ' ');

                queryResults.Add(classOption);
            }

            return queryResults;
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

    public enum QueryFilterType
    {
        Class,
        Individual
    }
}
