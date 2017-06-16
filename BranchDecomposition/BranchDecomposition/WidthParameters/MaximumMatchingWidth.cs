using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.WidthParameters
{
    class MaximumMatchingWidth : WidthParameter
    {
        public MaximumMatchingWidth() : base() { this.Name = "Maximum-Matching-width"; }

        protected override double computeWidth(Graph graph, BitSet left, BitSet right)
        {
            int dummy = graph.Vertices.Count;
            int[] match = new int[graph.Vertices.Count + 1];
            int[] distance = new int[match.Length];
            for (int i = 0; i < match.Length; i++)
                match[i] = dummy;

            int width = 0;
            BitSet partition = left.Count < right.Count ? left : right;
            int[] indices = partition.ToArray();
            int[][] adjacency = new int[graph.Vertices.Count][];
            for (int i = 0; i < indices.Length; i++)
                adjacency[indices[i]] = (graph.Vertices[indices[i]].Neighborhood - partition).ToArray();

            while (this.BFS(indices, adjacency, match, distance, dummy))
                foreach (int index in indices)
                    if (match[index] == dummy && this.DFS(adjacency, match, distance, index, dummy))
                        width++;

            return width;
        }

        protected bool BFS(int[] partition, int[][] adjacency, int[] match, int[] distance, int dummy)
        {
            Queue<int> queue = new Queue<int>();

            for (int i = 0; i < partition.Length; i++)
            {
                int index = partition[i];
                if (match[index] == dummy)
                {
                    distance[index] = 0;
                    queue.Enqueue(index);
                }
                else
                    // dummy + 1 is infinity
                    distance[index] = dummy + 1;
            }
            distance[dummy] = dummy + 1;

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                int dist = distance[index];
                if (dist < dummy + 1 && index != dummy)
                {
                    int[] adjacencylist = adjacency[index];
                    for (int i = 0; i < adjacencylist.Length; i++)
                    {
                        int pairedWithNeighbor = match[adjacencylist[i]];
                        if (distance[pairedWithNeighbor] > dummy)
                        {
                            distance[pairedWithNeighbor] = dist + 1;
                            queue.Enqueue(pairedWithNeighbor);
                        }
                    }
                }
            }

            return distance[dummy] <= dummy;
        }

        protected bool DFS(int[][] adjacency, int[] match, int[] distance, int index, int dummy)
        {
            if (index == dummy)
                return true;

            int dist = distance[index];
            int[] adjacencylist = adjacency[index];
            for (int i = 0; i < adjacencylist.Length; i++)
            {
                int neighbor = adjacencylist[i];
                int pariedWithNeighbor = match[neighbor];
                if (distance[pariedWithNeighbor] == dist + 1 && this.DFS(adjacency, match, distance, pariedWithNeighbor, dummy))
                {
                    match[index] = neighbor;
                    match[neighbor] = index;
                    return true;
                }
            }

            distance[index] = dummy + 1;
            return false;
        }
    }
}
