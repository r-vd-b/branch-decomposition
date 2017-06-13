﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition
{
    public static class Extensions
    {
        /// <summary>
        /// Returns the first item in the list that attains the minimum value provide by the selector function.
        /// </summary>
        /// <typeparam name="T1">The type of items in the list.</typeparam>
        /// <typeparam name="T">The type used to compare the items.</typeparam>
        /// <param name="elements">The IList of elements of type T1.</param>
        /// <param name="selector">The function that maps each element of type T1 to a value of type T.</param>
        /// <returns>The element of the list that has the minimum selector value.</returns>
        public static T1 MinItem<T1, T>(this IList<T1> elements, Func<T1, T> selector) where T : IComparable
        {
            bool first = true;
            T min = default(T);
            T1 item = default(T1);

            for (int i = 0; i < elements.Count; i++)
            {
                T1 element = elements[i];
                if (first)
                {
                    min = selector(element);
                    item = element;
                    first = false;
                }
                else
                {
                    T value = selector(element);
                    if (value.CompareTo(min) == -1)
                    {
                        min = value;
                        item = element;
                    }
                }
            }

            return item;
        }

        /// <summary>
        /// Returns the first item in the list that attains the maximum value provide by the selector function.
        /// </summary>
        /// <typeparam name="T1">The type of items in the list.</typeparam>
        /// <typeparam name="T">The type used to compare the items.</typeparam>
        /// <param name="elements">The IList of elements of type T1.</param>
        /// <param name="selector">The function that maps each element of type T1 to a value of type T.</param>
        /// <returns>The element of the list that has the maximum selector value.</returns>
        public static T1 MaxItem<T1, T>(this IList<T1> elements, Func<T1, T> selector) where T : IComparable
        {
            bool first = true;
            T max = default(T);
            T1 item = default(T1);

            for (int i = 0; i < elements.Count; i++)
            {
                T1 element = elements[i];
                if (first)
                {
                    max = selector(element);
                    item = element;
                    first = false;
                }
                else
                {
                    T value = selector(element);
                    if (value.CompareTo(max) == 1)
                    {
                        max = value;
                        item = element;
                    }
                }
            }

            return item;
        }

        public static int MinIndex<T>(this IList<T> elements) where T : IComparable<T>
        {
            T min = default(T);
            int index = -1;

            for (int i = 0; i < elements.Count; i++)
            {
                T current = elements[i];
                if (i == 0 || current.CompareTo(min) == -1)
                {
                    min = current;
                    index = i;
                }
            }
            return index;
        }

        public static int MinIndex<T, T1>(this IList<T> elements, Func<T, T1> selector) where T1 : IComparable<T1>
        {
            T1 min = default(T1);
            int index = -1;

            for (int i = 0; i < elements.Count; i++)
            {
                T1 current = selector(elements[i]);
                if (i == 0 || current.CompareTo(min) == -1)
                {
                    min = current;
                    index = i;
                }
            }
            return index;
        }

        public static int MaxIndex<T>(this IList<T> elements) where T : IComparable<T>
        {
            T max = default(T);
            int index = -1;

            for (int i = 0; i < elements.Count; i++)
            {
                T current = elements[i];
                if (i == 0 || current.CompareTo(max) == 1)
                {
                    max = current;
                    index = i;
                }
            }
            return index;
        }

        public static int MaxIndex<T, T1>(this IList<T> elements, Func<T, T1> selector) where T1 : IComparable<T1>
        {
            T1 max = default(T1);
            int index = -1;

            for (int i = 0; i < elements.Count; i++)
            {
                T1 current = selector(elements[i]);
                if (i == 0 || current.CompareTo(max) == 1)
                {
                    max = current;
                    index = i;
                }
            }
            return index;
        }

        public static int BinarySearchIndexOf<T>(this IList<T> list, T value, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            comparer = comparer ?? Comparer<T>.Default;

            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = comparer.Compare(value, list[middle]);
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return -1;
        }

        public static int BinarySearchClosestIndexOf<T>(this IList<T> list, T value, bool up = false, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            comparer = comparer ?? Comparer<T>.Default;

            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = comparer.Compare(value, list[middle]);
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            if (up)
                return Math.Min(lower, list.Count - 1);
            else
                return Math.Max(upper, 0);
        }

        public static IList<T> Shuffle<T>(this IList<T> elements, Random random)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                int index = random.Next(i);
                T temp = elements[index];
                elements[index] = elements[i];
                elements[i] = temp;
            }
            return elements;
        }
    }
}
