using BranchDecomposition.DecompositionTrees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.ImprovementHeuristics
{
    /// <summary>
    /// A generic local search framework.
    /// </summary>
    abstract class LocalSearchAlgorithm
    {
        public const double PRECISION = 0.0001;

        public int MaximumComputationTime { get; protected set; }
        public double CurrentComputationTime { get { return this.stopwatch.ElapsedMilliseconds / 1000.0; } }
        public int Iterations { get; protected set; }
        public int ExploredSolutions { get; protected set; }

        protected LocalSearchGraphSolution[] solutions { get; }
        protected LocalSearchOperator[] operators { get; }
        protected Random randomNumberGenerator { get; }
        protected int indexOfFocus { get; set; }
        protected LocalSearchGraphSolution focus { get { return this.solutions[this.indexOfFocus]; } }
        protected double cost { get { return this.focus.CurrentSolution.Cost; } }

        private Stopwatch stopwatch = new Stopwatch();

        public LocalSearchAlgorithm(IEnumerable<DecompositionTree> initial, LocalSearchOperator[] operators, Random rng)
        {
            this.operators = operators;
            this.randomNumberGenerator = rng;
            this.solutions = new LocalSearchGraphSolution[initial.Count()];
            int index = 0;
            foreach (var tree in initial)
                this.solutions[index++] = new LocalSearchGraphSolution(tree.Graph, tree);
            this.indexOfFocus = this.solutions.MaxIndex(sol => sol.CurrentSolution.Cost);
        }

        /// <summary>
        /// Start the local search procedure.
        /// </summary>
        /// <param name="duration">The duration of the search. Negative values indicate infinity running time.</param>
        /// <returns>The best solution for each provided initial tree.</returns>
        public virtual DecompositionTree[] Run(int duration = -1)
        {
            this.MaximumComputationTime = duration;
            this.ExploredSolutions = this.Iterations = 0;
            this.startTimer();
            while (!(this.MaximumComputationTime > 0 && this.CurrentComputationTime > this.MaximumComputationTime))
            {
                this.Iterations++;
                if (!this.PerformIteration())
                    break;
            }
            this.stopTimer();
            return this.solutions.Select(sol => sol.Revert()).ToArray();
        }

        /// <summary>
        /// Perform a single iteration of the local search.
        /// </summary>
        /// <returns>Should the search continue?</returns>
        protected virtual bool PerformIteration()
        {
            LocalSearchOperation candidate = this.GetCandidate();

            if (candidate == null)
                return false;
            
            this.focus.PerformOperation(candidate);
            this.UpdateFocus();
            Console.WriteLine($"{this.focus.CurrentSolution.Width.ToString("N2")} {this.focus.CurrentSolution.Cost.ToString("N0")}");
            return true;
        }

        /// <summary>
        /// Returns the candidate neighbor.
        /// </summary>
        protected abstract LocalSearchOperation GetCandidate();

        /// <summary>
        /// Sets the focus of the algorithm on the tree with the highest width.
        /// </summary>
        /// <returns>The new focus of the search.</returns>
        protected virtual LocalSearchGraphSolution UpdateFocus()
        {
            this.indexOfFocus = this.solutions.MaxIndex(sol => sol.BestWidth);
            return this.focus;
        }

        protected void startTimer()
        {
            this.stopwatch.Restart();
        }

        protected void stopTimer()
        {
            this.stopwatch.Stop();
        }
    }

    /// <summary>
    /// A simple first-improvement hillclimber.
    /// </summary>
    class FirstImprovementHillClimber : LocalSearchAlgorithm
    {
        public FirstImprovementHillClimber(IEnumerable<DecompositionTree> initial, LocalSearchOperator[] operators, Random rng) : base(initial, operators, rng) { }

        protected override LocalSearchOperation GetCandidate()
        {
            foreach (var op in this.operators)
                foreach (var operation in op.Operations(this.focus.CurrentSolution, this.randomNumberGenerator))
                {
                    this.ExploredSolutions++;
                    if (operation.Cost - this.cost < -PRECISION)
                        return operation;
                }
            return null;
        }
    }

    /// <summary>
    /// A simple best-improvement hillclimber.
    /// </summary>
    class BestImprovementHillClimber : LocalSearchAlgorithm
    {
        public BestImprovementHillClimber(IEnumerable<DecompositionTree> initial, LocalSearchOperator[] operators, Random rng) : base(initial, operators, rng) { }

        protected override LocalSearchOperation GetCandidate()
        {
            LocalSearchOperation candidate = null;
            double cost = this.cost;
            foreach (var op in this.operators)
                foreach (var operation in op.Operations(this.focus.CurrentSolution, this.randomNumberGenerator))
                {
                    this.ExploredSolutions++;
                    if (operation.Cost - cost < -PRECISION)
                    {
                        candidate = operation;
                        cost = candidate.Cost;
                    }
                }
            return candidate;
        }
    }

    /// <summary>
    /// An implementation of the Variable Neighborhood Search meta-heuristic.
    /// </summary>
    class VariableNeighborhoodSearch : FirstImprovementHillClimber
    {
        public int MinimumPerturbationSize { get; } = 2;
        public int MaximumPerturbationSize { get; } = 8;
        public int Failures { get; protected set; } = 0;
        public int CurrentPerturbationSize { get { return Math.Min(this.MaximumPerturbationSize, this.MinimumPerturbationSize + this.Failures); } }

        public VariableNeighborhoodSearch(IEnumerable<DecompositionTree> initial, LocalSearchOperator[] operators, Random rng) : base(initial, operators, rng) { }            

        public override DecompositionTree[] Run(int duration = -1)
        {
            this.Failures = 0;
            return base.Run(duration);
        }

        protected override bool PerformIteration()
        {
            LocalSearchOperation candidate = this.GetCandidate();

            // If no better neighbor is found
            if (candidate == null)
            {
                // Revert to the best solution so far.
                // Note: reverting is not necessary, it might be better to remove it.
                Console.WriteLine($"Perturbation of size {this.CurrentPerturbationSize} after {this.Failures + 1} failures; best = {this.focus.BestCost.ToString("F2")}");
                if (this.focus.CurrentSolution.Cost > this.focus.BestCost)
                    this.focus.Revert();

                // Perturb the solution by taking a number of random steps.
                for (int i = 0; i < this.CurrentPerturbationSize; i++)
                {
                    LocalSearchOperation operation = this.operators[this.randomNumberGenerator.Next(this.operators.Length)].GetRandomOperation(this.focus.CurrentSolution, this.randomNumberGenerator);
                    this.focus.PerformOperation(operation);
                }

                Console.WriteLine($"Result of perturbation: {this.focus.CurrentSolution.Width.ToString("F2")} {this.focus.CurrentSolution.Cost.ToString("F0")}");
                this.Failures++;
            }
            else
            {
                if (this.focus.PerformOperation(candidate))
                    this.Failures = 0;
                this.UpdateFocus();
                Console.WriteLine($"{this.focus.CurrentSolution.Width.ToString("F2")} {this.focus.CurrentSolution.Cost.ToString("F0")}");
            }

            return true;
        }
    }

    /// <summary>
    /// An implementation of the Simulated Annealing meta-heuristic.
    /// </summary>
    class SimulatedAnnealing : LocalSearchAlgorithm
    {
        public int FailedIterationsUntilReset { get; } = 1000;

        protected const double ALPHA = 0.97;
        protected double[] Ts { get; }
        protected double[] Qs { get; }
        protected double targetT { get; }
        protected double T { get { return this.Ts[this.indexOfFocus]; } set { this.Ts[this.indexOfFocus] = value; } }
        protected double Q { get { return this.Qs[this.indexOfFocus]; } set { this.Qs[this.indexOfFocus] = value; } }
        protected double[] cumulativeOperatorProbabilities { get; }

        protected double previousCooling { get; set; }
        protected int iterationsWithoutImprovement { get; set; }

        public SimulatedAnnealing(IEnumerable<DecompositionTree> initial, LocalSearchOperator[] operators, Random rng, double initialT, double targetT, double[] probabilities = null) : base(initial, operators, rng)
        {
            int treecount = initial.Count();
            this.Ts = new double[treecount];
            this.Qs = new double[treecount];
            for (int i = 0; i < treecount; i++)
                this.Ts[i] = initialT;
            this.targetT = targetT;

            this.cumulativeOperatorProbabilities = new double[operators.Length];
            if (probabilities == null)
            {
                for (int i = 0; i < operators.Length; i++)
                    this.cumulativeOperatorProbabilities[i] = (i + 1) * 1.0 / operators.Length;
            }
            else
            {
                for (int i = 0; i < operators.Length; i++)
                {
                    this.cumulativeOperatorProbabilities[i] = probabilities[i];
                    if (i > 0)
                        this.cumulativeOperatorProbabilities[i] += this.cumulativeOperatorProbabilities[i - 1];
                }
            }
        }

        public override DecompositionTree[] Run(int duration = -1)
        {
            this.previousCooling = 0;
            this.iterationsWithoutImprovement = 0;
            this.Q = this.computeCoolingInterval(this.T, this.targetT, duration);
            return base.Run(duration);
        }

        protected override bool PerformIteration()
        {   
            if (this.CurrentComputationTime - this.previousCooling > this.Q)
            {
                this.previousCooling = this.CurrentComputationTime;
                this.T *= ALPHA;
            }

            LocalSearchOperation candidate = this.GetCandidate();
            if (candidate.Cost < this.focus.CurrentSolution.Cost)
            {
                if (candidate.Cost < this.focus.BestCost)
                {
                    this.iterationsWithoutImprovement = 0;
                    Console.WriteLine($"{this.CurrentComputationTime.ToString("N0")}: {this.focus.BestWidth.ToString("N2")} {candidate.Cost.ToString("N0")} | T={this.T.ToString("N2")}");
                }
                this.focus.PerformOperation(candidate);
                var focus = this.focus;
                if (focus != this.UpdateFocus())
                    this.Q = this.computeCoolingInterval(this.T, this.targetT, (int)(this.MaximumComputationTime - this.CurrentComputationTime));
                //Console.WriteLine($"{this.CurrentComputationTime.ToString("N0")}: {this.focus.CurrentSolution.Width.ToString("N2")} {this.focus.CurrentSolution.Cost.ToString("N0")} | T={this.T.ToString("N2")}");
            }
            else
            {
                if (++this.iterationsWithoutImprovement > this.FailedIterationsUntilReset)
                {
                    this.focus.Revert();
                    this.iterationsWithoutImprovement = 0;
                    Console.WriteLine($"Reset");
                }
                else if (this.acceptDeterioration(candidate.Cost - this.focus.CurrentSolution.Cost, this.T))
                {
                    //Console.WriteLine($"Accepted deterioration of size {(candidate.Cost - this.focus.CurrentSolution.Cost).ToString("N0")}, rejection {this.iterationsWithoutImprovement}");
                    this.focus.PerformOperation(candidate);
                }
            }

            return true;
        }

        protected override LocalSearchOperation GetCandidate()
        {
            this.ExploredSolutions++;
            return this.getOperator().GetRandomOperation(this.focus.CurrentSolution, this.randomNumberGenerator);
        }

        protected double computeCoolingInterval(double initialT, double targetT, int maximumComputationTime)
        {
            // targetT = initialT * alpha^(maximumComputationTime / Q)
            return maximumComputationTime / Math.Log(targetT / initialT, ALPHA);
        }

        protected bool acceptDeterioration(double deterioration, double T)
        {
            return this.randomNumberGenerator.NextDouble() < Math.Exp(-Math.Max(10, deterioration) / T);
        }

        protected LocalSearchOperator getOperator()
        {
            double sample = this.randomNumberGenerator.NextDouble();
            for (int i = 0; i < this.cumulativeOperatorProbabilities.Length; i++)
                if (sample <= this.cumulativeOperatorProbabilities[i])
                    return this.operators[i];
            return this.operators[this.operators.Length - 1];
        }
    }
}
