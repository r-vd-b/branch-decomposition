using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BranchDecomposition.DecompositionTrees;

namespace BranchDecomposition.ImprovementHeuristics
{
    class SwapOperator : LocalSearchOperator
    {
        public override LocalSearchOperation GetRandomOperation(DecompositionTree tree, Random rng)
        {
            return this.Operations(tree, rng).First();
        }

        public override IEnumerable<LocalSearchOperation> Operations(DecompositionTree tree, Random rng)
        {
            int[] combinationcounters = new int[tree.Nodes.Length];
            DecompositionNode[] leftcandidates = new DecompositionNode[tree.Nodes.Length];
            DecompositionNode[][] rightcandidates = new DecompositionNode[tree.Nodes.Length][];
            DecompositionNode left = null, right = null;

            int leftindex = 0, total = 0;
            foreach (var node in tree.Root.SubTree(TreeTraversal.ParentFirst))
            {
                leftcandidates[leftindex] = node;
                    combinationcounters[leftindex] = tree.Nodes.Length - node.SubTreeSize - 1;
                for (DecompositionNode ancestor = node.Parent; ancestor != null; ancestor = ancestor.Parent)
                    combinationcounters[leftindex]--;
                total += combinationcounters[leftindex];
                leftindex++;
            }

            int maximum = tree.Nodes.Length;
            // As long as there is a operation possible
            while (total > 0)
            {
                int sample = rng.Next(total) + 1, sum = 0;
                // Find a random left candidate.
                for (leftindex = 0; leftindex < maximum; leftindex++)
                {
                    // Move invalid left candidates to the back.
                    while (combinationcounters[leftindex] == 0)
                    {
                        maximum--;
                        combinationcounters[leftindex] = combinationcounters[maximum];
                        leftcandidates[leftindex] = leftcandidates[maximum];
                        rightcandidates[leftindex] = rightcandidates[maximum];
                        if (maximum == leftindex)
                            break;
                    }

                    sum += combinationcounters[leftindex];
                    if (sample <= sum)
                    {
                        if (combinationcounters[leftindex] == 0)
                            throw new InvalidOperationException();

                        // Select the random left candidate.
                        left = leftcandidates[leftindex];

                        DecompositionNode[] candidates = rightcandidates[leftindex];
                        if (candidates == null)
                        {
                            // Initialize the set of all right candidates.
                            candidates = rightcandidates[leftindex] = new DecompositionNode[combinationcounters[leftindex]];
                            int index = 0;

                            foreach (DecompositionNode node in left.Sibling.SubTree(TreeTraversal.ParentFirst).Skip(1))
                                candidates[index++] = node;
                            for (DecompositionNode ancestor = left.Parent; !ancestor.IsRoot; ancestor = ancestor.Parent)
                                foreach (DecompositionNode node in ancestor.Sibling.SubTree(TreeTraversal.ParentFirst))
                                    candidates[index++] = node;
                        }

                        // Select a random position.
                        int positionindex = rng.Next(combinationcounters[leftindex]);
                        right = candidates[positionindex];
                        candidates[positionindex] = candidates[--combinationcounters[leftindex]];
                        total--;
                        break;
                    }
                }

                if (right == null)
                    throw new InvalidOperationException();

                yield return new SwapOperation(tree, left, right);
            }
        }
    }
}
