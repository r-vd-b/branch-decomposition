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

        public Graph()
        {
            this.Vertices = new List<Vertex>();
        }

        public Vertex AddVertex(string name, int bitsetsize)
        {
            Vertex v = new Vertex(name, this.Vertices.Count, bitsetsize);
            this.Vertices.Add(v);
            return v;
        }

        public void AddVertex(Vertex v)
        {
            v.Index = this.Vertices.Count;
            this.Vertices.Add(v);
        }

        public void AddEdge(Vertex v1, Vertex v2)
        {
            v1.AdjacencyList.Add(v2);
            v1.Neighborhood[v2.Index] = true;
            v2.AdjacencyList.Add(v1);
            v2.Neighborhood[v1.Index] = true;
        }

        public void RemoveVertex(Vertex v)
        {
            foreach (Vertex w in v.AdjacencyList)
            {
                w.AdjacencyList.Remove(v);
                w.Neighborhood[v.Index] = false;
            }
            this.Vertices.Remove(v);
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
        public int Index { get; set; }
        // The name of the vertex.
        public string Name;
        // The adjacency list of the vertex.
        public List<Vertex> AdjacencyList;
        // A bitset of the adjacency list of the vertex.
        public BitSet Neighborhood;

        public Vertex(string name, int index, int size)
        {
            this.Name = name;
            this.Index = index;
            this.AdjacencyList = new List<Vertex>();
            this.Neighborhood = new BitSet(size);
        }

        public override string ToString()
        {
            return "Vertex " + this.Name + "; index=" + this.Index + "; degree = " + this.AdjacencyList.Count;
        }
    }
}
