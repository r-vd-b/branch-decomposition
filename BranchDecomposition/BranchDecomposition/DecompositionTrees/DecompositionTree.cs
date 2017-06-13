using BranchDecomposition.WidthParameters;
using System.Collections.Generic;
using System;

namespace BranchDecomposition.DecompositionTrees
{
    public enum TreeTraversal { ParentFirst, ChildrenFirst }

    class DecompositionTree
    {

        public Graph Graph { get; }
        public WidthParameter WidthParameter { get; }
        public DecompositionNode[] Nodes { get; }
        public DecompositionNode Root { get; protected set; }
        public int Size { get { return this.Graph.Vertices.Count; } }

        public double Width { get { return this.Root.SubTreeWidth; } }
        public double Cost { get { return this.Root.SubTreeWidth * this.Size * this.Size + this.Root.SubTreeSum - (this.Root?.Right?.Width ?? 0); } }
        
        public DecompositionTree(Graph graph, WidthParameter parameter)
        {
            this.Graph = graph;
            this.WidthParameter = parameter;
            this.Nodes = new DecompositionNode[graph.Vertices.Count * 2 - 1];
        }

        public DecompositionTree(DecompositionTree tree) : this(tree.Graph, tree.WidthParameter)
        {
            this.Root = tree.Root.CopyTree(this, null);
        }

        public double ComputeWidth()
        {
            this.Root.UpdateWidthSubtree();
            return this.Width;
        }

        public void MoveTo(DecompositionNode node, int index)
        {
            DecompositionNode other = this.Nodes[index];
            other.Index = node.Index;
            this.Nodes[node.Index] = other;
            node.Index = index;
            this.Nodes[index] = node;
        }

        /// <summary>
        /// Add a node to the tree.
        /// </summary>
        /// <param name="add">The node that will be added to the tree.</param>
        /// <param name="sibling">The node in the tree selected to become the sibling of the added node.</param>
        /// <param name="parent">The new parent of the added node and the selected sibling.</param>
        public void AddNode(DecompositionNode add, DecompositionNode sibling, DecompositionNode parent)
        {
            this.Nodes[add.Index] = add;
            if (this.Root == null)
            {
                this.Root = add;
            }
            else
            {
                this.Nodes[parent.Index] = parent;

                // connect parent to parent of sibling
                parent.Parent = sibling.Parent;
                if (sibling.Parent != null)
                {
                    sibling.Parent.SetChild(sibling.Branch, parent);
                    parent.Branch = sibling.Branch;
                }
                else
                {
                    this.Root = parent;
                }

                // connect node and sibling to parent
                parent.Left = add;
                add.Branch = Branch.Left;
                add.Parent = parent;
                parent.Right = sibling;
                sibling.Branch = Branch.Right;
                sibling.Parent = parent;
            }
        }

        /// <summary>
        /// Replaces a parent by its child in the tree.
        /// </summary>
        /// <param name="parent">The parent to be replaced.</param>
        /// <param name="child">The replacement.</param>
        /// <param name="updateAncestorSets">Should the sets of all ancestors be updated as well?</param>
        public void ReplaceByChild(DecompositionNode parent, DecompositionNode child, bool updateAncestorSets = true)
        {
            // Remove the child from the parent.
            parent.SetChild(child.Branch, null);
            parent.Set.Exclude(child.Set);
            // Attach the child to its grandparent.
            child.Parent = parent.Parent;
            if (parent.IsRoot)
                this.Root = child;
            else
            {
                parent.Parent.SetChild(parent.Branch, child);
                child.Branch = parent.Branch;
                // Update the ancestors of the child.
                if (updateAncestorSets)
                {
                    BitSet removed = parent.GetChild(parent.Left == null ? Branch.Right : Branch.Left).Set;
                    for (DecompositionNode ancestor = child.Parent; ancestor != null; ancestor = ancestor.Parent)
                        ancestor.Set.Exclude(removed);
                }
            }
        }

        /// <summary>
        /// Insert the node as the sibling of a node in the tree.
        /// </summary>
        /// <param name="node">The node to be inserted.</param>
        /// <param name="sibling">The new sibling of the tree.</param>
        /// <param name="updateAncestorSets">Should the sets of all ancestors be updated as well?</param>
        public void InsertAsSibling(DecompositionNode node, DecompositionNode sibling, bool updateAncestorSets = true)
        {
            // Attach the node to the parent of the child.
            node.Parent = sibling.Parent;
            if (sibling.IsRoot)
                this.Root = node;
            else
            {
                sibling.Parent.SetChild(sibling.Branch, node);
                node.Branch = sibling.Branch;
            }

            // Attach the child to the node.
            sibling.Parent = node;
            Branch branch = node.Left == null ? Branch.Left : Branch.Right;
            node.SetChild(branch, sibling);
            sibling.Branch = branch;
            node.Set.Or(sibling.Set);

            // Update the ancestors of the child.
            if (updateAncestorSets)
            {
                BitSet added = sibling.Sibling.Set;
                for (DecompositionNode ancestor = node.Parent; ancestor != null; ancestor = ancestor.Parent)
                    ancestor.Set.Or(added);
            }
        }
        
        public override string ToString()
        {
            return $"|V|={this.Nodes.Length}, Width={this.Width}, Cost={this.Cost}";
        }
    }
}
