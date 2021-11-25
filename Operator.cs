using System;
using System.Collections.Generic;

namespace LogicParser
{
    enum Operators { OR, AND, IMPLIES, NAND, NOR, XOR }

    class Operator
    {
        public readonly Operators op;

        public static readonly Operator OR = new('v');
        public static readonly Operator AND = new('^');
        public static readonly Operator IMPLIES = new('>');
        public static readonly Operator NAND = new('/');
        public static readonly Operator NOR = new('\\');
        public static readonly Operator XOR = new('+');

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
                _ => throw new ArgumentException("Invalid operator!"),
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
                bool left = Left.Evaluate(vars) ^ (Left.HasOperator && Left.not);
                bool right = Right.Evaluate(vars) ^ (Right.HasOperator && Right.not);

                return op switch
                {
                    Operators.AND => (left && right),
                    Operators.OR => (left || right),
                    Operators.IMPLIES => (!left || right),
                    Operators.NOR => !(left || right),
                    Operators.NAND => !(left && right),
                    Operators.XOR => ((left && !right) || (right && !left)),
                    _ => false,
                };
            };
        }
    }
}