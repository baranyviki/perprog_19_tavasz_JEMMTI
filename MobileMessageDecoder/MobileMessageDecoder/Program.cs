using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileMessageDecoder
{
    class Program
    {
        static void Main(string[] args)
        {
            var fp = new FileProcesser("wordsEn.txt");
            //SequentialSolver s = new SequentialSolver(fp);
            //var list = s.DecodeNumberMessage("73255956646377243332633");

            ParallelSolver plSolver = new ParallelSolver(fp);
            var list = plSolver.DecodeNumberMessage("7325595664637888888888888");
            FileProcesser.WriteToFile(list, "parallelOutput.txt");
            
            
            Console.WriteLine("sequential:");

            SequentialSolver s = new SequentialSolver(fp);
            var sequentlist = s.DecodeNumberMessage("8447477272553567899");
            FileProcesser.WriteToFile(sequentlist,"sequentialOutput.txt");
            

            Console.ReadLine();

        }
       
    }
}
