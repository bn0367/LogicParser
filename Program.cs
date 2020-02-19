using System;
using System.Text;
using static LogicParser.Operator;
namespace LogicParser
{

    class Program
    {
        static void Main(string[] _)
        {
            var e = Expression.RandomExpression(100);
            e.PrintTable(e.GTable);
            Console.WriteLine();
        }
    }
}