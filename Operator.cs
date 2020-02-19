using System;
using System.Collections.Generic;

namespace LogicParser
{
    enum Operators { OR, AND, IMPLIES, NAND, NOR, XOR }

    class Operator
    {
        readonly Operators op;

        public static Operator OR = new Operator('v');
        public static Operator AND = new Operator('^');
        public static Operator IMPLIES = new Operator('>');
        public static Operator NAND = new Operator('/');
        public static Operator NOR = new Operator('\\');
        public static Operator XOR = new Operator('+');

        public Operator(char c)
        {
            op = (c.ToString().ToCharArray()[0]) switch
            {
                'v' => Operators.OR,
                '^' => Operators.AND,
                '>' => Operators.IMPLIES,
                '\\' => Operators.NOR,
                '/' => Operators.NAND,
                '+' => Operators.XOR,
                _ => throw new ArgumentException(),
            };
        }

        public override string ToString()
        {
            return op switch
            {
                Operators.AND => "^",
                Operators.IMPLIES => "->",
                Operators.OR => "v",
                Operators.NOR => "\\",
                Operators.NAND => "/",
                Operators.XOR => "+",
                _ => " ",
            };
        }

        public Func<bool> Run(Expression Left, Expression Right, Dictionary<string, bool> vars)
        {
            return () =>
            {
                bool left = Left.Evaluate(vars);
                left = Left.not ? !left : left;
                bool right = Right.Evaluate(vars);
                right = Right.not ? !right : right;

                return op switch
                {
                    Operators.AND => left && right,
                    Operators.OR => left || right,
                    Operators.IMPLIES => left ? right : true,
                    Operators.NOR => !(left || right),
                    Operators.NAND => !(left && right),
                    Operators.XOR => (left && !right) || (right && !left),
                    _ => false,
                };
            };
        }
    }
}