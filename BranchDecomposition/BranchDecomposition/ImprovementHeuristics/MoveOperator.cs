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
                total += candidatecounters[candidateindex] = tree.Nodes.Length - node.SubTreeSize - 2;
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
                            foreach (var node in insertioncandidate.Sibling.SubTree(TreeTraversal.ParentFirst).Skip(1))
                                positions[index++] = node;
                            for (DecompositionNode ancestor = insertioncandidate.Parent; !ancestor.IsRoot; ancestor = ancestor.Parent)
                            {
                                foreach (DecompositionNode node in ancestor.Sibling.SubTree(TreeTraversal.ParentFirst))
                                    positions[index++] = node;
                                if (ancestor != insertioncandidate.Parent)
                                    positions[index++] = ancestor;
                            }
                            if (!insertioncandidate.Parent.IsRoot)
                                positions[index++] = tree.Root;
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
            // No operations available.
            if (tree.Nodes.Length <= 3)
                return null;

            // Select a random node that will be moved to a different position in the tree.
            int index = this.getRandomNonRootIndex(tree, rng);
            DecompositionNode selected = tree.Find(index);

            // Select a random node that will become the new sibling of the selected node.
            int candidateCount = tree.Nodes.Length - selected.SubTreeSize - 2,
                positionIndex = 1 + rng.Next(candidateCount);

            // We need to ensure that we do not select a node in the subtree of the selected node, nor its current parent and sibling.
            if (selected.Branch == Branch.Left)
            {
                // Shift it outside the range of the subtree of the selected node.
                if (index - selected.SubTreeSize < positionIndex && positionIndex <= index)
                    positionIndex += selected.SubTreeSize;
                // Shift it past the sibling and the parent of the selected node.
                if (positionIndex == index + selected.Sibling.SubTreeSize)
                    positionIndex += 2;
                // Shift it past the parent of the selected node.
                else if (positionIndex == index + selected.Sibling.SubTreeSize + 1)
                    positionIndex++;
            }
            else
            {
                // Shift it past the sibling of the selected node.
                if (positionIndex == index - selected.SubTreeSize)
                    positionIndex++;
                // Shift it past the parent and outside the range of the subtree of the selected node.
                if (index - selected.SubTreeSize < positionIndex && positionIndex <= index + 1)
                    positionIndex += selected.SubTreeSize + 1;
            }
            DecompositionNode sibling = tree.Find(positionIndex);

            return new MoveOperation(tree, selected, sibling);
        }
    }
}
