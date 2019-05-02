using System;
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
        BlockingCollection<string[]> solutionCandidates;
        BlockingCollection<string> decodedSolutions;
        BlockingCollection<int[]> allDividers;

        public ParallelSolver(FileProcesser fp)
        {
            solutionCandidates = new BlockingCollection<string[]>();
            decodedSolutions = new BlockingCollection<string>();
            allDividers = new BlockingCollection<int[]>();
            this.fp = fp;
        }

        private void Worker(List<int> k, string message)
        {
            foreach (var item in k)
            {
                var dividers = Combinations(message.Length-1, item);
                List<string[]> solutions = new List<string[]>();

                foreach (var c in dividers)
                {
                    var sp = SplitAt(message, c);
                    if (sp != null)
                    {
                        solutions.Add(sp);
                    }
                }
                
                foreach (var s in solutions)
                {
                    /*var decodedmsg = */
                    SetDecodedMessage(s);
                    //foreach (var msg in decodedmsg)
                    //{
                    //    decodedSolutions.TryAdd(msg);
                    //}
                }

            }
        }

        private List<List<int>> LoadBalancing(int N, int taskCount)
        {
            List<List<int>> divideInit = new List<List<int>>();

            for (int i = 0; i < taskCount; i++)
            {
                divideInit.Add(new List<int>());
            }

            int x = 0;
            for (int i = 1; i < N; i++)
            {
                if (x >= taskCount)
                    x = 0;

                divideInit[x].Add(i);
                x++;
            }

            return divideInit;
        }

        public async Task<BlockingCollection<string>> DecodeNumberMessageWithLoadBalancing(string messageNumbers, int taskCount)
        {
            //get dividers
            int N = messageNumbers.Length;

            List<Task> taskList = new List<Task>();
            var divideInit = LoadBalancing(N, taskCount);

            foreach (var item in divideInit)
            {
                taskList.Add(
                    new Task(() =>
                    {
                        Worker(item, messageNumbers);
                    }
                    ));
            }

            foreach (var item in taskList)
            {
                item.Start();
            }


            await Task.WhenAll(taskList).ContinueWith(
               x =>
               {
                   Console.WriteLine("Its done");
                   FileProcesser.WriteToFile(decodedSolutions, "sajatLoadBalanceres.txt");

               });

            return decodedSolutions;
        }



        public BlockingCollection<string> DecodeNumberMessageWithLoopParallelism(string messageInNumbers)
        {
            Console.WriteLine("Message size: {0}", messageInNumbers.Length);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SetAllDividers(messageInNumbers);
            SetAllDivisions(messageInNumbers);

            Console.WriteLine("Search space generated in: {0} ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            SetAllDecodedMessages(solutionCandidates);
            Console.WriteLine("Encode possible solutions to messages in: {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Search space size: {0}", allDividers.Count);
            Console.WriteLine("Solution candidates size: {0}", solutionCandidates.Count);
            Console.WriteLine("All solution messages count: {0}", decodedSolutions.Count);

            return decodedSolutions;
        }

        private void CombinationTimesToConsole(string messageInNumbers)
        {
            var csd = combinationTimes.Select(y => new { n = int.Parse(y.Split(';')[0]), ms = y.Split(';')[1] }).OrderBy(x => x.n).ToList();
            foreach (var item in csd)
            {
                Console.WriteLine(messageInNumbers.Length + " choose " + item.n + "\t " + item.ms + " ms");
            }
        }

        private void SetAllDecodedMessages(IEnumerable<string[]> numCodedMessages)
        {
            Parallel.ForEach(numCodedMessages, nums => {
                SetDecodedMessage(nums);
            });
        } 

        private void SetDecodedMessage(string[] numCodedMessage)
        {
            int[] allCounts = new int[numCodedMessage.Count()];
            for (int i = 0; i < numCodedMessage.Count(); i++)
            {
                allCounts[i] = fp.WordsDict[numCodedMessage[i]].Count;
            }

            List<int[]> allPermutations = Permutations(allCounts);
            for (int j = 0; j < allPermutations.Count; j++)
            {
                string one = "";
                for (int k = 0; k < allPermutations[j].Length; k++) //ezt kell a list-el együtt indexelni
                {
                    int dictListIdx = allPermutations[j][k]; //j-ik permutáció k-ik eleme a szólista indexe
                    one += fp.WordsDict[numCodedMessage[k]][dictListIdx] + " ";
                }
                decodedSolutions.TryAdd(one);
            }
        }

        //private BlockingCollection<string> DecodeMessages(IEnumerable<string[]> possibilities)
        //{
        //    BlockingCollection<string> encoded = new BlockingCollection<string>();
        //    foreach (var list in possibilities)
        //    {

        //        int[] allCounts = new int[list.Count()];
        //        for (int i = 0; i < list.Count(); i++)
        //        {
        //            allCounts[i] = fp.WordsDict[list[i]].Count;
        //        }

        //        List<int[]> allPermutations = Permutations(allCounts);
        //        for (int j = 0; j < allPermutations.Count; j++)
        //        {
        //            string one = "";
        //            for (int k = 0; k < allPermutations[j].Length; k++) //ezt kell a list-el együtt indexelni
        //            {
        //                int dictListIdx = allPermutations[j][k]; //j-ik permutáció k-ik eleme a szólista indexe
        //                one += fp.WordsDict[list[k]][dictListIdx] + " ";
        //            }
        //            encoded.Add(one);
        //        }
        //    }
        //    return encoded;
        //}


        private void SetAllDivisions(string messageNumbers)
        {
            Parallel.ForEach(allDividers, oneDivider =>
            {
                var c = SplitAt(messageNumbers, oneDivider);
                if (c != null)
                {
                    solutionCandidates.TryAdd(c);
                }
            });

            if (fp.WordsDict.ContainsKey(messageNumbers))
                solutionCandidates.TryAdd(new string[] { messageNumbers });

        }

        private void SetAllDividers(string messageNumbers)
        {

            Parallel.For(1, messageNumbers.Length, i =>
            {
                var c = Combinations(messageNumbers.Length - 1, i);
                foreach (var curr in c)
                {
                    allDividers.TryAdd(curr);

                }
            });

            // fileprocesserre innentol van szukseg

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
            //Stopwatch s = new Stopwatch();
            //s.Start();
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
            //lock (lockobject)
            //{
            //    combinationTimes.Add(String.Format("{0};{1}", k, s.ElapsedMilliseconds));
            //}
            return combinations;
        }
    }
}
