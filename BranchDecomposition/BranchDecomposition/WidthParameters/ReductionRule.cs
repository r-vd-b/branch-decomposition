using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.WidthParameters
{
    abstract class ReductionRule
    {
        public abstract bool Apply(Graph graph);
    }

    class DegreeZeroRemovalRule : ReductionRule
    {
        public override bool Apply(Graph graph)
        {
            foreach (Vertex v in graph.Vertices)
                if (v.AdjacencyList.Count == 0)
                {
                    graph.RemoveVertex(v);
                    return true;
                }
            return false;
        }
    }

    class DegreeOneRemovalRule : ReductionRule
    {
        public override bool Apply(Graph graph)
        {
            foreach (Vertex v in graph.Vertices)
                if (v.AdjacencyList.Count == 1 && graph.Vertices.Count > 1)
                {
                    graph.RemoveVertex(v);
                    return true;
                }
            return false;
        }
    }

    class TwinRemovalRule : ReductionRule
    {
        public override bool Apply(Graph graph)
        {
            foreach (Vertex v in graph.Vertices)
            {
                foreach (Vertex w in graph.Vertices)
                {
                    if (v != w && (v.Neighborhood.Equals(w.Neighborhood) || (v.Neighborhood[w.Index] == true && v.Neighborhood.Xor(w.Neighborhood).Count == 2)))
                    {
                        graph.RemoveVertex(v);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
