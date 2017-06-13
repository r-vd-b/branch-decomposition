using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BranchDecomposition.DecompositionTrees;
using BranchDecomposition.WidthParameters;

namespace BranchDecomposition.ConstructionHeuristics
{
    /// <summary>
    /// A class to construct a linear decomposition tree from a graph.
    /// </summary>
    class RandomizeGreedyLinearConstructor : ConstructionHeuristic
    {
        protected Random random { get; set; }

        public RandomizeGreedyLinearConstructor(Random random) : base()
        {
            this.random = random;
        }

        public override DecompositionTree Construct(Graph graph, WidthParameter widthparameter)
        {
            return this.construct(graph, widthparameter, graph.Vertices[this.random.Next(graph.Vertices.Count)]);
        }

        /// <summary>
        /// Construct a linear decomposition tree from the given starting vertex.
        /// </summary>
        protected DecompositionTree construct(Graph graph, WidthParameter widthparameter, Vertex start)
        {
            DecompositionTree result = new DecompositionTree(graph, widthparameter);

            int index = 0;
            BitSet leftbits = new BitSet(graph.Vertices.Count, start.Index);
            BitSet rightbits = ~leftbits;

            BitSet neighborhood = new BitSet(start.Neighborhood);

            this.add(result, start, 1, index++);

            while (!rightbits.IsEmpty)
            {
                Vertex selected = null;
                double best = double.PositiveInfinity;

                var candidates = this.candidates(graph, rightbits, leftbits, neighborhood);
                foreach (var candidate in candidates)
                {
                    BitSet left = new BitSet(leftbits);
                    left[candidate.Index] = true;
                    BitSet right = new BitSet(rightbits);
                    right[candidate.Index] = false;
                    double width = widthparameter.GetWidth(graph, left, right);
                    if (width < best)
                    {
                        best = width;
                        selected = candidate;
                    }
                }
                
                leftbits[selected.Index] = true;
                rightbits[selected.Index] = false;
                neighborhood.Or(selected.Neighborhood);
                neighborhood[selected.Index] = false;

                this.add(result, selected, best, index);
                index += 2;
            }

            return result;
        }

        /// <summary>
        /// Return the set of all viable candidates to expand the current partial decomposition tree with.
        /// </summary>
        /// <param name="right">The bitset of all elements not yet in the partial tree.</param>
        /// <param name="left">The bitset of all elements currently in the partial tree.</param>
        /// <param name="leftneighbors">The set of all neighbors of elements currently in the partial tree.</param>
        protected IEnumerable<Vertex> candidates(Graph graph, BitSet right, BitSet left, BitSet leftneighbors)
        {
            List<Vertex> result = new List<Vertex>();
            BitSet leftneighborhoodset = left | leftneighbors;
            foreach (int index in right)
            {
                Vertex v = graph.Vertices[index];
                if (!(v.Neighborhood & leftneighborhoodset).IsEmpty || v.AdjacencyList.Count == 0)
                    result.Add(v);
            }

            return result;
        }

        /// <summary>
        /// Adds the vertex as leaf to the partial tree.
        /// </summary>
        /// <param name="tree">The partial tree.</param>
        /// <param name="vertex">The vertex that will be added.</param>
        /// <param name="width">The width of the parent of the vertex in the tree.</param>
        /// <param name="index">The index of the vertex in the tree.</param>
        protected void add(DecompositionTree tree, Vertex vertex, double width, int index)
        {
            DecompositionNode child = new DecompositionNode(vertex, index, 1, tree);

            if (tree.Root == null)
                tree.AddNode(child, null, null);
            else
            {
                BitSet rootset = new BitSet(tree.Root.Set);
                rootset[vertex.Index] = true;
                DecompositionNode root = new DecompositionNode(rootset, index + 1, width, tree);
                tree.AddNode(child, tree.Root, root);

                root.SubTreeWidth = Math.Max(root.Width, Math.Max(root.Right.SubTreeWidth, root.Left.SubTreeWidth));
                root.SubTreeSum = root.Width + root.Right.SubTreeSum + root.Left.SubTreeSum;
            }
        }
    }
}
