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
        private const string RDF_BASE_URI = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

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

        public static PublicationModel QueryIndividualPublication(string individualName)
        {
            PublicationModel publicationModel = new PublicationModel();

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("rdfs", new Uri(RDFS_BASE_URI));
            queryString.Namespaces.AddNamespace("lib", new Uri(ONTOLOGY_BASE_URI));

            queryString.CommandText = "SELECT DISTINCT ?title ?author ?series ?publisher ?imprint ";
            queryString.CommandText += "WHERE { ?class rdfs:label ?title. ";
            queryString.CommandText += "?class lib:hasAuthor ?author. ";
            queryString.CommandText += "?class lib:hasSeries ?series. ";
            queryString.CommandText += "?class lib:hasPublisher ?publisher. ";
            queryString.CommandText += "?class lib:hasImprint ?imprint. ";
            queryString.CommandText += "FILTER(regex(str(?title), '" + individualName.Replace('_', ' ') + "')) }";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            foreach (SparqlResult result in resultSet)
            {
                foreach (KeyValuePair<string, INode> _result in result)
                {
                    switch(_result.Key)
                    {
                        case "title":
                            string[] resultSplit = _result.Value.ToString().Split('@');
                            publicationModel.Title = resultSplit[0].Trim().Replace('_', ' ');
                            break;

                        case "author":
                            string[] authorSplit = _result.Value.ToString().Split(':');
                            publicationModel.Authors.Add(authorSplit[1].Trim().Replace('_', ' '));
                            break;

                        case "publisher":
                            string[] publisherSplit = _result.Value.ToString().Split(':');
                            publicationModel.Publisher = publisherSplit[1].Trim().Replace('_', ' ');
                            break;

                        case "series":
                            string[] seriesSplit = _result.Value.ToString().Split(':');
                            publicationModel.Series = seriesSplit[1].Trim().Replace('_', ' ');
                            break;

                        case "imprint":
                            string[] imprintSplit = _result.Value.ToString().Split(':');
                            publicationModel.Imprint = imprintSplit[1].Trim().Replace('_', ' ');
                            break;
                    }
                }
            }

            publicationModel.Types = QueryIndividualTypes(individualName);
            return publicationModel;
        }

        public static List<string> QueryIndividualTypes(string individualName)
        {
            List<string> individualTypes = new List<string>();

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("rdfs", new Uri(RDFS_BASE_URI));
            queryString.Namespaces.AddNamespace("rdf", new Uri(RDF_BASE_URI));
            queryString.Namespaces.AddNamespace("lib", new Uri(ONTOLOGY_BASE_URI));

            queryString.CommandText = "SELECT DISTINCT ?type ";
            queryString.CommandText += "WHERE { ?class rdfs:label ?title. ";
            queryString.CommandText += "?class rdf:type ?type. ";
            queryString.CommandText += "FILTER(regex(str(?title), '" + individualName.Replace('_', ' ') + "')) }";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            foreach (SparqlResult result in resultSet)
            {
                string resultValue = GetResultValue(result, "type");
                string[] resultSplit = resultValue.Split(':');
                string individualType = resultSplit[1].Trim().Replace('_', ' ');

                individualTypes.Add(individualType);
            }

            return individualTypes;
        }

        public static List<string> QueryFilterOptions(string filterName, QueryFilterType filterType)
        {
            List<string> filterOptions = new List<string>();

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("rdfs", new Uri(RDFS_BASE_URI));
            queryString.Namespaces.AddNamespace("lib", new Uri(ONTOLOGY_BASE_URI));

            switch (filterType)
            {
                //Reference: https://stackoverflow.com/questions/7557564/sparql-query-to-find-all-sub-classes-and-a-super-class-of-a-given-class
                case QueryFilterType.Class:
                    queryString.CommandText = "SELECT DISTINCT ?class ";
                    queryString.CommandText += "WHERE { ?class rdfs:subClassOf* lib:" + filterName + ". ";
                    queryString.CommandText += "FILTER(?class != lib:" + filterName + ") } "; // TODO: Make this exclude the lib:Fiction and lib:Non-Fiction classes as well.
                    queryString.CommandText += "ORDER BY ASC(?class)"; // TODO: Get the class' rdfs:label instead of it's literal name.
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

            if (searchModel.SearchClasses.Count > 0)
            {
                foreach (string searchClass in searchModel.SearchClasses)
                {
                    queryString.CommandText += "?class a lib:" + searchClass + ". ";
                }
            }

            if (searchModel.SearchIndividuals.Count > 0)
            {
                foreach (KeyValuePair<string, string> searchIndividual in searchModel.SearchIndividuals)
                {
                    queryString.CommandText += "?class lib:has" + searchIndividual.Key + " ?" + searchIndividual.Key.ToLower() + ". ";
                }

                foreach (KeyValuePair<string, string> searchIndividual in searchModel.SearchIndividuals)
                {
                    queryString.CommandText += "FILTER(regex(str(?" + searchIndividual.Key.ToLower() + "), '" + searchIndividual.Value.Replace(' ', '_') + "')) ";
                }
            }

            queryString.CommandText += " }";

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
