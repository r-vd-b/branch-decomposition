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
            this.updateTree(this.OriginalSibling, this.SelectedSibling);
            return base.Execute();
        }

        public override void Revert()
        {
            this.updateTree(this.SelectedSibling, this.OriginalSibling);
        }

        protected override double computeCost()
        {
            double maximum = this.SelectedNode.SubTreeWidth, 
                sum = this.SelectedNode.SubTreeSum,
                topwidth = this.Tree.Root.Right.Width;
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

                // update the width of the child of the root
                if (common.IsRoot)
                {
                    BitSet set = this.Tree.Root.Right.Set;
                    if (set.IsSupersetOf(this.SelectedNode.Set))
                        set -= this.SelectedNode.Set;
                    else
                        set |= this.SelectedNode.Set;
                    topwidth = this.Tree.WidthParameter.GetWidth(this.Tree.Graph, set);
                }

                // ancestors of parent
                this.costAncestors(this.SelectedNode.Parent, common, null, this.SelectedNode.Set, ref maximum, ref sum);
                // ancestors of the new sibling
                this.costAncestors(this.SelectedSibling, common, this.SelectedNode.Set, null, ref maximum, ref sum);
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

                // the common ancestor
                common = this.SelectedSibling;

                // update the width of the child of the root.
                if (this.SelectedSibling.IsRoot)
                    topwidth = this.SelectedNode.Width;

                // ancestors of parent
                this.costAncestors(this.SelectedNode.Parent, this.SelectedSibling.Parent, null, this.SelectedNode.Set, ref maximum, ref sum);
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

                // the common ancestor
                common = this.SelectedNode.Parent;

                // update the width of the child of the root.
                if (this.SelectedNode.Parent.IsRoot)
                    topwidth = this.OriginalSibling.Right.Set.IsSupersetOf(this.SelectedSibling.Set) ? this.OriginalSibling.Left.Width : this.OriginalSibling.Right.Width;

                // ancestors of the new sibling
                this.costAncestors(this.SelectedSibling, this.SelectedNode.Parent, this.SelectedNode.Set, null, ref maximum, ref sum);
            }

            // common ancestors
            this.costAncestors(common, null, null, null, ref maximum, ref sum);

            return DecompositionTree.ComputeCost(maximum, this.Tree.Size, sum, topwidth);
        }

        private void costAncestors(DecompositionNode child, DecompositionNode end, BitSet add, BitSet remove, ref double maximum, ref double sum)
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

        private void updateTree(DecompositionNode oldsibling, DecompositionNode newsibling)
        {
            DecompositionNode grandparent = this.SelectedNode.Parent.Parent;
            Branch parentbranch = this.SelectedNode.Parent.Branch;
            // connect parent to parent of new sibling
            this.SelectedNode.Parent.Parent = newsibling.Parent;
            if (newsibling.IsRoot)
                this.Tree.Root = this.SelectedNode.Parent;
            else
            {
                newsibling.Parent.SetChild(newsibling.Branch, this.SelectedNode.Parent);
                this.SelectedNode.Parent.Branch = newsibling.Branch;
            }
            // connect new sibling to parent
            newsibling.Parent = this.SelectedNode.Parent;
            this.SelectedNode.Parent.SetChild(oldsibling.Branch, newsibling);
            newsibling.Branch = oldsibling.Branch;
            // connect old sibling to grandparent
            oldsibling.Parent = grandparent;
            if (grandparent == null)
                this.Tree.Root = oldsibling;
            else
            {
                grandparent.SetChild(parentbranch, oldsibling);
                oldsibling.Branch = parentbranch;
            }

            // update the sets and width properties
            DecompositionNode common = null;
            if (!this.SelectedNode.Parent.Set.Intersects(newsibling.Set))
            {
                // parent
                this.SelectedNode.Parent.Set = this.SelectedNode.Set | newsibling.Set;
                this.SelectedNode.Parent.UpdateWidthProperties();

                // the common ancestor
                common = this.findCommonAncestor(this.SelectedNode.Parent, oldsibling.Parent);

                // ancestors of parent
                this.updateAncestors(this.SelectedNode.Parent, common, this.SelectedNode.Set, null);
                // ancestors of the old sibling
                this.updateAncestors(oldsibling, common, null, this.SelectedNode.Set);

                common.UpdateWidthProperties(false);
            }
            else if (this.SelectedNode.Set.IsSubsetOf(newsibling.Set))
            {
                // the common ancestor
                common = this.SelectedNode.Parent;
                // ancestors of oldsibling
                this.updateAncestors(oldsibling, common, null, this.SelectedNode.Set);

                // parent
                this.SelectedNode.Parent.Set = this.SelectedNode.Set | newsibling.Set;
                this.SelectedNode.Parent.UpdateWidthProperties();
            }
            else // the new sibling is a descendant of the original sibling
            {
                // parent
                this.SelectedNode.Parent.Set = this.SelectedNode.Set | newsibling.Set;
                this.SelectedNode.Parent.UpdateWidthProperties();

                // the common ancestor
                common = oldsibling.Parent;

                // ancestors of the new sibling
                this.updateAncestors(this.SelectedNode.Parent, common, this.SelectedNode.Set, null);

                if (common != null)
                    common.UpdateWidthProperties(false);
            }

            this.updateAncestors(common, null, null, null);
        }

        private void updateAncestors(DecompositionNode child, DecompositionNode end, BitSet add, BitSet remove)
        {
            if (child == null)
                return;

            for (DecompositionNode ancestor = child.Parent; ancestor != end && ancestor != null; child = ancestor, ancestor = ancestor.Parent)
            {
                if (add != null || remove != null)
                {
                    if (add != null)
                        ancestor.Set.Or(add);
                    if (remove != null)
                        ancestor.Set.Exclude(remove);
                    ancestor.UpdateWidthProperties();
                }
                else
                    ancestor.UpdateWidthProperties(false);
            }
        }
    }
}
