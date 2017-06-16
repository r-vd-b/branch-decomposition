using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BranchDecomposition.DecompositionTrees;
using BranchDecomposition.WidthParameters;

namespace BranchDecomposition.ConstructionHeuristics
{
    class RandomBottomUpConstructor : ConstructionHeuristic
    {
        protected Random rng;

        public RandomBottomUpConstructor(Random rng)
        {
            this.rng = rng;
        }

        public override DecompositionTree Construct(Graph graph, WidthParameter widthparameter)
        {
            DecompositionTree tree = new DecompositionTree(graph, widthparameter);

            // Create the leaves.
            DecompositionNode[] nodes = new DecompositionNode[graph.Vertices.Count];
            for (int i = 0; i < nodes.Length; i++)
                tree.Nodes[i] = nodes[i] = new DecompositionNode(new BitSet(nodes.Length, graph.Vertices[i].Index), i, tree);

            int size = nodes.Length;
            while (size > 1)
            {
                int first = this.rng.Next(size);
                int second = this.rng.Next(size - 1);
                if (second <= first)
                    second++;

                // Create the parent and connect it to its children.
                DecompositionNode node = new DecompositionNode(nodes[first].Set | nodes[second].Set, nodes.Length * 2 - size, tree);
                tree.Attach(node, nodes[first], Branch.Left);
                tree.Attach(node, nodes[second], Branch.Right);
                tree.Nodes[node.Index] = node;

                // Update the active set of nodes.
                nodes[first] = node;
                nodes[second] = nodes[size - 1];

                size--;
            }

            tree.Attach(null, nodes[0], Branch.Left);
            tree.ComputeWidth();
            return tree;
        }
    }
}
