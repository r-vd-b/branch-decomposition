﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.DecompositionTrees
{
    public enum Branch { Left, Right }

    /// <summary>
    /// The DecompositionNode class represents a node in the decomposition tree.
    /// </summary>
    class DecompositionNode
    {
        public DecompositionTree Tree { get; }
        public DecompositionNode Left { get; set; }
        public DecompositionNode Right { get; set; }
        public DecompositionNode Parent { get; set; }
        public DecompositionNode Sibling { get { return this.Parent?.GetChild(this.Branch == Branch.Left ? Branch.Right : Branch.Left); } }
        public Branch Branch { get; set; }
        public BitSet Set { get; set; }

        public int Index { get; set; }
        public int SubTreeSize { get { return 2 * this.Set.Count - 1; } }

        /// <summary>
        /// The width of a node is the width of the cut (node.Set, complement of node.Set), i.e. the width of the cut induced by the edge to the parent of the node.
        /// </summary>
        public double Width { get; set; }
        public double SubTreeWidth { get; set; }
        public double SubTreeSum { get; set; }
        
        public bool IsRoot { get { return this.Parent == null; } }

        public bool IsLeaf { get { return this.Left == null; } }

        // The vertex field is only set for leaf nodes.
        protected Vertex vertex { get; }

        public DecompositionNode(BitSet set, int index, DecompositionTree tree)
        {
            this.Tree = tree;
            this.Index = index;
            this.SubTreeWidth = this.SubTreeSum = this.Width = tree.WidthParameter.GetWidth(tree.Graph, set);
            this.Set = set;
        }

        public DecompositionNode(Vertex vertex, int index, DecompositionTree tree) : this(new BitSet(tree.VertexCount, vertex.Index), index, tree)
        {
            this.vertex = vertex;
        }

        public DecompositionNode(DecompositionNode node, DecompositionTree tree = null)
        {
            this.Set = new BitSet(node.Set);
            this.Left = node.Left;
            this.Right = node.Right;
            this.Parent = node.Parent;
            this.Branch = node.Branch;
            this.Width = node.Width;
            this.SubTreeWidth = node.SubTreeWidth;
            this.SubTreeSum = node.SubTreeSum;
            this.Index = node.Index;
            this.vertex = node.vertex;
            this.Tree = tree != null ? tree : node.Tree;
        }

        /// <summary>
        /// Returns the child in the given branch.
        /// </summary>
        public DecompositionNode GetChild(Branch branch) { return branch == Branch.Left ? this.Left : this.Right; }

        /// <summary>
        /// Sets the child in the given branch.
        /// </summary>
        public void SetChild(Branch branch, DecompositionNode child) { if (branch == Branch.Left) this.Left = child; else this.Right = child; }

        /// <summary>
        /// Updates the width properties of this subtree bottom up.
        /// </summary>
        public void UpdateWidthSubtree()
        {
            foreach (DecompositionNode node in this.SubTree(TreeTraversal.ChildrenFirst))
                node.UpdateWidthProperties();
        }

        /// <summary>
        /// Update the width properties of this node.
        /// </summary>
        /// <param name="recomputeOwnWidth">Should the width of the node itself be recomputed as well?</param>
        public void UpdateWidthProperties(bool recomputeOwnWidth = true)
        {
            if (recomputeOwnWidth)
                this.Width = this.Tree.WidthParameter.GetWidth(this.Tree.Graph, this.Set);

            if (this.IsLeaf)
                this.SubTreeWidth = this.SubTreeSum = this.Width;
            else
            {
                this.SubTreeSum = this.Width + this.Left.SubTreeSum + this.Right.SubTreeSum;
                this.SubTreeWidth = Math.Max(this.Width, Math.Max(this.Left.SubTreeWidth, this.Right.SubTreeWidth));
            }
        }

        /// <summary>
        /// A deep copy of this subtree.
        /// </summary>
        /// <param name="tree">The original tree.</param>
        /// <param name="parent">The parent of the copied node.</param>
        /// <returns>A deep copy of the node.</returns>
        public DecompositionNode CopyTree(DecompositionTree tree, DecompositionNode parent)
        {
            DecompositionNode node = tree.Nodes[this.Index] = new DecompositionNode(this, tree);
            node.Parent = parent;
            if (!this.IsLeaf)
            {
                node.Left = this.Left.CopyTree(tree, node);
                node.Right = this.Right.CopyTree(tree, node);
            }
            return node;
        }

        /// <summary>
        /// The set of all nodes in this subtree.
        /// </summary>
        /// <param name="mode">The order in which we will travers the nodes in the subtree.</param>
        /// <returns>The nodes in the subtree.</returns>
        public IEnumerable<DecompositionNode> SubTree(TreeTraversal mode)
        {
            if (mode == TreeTraversal.ParentFirst)
            {
                Queue<DecompositionNode> queue = new Queue<DecompositionNode>();
                queue.Enqueue(this);
                while (queue.Count > 0)
                {
                    DecompositionNode node = queue.Dequeue();
                    if (!node.IsLeaf)
                    {
                        queue.Enqueue(node.Left);
                        queue.Enqueue(node.Right);
                    }
                    yield return node;
                }
            }
            else
            {
                DecompositionNode finished = this;
                Stack<DecompositionNode> stack = new Stack<DecompositionNode>();
                stack.Push(this);
                while (stack.Count > 0)
                {
                    DecompositionNode node = stack.Peek();
                    if (node.IsLeaf || node.Right == finished)
                    {
                        stack.Pop();
                        finished = node;
                        yield return node;
                    }
                    else
                    {
                        stack.Push(node.Right);
                        stack.Push(node.Left);
                    }
                }
            }
        }

        public override string ToString()
        {
            return this.IsLeaf ? this.vertex.Name : $"({this.Left}, {this.Right})";
        }
    }
}
