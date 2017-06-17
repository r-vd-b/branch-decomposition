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
        public DecompositionNode SelectedNodeA { get; }
        public DecompositionNode SelectedNodeB { get; }

        public SwapOperation(DecompositionTree tree, DecompositionNode nodeA, DecompositionNode nodeB) : base(tree)
        {
            this.SelectedNodeA = nodeA;
            this.SelectedNodeB = nodeB;
        }

        public override double Execute()
        {
            this.updateTree(this.SelectedNodeA, this.SelectedNodeB);
            return base.Execute();
        }

        public override void Revert()
        {
            this.updateTree(this.SelectedNodeA, this.SelectedNodeB);
        }

        protected override double computeCost()
        {
            return base.computeCost();
        }

        private void updateTree(DecompositionNode a, DecompositionNode b)
        {
            DecompositionNode oldParentA = a.Parent;
            Branch            oldBranchA = a.Branch;
            DecompositionNode oldParentB = b.Parent;
            Branch            oldBranchB = b.Branch;

            //swap position of the subtrees
            this.Tree.Attach(oldParentA, b, oldBranchA);
            this.Tree.Attach(oldParentB, a, oldBranchB);
            
            //update only those widths that have changed; does this bottom-up to facilitate passing maximums and sums upwards.
//Are the sets set up correctly at the moment of running this? if not, this does not work as is (either work with subtrees, fix sets as this runs or use the un-updated data)
  //How does move operation update widths? If that is similar (or better) maybe emulate that here instead.
//Is the width stored in a node the width of the edge between it and its parent?
            BitSet collection = new BitSet(a.Parent.Set);
            DecompositionNode currentNode = a.Parent;

            do //go up from node A until we find common ancestor, update along the way
            {
                currentNode.UpdateWidthProperties();
                collection.Or(currentNode.Sibling.Set);
                currentNode = currentNode.Parent;
            } while (!collection.IsSupersetOf(b.Set)) ;

            DecompositionNode commonAncestor = currentNode;
            currentNode = b.Parent;

            do //go up from node B until we are at common ancestor, update along the way
            {
                currentNode.UpdateWidthProperties();
                currentNode = currentNode.Parent;
            } while (currentNode != commonAncestor);

            //update common ancestor
            commonAncestor.UpdateWidthProperties();

            //all edges with possibly different score have been recalculated, so pass the maximums and sums up to the root
            this.updateAncestors(commonAncestor, null, null, null);
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
