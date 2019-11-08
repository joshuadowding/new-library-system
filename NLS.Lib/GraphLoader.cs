using System;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Builder;

namespace NLS.Lib
{
    public class GraphLoader
    {
        public void Load()
        {
            try
            {
                IGraph graph = new Graph();
                IRdfReader reader = new RdfXmlParser();

                using (StreamReader stream = new StreamReader("Resources/literature-example-1.rdf"))
                {
                    reader.Load(graph, stream);
                }

                PrintUriNodes(graph);
                Console.WriteLine("");
                TestSimpleSearch(graph, "Harry Potter: The Chamber of Secrets");
                Console.WriteLine("");
                TestTripleSearch(graph);

                graph.Dispose();
            }
            catch (RdfParseException parseException)
            {
                Console.Write(parseException.Message);
            }
            catch (RdfException exception)
            {
                Console.Write(exception.Message);
            }
            catch (IOException ioException)
            {
                Console.Write(ioException.Message);
            }
        }

        public void PrintUriNodes(IGraph graph)
        {
            if (graph != null)
            {
                foreach (IUriNode node in graph.Nodes.UriNodes())
                {
                    Console.WriteLine(node.Uri.ToString());
                }
            }
        }

        public void TestSimpleSearch(IGraph _graph, string _object)
        {
            string x = "x";
            var queryBuilder = QueryBuilder.Select(new string[] { x }).Where(
                (triplePatturnBuilder) =>
                {
                    triplePatturnBuilder
                        .Subject(x)
                        .PredicateUri(new Uri(_graph.BaseUri.ToString()))
                        .Object(_object);
                }); // e.g. Fetches all individuals whose name is 'Harry Potter: The Chamber of Secrets'

            Console.WriteLine(queryBuilder.BuildQuery().ToString());
        }

        public void TestTripleSearch(IGraph graph)
        {
            if(graph != null)
            {
                foreach(Triple triple in graph.Triples)
                {
                    Console.WriteLine(triple.ToString());
                }
            }
        }
    }
}
