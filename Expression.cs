﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static LogicParser.Operator;

namespace LogicParser
{
    class Expression
    {
        public bool HasOperator;
        public bool not;
        public Operator op;
        public Expression Left;
        public Expression Right;
        public string variable;

        public Expression(string v, bool n = false)
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
            List<string> vars = string.Concat(ToString().Where(e => !"-~()v>^+/\\".Contains(e))).Split().Where(e => e != string.Empty).Distinct().ToList();
            List<Dictionary<string, bool>> r = new List<Dictionary<string, bool>>();
            for (int i = 0; i < (int)Math.Pow(2, vars.Count); i++)
            {
                Dictionary<string, bool> row = new Dictionary<string, bool>();
                for (int j = 0; j < vars.Count; j++)
                {
                    row.Add(vars[j], (i & (1 << j)) == 0);
                }
                r.Add(row);
            }
            return r;
        }

        public void PrintTable(List<Dictionary<string, bool>> table = null)
        {
            if (table == null) table = TruthTable();
            foreach (Dictionary<string, bool> row in table)
            {
                foreach (KeyValuePair<string, bool> variable in row)
                {
                    var t = variable.Value ? "T" : "F";
                    Console.Write($"{variable.Key}: {t} | ", Encoding.Unicode);
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

        public List<Dictionary<string, bool>> TruthTable(List<Dictionary<string, bool>> table = null, bool generic = false)
        {
            if (table == null) table = BasicTruthTable();
            if (!generic)
            {
                if (HasOperator)
                {
                    table = Left.TruthTable(table);
                    table = Right.TruthTable(table);
                }
            }
            foreach (var row in table)
            {
                row.TryAdd(generic ? "conclusion" : ToString(), Evaluate(row));
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
            var first = exp.Split('>');
            if (first.Length > 1)
            {
                return new Expression(Parse(first[0]), IMPLIES, Parse(first[1]));
            }
            if (pos != -1)
            {
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
            }
            if (first.Length == 1)
            {
                if (TooManyOps(exp, new List<char>() { 'v', '^', '/', '\\', '+' }))
                {
                    throw new ArgumentException("Ambiguous Expression");
                }
                var ands = exp.Split("^");
                var ors = exp.Split("v");
                var nors = exp.Split("\\");
                var nands = exp.Split("/");
                var xors = exp.Split("+");
                if (ands.Length > 1)
                {
                    return CreateChainSingleExpression(ands, AND);
                }
                else if (ors.Length > 1)
                {
                    return CreateChainSingleExpression(ors, OR);
                }
                else if (nors.Length > 1)
                {
                    return CreateChainSingleExpression(nors, NOR);
                }
                else if (nands.Length > 1)
                {
                    return CreateChainSingleExpression(nands, NAND);
                }
                else if (xors.Length > 1)
                {
                    return CreateChainSingleExpression(xors, XOR);
                }
                else
                {
                    return new Expression(exp);
                }

            }
            else
            {
                throw new Exception("literally how this is not possible (length of array < 1)");
            }


        }

        public static Expression CreateChainSingleExpression(IEnumerable<string> ss, Operator op)
        {
            if (ss.Count() == 1)
            {
                if (ss.First()[0] == '~')
                    return new Expression(ss.First()[1..], true);
                return new Expression(ss.First());
            }
            Expression l;
            if (ss.First()[0] == '~')
                l = new Expression(ss.First()[1..], true);
            else
                l = new Expression(ss.First());
            return new Expression(l, op, CreateChainSingleExpression(ss.Skip(1), op));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Expression)) return false;
            Expression other = obj as Expression;
            if (!TableEquals(TruthTable(generic: true), other.TruthTable(generic: true))) return false;

            return true;
        }

        public static bool RowEquals<K, V>(Dictionary<K, V> first, Dictionary<K, V> second)
        {
            return first.Count == second.Count && !first.Except(second).Any();
        }

        public static bool TableEquals<K, V>(List<Dictionary<K, V>> first, List<Dictionary<K, V>> second)
        {
            if (first.Count != second.Count) return false;
            for (int i = 0; i < first.Count; i++)
            {
                if (!RowEquals(first[i], second[i])) return false;
            }
            return true;
        }

        public static bool TooManyOps(string s, List<char> ops)
        {
            bool hasOne = false;
            foreach (char c in ops)
            {
                if (s.Split(c).Length > 1)
                {
                    if (hasOne) return true;
                    hasOne = true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HasOperator, not, op, Left, Right, variable);
        }
    }
}