using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.WidthParameters
{
    /// <summary>
    /// The rank-width is the rank of the adjacency matrix over the cut in Gf(2).
    /// </summary>
    /// <remarks>
    /// The rank of a binary matrix in Gf(2) can be computed with Gaussian elimination using only Boolean operators (AND, OR).
    /// </remarks>
    class RankWidth : WidthParameter
    {
        public RankWidth() : base() { this.Name = "rank-width"; }

        protected override double computeWidth(Graph graph, BitSet left, BitSet right)
        {
            BitSet selected = left.Count < right.Count ? left : right;

            BitSet involved = new BitSet(left.Size);

            BitSet[] rows = new BitSet[selected.Count];

            int i = 0;
            // Construct the adjacency matrix and the set of all neighbors over the cut.
            foreach (int index in selected)
            {
                rows[i] = graph.Vertices[index].Neighborhood - selected;
                involved.Or(rows[i++]);
            }

            return rank(rows, involved.ToArray());
        }

        /// <summary>
        /// Computes the rank of a binary matrix over Gf(2).
        /// </summary>
        /// <param name="rows">The set of rows.</param>
        /// <param name="indices">The column-indices of all ones in the matrix.</param>
        /// <returns>The rank of the matrix.</returns>
        protected int rank(BitSet[] rows, int[] indices)
        {
            if (rows.Length == 0)
                return 0;

            int rank = 0, columnindex = 0, rowindex = 0;
            while (rowindex != rows.Length && columnindex != indices.Length)
            {
                int index = indices[columnindex];

                int maximum = rows.Length;
                // Pivot until we find either an empty row or a row with a one in the column.
                while (!rows[rowindex].IsEmpty && !rows[rowindex][index] && maximum > rowindex)
                    swap(rows, rowindex, --maximum);

                // Skip empty rows.
                if (rows[rowindex].IsEmpty)
                {
                    rowindex++;
                    continue;
                }

                // Skip empty columns.
                if (maximum == rowindex)
                {
                    columnindex++;
                    continue;
                }

                // We have found a row that is independend of all previous rows.
                rank++;

                // XOR all subsequent rows with a one in the column.
                for (int j = rowindex + 1; j < maximum; j++)
                    if (rows[j][index])
                        rows[j].Xor(rows[rowindex]);

                // Move to the next row and column.
                columnindex++;
                rowindex++;
            }
            return rank;
        }

        /// <summary>
        /// Swap two rows of a matrix.
        /// </summary>
        /// <param name="rows">The rows in the matrix.</param>
        /// <param name="i">The index of the first row.</param>
        /// <param name="j">The index of the second row.</param>
        protected void swap(BitSet[] rows, int i, int j)
        {
            BitSet temp = rows[i];
            rows[i] = rows[j];
            rows[j] = temp;
        }
    }
}
