﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MobileMessageDecoder
{
    class ParallelSolver
    {
        FileProcesser fp;

        public ParallelSolver(FileProcesser fp)
        {
            this.fp = fp;
        }

        public List<string> DecodeNumberMessage(string messageInNumbers)
        {
            Console.WriteLine("Message size: {0}", messageInNumbers.Length);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            BlockingCollection<string[]> searchSpace = GetAllDivisons(messageInNumbers);

            Console.WriteLine("Search space generated in: {0} ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            BlockingCollection<string[]> solutions = searchSpace;

            //int i;
            //List<string> oneSolution;
            //foreach (var solutionCandidate in searchSpace)
            //{
            //    i = 0;
            //    while (i < solutionCandidate.Length && fp.WordsDict.ContainsKey(solutionCandidate[i]))
            //    {
            //        i++;

            //    }
            //    if (i >= solutionCandidate.Length)
            //    {
            //        oneSolution = new List<string>();
            //        foreach (var scpart in solutionCandidate)
            //        {
            //            oneSolution.Add(scpart);
            //        }

            //        solutions.Add(oneSolution);
            //    }
            //    else {
            //        Console.WriteLine(i);
            //        Console.WriteLine(solutionCandidate);
            //    }
            //}

            Console.WriteLine("Validate search candidates: {0} ms", stopwatch.ElapsedMilliseconds);
            var csd =  combinationTimes.Select(y => new { n= int.Parse(y.Split(';')[0]), ms = y.Split(';')[1] }).OrderBy(x => x.n).ToList();
            foreach (var item in csd)
            {
                Console.WriteLine(messageInNumbers.Length+" choose "+ item.n+"\t "+item.ms+" ms");
            }

            stopwatch.Restart();
            var ret = EncodeMessage(solutions);
            Console.WriteLine("Encode possible solutions to messages in: {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Search space size: {0}", searchSpace.Count);
            Console.WriteLine("Solution candidates size: {0}", solutions.Count);
            Console.WriteLine("All solution messages count: {0}", ret.Count);
            fp.WriteTofilePath = "solutions";
            fp.WriteToFile(ret);
            return ret;
        }

        private List<string> EncodeMessage(BlockingCollection<string[]> possibilities)
        {
            List<string> encoded = new List<string>();
            foreach (var list in possibilities)
            {
                List<string> others = new List<string>();

                int[] allCounts = new int[list.Count()];
                for (int i = 0; i < list.Count(); i++)
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

        public BlockingCollection<string[]> GetAllDivisons(string messageNumbers)
        {
            BlockingCollection<int[]> allDividers = new BlockingCollection<int[]>();
            BlockingCollection<string[]> allDivisions = new BlockingCollection<string[]>();

            Parallel.For(1, messageNumbers.Length, i =>
            {
                var c = Combinations(messageNumbers.Length - 1, i);
                foreach (var curr in c)
                {
                    allDividers.TryAdd(curr);

                }
            });

            // fileprocesserre innentol van szukseg
            //versenyhelyzet??
            Parallel.ForEach(allDividers, oneDivider =>
            {
                var c = SplitAt(messageNumbers, oneDivider);
                if (c != null)
                {
                    allDivisions.TryAdd(c);
                }
            });

            if (fp.WordsDict.ContainsKey(messageNumbers))
                allDivisions.TryAdd(new string[] { messageNumbers });

            return allDivisions;
        }


        private string[] SplitAt(string source, int[] index)
        {
            Array.Sort(index);
            string[] output = new string[index.Length + 1];
            int pos = 0;

            for (int i = 0; i < index.Length; i++)
            {

                output[i] = source.Substring(pos, index[i] - pos);
                if (!fp.WordsDict.ContainsKey(output[i]))
                {
                    return null;
                }
                pos = index[i];
            }
            output[index.Length] = source.Substring(pos);
            if (!fp.WordsDict.ContainsKey(output[index.Length]))
            {
                return null;
            }
            return output;
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

        private static object lockobject = new object();
        List<string> combinationTimes = new List<string>();
        //https://www.videlin.eu/2016/04/10/how-to-generate-combinations-without-repetition-interatively-in-c/
        private List<int[]> Combinations(int n, int k)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            List<int[]> combinations = new List<int[]>();
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
                        var f = new int[k];
                        for (int i = 0; i < k; i++)
                        {
                            f[i] = result[i];
                        }
                        combinations.Add(f);
                        break;
                    }
                }
            }
            //combinationTimes.Add(String.Format(" {1} combinations:\t{2} ms", n, k, s.ElapsedMilliseconds));
            lock (lockobject)
            {
                combinationTimes.Add(String.Format("{0};{1}", k, s.ElapsedMilliseconds));
            }
            return combinations;
        }
    }
}
