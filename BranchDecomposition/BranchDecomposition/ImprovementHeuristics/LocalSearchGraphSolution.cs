using BranchDecomposition.DecompositionTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.ImprovementHeuristics
{
    /// <summary>
    /// The LocalSearchGraphSolution class contains information of a solution in the local search for a single graph.
    /// </summary>
    class LocalSearchGraphSolution
    {
        public Graph Graph { get; }
        public double BestCost { get; protected set; }
        public double BestWidth { get; protected set; }
        public DecompositionTree CurrentSolution { get; protected set; }
        public List<LocalSearchOperation> PerformedOperations { get; }

        public LocalSearchGraphSolution(Graph graph, DecompositionTree tree)
        {
            this.Graph = graph;
            this.CurrentSolution = tree;
            this.BestCost = tree.Cost;
            this.BestWidth = tree.Width;
            this.PerformedOperations = new List<LocalSearchOperation>();
            this.PerformedOperations.Add(new IdentityOperation(tree));
        }

        /// <summary>
        /// Perform an operation on the current solution.
        /// </summary>
        /// <param name="operation">The local search operation to be performed.</param>
        /// <returns>If the operation improved the best known upper bound on the cost.</returns>
        public virtual bool PerformOperation(LocalSearchOperation operation)
        {
            this.PerformedOperations.Add(operation);
            operation.Execute();
            if (this.CurrentSolution.Cost < this.BestCost)
            {
                this.BestCost = this.CurrentSolution.Cost;
                this.BestWidth = this.CurrentSolution.Width;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Revert the current solution back to the best solution seen so far.
        /// </summary>
        public DecompositionTree Revert()
        {
            int index = this.PerformedOperations.MinIndex(op => op.Cost);
            for (int i = this.PerformedOperations.Count - 1; i > index; i--)
                this.PerformedOperations[i].Revert();
            this.PerformedOperations.RemoveRange(index + 1, this.PerformedOperations.Count - index - 1);
            return this.CurrentSolution;
        }
    }
}
