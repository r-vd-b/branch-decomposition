using BranchDecomposition.DecompositionTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.ImprovementHeuristics
{
    class SwapOperation : LocalSearchOperation
    {
        public DecompositionNode SelectedNodeLeft { get; }
        public DecompositionNode SelectedNodeRight { get; }

        public SwapOperation(DecompositionTree tree, DecompositionNode left, DecompositionNode right) : base(tree)
        {
            this.SelectedNodeLeft = left;
            this.SelectedNodeRight = right;
        }

        public override double Execute()
        {
            this.updateTree(this.SelectedNodeLeft, this.SelectedNodeRight);
            return base.Execute();
        }

        public override void Revert()
        {
            this.updateTree(this.SelectedNodeLeft, this.SelectedNodeRight);
        }

        protected override double computeCost()
        {
            double maximum = Math.Max(this.SelectedNodeLeft.SubTreeWidth, this.SelectedNodeRight.SubTreeWidth), 
                sum = this.SelectedNodeLeft.SubTreeSum + this.SelectedNodeRight.SubTreeSum;

            // Find the first common ancestor of a and b.
            DecompositionNode common = this.findCommonAncestor(this.SelectedNodeLeft, this.SelectedNodeRight);
            // Compute the width of the ancestors of a up to the common ancestor.
            this.computeWidthAncestors(this.SelectedNodeLeft, common, this.SelectedNodeRight.Set, this.SelectedNodeLeft.Set, ref maximum, ref sum);
            // Compute the width of the ancestors of b up to the common ancestor.
            this.computeWidthAncestors(this.SelectedNodeRight, common, this.SelectedNodeLeft.Set, this.SelectedNodeRight.Set, ref maximum, ref sum);

            // Compute the width of the first common ancestor.
            if (maximum < common.Width)
                maximum = common.Width;
            sum += common.Width;
            
            // Compute the width of the remaining common ancestors.
            this.computeWidthAncestors(common, null, null, null, ref maximum, ref sum);

            double topwidth = this.Tree.Root.Right.Width;
            if (common.IsRoot)
            {
                BitSet set = this.Tree.Root.Right.Set - this.SelectedNodeRight.Set | this.SelectedNodeLeft.Set;
                topwidth = this.Tree.WidthParameter.GetWidth(this.Tree.Graph, set);
            }

            return DecompositionTree.ComputeCost(maximum, this.Tree.VertexCount, sum, topwidth);
        }

        private void updateTree(DecompositionNode a, DecompositionNode b)
        {
            DecompositionNode oldParentA = a.Parent;
            Branch            oldBranchA = a.Branch;
            DecompositionNode oldParentB = b.Parent;
            Branch            oldBranchB = b.Branch;

            // Find the first common ancestor of a and b.
            DecompositionNode common = this.findCommonAncestor(a, b);

            // Swap position of the subtrees.
            this.Tree.Attach(oldParentA, b, oldBranchA);
            this.Tree.Attach(oldParentB, a, oldBranchB);

            // Update the ancestors of a up to the common ancestor.
            this.updateAncestors(a, common, a.Set, b.Set);
            // Update the ancestors of b up to the common ancestor.
            this.updateAncestors(b, common, b.Set, a.Set);
            // Update the first common ancestor.
            common.UpdateWidthProperties(false);
            // Update the remaining common ancestors.
            this.updateAncestors(common, null, null, null);
        }
    }
}
