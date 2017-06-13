using BranchDecomposition.DecompositionTrees;
using BranchDecomposition.WidthParameters;

namespace BranchDecomposition.ConstructionHeuristics
{
    abstract class ConstructionHeuristic
    {
        /// <summary>
        /// Construct a decomposition tree from the graph.
        /// </summary>
        /// <param name="graph">The graph that will be decomposed.</param>
        /// <param name="widthparameter">The width-parameter used to create the decomposition.</param>
        /// <returns>A binary decomposition tree of the graph.</returns>
        public abstract DecompositionTree Construct(Graph graph, WidthParameter widthparameter);
    }
}
