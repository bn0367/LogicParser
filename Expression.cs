using Microsoft.Z3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static LogicParser.Operator;

namespace LogicParser
{
    class Expression
    {
        public readonly bool HasOperator;
        public bool not;
        public readonly Operator op;
        public readonly Expression Left;
        public readonly Expression Right;
        public readonly string variable;
        public readonly List<Dictionary<string, bool>> TTable;
        public readonly List<Dictionary<string, bool>> GTable;
        public readonly List<Dictionary<string, bool>> BTable;
        public readonly Context ctx;

        public Expression(string v, bool n = false)
        {
            ctx = new Context();
            HasOperator = false;
            variable = v;
            not = n;
            BTable = BasicTruthTable();
            TTable = TruthTable(generic: false);
            GTable = TruthTable(generic: true);
        }
        public Expression(Expression l, Operator o, Expression r, bool n = false)
        {
            ctx = new Context();
            Left = l;
            op = o;
            Right = r;
            not = n;
            HasOperator = true;
            BTable = BasicTruthTable();
            TTable = TruthTable(generic: false);
            GTable = TruthTable(generic: true);
        }
        public bool Evaluate(Dictionary<string, bool> vars)
        {
            if (!HasOperator)
            {
                return vars[variable] ^ not;
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
            table ??= TruthTable();
            StringBuilder output = new StringBuilder();
            foreach (Dictionary<string, bool> row in table)
            {
                foreach (KeyValuePair<string, bool> variable in row)
                {
                    var t = variable.Value ^ not ? "T" : "F";
                    output.Append($"{variable.Key}: {t} | ");
                }
                output.Append("\n");
            }
            Console.WriteLine(output);
        }

        public static Expression Invert(Expression e)
        {
            e.not ^= true;
            return e;
        }

        public override string ToString()
        {
            if (!HasOperator)
            {
                return (not ? "~" : "") + variable;
            }
            bool lParen = Left.HasOperator;
            bool rParen = Left.HasOperator;
            return (not ? "~(" : "") + (lParen ? "(" : "") + Left.ToString() + (lParen ? ") " : " ") + op + (rParen ? " (" : " ") + Right.ToString() + (lParen ? ")" : "") + (not ? ")" : "");
        }

        internal List<Dictionary<string, bool>> TruthTable(bool generic = false, List<Dictionary<string, bool>> table = null)
        {
            if (table == null) table = BasicTruthTable();
            if (!generic)
            {
                if (HasOperator)
                {
                    table = Left.TruthTable(false, table);
                    table = Right.TruthTable(false, table);
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
                            if (exp.StartsWith("~"))
                            {
                                return Invert(Parse(exp[(exp.IndexOf("(") + 1)..i]));
                            }
                            return Parse(exp[(exp.IndexOf("(") + 1)..i]);
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
                    return (exp.StartsWith("~") ? new Expression(exp[1..], true) : new Expression(exp));
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
            if (!TableEquals(GTable, other.GTable)) return false;

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

        public static Expression RandomExpression(int length = 1)
        {
            var ops = new string[] { "^", "v", ">", "\\", "/", "+" };
            var vars = new string[] { "p", "q", "r", "s", "t", "u" };
            var ran = new Random();
            if (length <= 1) return Parse(vars[ran.Next(vars.Length)] + " " + ops[ran.Next(ops.Length)] + " " + vars[ran.Next(vars.Length)]);
            else return new Expression(Parse(vars[ran.Next(vars.Length)] + " " + ops[ran.Next(ops.Length)] + " " + vars[ran.Next(vars.Length)]), new Operator(ops[ran.Next(ops.Length)][0]), RandomExpression(--length));
        }

        public BoolExpr GetBoolExpr(Expression original = null)
        {
            original ??= this;
            //Context c = new Context();
            //List<BoolExpr> bexp = new List<BoolExpr>();
            //string.Concat(ToString().Where(e => !"-~()v>^+/\\".Contains(e))).Split().Where(e => e != string.Empty).Distinct().ToList().ForEach(e => bexp.Add(c.MkBoolConst(e)));
            //Solver s = c.MkSolver();
            if (!HasOperator)
            {
                return not ? original.ctx.MkNot(original.ctx.MkBoolConst(variable)) : original.ctx.MkBoolConst(variable);
            }
            BoolExpr output = (op.op switch
            {
                Operators.AND => original.ctx.MkAnd(Left.GetBoolExpr(original), Right.GetBoolExpr(original)),
                Operators.OR => original.ctx.MkOr(Left.GetBoolExpr(original), Right.GetBoolExpr(original)),
                Operators.NOR => original.ctx.MkOr(original.ctx.MkNot(Left.GetBoolExpr(original)), original.ctx.MkNot(Right.GetBoolExpr(original))),
                Operators.XOR => original.ctx.MkOr(original.ctx.MkAnd(Left.GetBoolExpr(original), original.ctx.MkNot(Right.GetBoolExpr(original))), original.ctx.MkAnd(original.ctx.MkNot(Left.GetBoolExpr(original)), Right.GetBoolExpr(original))),
                Operators.NAND => original.ctx.MkAnd(original.ctx.MkNot(Left.GetBoolExpr(original)), original.ctx.MkNot(Right.GetBoolExpr(original))),
                Operators.IMPLIES => original.ctx.MkImplies(Left.GetBoolExpr(original), Right.GetBoolExpr(original)),
                _ => null
            });
            if (not)
            {
                output = original.ctx.MkNot(output);
            }
            return output;
        }
        public void Prove(BoolExpr e = null)
        {
            e ??= GetBoolExpr();
            Solver s = ctx.MkSimpleSolver();
            s.Assert(e);
            Console.WriteLine($"{this} is {s.Check()}");
            Expression simplified = ExprToExpression(e.Simplify());
            Console.WriteLine($"simplified: {simplified}");
        }

        public Expression ExprToExpression(Expr e)
        {
            if (e.NumArgs < 1 || e.IsVar)
            {
                return new Expression(e.ToString(), e.IsNot);
            }
            Operator temp = Z3OpToOperator(e);
            if (temp != null)
            {
                return new Expression(ExprToExpression(e.Arg(0)), temp, ExprToExpression(e.Arg(1)));
            }
            else
            {
                return Invert(ExprToExpression(e.Arg(0)));
            }
        }
        public static Operator Z3OpToOperator(Expr e)
        {
            if (e.IsNot)
            {
                if (e.IsOr) return NOR;
                if (e.IsAnd) return NAND;
                return null;
            }
            if (e.IsImplies) return IMPLIES;
            if (e.IsXor) return XOR;
            if (e.IsOr) return OR;
            if (e.IsAnd) return AND;
            throw new NotImplementedException("Unknown operator");
        }
    }
}
