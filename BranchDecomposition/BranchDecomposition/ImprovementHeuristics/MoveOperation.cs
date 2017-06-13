using BranchDecomposition.DecompositionTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.ImprovementHeuristics
{
    class MoveOperation : LocalSearchOperation
    {
        public DecompositionNode SelectedNode { get; }
        public DecompositionNode SelectedSibling { get; }

        protected DecompositionNode OriginalSibling { get; }

        public MoveOperation(DecompositionTree tree, DecompositionNode node, DecompositionNode sibling) : base(tree)
        {
            this.SelectedNode = node;
            this.SelectedSibling = sibling;
            this.OriginalSibling = node.Sibling;
        }

        public override double Execute()
        {
            this.Tree.ReplaceByChild(this.SelectedNode.Parent, this.OriginalSibling);
            this.Tree.InsertAsSibling(this.SelectedNode.Parent, this.SelectedSibling);
            return base.Execute();
        }

        public override void Revert()
        {
            this.Tree.ReplaceByChild(this.SelectedNode.Parent, this.SelectedSibling);
            this.Tree.InsertAsSibling(this.SelectedNode.Parent, this.OriginalSibling);
            this.Tree.ComputeWidth();
        }
    }
}
