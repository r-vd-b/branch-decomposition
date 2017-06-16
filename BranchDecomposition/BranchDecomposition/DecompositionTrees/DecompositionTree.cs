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
        public DecompositionNode Root { get; set; }
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
        
        public override string ToString()
        {
            return $"|V|={this.Nodes.Length}, Width={this.Width}, Cost={this.Cost}";
        }

        public static double ComputeCost(double maximumWidth, int numberOfVertices, double sumOfWidths, double topWidth)
        {
            return maximumWidth * numberOfVertices * numberOfVertices + sumOfWidths - topWidth;
        }
    }
}
