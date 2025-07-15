using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DIL.Components.ValueComponent.Tokens
{
    public record Token(
        TokenType tokenType,
        TokenOperator @operator,
        object value,
        int postion_start,
        int postion_end
    );

    public class InlineEvaluate : ClassComponents.ClassComponent
    {
        private const string TokenRegex =
            @"(?<NUMBER>\d+(\.\d+)?)|(?<STRING>"".*?"")|(?<OPERATOR>(\+|\-|\*|/|%|&|\|\||\|\||==|is|!=|<=|>=|<<|>>))|(?<SETTER>=)|(?<BRACKET>[(){}\[\]])|(?<IDENTIFIER>(\b[a-zA-Z_]\w*(?:->\w+|\[\d+\])*(?:->\w+|\[\d+\])*?(?:\s*,\s*\b[a-zA-Z_]\w*(?:->\w+|\[\d+\])*(?:->\w+|\[\d+\])*)*))";

        private readonly List<Token> tokens;
        private readonly string _inline;
        private static readonly GetComponent _getcomponent = new();

        // Main constructor
        public InlineEvaluate(string inline)
        {
            _inline = inline;
            tokens = BuildTokens(inline);
            ValidateTokens(tokens);
        }

        // Internal use for recursive bracket evaluation
        internal InlineEvaluate(List<Token> tokens)
        {
            _inline = string.Empty;
            this.tokens = tokens;
        }

        private List<Token> BuildTokens(string inline)
        {
            var tokenList = new List<Token>();
            var regexResults = Regex.Matches(inline, TokenRegex);

            foreach (Match match in regexResults)
            {
               
                if (match.Groups["NUMBER"].Success)
                    tokenList.Add(new Token(TokenType.Number, TokenOperator.None, double.Parse(match.Value), match.Index, match.Index + match.Length));
                else if (match.Groups["STRING"].Success)
                    tokenList.Add(new Token(TokenType.String, TokenOperator.None, match.Value.Trim('"'), match.Index, match.Index + match.Length));
                else if (match.Groups["OPERATOR"].Success)
                    tokenList.Add(new Token(TokenType.Operator, ParseOperator(match.Value.Trim()), match.Value, match.Index, match.Index + match.Length));
                else if (match.Groups["BRACKET"].Success)
                    tokenList.Add(new Token(TokenType.Bracket, TokenOperator.None, match.Value, match.Index, match.Index + match.Length));
                else if (match.Groups["IDENTIFIER"].Success)
                    tokenList.Add(new Token(TokenType.Identifier, TokenOperator.None, match.Value, match.Index, match.Index + match.Length));
                else if (match.Groups["SETTER"].Success)
                    tokenList.Add(new Token(TokenType.Identifier, TokenOperator.SETTER, match.Value, match.Index, match.Index + match.Length));
            }

            return tokenList;
        }

        private void ValidateTokens(List<Token> tokenList)
        {
            var stack = new Stack<Token>();
            foreach (var token in tokenList)
            {
                if (token.tokenType == TokenType.Bracket)
                {
                    var ch = token.value.ToString();
                    if (IsOpeningBracket(ch)) stack.Push(token);
                    else if (IsClosingBracket(ch))
                    {
                        if (stack.Count == 0 || !IsMatchingBracket(stack.Pop().value.ToString(), ch))
                            throw new InvalidOperationException($"Mismatched or unmatched bracket '{ch}' at position {token.postion_start}.");
                    }
                }
            }
            if (stack.Count > 0)
                throw new InvalidOperationException("Unmatched opening brackets found.");
        }

        public object Parse(bool returnRaw = false)
        {
            object result = EvaluateTokens(tokens);
            return returnRaw
                ? result
                : EditRange(_inline, tokens[0].postion_start, tokens[^1].postion_end - 1, result.ToString());
        }

        private object EvaluateTokens(List<Token> tokens)
        {
            object left_name = null;
            bool RefSet = false;
            object? left = null, right = null;
            TokenOperator currentOp = TokenOperator.None;

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                if (token.tokenType == TokenType.Bracket && IsOpeningBracket(token.value.ToString()))
                {
                    var subTokens = ExtractBracketTokens(tokens, i, out int newIndex);
                    left ??= new InlineEvaluate(subTokens).Parse(true);
                    i = newIndex;
                    continue;
                }

                switch (token.tokenType)
                {
                    case TokenType.Number:
                    case TokenType.String:
                        left ??= token.value;
                        break;

                    case TokenType.Identifier:
                        left_name = token.value;
                        left ??= _getcomponent.GetVariable(token.value.ToString())
                        ?? throw new InvalidOperationException($"Unknown identifier: {token.value}");

                        break;

                    case TokenType.Operator:
                        if (left == null) throw new InvalidOperationException("Missing left operand.");
                        currentOp = token.@operator;
                        break;

                }

                if (currentOp != TokenOperator.None && i + 1 < tokens.Count)
                {
                    var rightToken = tokens[++i];
                    if(rightToken.@operator == TokenOperator.SETTER)
                    {
                        if(left_name is not "")
                        {
                            RefSet = true;
                            
                        }
                                else
                            throw new InvalidOperationException("Not able to set value .null.");
                        rightToken = tokens[++i];

                    }

                    if (rightToken.tokenType == TokenType.Bracket && IsOpeningBracket(rightToken.value.ToString()))
                    {
                        var subTokens = ExtractBracketTokens(tokens, i, out int newIndex);
                        right = new InlineEvaluate(subTokens).Parse(true);
                        
                        i = newIndex;
                    }
                    else
                    {
                       
                        right = rightToken.tokenType switch
                        {
                            TokenType.Number => Convert.ToDouble(rightToken.value),
                            TokenType.String => rightToken.value,
                            TokenType.Identifier => _getcomponent.GetVariable(rightToken.value.ToString()),
                            _ => throw new InvalidOperationException($"Invalid right operand: {rightToken.value}")
                        };
                    }
                    var leftType = left.GetType();
                    left = Evaluate(left!, currentOp, right!);
                    if(left is bool v &&v)
                    {
                        //(b-20)(condtions(>>,<<))=(setter)=20
                        //if the codtion is true the identifier is automac reset to the right value
                        // If the condition (e.g., b > 20) is true, assign right to the original identifier
                        if (RefSet)
                        {
                            if (string.IsNullOrWhiteSpace(left_name?.ToString()))
                                throw new InvalidOperationException(
                                                                   $"Failed to assign value '{right}' to 'Identifier' of type '{leftType.Name}' because there was no direct Identifier");
                            RefSet = false; // mark ref assignment as consumed

                            try
                            {
                                var convertedRight = Convert.ChangeType(right, leftType);
                                _getcomponent.NewSet(left_name!.ToString(), convertedRight);
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException(
                                    $"Failed to assign value '{right}' to '{left_name}' of type '{leftType.Name}': {ex.Message}", ex);
                            }
                        }
                    }
                    currentOp = TokenOperator.None;
                  
                    right = null;
                }
            }
            

            return left ?? throw new InvalidOperationException("No valid expression to evaluate.");
        }

        private static List<Token> ExtractBracketTokens(List<Token> allTokens, int startIndex, out int newIndex)
        {
            var open = allTokens[startIndex].value.ToString();
            var close = open switch
            {
                "(" => ")",
                "[" => "]",
                "{" => "}",
                _ => throw new InvalidOperationException("Invalid bracket.")
            };

            int depth = 1;
            var subTokens = new List<Token>();
            newIndex = startIndex + 1;

            for (; newIndex < allTokens.Count; newIndex++)
            {
                var t = allTokens[newIndex];
                if (t.tokenType == TokenType.Bracket)
                {
                    if (t.value.ToString() == open) depth++;
                    else if (t.value.ToString() == close) depth--;
                }

                if (depth == 0) break;
                subTokens.Add(t);
            }

            if (depth != 0)
                throw new InvalidOperationException($"Unmatched bracket starting at position {allTokens[startIndex].postion_start}");

            return subTokens;
        }

        public static object Evaluate(object left, TokenOperator op, object right)
        {
            right = Convert.ChangeType(right, left.GetType());
            return op switch
            {
                TokenOperator.Add => TryNumeric(left, right, (a, b) => a + b) ?? left.ToString() + right.ToString(),
                TokenOperator.Subtract => ToDouble(left) - ToDouble(right),
                TokenOperator.Multiply => ToDouble(left) * ToDouble(right),
                TokenOperator.Divide => ToDouble(right) == 0 ? throw new DivideByZeroException() : ToDouble(left) / ToDouble(right),
                TokenOperator.Modulus => ToDouble(left) % ToDouble(right),
                TokenOperator.BitwiseAnd => ToInt(left) & ToInt(right),
                TokenOperator.BitwiseOr => ToInt(left) | ToInt(right),
                TokenOperator.LogicalAnd => ToBool(left) && ToBool(right),
                TokenOperator.LogicalOr => ToBool(left) || ToBool(right),
                TokenOperator.Equal => Equals(left, right),
                TokenOperator.NotEqual => !Equals(left, right),
                TokenOperator.LessThan => ToDouble(left) < ToDouble(right),
                TokenOperator.GreaterThan => ToDouble(left) > ToDouble(right),
                TokenOperator.LessThanOrEqual => ToDouble(left) <= ToDouble(right),
                TokenOperator.GreaterThanOrEqual => ToDouble(left) >= ToDouble(right),
                _ => throw new InvalidOperationException("Unsupported operator")
            };
        }

        public static string EditRange(string input, int start, int end, string newValue)
        {
            return input.Substring(0, start) + newValue + input.Substring(end + 1);
        }

        private static double ToDouble(object val) => Convert.ToDouble(val);
        private static int ToInt(object val) => Convert.ToInt32(val);
        private static bool ToBool(object val) => Convert.ToBoolean(val);

        private static object? TryNumeric(object a, object b, Func<double, double, object> op)
        {
            try { return op(ToDouble(a), ToDouble(b)); } catch { return null; }
        }

        private static bool IsOpeningBracket(string b) => b is "(" or "{" or "[";
        private static bool IsClosingBracket(string b) => b is ")" or "}" or "]";
        private static bool IsMatchingBracket(string open, string close) =>
            (open, close) switch
            {
                ("(", ")") => true,
                ("[", "]") => true,
                ("{", "}") => true,
                _ => false
            };

        private TokenOperator ParseOperator(string op) => op switch
        {
            "+" => TokenOperator.Add,
            "-" => TokenOperator.Subtract,
            "*" => TokenOperator.Multiply,
            "/" => TokenOperator.Divide,
            "%" => TokenOperator.Modulus,
            "&" => TokenOperator.BitwiseAnd,
            "|" => TokenOperator.BitwiseOr,
            "&&" => TokenOperator.LogicalAnd,
            "||" => TokenOperator.LogicalOr,
            ">>" => TokenOperator.GreaterThan,
            "<<" => TokenOperator.LessThan,
            "<=" => TokenOperator.LessThanOrEqual,
            ">=" => TokenOperator.GreaterThanOrEqual,
            "==" or "is" => TokenOperator.Equal,
            "!=" or "is not"=> TokenOperator.NotEqual,
            _ => throw new InvalidOperationException($"Unknown operator: {op}")
        };
    }
}
