using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileMessageDecoder
{

    public static class StringExtensions
    {
        //stackoverflow <3
        /// <summary>
        ///     Returns a string array that contains the substrings in this instance that are delimited by specified indexes.
        /// </summary>
        /// <param name="source">The original string.</param>
        /// <param name="index">An index that delimits the substrings in this string.</param>
        /// <returns>An array whose elements contain the substrings in this instance that are delimited by one or more indexes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="index" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">An <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
        public static string[] SplitAt(this string source, params int[] index)
        {
            Array.Sort(index);
            string[] output = new string[index.Length + 1];
            int pos = 0;

            for (int i = 0; i < index.Length; pos = index[i++])
                output[i] = source.Substring(pos, index[i] - pos);

            output[index.Length] = source.Substring(pos);
            return output;
        }
    }


    class SequentialSolver
    {

        //Dictionary<string, string> wordsDict;
        FileProcesser fp;

        public SequentialSolver(FileProcesser processer)
        {
            fp = processer;

        }

        public List<string> DecodeNumberMessage(string messageInNumbers)
        {
            //List<List<string>> searchSpace =GetAllDivisons(messageInNumbers);

            ////StreamReader sr = new StreamReader(fp.WriteTofilePath);

            //List<List<string>> possibleSolutions = new List<List<string>>();
            //while (!sr.EndOfStream)
            //{
            //    string oneSub = sr.ReadLine();
            //    var subs = oneSub.Split('|');
            //    int i = 0;
            //    List<string> onemessage = new List<string>();
            //    while (i < subs.Count() && fp.WordsDict.ContainsKey(subs[i]))
            //    {
            //        onemessage.Add(subs[i]);
            //        i++;
            //    }
            //    if (i >= subs.Count())
            //    {
            //        possibleSolutions.Add(onemessage);
            //    }
            //}
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<List<string>> searchSpace = GetAllDivisons(messageInNumbers);
            Console.WriteLine("All divisions computed in: {0} ms",stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
            List<List<string>> solutions = new List<List<string>>();

            int i;
            List<string> oneSolution;
            foreach (var solutionCandidate in searchSpace)
            {
                i = 0;
                while (i < solutionCandidate.Count && fp.WordsDict.ContainsKey(solutionCandidate[i]))
                {
                    i++;
                }
                if (i >= solutionCandidate.Count)
                {
                    oneSolution = new List<string>();
                    solutionCandidate.ForEach(x => oneSolution.Add(x));
                    solutions.Add(oneSolution);
                }
            }
            Console.WriteLine("Finding solutions in all search space: {0} ms",stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Search space size: {0}",searchSpace.Count);
            Console.WriteLine("Solution candidates size: {0}",solutions.Count);
            stopwatch.Restart();
            var ret=  EncodeMessage(solutions);
            Console.WriteLine("Encode possible solutions to messages in: {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("All solution messages count: {0}", ret.Count);
            return ret;
        }

        private List<string> EncodeMessage(List<List<string>> possibilities)
        {
            List<string> encoded = new List<string>();
            foreach (var list in possibilities)
            {
                List<string> others = new List<string>();


                int[] allCounts = new int[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    allCounts[i] = fp.WordsDict[list[i]].Count;
                }

                List<int[]> allPermutations = Permutations(allCounts);
                for (int j = 0; j < allPermutations.Count; j++)
                {
                    string one = "";
                    for (int k = 0; k < allPermutations[j].Length; k++) //ezt kell a list-el együtt indexelni
                    {
                        int dictListIdx = allPermutations[j][k]; //j-ik permutáció k-ik eleme a szólista indexe
                        one += fp.WordsDict[list[k]][dictListIdx] + " ";
                    }
                    others.Add(one);
                }
                encoded.AddRange(others);
            }
            return encoded;
        }

        public void WriteAllDivisons(string messageNumbers)
        {

            for (int i = 1; i < messageNumbers.Length; i++)
            {
                var combs = GetCombinationsWithXSeparators(i, messageNumbers);
                combs.ForEach(x => fp.WriteToFile(x));
            }
            fp.WriteToFile(new List<string>() { messageNumbers });
            fp.Writer.Close();
        }

        public List<List<string>> GetAllDivisons(string messageNumbers)
        {
            List<List<string>> allDivisions = new List<List<string>>();
            for (int i = 1; i < messageNumbers.Length; i++)
            {
                allDivisions.AddRange( GetCombinationsWithXSeparators(i, messageNumbers));
            }
            allDivisions.Add(new List<string>() { messageNumbers });

            return allDivisions;
        }

        private List<List<string>> GetCombinationsWithXSeparators(int sepCount, string numbers)
        {
            List<List<string>> divisions = new List<List<string>>();
            var combinations = Combinations(numbers.Length - 1, sepCount);
            foreach (var oneCombination in combinations)
            {
                //fp.WriteToFile(numbers.SplitAt(oneCombination));
                divisions.Add(numbers.SplitAt(oneCombination).ToList());
            }
            return divisions;

        }

        private List<int[]> Permutations(int[] set)
        {
            List<int[]> list = new List<int[]>();
            int allPermutations = 1;
            for (int i = 0; i < set.Length; i++)
            {
                allPermutations *= set[i];
            }
            for (int j = 0; j < allPermutations; j++)
            {
                int[] temp = new int[set.Length];
                int divider = 1;

                for (int k = 0; k < set.Length; k++)
                {
                    if (k == 0)
                    {
                        temp[k] = j % (set[k]);
                    }
                    else
                    {
                        divider *= (set[k - 1]);
                        temp[k] = ((int)j / divider) % (set[k]);
                    }
                }
                list.Add(temp);
            }
            return list;


        }


        //https://www.videlin.eu/2016/04/10/how-to-generate-combinations-without-repetition-interatively-in-c/
        private IEnumerable<int[]> Combinations(int n, int k)
        {
            var result = new int[k];
            var stack = new Stack<int>();
            stack.Push(1);

            while (stack.Count > 0)
            {
                var index = stack.Count - 1;
                var value = stack.Pop();

                while (value <= n)
                {
                    result[index++] = value++;
                    stack.Push(value);
                    if (index == k)
                    {
                        yield return result;
                        break;
                    }
                }
            }
        }



    }
}
