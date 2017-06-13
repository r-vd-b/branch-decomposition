using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BranchDecomposition.DecompositionTrees;

namespace BranchDecomposition.ImprovementHeuristics
{
    /// <summary>
    /// The IdentityOperation models the identity transformation.
    /// </summary>
    class IdentityOperation : LocalSearchOperation
    {
        public IdentityOperation(DecompositionTree tree) : base(tree) { this.cost = tree.Cost; }

        public override double Cost { get { return this.cost; } }

        public override double Execute()
        {
            return this.cost;
        }

        public override void Revert()
        {
        }
    }
}
