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
    /// A class to construct a linear decomposition tree by starting with a random leaf and repeatedly extending the current root with a leaf that is not yet in the tree, such that the width of the new root is minimal over all leaves not in the tree.
    /// </summary>
    class GreedyLinearConstructor : ConstructionHeuristic
    {
        protected Random random { get; set; }

        public GreedyLinearConstructor(Random random) : base()
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

            this.add(result, start, index++);

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

                this.add(result, selected, index);
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
                if (!(v.Neighborhood & leftneighborhoodset).IsEmpty)
                    result.Add(v);
            }

            if (result.Count == 0)
                foreach (int index in right)
                    result.Add(graph.Vertices[index]);

            return result;
        }

        /// <summary>
        /// Adds the vertex as leaf to the partial tree.
        /// </summary>
        /// <param name="tree">The partial tree.</param>
        /// <param name="vertex">The vertex that will be added.</param>
        /// <param name="index">The index of the vertex in the tree.</param>
        protected void add(DecompositionTree tree, Vertex vertex, int index)
        {
            // Create the child node.
            DecompositionNode child = new DecompositionNode(vertex, index, tree);
            tree.Nodes[index] = child;

            if (tree.Root == null)
                tree.Attach(null, child, Branch.Left);
            else
            {
                BitSet set = new BitSet(tree.Root.Set);
                set[vertex.Index] = true;
                DecompositionNode parent = new DecompositionNode(set, index + 1, tree);
                tree.Nodes[index + 1] = parent;

                tree.Attach(parent, tree.Root, Branch.Right);
                tree.Attach(parent, child, Branch.Left);
                tree.Attach(null, parent, Branch.Left);
                parent.UpdateWidthProperties(false);
            }
        }
    }
}
