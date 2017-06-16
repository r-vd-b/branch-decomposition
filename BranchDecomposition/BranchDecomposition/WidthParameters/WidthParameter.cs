using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.WidthParameters
{
    /// <summary>
    /// The WidthParameter class models a symmetric f-width set-function that assigns a real value to each 2-partition of a graph.
    /// </summary>
    abstract class WidthParameter
    {
        protected WidthCache Cache;

        public WidthParameter()
        {
            this.Cache = new WidthCache();
        }

        /// <summary>
        /// Get the f-width of a cut.
        /// </summary>
        /// <param name="graph">The graph that is partitioned.</param>
        /// <param name="left">The elements in one partition.</param>
        /// <param name="right">The elements in the other partion.</param>
        /// <returns>The f-width of the cut.</returns>
        public virtual double GetWidth(Graph graph, BitSet left, BitSet right = null)
        {
            double width = -1;
            // First, check if the cache already contains the cut.
            if (!this.Cache.TryGetValue(graph, left, out width))
            {
                // If the right side of the 2-partition is not provide, construct it.
                if (right == null)
                    right = ~left;
                // Compute the f-width.
                width = computeWidth(graph, left, right);
                // Store it for both partitions.
                this.Cache.Add(graph, new BitSet(left), width);
                this.Cache.Add(graph, new BitSet(right), width);
            }
            return width;
        }

        /// <summary>
        /// Computes the f-width of a cut.
        /// </summary>
        /// <param name="graph">The graph that is partitioned.</param>
        /// <param name="left">The elements in one partition.</param>
        /// <param name="right">The elements in the other partion.</param>
        /// <returns>The f-width of the cut.</returns>
        protected abstract double computeWidth(Graph graph, BitSet left, BitSet right);

        /// <summary>
        /// The WidthCache class is used to cache the width of cuts.
        /// </summary>
        protected class WidthCache
        {
            private Dictionary<Graph, Dictionary<BitSet, Entry>> graphcache = new Dictionary<Graph, Dictionary<BitSet, Entry>>();
            private LinkedList<Entry> history = new LinkedList<Entry>();
            
            public int Hits { get; private set; }
            public int Requests { get; private set; }
            public double HitRatio { get { return this.Requests == 0 ? 0 : this.Hits / (double)this.Requests; } }
            public double AverageEntryAccesCount { get { return this.history.Average(entry => entry.AccessCount); } }
            
            /// <summary>
            /// The maximum size of the cache.
            /// </summary>
            public int MaximumSize { get; }
            /// <summary>
            /// The current size of the cache.
            /// </summary>
            public int Size { get { return this.graphcache.Count; } }

            public WidthCache(int maxcachesize = 1000000)
            {
                this.MaximumSize = maxcachesize;
            }

            /// <summary>
            /// Retrieves the value for a certain key and updates the access history.
            /// </summary>
            /// <param name="hashmap">The collection of (key, value)-pairs.</param>
            /// <param name="history">The collection access history.</param>
            /// <param name="key">The bitstring.</param>
            /// <param name="value">The corresponding value.</param>
            /// <returns>Whether the retrieval was successful.</returns>
            public bool TryGetValue(Graph graph, BitSet key, out double value)
            {
                this.Requests++;
                Dictionary<BitSet, Entry> graphcache = null;
                bool success = this.graphcache.TryGetValue(graph, out graphcache);
                Entry entry = default(Entry);
                if (success)
                    success = graphcache.TryGetValue(key, out entry);

                if (success)
                {
                    value = entry.Value;
                    this.history.Remove(entry.Node);
                    this.history.AddLast(entry.Node);
                    entry.AccessCount++;
                    this.Hits++;
                }
                else
                    value = -1;
                return success;
            }

            /// <summary>
            /// Adds a (key, value)-pair to the collection.
            /// </summary>
            /// <param name="hashmap">The collection of (key, value)-pairs.</param>
            /// <param name="history">The collection access history.</param>
            /// <param name="key">The bitstring.</param>
            /// <param name="value">The corresponding value.</param>
            public void Add(Graph graph, BitSet key, double value)
            {
                Dictionary<BitSet, Entry> graphcache = null;
                if (!this.graphcache.TryGetValue(graph, out graphcache))
                    graphcache = this.graphcache[graph] = new Dictionary<BitSet, Entry>();

                Entry entry = graphcache[key] = new Entry(graph, key, value);
                entry.Node = this.history.AddLast(entry);
                if (this.history.Count > this.MaximumSize)
                {
                    this.graphcache[history.First.Value.Graph].Remove(history.First.Value.Key);
                    this.history.RemoveFirst();
                }
            }

            public override string ToString()
            {
                return $"Size = {this.history.Count}, maximum size = {this.MaximumSize}, received requests = {this.Requests}, hit ratio = {this.HitRatio.ToString("N2")}";
            }

            private class Entry
            {
                public Graph Graph { get; }
                public BitSet Key { get; }
                public double Value { get; }
                public int AccessCount { get; set; }
                public LinkedListNode<Entry> Node { get; set; }

                public Entry(Graph graph, BitSet key, double value)
                {
                    this.Graph = graph;
                    this.Key = key;
                    this.Value = value;
                }

                public override string ToString()
                {
                    return $"({this.Key}, {this.Value}) has been accessed {this.AccessCount} times";
                }
            }
        }
    }
}
