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
           var fp= new FileProcesser("wordsEn.txt", "writeThis.txt");
            SequentialSolver s = new SequentialSolver(fp);
          
            var list = s.DecodeNumberMessage("96846862459685488537263");
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine("divisions done");
            Console.ReadLine();

        }

        //notmine 

    }
}
