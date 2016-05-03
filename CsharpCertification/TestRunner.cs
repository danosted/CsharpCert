using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter3
{
    class TestRunner
    {
        public static void Main(string[] args)
        {
            /*
             * Entity Framework
             */
            //var test1 = new EntityFrameworkExample();
            //test1.Run();

            /*
             * Encrption
             */
            var test2 = new EncryptionExample();
            test2.Run();

            Console.WriteLine();
            Console.WriteLine("Press the 'any' key to continue...");
            Console.ReadLine();
        }
    }
}
