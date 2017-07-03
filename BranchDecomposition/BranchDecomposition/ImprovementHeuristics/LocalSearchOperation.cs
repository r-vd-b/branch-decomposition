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

        /// <summary>
        /// Computes the cost of the decomposition tree after applying this operation.
        /// </summary>
        /// <returns>The resulting cost.</returns>
        protected virtual double computeCost()
        {
            double cost = this.Execute();
            this.Revert();
            return cost;
        }

        /// <summary>
        /// Searches for the first common ancestor of two nodes.
        /// </summary>
        /// <param name="n1">One of the nodes.</param>
        /// <param name="n2">The other node.</param>
        /// <returns>The common ancestor.</returns>
        protected DecompositionNode findCommonAncestor(DecompositionNode n1, DecompositionNode n2)
        {
            for (DecompositionNode ancestor = n1; ancestor != null; ancestor = ancestor.Parent)
                if (ancestor.Set.IsSupersetOf(n2.Set))
                    return ancestor;
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Updates the vertex-set and the width properties of all ancestors of a node in a bottom-up fashion, until the given ancestor is reached.
        /// </summary>
        /// <param name="child">A descendant of all nodes that will be updated.</param>
        /// <param name="end">The updating of the ancestor will stop once this node is reach. The end-node itself will not be updated.</param>
        /// <param name="add">The vertices that will be added to the vertex-set of the ancestors. NULL can be passed if no vertices should be added.</param>
        /// <param name="remove">The vertices that will be removed from the vertex-set of the ancestors. NULL can be passed if no vertices should be removed.</param>
        protected void updateAncestors(DecompositionNode child, DecompositionNode end, BitSet add, BitSet remove)
        {
            bool addToVertexSet = add != null && !add.IsEmpty;
            bool removeFromVertexSet = remove != null && !remove.IsEmpty;

            for (DecompositionNode ancestor = child?.Parent; ancestor != end && ancestor != null; child = ancestor, ancestor = ancestor.Parent)
            {
                if (addToVertexSet || removeFromVertexSet)
                {
                    if (addToVertexSet)
                        ancestor.Set.Or(add);
                    if (removeFromVertexSet)
                        ancestor.Set.Exclude(remove);
                    ancestor.UpdateWidthProperties();
                }
                else
                    ancestor.UpdateWidthProperties(false);
            }
        }


        /// <summary>
        /// Computes the maximum width and the sum of widths of the nodes of the subtree ...
        /// </summary>
        /// <param name="child">The node whose subtree has already been computed.</param>
        /// <param name="end">The parent of the root of the subtree that we want to compute.</param>
        /// <param name="add">he vertices that will be added to the vertex-set of the ancestors. NULL can be passed if no vertices should be added.</param>
        /// <param name="remove">The vertices that will be removed from the vertex-set of the ancestors. NULL can be passed if no vertices should be removed.</param>
        /// <param name="maximum">The maximum width of the subtree, excluding the width of the subtree rooted at the child.</param>
        /// <param name="sum">The sum of the width of the nodes of the subtree, excluding the sum of the subtree rooted at the child.</param>
        protected void computeWidthAncestors(DecompositionNode child, DecompositionNode end, BitSet add, BitSet remove, ref double maximum, ref double sum)
        {
            bool addToVertexSet = add != null && !add.IsEmpty;
            bool removeFromVertexSet = remove != null && !remove.IsEmpty;

            for (DecompositionNode ancestor = child?.Parent; ancestor != end && ancestor != null; child = ancestor, ancestor = ancestor.Parent)
            {
                double width = ancestor.Width;
                if (addToVertexSet || removeFromVertexSet)
                {
                    BitSet set = new BitSet(ancestor.Set);
                    if (addToVertexSet)
                        set.Or(add);
                    if (removeFromVertexSet)
                        set.Exclude(remove);
                    width = this.Tree.WidthParameter.GetWidth(this.Tree.Graph, set);
                }
                maximum = Math.Max(maximum, Math.Max(width, child.Sibling.SubTreeWidth));
                sum += width + child.Sibling.SubTreeSum;
            }
        }
    }
}
