using BranchDecomposition.DecompositionTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.ImprovementHeuristics
{
    /// <summary>
    /// The LocalSearchOperation represents a transformation from a decomposition tree to a neighbor.
    /// </summary>
    abstract class LocalSearchOperation
    {
        /// <summary>
        /// The decomposition tree.
        /// </summary>
        public DecompositionTree Tree { get; }
        public Graph Graph { get { return this.Tree.Graph; } }
        /// <summary>
        /// The cost of the tree after the tranformation.
        /// </summary>
        protected double cost = -1;
        public virtual double Cost
        {
            get
            {
                if (this.cost < 0)
                    this.cost = this.computeCost();
                return this.cost;
            }
        }
        
        public LocalSearchOperation(DecompositionTree tree)
        {
            this.Tree = tree;
        }

        /// <summary>
        /// Execute the transformation.
        /// </summary>
        /// <returns>The cost of the decomposition tree after the transformation.</returns>
        public virtual double Execute()
        {
            if (this.cost < 0)
                this.cost = this.Tree.Cost;
            return this.Tree.Cost;
        }

        /// <summary>
        /// Reverts a transformed tree back to its original state.
        /// </summary>
        public abstract void Revert();

        protected virtual double computeCost()
        {
            double cost = this.Execute();
            this.Revert();
            return cost;
        }
    }
}
