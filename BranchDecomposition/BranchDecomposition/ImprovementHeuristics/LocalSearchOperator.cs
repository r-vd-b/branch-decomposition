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
    }
}
