using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BranchDecomposition.DecompositionTrees;
using BranchDecomposition.WidthParameters;

namespace BranchDecomposition
{
    static class Parser
    {
        /// <summary>
        /// Parse a file in DGF-format to a graph.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The resulting graph.</returns>
        public static Graph ParseGraphFromDGF(string path)
        {
            string line;
            char[] separators = new char[1] { ' ' };

            Graph graph = new Graph();

            Dictionary<string, Vertex> verticesByName = new Dictionary<string, Vertex>();
            int graphsize = -1;

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
                            graphsize = int.Parse(parts[2]);
                            break;
                        case 'n':
                            string name = parts[1];
                            if (!verticesByName.ContainsKey(name))
                                verticesByName[name] = graph.AddVertex(name);
                            break;
                        case 'e':
                            string v1 = parts[1];
                            if (!verticesByName.ContainsKey(v1))
                                verticesByName[v1] = graph.AddVertex(v1);

                            string v2 = parts[2];
                            if (!verticesByName.ContainsKey(v2))
                                verticesByName[v2] = graph.AddVertex(v2);

                            // Prevent duplicate edges.
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

            // Insert zero-degree vertices.
            int counter = 0;
            while (graph.Vertices.Count < graphsize)
            {
                string name;
                do
                    name = "dummy_" + counter++;
                while (verticesByName.ContainsKey(name));
                verticesByName[name] = graph.AddVertex(name);
            }

            // Update all the indices.
            if (graph.RequiresIndexing)
                graph.UpdateIndices();

            return graph;
        }

        /// <summary>
        /// Converts a graph in DGF-format to DOT-format for easy visualisation. Note that degree-zero vertices are omitted.
        /// </summary>
        /// <param name="input">The path to the input graph in DGF-format.</param>
        /// <param name="output">The path to the output file in DOT-format.</param>
        public static void ConvertDGFToDOT(string input, string output)
        {
            char[] separator = new char[] { ' ' };

            using (StreamReader reader = new StreamReader(input))
            {
                using (StreamWriter writer = new StreamWriter(output))
                {
                    writer.WriteLine($"graph {Path.GetFileNameWithoutExtension(input)} {{");

                    string inputline = string.Empty;
                    while ((inputline = reader.ReadLine()) != null)
                    {
                        if (inputline.Length > 0 && inputline[0] == 'e')
                        {
                            string[] parts = inputline.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            writer.WriteLine($"\t{parts[1]} -- {parts[2]}");
                        }
                    }
                    writer.WriteLine("}");
                }
            }
        }
    }
}
