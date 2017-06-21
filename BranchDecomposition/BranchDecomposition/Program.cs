using System;
using BranchDecomposition.DecompositionTrees;
using BranchDecomposition.WidthParameters;
using BranchDecomposition.ConstructionHeuristics;
using BranchDecomposition.ImprovementHeuristics;
using System.Diagnostics;

namespace BranchDecomposition
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Path to graph file? Defaults to the anna graph from the TreewidthLIB set.");
            string path = Console.ReadLine();
            if (path.Length == 0)
                path = @"..\..\ExampleGraphs\anna.dgf";
            Graph graph = Parser.ParseDGF(path);

            Console.WriteLine("Width Parameter is (M)aximum-matching, (R)ank or (B)oolean? Defaults to rank-width.");
            WidthParameter width = null;
            string widthstring = Console.ReadLine().ToLower();
            if (widthstring.Length == 0)
                width = new RankWidth();
            else
                switch (widthstring.ToLower()[0])
                {
                    case 'm':
                        width = new MaximumMatchingWidth();
                        break;
                    case 'b':
                        width = new BooleanWidth();
                        break;
                    default:
                        width = new RankWidth();
                        break;
                }

            Console.WriteLine("Local search duration in seconds? Defaults to 60 seconds.");
            int duration = 0;
            if (!int.TryParse(Console.ReadLine(), out duration))
                duration = 60;

            Console.WriteLine("Seed for the random number generator? Defaults to a random seed.");
            int seed = -1;
            if (!int.TryParse(Console.ReadLine(), out seed))
            {
                Random r = new Random();
                seed = r.Next();
                Console.WriteLine("Seed is " + seed);
            }
            Random rng = new Random(seed);

            Console.WriteLine();
            Console.WriteLine("---------------------- Starting -----------------------");
            Console.WriteLine();

            Console.WriteLine("Constructing initial solution...");
            Stopwatch sw = Stopwatch.StartNew();
            ConstructionHeuristic constructor = new GreedyLinearConstructor(rng);
            DecompositionTree tree = constructor.Construct(graph, width);
            sw.Stop();
            Console.WriteLine($"Finished construction of initial solution in {(sw.ElapsedMilliseconds / 1000).ToString("F2")} seconds.");
            Console.WriteLine($"Initial {width.Name} = {tree.Width.ToString("F2")}");
            Console.WriteLine($"Initial cost = {tree.Cost.ToString("F2")}");

            Console.WriteLine();

            Console.WriteLine("Improving solution with DP...");
            sw.Restart();
            OptimalOrderedTree oot = new OptimalOrderedTree();
            tree = oot.Construct(tree);
            sw.Stop();
            Console.WriteLine($"Finished DP in {(sw.ElapsedMilliseconds / 1000.0).ToString("F2")} seconds.");
            Console.WriteLine($"Resulting {width.Name} = {tree.Width.ToString("F2")}");
            Console.WriteLine($"Resulting cost = {tree.Cost.ToString("F2")}");

            Console.WriteLine();

            Console.WriteLine("Improving solution with local search...");
            LocalSearchAlgorithm localsearch = new VariableNeighborhoodSearch(new DecompositionTree[] { tree }, new LocalSearchOperator[] { new MoveOperator() }, rng);
            localsearch.Run(duration);
            Console.WriteLine($"Finished local search in {localsearch.CurrentComputationTime.ToString("F2")} seconds.");
            Console.WriteLine($"Total iterations = {localsearch.Iterations}");
            Console.WriteLine($"Explored solutions = {localsearch.ExploredSolutions}");
            Console.WriteLine($"Best {width.Name} = {tree.Width.ToString("F2")}");
            Console.WriteLine($"Best cost = {tree.Cost.ToString("F2")}");
            Console.WriteLine("Best decomposition tree:");
            Console.WriteLine(tree.Root);

            Console.WriteLine();
            Console.WriteLine("Cache statistics:");
            Console.WriteLine($"Cache request count = {width.CacheRequests}");
            Console.WriteLine($"Cache hit ratio = {width.CacheHitRatio.ToString("F3")}");

            Console.ReadLine();
        }
    }
}
