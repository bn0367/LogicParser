using System;
using System.Text;

namespace LogicParser
{

    class Program
    {
        static void Main(string[] _)
        {
            
            while (true)
            {
                Console.WriteLine("Enter a logical expression: ");
                Expression l = Expression.Parse(Console.ReadLine());
                Console.WriteLine();
                l.PrintTable();
                Console.WriteLine();
            }
        }
    }
}