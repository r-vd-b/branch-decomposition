using BranchDecomposition.DecompositionTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.ImprovementHeuristics
{
    /// <summary>
    /// The LocalSearchOperator class represents a local search operator that can be used to generate neighbors of a solution.
    /// </summary>
    abstract class LocalSearchOperator
    {
        /// <summary>
        /// Returns all neighbors in the neighborhood of the tree.
        /// </summary>
        /// <param name="tree">The decomposition tree at the center of the neighborhood.</param>
        /// <param name="rng">A random number generator.</param>
        /// <returns>The neighborhood of the tree.</returns>
        public abstract IEnumerable<LocalSearchOperation> Operations(DecompositionTree tree, Random rng);

        /// <summary>
        /// Returns a random neighbor in the neighborhood.
        /// </summary>
        /// <param name="tree">The decomposition tree at the center of the neighborhood.</param>
        /// <param name="rng">A random number generator.</param>
        public abstract LocalSearchOperation GetRandomOperation(DecompositionTree tree, Random rng);

        /// <summary>
        /// Returns the index of a random node in the tree that is not the root OR the child of the root with a leaf as sibling.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="rng">A random number generator.</param>
        /// <returns>The index of a node in the tree.</returns>
        protected int getRandomNonRootIndex(DecompositionTree tree, Random rng)
        {
            // No operations available.
            if (tree.Nodes.Length <= 3)
                throw new IndexOutOfRangeException("No valid index can be found in the tree.");

            int index = -1;
            // We select neither the root, nor a child of the root if its sibling is a leaf.
            if (tree.Root.Left.IsLeaf)
                index = 1 + rng.Next(tree.Nodes.Length - 2);
            else if (tree.Root.Right.IsLeaf)
            {
                index = 1 + rng.Next(tree.Nodes.Length - 2);
                if (index == tree.Root.SubTreeSize - 2)
                    index++;
            }
            else
                index = 1 + rng.Next(tree.Nodes.Length - 1);

            return index;
        }
    }
}
