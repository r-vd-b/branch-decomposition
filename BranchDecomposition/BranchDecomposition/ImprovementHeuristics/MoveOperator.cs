using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BranchDecomposition.DecompositionTrees;

namespace BranchDecomposition.ImprovementHeuristics
{
    class MoveOperator : LocalSearchOperator
    {
        // Get all operations in random order.
        public override IEnumerable<LocalSearchOperation> Operations(DecompositionTree tree, Random rng)
        {
            // Initialize the insertion candidate list
            int[] candidatecounters = new int[tree.Nodes.Length];
            DecompositionNode[] insertioncandidates = new DecompositionNode[tree.Nodes.Length];
            DecompositionNode[][] positioncandidates = new DecompositionNode[tree.Nodes.Length][];
            DecompositionNode insertioncandidate = null, positioncandidate = null;

            int candidateindex = 0, total = 0;
            foreach (var node in tree.Root.SubTree(TreeTraversal.ParentFirst))
            {
                insertioncandidates[candidateindex] = node;
                total += candidatecounters[candidateindex] = tree.Nodes.Length - (2 * node.Set.Count - 1) - 2;
                candidateindex++;
            }

            int maximum = tree.Nodes.Length;
            // As long as there is a operation possible
            while (total > 0)
            {
                int sample = rng.Next(total) + 1, sum = 0;
                // Find a random insertion candidate.
                for (candidateindex = 0; candidateindex < maximum; candidateindex++)
                {
                    // Move invalid insertion candidates to the back.
                    while (candidatecounters[candidateindex] == 0)
                    {
                        maximum--;
                        candidatecounters[candidateindex] = candidatecounters[maximum];
                        insertioncandidates[candidateindex] = insertioncandidates[maximum];
                        positioncandidates[candidateindex] = positioncandidates[maximum];
                        if (maximum == candidateindex)
                            break;
                    }

                    sum += candidatecounters[candidateindex];
                    if (sample <= sum)
                    {
                        if (candidatecounters[candidateindex] == 0)
                            throw new InvalidOperationException();

                        // Select the random insertion candidate.
                        insertioncandidate = insertioncandidates[candidateindex];

                        DecompositionNode[] positions = positioncandidates[candidateindex];
                        if (positions == null)
                        {
                            // Initialize the set of all candidate positions.
                            positions = positioncandidates[candidateindex] = new DecompositionNode[candidatecounters[candidateindex]];
                            int index = 0;
                            foreach (var node in insertioncandidate.Parent.TreeExcludingSubTree)
                                positions[index++] = node;
                            foreach (var node in insertioncandidate.Sibling.SubTree(TreeTraversal.ParentFirst).Skip(1))
                                positions[index++] = node;
                        }

                        // Select a random position.
                        int positionindex = rng.Next(candidatecounters[candidateindex]);
                        positioncandidate = positions[positionindex];
                        positions[positionindex] = positions[--candidatecounters[candidateindex]];
                        total--;
                        break;
                    }
                }

                if (positioncandidate == null)
                    throw new InvalidOperationException();

                yield return new MoveOperation(tree, insertioncandidate, positioncandidate);
            }
        }

        public override LocalSearchOperation GetRandomOperation(DecompositionTree tree, Random rng)
        {
            // Select a random node x (such that x is not the root) that will be inserted at a different position in the tree.
            DecompositionNode selected = tree.Nodes[rng.Next(tree.Nodes.Length)];
            int index = tree.Nodes.Length - 1;
            while (selected.IsRoot || selected.Parent.IsRoot && selected.Sibling.IsLeaf)
            {
                tree.MoveTo(selected, index);
                selected = tree.Nodes[rng.Next(index--)];
            }
            tree.MoveTo(selected, tree.Nodes.Length - 1);

            // Select a random node y that will become the new sibling of x, such that y is not in the subtree of x and not parent(x).
            DecompositionNode sibling = null;
            for (int i = tree.Nodes.Length - 1; i > 0; i--)
            {
                sibling = tree.Nodes[rng.Next(i)];
                tree.MoveTo(sibling, i - 1);
                if (sibling != selected.Parent && sibling != selected.Sibling && !sibling.Set.IsSubsetOf(selected.Set))
                    break;
            }

            return new MoveOperation(tree, selected, sibling);
        }
    }
}
