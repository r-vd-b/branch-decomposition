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
    /// A construction strategy that creates a decomposition tree from the root node by repeatedly selecting a childless internal node to extend and partitioning its set randomly into two non-empty sets.
    /// </summary>
    class RandomTopDownConstructor : ConstructionHeuristic
    {
        protected Random rng;

        public RandomTopDownConstructor(Random rng)
        {
            this.rng = rng;
        }

        public override DecompositionTree Construct(Graph graph, WidthParameter widthparameter)
        {
            int index = 0;
            DecompositionTree tree = new DecompositionTree(graph, widthparameter);
            DecompositionNode root = new DecompositionNode(~(new BitSet(tree.Size)), index, tree);
            tree.Nodes[index++] = root;
            tree.Attach(null, root, Branch.Left);

            List<DecompositionNode> candidates = new List<DecompositionNode>();
            candidates.Add(root);

            while (candidates.Count > 0)
            {
                // Select a random childless internal node.
                DecompositionNode parent = candidates[this.rng.Next(candidates.Count)];
                candidates.Remove(parent);

                // Shuffle its set of vertices.
                int[] indices = parent.Set.ToArray();
                indices.Shuffle(this.rng);

                // Split the set randomly into two non-empty sets.
                int split = 1 + this.rng.Next(indices.Length - 2);

                // Create its children.
                DecompositionNode left = null;
                if (split == 1)
                    left = new DecompositionNode(graph.Vertices[indices[0]], index, tree);
                else
                {
                    BitSet leftset = new BitSet(tree.Size);
                    for (int i = 0; i < split; i++)
                        leftset[indices[i]] = true;
                    left = new DecompositionNode(leftset, index, tree);
                    candidates.Add(left);
                }
                tree.Nodes[index++] = left;
                tree.Attach(parent, left, Branch.Left);

                DecompositionNode right = null;
                if (split == indices.Length - 1)
                    right = new DecompositionNode(graph.Vertices[indices[indices.Length - 1]], index, tree);
                else
                {
                    BitSet rightset = new BitSet(tree.Size);
                    for (int i = split; i < indices.Length; i++)
                        rightset[indices[i]] = true;
                    right = new DecompositionNode(rightset, index, tree);
                    candidates.Add(right);
                }
                tree.Nodes[index++] = right;
                tree.Attach(parent, right, Branch.Right);
            }

            tree.ComputeWidth();
            return tree;
        }
    }
}
