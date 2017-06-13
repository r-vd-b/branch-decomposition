using System;
using BranchDecomposition.DecompositionTrees;
using BranchDecomposition.WidthParameters;
using BranchDecomposition.ConstructionHeuristics;
using BranchDecomposition.ImprovementHeuristics;

namespace BranchDecomposition
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Path to graph file?");
            Graph graph = Parser.ParseDGF(Console.ReadLine());
            Console.WriteLine("Local search duration in seconds?");
            int duration = int.Parse(Console.ReadLine());

            WidthParameter width = new BooleanWidth();

            Random rng = new Random(0);

            ConstructionHeuristic constructor = new RandomizeGreedyLinearConstructor(rng);
            DecompositionTree tree = constructor.Construct(graph, width);

            LocalSearchAlgorithm localsearch = new VariableNeighborhoodSearch(new DecompositionTree[] { tree }, new LocalSearchOperator[] { new MoveOperator() }, rng);
            localsearch.Run(duration);

            Console.WriteLine($"Finished in {localsearch.CurrentComputationTime.ToString("N0")} seconds.");
            Console.WriteLine($"Best width = {tree.Width}");
            Console.WriteLine($"Best cost = {tree.Cost}");
            Console.WriteLine("Best tree decomposition:");
            Console.WriteLine(tree.Root);
            Console.ReadLine();
        }
    }
}
