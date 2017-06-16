using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.WidthParameters
{
    class RankWidth : WidthParameter
    {
        public RankWidth() : base() { this.Name = "Rank-width"; }

        protected override double computeWidth(Graph graph, BitSet left, BitSet right)
        {
            BitSet selected = left.Count < right.Count ? left : right;

            BitSet involved = new BitSet(left.Size);

            BitSet[] rows = new BitSet[selected.Count];

            int i = 0;
            foreach (int index in selected)
            {
                rows[i] = graph.Vertices[index].Neighborhood - selected;
                involved.Or(rows[i++]);
            }

            return rank(rows, involved.ToArray());
        }

        protected int rank(BitSet[] rows, int[] indices)
        {
            if (rows.Length == 0 || (rows.Length == 1 && rows[0].IsEmpty))
                return 0;

            int rank = 0;
            for (int i = 0; i < Math.Min(rows.Length, indices.Length); i++)
            {
                int index = indices[i];
                int maximum = rows.Length;
                while (!rows[i][index])
                    swap(rows, i, --maximum);

                if (maximum > i)
                {
                    rank++;
                    for (int j = i + 1; j < maximum; j++)
                        if (rows[j][index])
                            rows[j].Xor(rows[i]);
                }
            }
            return rank;
        }

        protected void swap(BitSet[] rows, int i, int j)
        {
            BitSet temp = rows[i];
            rows[i] = rows[j];
            rows[j] = temp;
        }
    }
}
