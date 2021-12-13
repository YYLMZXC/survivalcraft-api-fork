using System;

namespace ConsoleTest
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Compiler.M();
            Console.ReadLine();
        }
    }
}
