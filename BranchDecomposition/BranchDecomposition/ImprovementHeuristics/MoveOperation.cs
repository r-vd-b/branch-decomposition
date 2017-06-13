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


        protected override double computeCost()
        {
            double maximum = this.SelectedNode.SubTreeWidth, sum = this.SelectedNode.SubTreeSum;
            DecompositionNode common = null;

            if (!this.SelectedNode.Parent.Set.Intersects(this.SelectedSibling.Set))
            {
                maximum = Math.Max(maximum, Math.Max(this.SelectedSibling.SubTreeWidth, this.OriginalSibling.SubTreeWidth));
                sum += this.SelectedSibling.SubTreeSum + this.OriginalSibling.SubTreeSum;

                // parent
                double width = this.Tree.WidthParameter.GetWidth(this.Tree.Graph, this.SelectedNode.Set | this.SelectedSibling.Set);
                sum += width;
                if (width > maximum)
                    maximum = width;

                // the common ancestor
                common = this.findCommonAncestor(this.SelectedNode.Parent, this.SelectedSibling);
                sum += common.Width;
                if (maximum < common.Width)
                    maximum = common.Width;

                // ancestors of parent
                this.computeCost(this.SelectedNode.Parent, common, null, this.SelectedNode.Set, ref maximum, ref sum);
                // ancestors of the new sibling
                this.computeCost(this.SelectedSibling, common, this.SelectedNode.Set, null, ref maximum, ref sum);
                // common ancestors
                this.computeCost(common, null, null, null, ref maximum, ref sum);
            }
            else if (this.SelectedNode.Set.IsSubsetOf(this.SelectedSibling.Set))
            {
                sum += this.OriginalSibling.SubTreeSum;
                if (maximum < this.OriginalSibling.SubTreeWidth)
                    maximum = this.OriginalSibling.SubTreeWidth;

                // parent
                sum += this.SelectedSibling.Width;
                if (maximum < this.SelectedSibling.Width)
                    maximum = this.SelectedSibling.Width;


                // ancestors of parent
                this.computeCost(this.SelectedNode.Parent, this.SelectedSibling.Parent, null, this.SelectedNode.Set, ref maximum, ref sum);
                // common ancestors
                this.computeCost(this.SelectedSibling, null, null, null, ref maximum, ref sum);
            }
            else // the new sibling is a descendant of the original sibling
            {
                sum += this.SelectedSibling.SubTreeSum;
                if (maximum < this.SelectedSibling.SubTreeWidth)
                    maximum = this.SelectedSibling.SubTreeWidth;

                // parent
                double width = this.Tree.WidthParameter.GetWidth(this.Tree.Graph, this.SelectedNode.Set | this.SelectedSibling.Set);
                sum += width;
                if (maximum < width)
                    maximum = width;

                // ancestors of the new sibling
                this.computeCost(this.SelectedSibling, this.SelectedNode.Parent, this.SelectedNode.Set, null, ref maximum, ref sum);
                // common ancestors
                this.computeCost(this.SelectedNode.Parent, null, null, null, ref maximum, ref sum);
            }

            /*double topwidth = this.Tree.Root.Right.Width;
            if (this.SelectedNode.Parent.IsRoot)
            {
                topwidth = this.OriginalSibling.Left.Set.Intersects(this.SelectedSibling.Set) ? this.OriginalSibling.Right.Width : this.OriginalSibling.Left.Width;
            }
            else if (this.SelectedNode.Parent.Parent.IsRoot)
            {
                if (this.SelectedSibling.Set.IsSubsetOf(this.OriginalSibling.Set))
                {
                    topwidth = this.SelectedNode.Parent.Width;
                }
                else
                {
                    topwidth = this.OriginalSibling.Width;
                }
            }
            else if (comm)*/
            double topwidth = 0;

            double cost = DecompositionTree.ComputeCost(maximum, this.Tree.Size, sum, topwidth);
            double actualcost = this.Execute();

            if (Math.Abs(cost - actualcost) > 0.0001)
                throw new InvalidOperationException();


            this.Revert();

            return cost;
        }

        private void computeCost(DecompositionNode child, DecompositionNode end, BitSet add, BitSet remove, ref double maximum, ref double sum)
        {
            for (DecompositionNode ancestor = child?.Parent; ancestor != end && ancestor != null; child = ancestor, ancestor = ancestor.Parent)
            {
                double width = ancestor.Width;
                if (add != null || remove != null)
                {
                    BitSet set = ancestor.Set;
                    if (add != null)
                        set |= add;
                    if (remove != null)
                        set -= remove;
                    width = this.Tree.WidthParameter.GetWidth(this.Tree.Graph, set);
                }
                maximum = Math.Max(maximum, Math.Max(width, child.Sibling.SubTreeWidth));
                sum += width + child.Sibling.SubTreeSum;
            }
        }

        private DecompositionNode findCommonAncestor(DecompositionNode n1, DecompositionNode n2)
        {
            for (DecompositionNode ancestor = n1; ancestor != null; ancestor = ancestor.Parent)
                if (ancestor.Set.IsSupersetOf(n2.Set))
                    return ancestor;
            return null;
        }
    }
}
