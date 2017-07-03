using System;
using System.Collections.Generic;
using System.IO;

namespace BranchDecomposition
{
    /// <summary>
    /// A simple graph class.
    /// </summary>
    class Graph
    {
        public List<Vertex> Vertices { get; } = new List<Vertex>();
        public bool RequiresIndexing { get; private set; }

        protected Dictionary<string, Vertex> vertexMap;

        public Graph()
        {
            this.Vertices = new List<Vertex>();
            this.vertexMap = new Dictionary<string, Vertex>();
        }

        public Vertex AddVertex(string name)
        {
            this.RequiresIndexing = true;
            Vertex vertex = new Vertex(this, name, this.Vertices.Count);
            this.Vertices.Add(vertex);
            this.vertexMap[name] = vertex;
            return vertex;
        }

        public void AddEdge(Vertex v1, Vertex v2)
        {
            this.RequiresIndexing = true;
            v1.AdjacencyList.Add(v2);
            v2.AdjacencyList.Add(v1);
        }

        public void RemoveVertex(Vertex vertex)
        {
            this.RequiresIndexing = true;
            foreach (Vertex neighbor in vertex.AdjacencyList)
            {
                neighbor.AdjacencyList.Remove(vertex);
                neighbor.Neighborhood[vertex.Index] = false;
            }
            this.Vertices.Remove(vertex);
            this.vertexMap.Remove(vertex.Name);
        }

        public void UpdateIndices()
        {
            for (int i = 0; i < this.Vertices.Count; i++)
                this.Vertices[i].UpdateIndex(i);

            for (int i = 0; i < this.Vertices.Count; i++)
                this.Vertices[i].UpdateNeighborhood();

            this.RequiresIndexing = false;
        }

        public Vertex GetVertex(string name)
        {
            return this.vertexMap[name];
        }

        public override string ToString()
        {
            return this.Vertices.Count.ToString();
        }
    }

    /// <summary>
    /// A simple vertex class.
    /// </summary>
    class Vertex
    {
        // The index of the vertex in the list of its graph.
        public int Index { get; private set; }
        // The name of the vertex.
        public string Name { get; }
        // The adjacency list of the vertex.
        public List<Vertex> AdjacencyList { get; }
        // A bitset of the adjacency list of the vertex.
        public BitSet Neighborhood { get; private set; }
        // The graph to which this vertex belongs.
        public Graph Graph { get; }

        public Vertex(Graph graph, string name, int index)
        {
            this.Graph = graph;
            this.Name = name;
            this.Index = index;
            this.AdjacencyList = new List<Vertex>();
        }

        public void UpdateIndex(int index)
        {
            this.Index = index;
        }

        public void UpdateNeighborhood()
        {
            this.Neighborhood = new BitSet(this.Graph.Vertices.Count);
            foreach (Vertex neighbor in this.AdjacencyList)
                this.Neighborhood[neighbor.Index] = true;
        }

        public override string ToString()
        {
            return "Vertex " + this.Name + "; index=" + this.Index + "; degree = " + this.AdjacencyList.Count;
        }
    }
}
