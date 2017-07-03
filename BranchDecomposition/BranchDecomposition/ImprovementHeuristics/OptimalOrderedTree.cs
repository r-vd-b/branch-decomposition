using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BranchDecomposition.DecompositionTrees;
using BranchDecomposition.WidthParameters;

namespace BranchDecomposition.ImprovementHeuristics
{
    class OptimalOrderedTree
    {
        public DecompositionTree Construct(DecompositionTree tree)
        {
            DecompositionNode[] leaves = tree.Root.SubTree(TreeTraversal.ParentFirst).Where(node => node.IsLeaf).ToArray();
            double[,] width = new double[tree.VertexCount, tree.VertexCount + 1];
            BitSet[,] sets = new BitSet[tree.VertexCount, tree.VertexCount + 1];
            for (int index = 0; index < tree.VertexCount; index++)
            {
                sets[index, 1] = leaves[index].Set;
                width[index, 1] = leaves[index].Width;
                for (int length = 2; length <= tree.VertexCount - index; length++)
                    sets[index, length] = sets[index, length - 1] | leaves[index + length - 1].Set;
            }

            for (int length = 2; length <= tree.VertexCount; length++)
            {
                for (int index = 0; index <= tree.VertexCount - length; index++)
                {
                    double childWidth = double.PositiveInfinity;
                    for (int k = index + 1; k < index + length; k++)
                        childWidth = Math.Min(childWidth, Math.Max(width[index, k - index], width[k, length - (k - index)]));

                    if (childWidth > tree.Width)
                        width[index, length] = double.PositiveInfinity;
                    else
                        width[index, length] = Math.Max(childWidth, tree.WidthParameter.GetWidth(tree.Graph, sets[index, length]));
                }
            }

            DecompositionTree result = new DecompositionTree(tree.Graph, tree.WidthParameter);
            int treeindex = tree.Nodes.Length - 1;
            this.backtrack(result, leaves, width, sets, 0, tree.VertexCount, ref treeindex);
            return result;
        }

        protected DecompositionNode backtrack(DecompositionTree tree, DecompositionNode[] leaves, double[,] width, BitSet[,] sets, int index, int size, ref int treeindex)
        {
            if (size == 1)
            {
                DecompositionNode node = new DecompositionNode(leaves[index], tree);
                node.Index = index;
                tree.Nodes[index] = node;
                return node;
            }

            DecompositionNode parent = new DecompositionNode(sets[index, size], treeindex--, tree);
            tree.Nodes[parent.Index] = parent;

            int split = -1;
            double splitwidth = double.PositiveInfinity;
            for (int i = index + 1; i < index + size; i++)
            {
                double max = Math.Max(width[index, i - index], width[i, size - (i - index)]);
                if (max <= splitwidth)
                {
                    splitwidth = max;
                    split = i;
                }
            }

            DecompositionNode left = this.backtrack(tree, leaves, width, sets, index, split - index, ref treeindex);
            DecompositionNode right = this.backtrack(tree, leaves, width, sets, split, size - (split - index), ref treeindex);

            tree.Attach(parent, left, Branch.Left);
            tree.Attach(parent, right, Branch.Right);
            tree.Attach(null, parent, Branch.Left);

            parent.UpdateWidthProperties(false);

            return parent;
        }
    }
}
