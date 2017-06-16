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

        public void Attach(DecompositionNode parent, DecompositionNode child, Branch branch)
        {
            child.Parent = parent;
            child.Branch = branch;
            if (parent == null)
                this.Root = child;
            else
                parent.SetChild(branch, child);
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
