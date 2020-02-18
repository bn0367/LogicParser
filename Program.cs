using System;
using System.Collections.Generic;
using System.Linq;
using static LogicParser.Operator;

namespace LogicParser
{
    enum Operators { OR, AND, IMPLIES }

    class Program
    {
        static void Main(string[] args)
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

    class Expression
    {
        public bool HasOperator;
        public bool not;
        public Operator op;
        public Expression Left;
        public Expression Right;
        public char variable;

        public Expression(char v, bool n = false)
        {
            HasOperator = false;
            variable = v;
            not = n;
        }
        public Expression(Expression l, Operator o, Expression r, bool n = false)
        {
            Left = l;
            op = o;
            Right = r;
            not = n;
            HasOperator = true;
        }
        public bool Evaluate(Dictionary<string, bool> vars)
        {
            if (!HasOperator)
            {
                return vars[variable.ToString()];
            }
            return op.Run(Left, Right, vars)();

        }

        internal List<Dictionary<string, bool>> BasicTruthTable()
        {
            List<string> vars = string.Concat(ToString().Where(e => !"-~()v>^".Contains(e))).Split().Where(e => e != string.Empty).Distinct().ToList();
            List<Dictionary<string, bool>> r = new List<Dictionary<string, bool>>();
            for (int i = 0; i < (int)Math.Pow(2, vars.Count); i++)
            {
                Dictionary<string, bool> row = new Dictionary<string, bool>();
                for (int j = 0; j < vars.Count; j++)
                {
                    row.Add(vars[j], (i & (1 << j)) != 0);
                }
                r.Add(row);
            }
            return r;
        }

        public void PrintTable()
        {
            var table = TruthTable();
            foreach (Dictionary<string, bool> row in table)
            {
                foreach (KeyValuePair<string, bool> variable in row)
                {
                    var t = variable.Value ? "T" : "F";
                    Console.Write($"{variable.Key}: {t} | ");
                }
                Console.WriteLine();
            }
        }

        public override string ToString()
        {
            if (!HasOperator)
            {
                return (not ? "~" : "") + variable.ToString();
            }
            bool lParen = Left.HasOperator;
            bool rParen = Left.HasOperator;
            return (lParen ? "(" : "") + Left.ToString() + (lParen ? ") " : " ") + op + (rParen ? " (" : " ") + Right.ToString() + (lParen ? ")" : "");
        }

        public List<Dictionary<string, bool>> TruthTable(List<Dictionary<string, bool>> table = null)
        {
            if (table == null) table = BasicTruthTable();

            if (HasOperator)
            {
                table = Left.TruthTable(table);
                table = Right.TruthTable(table);
            }
            foreach (var row in table)
            {
                row.TryAdd(ToString(), Evaluate(row));
            }
            return table;
        }

        public static Expression Parse(string exp)
        {
            exp = exp.Replace(" ", "");
            if (exp.Count(e => e == '(') != exp.Count(e => e == ')'))
            {
                throw new ArgumentException("Invalid Expression");
            }
            int pos = exp.IndexOf("(");
            int depth = 0;
            bool foundOne = false;
            for (int i = 0; i < exp.Length; i++)
            {
                if (exp[i] == '(')
                {
                    foundOne = true;
                    depth++;
                }
                if (exp[i] == ')')
                {
                    depth--;
                }
                if (depth == 0 && foundOne)
                {
                    if (i + 2 < exp.Length)
                    {
                        var left = exp[1..i];
                        var right = exp[(i + 2)..];
                        var op = new Operator(exp.Substring(i + 1, 1)[0]);
                        return new Expression(Parse(left), op, Parse(right));
                    }
                    else
                    {
                        return Parse(exp[1..i]);
                    }
                }
            }

            var first = exp.Split(">", 2);
            if (first.Length == 1)
            {
                if (exp.Split("^").Length > 1 && exp.Split("v").Length > 1)
                {
                    throw new ArgumentException("Ambiguous Expression");
                }
                var ands = exp.Split("^");
                var ors = exp.Split("v");
                if (ands.Length > 1)
                {
                    return CreateChainSingleExpression(ands, AND);
                }
                else if (ors.Length > 1)
                {
                    return CreateChainSingleExpression(ors, OR);
                }
                else
                {
                    return new Expression(exp[0]);
                }

            }
            else
            {
                return new Expression(Parse(first[0]), IMPLIES, Parse(first[1]));
            }


        }

        public static Expression CreateChainSingleExpression(IEnumerable<string> ss, Operator op)
        {
            if (ss.Count() == 1)
            {
                if (ss.First()[0] == '~')
                    return new Expression(ss.First()[1], true);
                return new Expression(ss.First()[0]);
            }
            Expression l;
            if (ss.First()[0] == '~')
                l = new Expression(ss.First()[1], true);
            else
                l = new Expression(ss.First()[0]);
            return new Expression(l, op, CreateChainSingleExpression(ss.Skip(1), op));
        }

    }

    class Operator
    {
        readonly Operators op;

        public static Operator OR = new Operator('v');
        public static Operator AND = new Operator('^');
        public static Operator IMPLIES = new Operator('>');

        public Operator(char c)
        {
            op = (c.ToString().ToLower().ToCharArray()[0]) switch
            {
                'v' => Operators.OR,
                '^' => Operators.AND,
                '>' => Operators.IMPLIES,
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
                    _ => false,
                };
            };
        }
    }
}
