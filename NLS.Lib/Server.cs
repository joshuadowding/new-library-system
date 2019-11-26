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
        private const string OWL_BASE_URI = "http://www.w3.org/2002/07/owl#";

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

        /// <summary>
        /// Gets the publication that's selected from the search results.
        /// </summary>
        /// <param name="individualName"></param>
        /// <returns>PublicationModel</returns>
        public static PublicationModel QueryIndividualPublication(string individualName)
        {
            PublicationModel publicationModel = new PublicationModel();

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("rdfs", new Uri(RDFS_BASE_URI));
            queryString.Namespaces.AddNamespace("lib", new Uri(ONTOLOGY_BASE_URI));

            queryString.CommandText = "SELECT DISTINCT ?title ?author ?series ?publisher ?imprint ?copyCount ?date ?edition ?isbn ?language ?pageCount ?source ?subject ?subtitle ?summary ?weight ?location ?service ";
            queryString.CommandText += "WHERE { ?class rdfs:label ?title. ";
            queryString.CommandText += "?class lib:hasAuthor ?author. ";
            queryString.CommandText += "OPTIONAL { ?class lib:hasSeries ?series }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:hasPublisher ?publisher }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:hasImprint ?imprint }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:hasLocation ?location }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:hasService ?service }. ";

            queryString.CommandText += "OPTIONAL { ?class lib:publicationCopyTotal ?copyCount }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:publicationDate ?date }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:publicationEdition ?edition }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:publicationISBN ?isbn }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:publicationLanguage ?language }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:publicationPageCount ?pageCount }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:publicationSource ?source }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:publicationSubject ?subject }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:publicationSubtitle ?subtitle }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:publicationSummary ?summary }. ";
            queryString.CommandText += "OPTIONAL { ?class lib:publicationWeight ?weight }. ";

            if (individualName.Contains("'"))
            {
                individualName = individualName.Replace("'", @"\'");
            }

            queryString.CommandText += "FILTER(regex(str(?title), '" + individualName.Replace('_', ' ') + "')) }";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);
            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());

            int index = 0;
            foreach (SparqlResult result in resultSet)
            {
                if (index > 0)
                {
                    foreach (KeyValuePair<string, INode> _result in result)
                    {
                        if (_result.Value != null)
                        {
                            switch (_result.Key)
                            {
                                case "author":
                                    string[] authorSplit = _result.Value.ToString().Split('#');
                                    string author = authorSplit[1].Trim().Replace('_', ' ');
                                    if (!publicationModel.Authors.Contains(author))
                                    {
                                        publicationModel.Authors.Add(author);
                                    }
                                    break;

                                case "service":
                                    string[] serviceSplit = _result.Value.ToString().Split('#');
                                    string service = serviceSplit[1].Trim().Replace('_', ' ');
                                    if (!publicationModel.Services.Contains(service))
                                    {
                                        publicationModel.Services.Add(service);
                                    }
                                    break;

                                case "location":
                                    string[] locationSplit = _result.Value.ToString().Split('#');
                                    string location = locationSplit[1].Trim().Replace('_', ' ');
                                    if (!publicationModel.Locations.Contains(location))
                                    {
                                        publicationModel.Locations.Add(location);
                                    }
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, INode> _result in result)
                    {
                        if (_result.Value != null)
                        {
                            switch (_result.Key)
                            {
                                case "title":
                                    string[] resultSplit = _result.Value.ToString().Split('@');
                                    publicationModel.Title = resultSplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "author":
                                    string[] authorSplit = _result.Value.ToString().Split('#');
                                    publicationModel.Authors.Add(authorSplit[1].Trim().Replace('_', ' '));
                                    break;

                                case "publisher":
                                    string[] publisherSplit = _result.Value.ToString().Split('#');
                                    publicationModel.Publisher = publisherSplit[1].Trim().Replace('_', ' ');
                                    break;

                                case "series":
                                    string[] seriesSplit = _result.Value.ToString().Split('#');
                                    publicationModel.Series = seriesSplit[1].Trim().Replace('_', ' ');
                                    break;

                                case "imprint":
                                    string[] imprintSplit = _result.Value.ToString().Split('#');
                                    publicationModel.Imprint = imprintSplit[1].Trim().Replace('_', ' ');
                                    break;

                                case "copyCount":
                                    string[] copySplit = _result.Value.ToString().Split('#');
                                    string[] _copySplit = copySplit[0].Split('^');
                                    publicationModel.CopyTotal = _copySplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "date":
                                    string[] dateSplit = _result.Value.ToString().Split('#');
                                    string[] _dateSplit = dateSplit[0].Split('^');
                                    publicationModel.Date = _dateSplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "edition":
                                    string[] editionSplit = _result.Value.ToString().Split('#');
                                    string[] _editionSplit = editionSplit[0].Split('^');
                                    publicationModel.Edition = _editionSplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "isbn":
                                    string[] isbnSplit = _result.Value.ToString().Split('#');
                                    string[] _isbnSplit = isbnSplit[0].Split('^');
                                    publicationModel.ISBN = _isbnSplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "language":
                                    string[] languageSplit = _result.Value.ToString().Split('#');
                                    publicationModel.Language = languageSplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "pageCount":
                                    string[] pageSplit = _result.Value.ToString().Split('#');
                                    string[] _pageSplit = pageSplit[0].Split('^');
                                    publicationModel.PageCount = _pageSplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "source":
                                    string[] sourceSplit = _result.Value.ToString().Split('#');
                                    publicationModel.Source = sourceSplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "subject":
                                    string[] subjectSplit = _result.Value.ToString().Split('#');
                                    publicationModel.Subject = subjectSplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "subtitle":
                                    string[] subtitleSplit = _result.Value.ToString().Split('#');
                                    publicationModel.Subtitle = subtitleSplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "summary":
                                    string[] summarySplit = _result.Value.ToString().Split('#');
                                    publicationModel.Summary = summarySplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "weight":
                                    string[] weightSplit = _result.Value.ToString().Split('#');
                                    string[] _weightSplit = weightSplit[0].Split('^');
                                    publicationModel.Weight = weightSplit[0].Trim().Replace('_', ' ');
                                    break;

                                case "service":
                                    string[] serviceSplit = _result.Value.ToString().Split('#');
                                    publicationModel.Services.Add(serviceSplit[1].Trim().Replace('_', ' '));
                                    break;

                                case "location":
                                    string[] locationSplit = _result.Value.ToString().Split('#');
                                    publicationModel.Locations.Add(locationSplit[1].Trim().Replace('_', ' '));
                                    break;
                            }
                        }
                    }
                }

                index++;
            }

            publicationModel.Types = QueryIndividualTypes(individualName);
            return publicationModel;
        }

        /// <summary>
        /// Gets the class types for a given individual.
        /// </summary>
        /// <param name="individualName">Individul Name</param>
        /// <returns>Types (List)</returns>
        public static List<string> QueryIndividualTypes(string individualName)
        {
            List<string> individualTypes = new List<string>();

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("rdfs", new Uri(RDFS_BASE_URI));
            queryString.Namespaces.AddNamespace("rdf", new Uri(RDF_BASE_URI));
            queryString.Namespaces.AddNamespace("lib", new Uri(ONTOLOGY_BASE_URI));
            queryString.Namespaces.AddNamespace("owl", new Uri(OWL_BASE_URI));

            queryString.CommandText = "SELECT DISTINCT ?type ";
            queryString.CommandText += "WHERE { ?class rdfs:label ?title. ";
            queryString.CommandText += "?class rdf:type ?type. ";
            queryString.CommandText += "FILTER(regex(str(?title), '" + individualName.Replace('_', ' ') + "')). ";
            queryString.CommandText += "FILTER(?type != owl:NamedIndividual) }";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            foreach (SparqlResult result in resultSet)
            {
                string resultValue = GetResultValue(result, "type");
                string[] resultSplit = resultValue.Split('#');
                string individualType = resultSplit[1].Trim().Replace('_', ' ');

                individualTypes.Add(individualType);
            }

            return individualTypes;
        }

        /// <summary>
        /// Gets the classes and individuals that make up the filter options.
        /// </summary>
        /// <param name="filterName">Filter Name</param>
        /// <param name="filterType">Filter Type (Class/Individual)</param>
        /// <returns>Filters (List)</returns>
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

        /// <summary>
        /// Gets individuals relative to search filters.
        /// </summary>
        /// <param name="searchModel">SearchModel</param>
        /// <returns>Results (List)</returns>
        public static List<string> QueryIndividualsWithSearchModel(SearchModel searchModel)
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

            queryString.CommandText += " } ORDER BY ASC(?class) ";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            foreach (SparqlResult result in resultSet)
            {
                string classURI = GetResultValue(result, "class");
                string[] classSplit = classURI.Split('#');
                string classOption = classSplit[(classSplit.Length - 1)].Trim().Replace('_', ' ');

                queryResults.Add(classOption);
            }

            return queryResults;
        }

        /// <summary>
        /// Gets all individuals.
        /// </summary>
        /// <returns>Results (List)</returns>
        public static List<string> QueryAllIndividuals()
        {
            List<string> queryResults = new List<string>();

            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("rdfs", new Uri(RDFS_BASE_URI));
            queryString.Namespaces.AddNamespace("lib", new Uri(ONTOLOGY_BASE_URI));

            queryString.CommandText = "SELECT ?title ";
            queryString.CommandText += "WHERE { ";
            queryString.CommandText += "?class rdfs:label ?title. ";
            queryString.CommandText += "{ ?class a lib:Article } UNION ";
            queryString.CommandText += "{ ?class a lib:eBook } UNION ";
            queryString.CommandText += "{ ?class a lib:Hardback } UNION ";
            queryString.CommandText += "{ ?class a lib:Paperback } UNION ";
            queryString.CommandText += "{ ?class a lib:Textbook }. } ";
            queryString.CommandText += "ORDER BY ASC(?title) ";

            SparqlQueryParser queryParser = new SparqlQueryParser();
            SparqlQuery query = queryParser.ParseFromString(queryString);

            SparqlResultSet resultSet = (SparqlResultSet)fusekiConnector.Query(query.ToString());
            foreach (SparqlResult result in resultSet)
            {
                string classURI = GetResultValue(result, "title");
                string[] classSplit = classURI.Split('#');
                string classOption = classSplit[(classSplit.Length - 1)].Trim().Replace('_', ' ');

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
                if (node != null)
                {
                    switch (node.NodeType)
                    {
                        case NodeType.Uri:
                            value = ((IUriNode)node).Uri.ToString();
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
