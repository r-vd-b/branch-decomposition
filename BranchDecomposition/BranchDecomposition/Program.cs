using System;
using BranchDecomposition.DecompositionTrees;
using BranchDecomposition.WidthParameters;
using BranchDecomposition.ConstructionHeuristics;
using BranchDecomposition.ImprovementHeuristics;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BranchDecomposition
{
    class Program
    {
        public static void Main(string[] args)
        {
            RunUserTest();
        }

        public static void RunUserTest()
        {
            Console.WriteLine("Path to graph file? Defaults to the anna graph from the TreewidthLIB set.");
            string pathToGraph = Console.ReadLine();
            if (pathToGraph.Length == 0)
                pathToGraph = @"..\..\ExampleGraphs\anna.dgf";
            Graph graph = Parser.ParseGraphFromDGF(pathToGraph);

            Console.WriteLine("Width Parameter is (M)aximum-matching, (R)ank or (B)oolean? Defaults to rank-width.");
            WidthParameter width = null;
            string widthParameterString = Console.ReadLine().ToLower();
            if (widthParameterString.Length == 0)
                width = new RankWidth();
            else
                switch (widthParameterString.ToLower()[0])
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
            int localSearchDuration = 0;
            if (!int.TryParse(Console.ReadLine(), out localSearchDuration))
                localSearchDuration = 60;

            Console.WriteLine("Seed for the random number generator? Defaults to a random seed.");
            int seed = -1;
            if (!int.TryParse(Console.ReadLine(), out seed))
            {
                Random r = new Random();
                seed = r.Next();
                Console.WriteLine("Seed is " + seed);
            }
            Random rng = new Random(seed);

            Console.WriteLine("Should reduction rules be applied, (Y)es or (N)o? Defaults to yes.");
            string reductionString = Console.ReadLine().ToLower();
            bool useReductionRules = reductionString.Length == 0 || reductionString[0] != 'n';


            double totalseconds = 0;

            Console.WriteLine();
            Console.WriteLine("---------------------- Starting -----------------------");
            Console.WriteLine();

            Graph[] graphs = null;
            if (useReductionRules)
            {
                int originalvertexcount = graph.Vertices.Count;
                Console.WriteLine($"Graph {Path.GetFileNameWithoutExtension(pathToGraph)} consists of {originalvertexcount} vertices.");
                Console.WriteLine($"Applying the reduction rules...");
                graphs = width.ApplyReductionRules(graph);
                Console.WriteLine($"Number of vertices removed by the reduction rules = {originalvertexcount - graphs.Sum(g => g.Vertices.Count)}");
                Console.WriteLine($"The reduction rules splitted the graph in {graphs.Length} subgraphs of sizes {string.Join(", ", graphs.Select(g => g.Vertices.Count))}.");
            }
            else
                graphs = new Graph[] { graph };

            Console.WriteLine();

            Console.WriteLine("Constructing initial solution...");
            Stopwatch sw = Stopwatch.StartNew();
            ConstructionHeuristic constructor = new GreedyLinearConstructor(rng);
            DecompositionTree[] trees = graphs.Select(g => constructor.Construct(g, width)).ToArray();

            sw.Stop();
            totalseconds += sw.ElapsedMilliseconds / 1000.0;
            Console.WriteLine($"Finished construction of initial solution in {(sw.ElapsedMilliseconds / 1000).ToString("F2")} seconds.");
            Console.WriteLine($"Initial {width.Name} = {string.Join(", ", trees.Select(tree => tree.Width.ToString("F2")))}");
            Console.WriteLine($"Initial cost = {string.Join(", ", trees.Select(tree => tree.Cost.ToString("F2")))}");

            Console.WriteLine();

            Console.WriteLine("Improving solution with DP...");
            sw.Restart();
            OptimalOrderedTree oot = new OptimalOrderedTree();
            trees = trees.Select(tree => oot.Construct(tree)).ToArray();
            sw.Stop();
            totalseconds += sw.ElapsedMilliseconds / 1000.0;
            Console.WriteLine($"Finished DP in {(sw.ElapsedMilliseconds / 1000.0).ToString("F2")} seconds.");
            Console.WriteLine($"Resulting {width.Name} = {string.Join(", ", trees.Select(tree => tree.Width.ToString("F2")))}");
            Console.WriteLine($"Resulting cost = {string.Join(", ", trees.Select(tree => tree.Cost.ToString("F2")))}");

            Console.WriteLine();

            Console.WriteLine("Improving solution with local search...");
            LocalSearchAlgorithm localsearch = new VariableNeighborhoodSearch(trees, new LocalSearchOperator[] { new MoveOperator(), new SwapOperator() }, rng);
            localsearch.Run(localSearchDuration);
            totalseconds += localsearch.CurrentComputationTime;
            Console.WriteLine($"Finished local search in {localsearch.CurrentComputationTime.ToString("F2")} seconds.");
            Console.WriteLine($"Total iterations = {localsearch.Iterations}");
            Console.WriteLine($"Explored solutions = {localsearch.ExploredSolutions}");
            Console.WriteLine($"Best {width.Name} = {string.Join(", ", trees.Select(tree => tree.Width.ToString("F2")))}");
            Console.WriteLine($"Best cost = {string.Join(", ", trees.Select(tree => tree.Cost.ToString("F2")))}");
            Console.WriteLine("Best decomposition trees:");
            for (int i = 0; i < trees.Length; i++)
                Console.WriteLine($"{i}: {trees[i].Root}");

            Console.WriteLine();

            Console.WriteLine("Cache statistics:");
            Console.WriteLine($"    request count = {width.CacheRequests}");
            Console.WriteLine($"    hit ratio = {width.CacheHitRatio.ToString("F3")}");

            Console.WriteLine();

            Console.WriteLine("Write result to disk? (Y)es / (N)o, defaults to Yes.");
            string outputResponseString = Console.ReadLine().ToLower();
            if (outputResponseString.Length == 0 || outputResponseString[0] == 'y')
            {
                Console.WriteLine("Output directory? Defaults to \\output.");
                string outputDirectory = Console.ReadLine();
                if (outputDirectory.Length == 0)
                    outputDirectory = "output";
                Console.WriteLine($"Stored the result as {WriteResult(pathToGraph, outputDirectory, trees, seed, totalseconds)}");
            }
            else
                Console.ReadLine();
        }

        public static string WriteResult(string inputPath, string outputDirectory, DecompositionTree[] trees, int seed, double computationTime)
        {
            Directory.CreateDirectory(outputDirectory);
            string graph = Path.GetFileNameWithoutExtension(inputPath);
            string outputPath = Path.Combine(outputDirectory, $"{graph}_{trees[0].WidthParameter.Name}_{trees.Max(tree => tree.Width).ToString("F2")}.txt");

            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                writer.WriteLine($"Parameter = {trees[0].WidthParameter.Name}");
                writer.WriteLine($"Seed = {seed}");
                writer.WriteLine($"Graph = {graph}");
                writer.WriteLine($"Seconds to construct the trees = {computationTime.ToString("F2")}");
                writer.WriteLine();
                writer.WriteLine("Trees: ");
                for (int i = 0; i < trees.Length; i++)
                {
                    DecompositionTree tree = trees[i];
                    Console.WriteLine("    " + i);
                    writer.WriteLine($"    Tree = {tree.Root}");
                    writer.WriteLine($"    Width of tree = {tree.Width.ToString("F3")}");
                    writer.WriteLine($"    Cost of tree = {tree.Cost.ToString("F0")}");
                    writer.WriteLine();
                }
            }

            return Path.GetFullPath(outputPath);
        }
    }
}
