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
            FileProcesser.WriteToFile(sequentlist, "sequentialOutput.txt");
            Console.WriteLine("elapsed time: {0} ms\r\nsolution count:{1}", stopwatch.ElapsedMilliseconds, sequentlist.Count);
        }

        static void RunParallelLoadBalancer(FileProcesser fp, string encodedMessage)
        {
            ParallelSolver plSolver = new ParallelSolver(fp);
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("\r\nload balancer:");
            //Task.Run(() =>
            //{
            stopwatch.Start();
            var th = plSolver.DecodeNumberMessageWithLoadBalancing(encodedMessage, 8).Result;
            FileProcesser.WriteToFile(th, "parallelOutput.txt");
            Console.WriteLine("elapsed time: {0} ms\r\nsolution count:{1}", stopwatch.ElapsedMilliseconds, th.Count);
            //});


        }

        static void RunLoopParallelism(FileProcesser fp, string encodedMessage)
        {
            ParallelSolver plSolver = new ParallelSolver(fp);
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("\r\nloop parallelism:");
            stopwatch.Start();
            var lm = plSolver.DecodeNumberMessageWithLoopParallelism(encodedMessage);
            FileProcesser.WriteToFile(lm, "loopParallelism.txt");
            Console.WriteLine("elapsed time: {0} ms\r\nsolution count: {1}", stopwatch.ElapsedMilliseconds, lm.Count);

        }

        static void Main(string[] args)
        {
            string encoded = "84474772725535678464928";
            var fp = new FileProcesser("wordsEn.txt");
            RunSequential(fp, encoded);
            RunLoopParallelism(fp, encoded);
            RunParallelLoadBalancer(fp, encoded);

            Console.ReadLine();

        }

    }
}
