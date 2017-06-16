using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.WidthParameters
{
    /// <summary>
    /// The boolean-width parameter is the log2 of the number of distinct neighborhoods of all subsets of a partition over the cut.
    /// </summary>
    class BooleanWidth : WidthParameter
    {
        public BooleanWidth() : base() { this.Name = "Boolean-width"; }

        protected override double computeWidth(Graph graph, BitSet left, BitSet right)
        {
            HashSet<BitSet> neighborhoods = new HashSet<BitSet>();
            // Add the empty set as a neighborhood.
            neighborhoods.Add(new BitSet(left.Size));

            foreach (int index in left)
            {
                Vertex v = graph.Vertices[index];
                // The neighborhood of v in the right set.
                BitSet neighborhood = v.Neighborhood & right;

                if (neighborhood.IsEmpty)
                    continue;

                // Add the union of the neighborhood of v with every element in the neighborhood set.
                foreach (BitSet n in neighborhoods.ToArray())
                    neighborhoods.Add(neighborhood | n);
            }

            return Math.Log(neighborhoods.Count, 2);
        }
    }
}
