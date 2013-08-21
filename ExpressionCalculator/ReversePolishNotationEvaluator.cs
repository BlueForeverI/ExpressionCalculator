using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace ExpressionCalculator
{
    public enum TokenType
    {
        None,
        Number,
        Constant,
        Plus,
        Minus,
        Multiply,
        Divide,
        Exponent,
        Ln,
        Sqrt,
        Pow,
        UnaryMinus,
        LeftParenthesis,
        RightParenthesis
    }

    public struct Token
    {
        public string Value { get; set; }
        public TokenType Type { get; set; }
    }

    class ReversePolishNotationEvaluator
    {
        private Queue output;
        private Stack ops;
        private string postfixExpression;

        public ReversePolishNotationEvaluator()
        {
            output = new Queue();
            ops = new Stack();

            postfixExpression = string.Empty;
        }

        public void Parse(string expression)
        {
            string buffer = expression.ToLower();
            buffer = Regex.Replace(buffer, @"(?<number>\d+(\.\d+)?)", " ${number} ");
            buffer = Regex.Replace(buffer, @"(?<ops>[+\-*/^()])", " ${ops} ");
            buffer = Regex.Replace(buffer, @"\s+", " ").Trim();

            buffer = Regex.Replace(buffer, "-", "MINUS");
            buffer = Regex.Replace(buffer, @"(?<number>((\d+(\.\d+)?)))\s+MINUS", "${number} -");
            buffer = buffer.Replace(") MINUS", ") -");
            buffer = Regex.Replace(buffer, "(?<alpha>(ln|sqrt|pow))", " ${alpha} ");
            buffer = Regex.Replace(buffer, "MINUS", "~");  //~ as unary '-' operator
            buffer = Regex.Replace(buffer, ",", " ");

            //tokenise
            string[] parsed = buffer.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            double tokenValue;
            Token token, opsToken = new Token();
            for (i = 0; i < parsed.Length; ++i)
            {
                token = new Token();
                token.Value = parsed[i];
                token.Type = TokenType.None;

                try
                {
                    tokenValue = double.Parse(parsed[i]);
                    token.Type = TokenType.Number;
                    output.Enqueue(token);
                }
                catch (Exception)
                {
                    switch (parsed[i])
                    {
                        case "+":
                            token.Type = TokenType.Plus;
                            if (ops.Count > 0)
                            {
                                opsToken = (Token)ops.Peek();

                                while (IsOperatorToken(opsToken.Type))
                                {
                                    output.Enqueue(ops.Pop());
                                    if (ops.Count > 0)
                                    {
                                        opsToken = (Token)ops.Peek();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            ops.Push(token);
                            break;

                        case "-":
                            token.Type = TokenType.Minus;
                            if (ops.Count > 0)
                            {
                                opsToken = (Token)ops.Peek();

                                while (IsOperatorToken(opsToken.Type))
                                {
                                    output.Enqueue(ops.Pop());
                                    if (ops.Count > 0)
                                    {
                                        opsToken = (Token)ops.Peek();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            ops.Push(token);
                            break;

                        case "*":
                            token.Type = TokenType.Multiply;
                            if (ops.Count > 0)
                            {
                                opsToken = (Token)ops.Peek();

                                while (IsOperatorToken(opsToken.Type))
                                {
                                    if (opsToken.Type == TokenType.Plus || opsToken.Type == TokenType.Minus)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        output.Enqueue(ops.Pop());
                                        if (ops.Count > 0)
                                        {
                                            opsToken = (Token)ops.Peek();
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            ops.Push(token);
                            break;
                        case "/":
                            token.Type = TokenType.Divide;
                            if (ops.Count > 0)
                            {
                                opsToken = (Token)ops.Peek();

                                while (IsOperatorToken(opsToken.Type))
                                {
                                    if (opsToken.Type == TokenType.Plus || opsToken.Type == TokenType.Minus)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        output.Enqueue(ops.Pop());
                                        if (ops.Count > 0)
                                        {
                                            opsToken = (Token)ops.Peek();
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            ops.Push(token);
                            break;
                        case "^":
                            token.Type = TokenType.Exponent;
                            ops.Push(token);
                            break;
                        case "~":
                            token.Type = TokenType.UnaryMinus;
                            ops.Push(token);
                            break;
                        case "(":
                            token.Type = TokenType.LeftParenthesis;
                            ops.Push(token);
                            break;
                        case ")":
                            token.Type = TokenType.RightParenthesis;
                            if (ops.Count > 0)
                            {
                                opsToken = (Token)ops.Peek();

                                while (opsToken.Type != TokenType.LeftParenthesis)
                                {
                                    output.Enqueue(ops.Pop());
                                    if (ops.Count > 0)
                                    {
                                        opsToken = (Token)ops.Peek();
                                    }
                                    else
                                    {
                                        throw new Exception("Wrong number of parenthesis");
                                    }
                                }

                                ops.Pop();
                            }

                            if (ops.Count > 0)
                            {
                                opsToken = (Token)ops.Peek();

                                if (IsFunctionToken(opsToken.Type))
                                {
                                    output.Enqueue(ops.Pop());
                                }
                            }

                            break;
                        case "ln":
                            token.Type = TokenType.Ln;
                            ops.Push(token);
                            break;
                        case "sqrt":
                            token.Type = TokenType.Sqrt;
                            ops.Push(token);
                            break;
                        case "pow":
                            token.Type = TokenType.Pow;
                            ops.Push(token);
                            break;
                    }
                }
            }

            while (ops.Count != 0)
            {
                opsToken = (Token)ops.Pop();
                if (opsToken.Type == TokenType.LeftParenthesis)
                {
                    throw new Exception("Wrong number of parenthesis");
                }
                else
                {
                    output.Enqueue(opsToken);
                }
            }

            this.postfixExpression = string.Empty;
            foreach (object obj in output)
            {
                opsToken = (Token)obj;
                this.postfixExpression += string.Format("{0}", opsToken.Value);
            }
        }

        public double Evaluate()
        {
            Stack result = new Stack();
            double oper1 = 0.0;
            double oper2 = 0.0;
            Token token = new Token();

            foreach (object obj in output)
            {
                token = (Token)obj;

                switch (token.Type)
                {
                    case TokenType.Number:
                        result.Push(double.Parse(token.Value));
                        break;
                    case TokenType.Plus:
                        if (result.Count >= 2)
                        {
                            oper2 = (double)result.Pop();
                            oper1 = (double)result.Pop();
                            result.Push(oper1 + oper2);
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Minus:
                        if (result.Count >= 2)
                        {
                            oper2 = (double)result.Pop();
                            oper1 = (double)result.Pop();
                            result.Push(oper1 - oper2);
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Multiply:
                        if (result.Count >= 2)
                        {
                            oper2 = (double)result.Pop();
                            oper1 = (double)result.Pop();
                            result.Push(oper1 * oper2);
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Divide:
                        if (result.Count >= 2)
                        {
                            oper2 = (double)result.Pop();
                            oper1 = (double)result.Pop();
                            result.Push(oper1 / oper2);
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Exponent:
                        if (result.Count >= 2)
                        {
                            oper2 = (double)result.Pop();
                            oper1 = (double)result.Pop();
                            result.Push(Math.Pow(oper1, oper2));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.UnaryMinus:
                        if (result.Count >= 2)
                        {
                            oper1 = (double)result.Pop();
                            result.Push(-oper1);
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Ln:
                        if(result.Count >= 1)
                        {
                            oper1 = (double) result.Pop();
                            result.Push(Math.Log(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Sqrt:
                        if(result.Count >= 1)
                        {
                            oper1 = (double) result.Pop();
                            result.Push(Math.Sqrt(oper1));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                    case TokenType.Pow:
                        if(result.Count >= 1)
                        {
                            oper2 = (double) result.Pop();
                            oper1 = (double) result.Pop();
                            result.Push(Math.Pow(oper1, oper2));
                        }
                        else
                        {
                            throw new Exception("Evaluation error!");
                        }
                        break;
                }
            }

            if (result.Count == 1)
            {
                return (double)result.Pop();
            }
            else
            {
                throw new Exception("Evaluation error");
            }
        }

        private bool IsOperatorToken(TokenType t)
        {
            bool result = false;
            switch (t)
            {
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Multiply:
                case TokenType.Divide:
                case TokenType.Exponent:
                case TokenType.UnaryMinus:
                    result = true;
                    break;
            }

            return result;
        }

        private bool IsFunctionToken(TokenType t)
        {
            bool result = false;
            switch (t)
            {
                case TokenType.Ln:
                case TokenType.Sqrt:
                case TokenType.Pow:
                    result = true;
                    break;
            }

            return result;
        }
    }
}
