using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.ReductionRules
{
    class RemoveTwinRule : ReductionRule
    {
        public override bool IsApplicable(Graph graph)
        {
            return graph.Vertices.Any(v => v.AdjacencyList.Count > 1 && v.AdjacencyList.SelectMany(neighbor => neighbor.AdjacencyList).Distinct().Any(w => v != w && (v.Neighborhood.Equals(w.Neighborhood) || (v.Neighborhood[w.Index] == true && (v.Neighborhood ^ w.Neighborhood).Count == 2))));
        }

        public override Graph[] Apply(Graph graph)
        {
            foreach (Vertex v in graph.Vertices)
            {
                if (v.AdjacencyList.Count <= 1)
                    continue;

                foreach (Vertex w in v.AdjacencyList.SelectMany(neighbor => neighbor.AdjacencyList).Distinct())
                    if (v != w && (v.Neighborhood.Equals(w.Neighborhood) || (v.Neighborhood[w.Index] == true && (v.Neighborhood ^ w.Neighborhood).Count == 2)))
                    {
                        graph.RemoveVertex(v);
                        return new Graph[] { graph };
                    }
            }

            throw new InvalidOperationException("Failed to apply the reduction rule");
        }
    }
}
