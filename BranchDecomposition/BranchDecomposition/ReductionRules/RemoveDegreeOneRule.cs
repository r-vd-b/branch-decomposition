using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.ReductionRules
{
    class RemoveDegreeOneRule : ReductionRule
    {
        public override bool IsApplicable(Graph graph)
        {
            return graph.Vertices.Any(v => v.AdjacencyList.Count == 1 && v.AdjacencyList[0].AdjacencyList.Count > 1);
        }

        public override Graph[] Apply(Graph graph)
        {
            foreach (Vertex v in graph.Vertices)
                if (v.AdjacencyList.Count == 1 && v.AdjacencyList[0].AdjacencyList.Count > 1)
                {
                    graph.RemoveVertex(v);
                    return new Graph[] { graph };
                }

            throw new InvalidOperationException("Failed to apply the reduction rule");
        }
    }
}
