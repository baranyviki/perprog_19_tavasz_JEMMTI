using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileMessageDecoder
{
    class Program
    {

        static void RunSequential(FileProcesser fp, string encodedMessage)
        {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("sequential:");
            SequentialSolver s = new SequentialSolver(fp);
            var sequentlist = s.DecodeNumberMessage(encodedMessage);
            Console.WriteLine("elapsed time: {0} ms\r\nsolution count:{1}", stopwatch.ElapsedMilliseconds, sequentlist.Count);
            FileProcesser.WriteToFile(sequentlist, "sequentialOutput.txt");

        }

        static void RunParallelLoadBalancer(FileProcesser fp, string encodedMessage, int taskCount)
        {
            ParallelSolver plSolver = new ParallelSolver(fp);
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("\r\nload balancer with {0} task:",taskCount);
            
            stopwatch.Start();
            var th = plSolver.DecodeNumberMessageWithLoadBalancing(encodedMessage, taskCount).Result;
            Console.WriteLine("elapsed time: {0} ms\r\nsolution count:{1}", stopwatch.ElapsedMilliseconds, th.Count);
            FileProcesser.WriteToFile(th, "loadBalancer.txt");
        }

        static void RunLoopParallelism(FileProcesser fp, string encodedMessage)
        {
            ParallelSolver plSolver = new ParallelSolver(fp);
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("\r\nloop parallelism:");
            stopwatch.Start();
            var lm = plSolver.DecodeNumberMessageWithLoopParallelism(encodedMessage);
            Console.WriteLine("elapsed time: {0} ms\r\nsolution count: {1}", stopwatch.ElapsedMilliseconds, lm.Count);
            FileProcesser.WriteToFile(lm, "loopParallelism.txt");

        }

        static void Main(string[] args)
        {
            //73255956646377243863326339324
            string encoded = "7325595664637724386332";
            int taskCount = 8;

            Console.WriteLine("Run times for input with {0} length" , encoded.Length);

            var fp = new FileProcesser("wordsEn.txt");
            RunSequential(fp, encoded);
            RunLoopParallelism(fp, encoded);
            RunParallelLoadBalancer(fp, encoded,taskCount);

            Console.ReadLine();

        }

    }
}
