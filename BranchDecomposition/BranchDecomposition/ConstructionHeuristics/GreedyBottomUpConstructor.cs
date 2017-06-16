using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BranchDecomposition.DecompositionTrees;
using BranchDecomposition.WidthParameters;

namespace BranchDecomposition.ConstructionHeuristics
{
    class GreedyBottomUpConstructor : ConstructionHeuristic
    {
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
                // Find the pair of nodes whose combination is of minimal width.
                double min = double.PositiveInfinity;
                int first = -1, second = -1;
                for (int i = 0; i < size; i++)
                    for (int j = i + 1; j < size; j++)
                    {
                        double width = widthparameter.GetWidth(graph, nodes[i].Set | nodes[j].Set);
                        if (width < min)
                        {
                            min = width;
                            first = i;
                            second = j;
                        }
                    }
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