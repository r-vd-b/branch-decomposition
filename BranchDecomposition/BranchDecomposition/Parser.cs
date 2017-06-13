using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace BranchDecomposition
{
    static class Parser
    {
        /// <summary>
        /// Parse a file in DGF-format to a graph.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The resulting graph.</returns>
        public static Graph ParseDGF(string path)
        {
            string line;
            char[] separators = new char[1] { ' ' };

            Graph graph = new Graph();
            int vertexcount = 0;

            Dictionary<string, Vertex> verticesByName = new Dictionary<string, Vertex>();

            using (StreamReader reader = new StreamReader(path))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        continue;

                    char command = line[0];
                    string[] parts = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    switch (command)
                    {
                        case 'p':
                            vertexcount = int.Parse(parts[2]);
                            break;
                        case 'n':
                            string name = parts[1];
                            if (!verticesByName.ContainsKey(name))
                                verticesByName[name] = graph.AddVertex(name, vertexcount);
                            break;
                        case 'e':
                            string v1 = parts[1];
                            if (!verticesByName.ContainsKey(v1))
                                verticesByName[v1] = graph.AddVertex(v1, vertexcount);

                            string v2 = parts[2];
                            if (!verticesByName.ContainsKey(v2))
                                verticesByName[v2] = graph.AddVertex(v2, vertexcount);

                            // Prevent duplicate edges
                            Vertex v = verticesByName[v1];
                            bool duplicate = false;
                            foreach (Vertex w in v.AdjacencyList)
                                if (w.Name == v2)
                                {
                                    duplicate = true;
                                    break;
                                }

                            if (!duplicate)
                                graph.AddEdge(v, verticesByName[v2]);
                            break;
                        default:
                            break;
                    }
                }
            }

            // Add missing vertices
            while (graph.Vertices.Count < vertexcount)
                graph.AddVertex("dummy_" + (vertexcount - graph.Vertices.Count), vertexcount);

            return graph;
        }
    }
}
