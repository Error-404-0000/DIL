using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DIL.Components.ValueComponent.Tokens
{
    public record Token(TokenType tokenType, TokenOperator @operator, object @value, int postion_start, int postion_end);

    public class InlineEvaluate : GetComponent
    {
        private const string TokenRegex =
            @"(?<NUMBER>\d+(\.\d+)?)|(?<STRING>"".*?"")|(?<OPERATOR>[+\-*/%&|^<>=!~]+)|(?<BRACKET>[(){}[\]])|(?<IDENTIFIER>[a-zA-Z_][a-zA-Z0-9_]*)";

        private readonly List<Token> tokens;
        private static readonly Dictionary<TokenOperator, string> OperatorSymbols = new()
        {
            { TokenOperator.None, "" },
            { TokenOperator.Add, "+" },
            { TokenOperator.Subtract, "-" },
            { TokenOperator.Multiply, "*" },
            { TokenOperator.Divide, "/" },
            { TokenOperator.Modulus, "%" },
            { TokenOperator.BitwiseAnd, "&" },
            { TokenOperator.BitwiseOr, "|" },
            { TokenOperator.LogicalAnd, "&&" },
            { TokenOperator.LogicalOr, "||" },
            { TokenOperator.GreaterThan, ">" },
            { TokenOperator.LessThan, "<" },
            { TokenOperator.Equal, "==" },
            { TokenOperator.NotEqual, "!=" }
        };
        private readonly string _inline;
        public InlineEvaluate(string inline)
        {
            this._inline = inline;
            tokens = BuildTokens(inline);
            ValidateTokens(tokens, inline);
        }

        private List<Token> BuildTokens(string inline)
        {
            var regexResults = Regex.Matches(inline, TokenRegex);
            var tokenList = new List<Token>();
            var matches = Regex.Matches(inline, @"(\w+)\s*:\s*((""[^""]*"")|(\{(?:[^{}]*|(?<Open>\{)|(?<-Open>\}))*\}(?(Open)(?!)))|(\[(?:[^\[\]]*|(?<Open>\[)|(?<-Open>\]))*\](?(Open)(?!)))|([^,{}]+))");
            if(matches.Count>0)
            {
                foreach (Match match in matches)
                {
                    foreach(var token in BuildTokens(match.Groups[2].Value))
                    {
                        tokenList.Add(new Token(token.tokenType,token.@operator,token.value, match.Groups[1].Value.Length + 2,token.postion_end));
                    }
                }
                return tokenList;   
            }

            foreach (Match match in regexResults)
            {
                if (match.Groups["NUMBER"].Success)
                {
                    tokenList.Add(new Token(
                        TokenType.Number,
                        TokenOperator.None,
                        double.Parse(match.Groups["NUMBER"].Value),
                        match.Index,
                        match.Index + match.Length
                    ));
                }
                else if (match.Groups["STRING"].Success)
                {
                    tokenList.Add(new Token(
                        TokenType.String,
                        TokenOperator.None,
                        match.Groups["STRING"].Value.Trim('"'),
                        match.Index,
                        match.Index + match.Length
                    ));
                }
                else if (match.Groups["OPERATOR"].Success)
                {
                    tokenList.Add(new Token(
                        TokenType.Operator,
                        ParseOperator(match.Groups["OPERATOR"].Value),
                        match.Groups["OPERATOR"].Value,
                        match.Index,
                        match.Index + match.Length
                    ));
                }
                else if (match.Groups["BRACKET"].Success)
                {
                    tokenList.Add(new Token(
                        TokenType.Bracket,
                        TokenOperator.None,
                        match.Groups["BRACKET"].Value,
                        match.Index,
                        match.Index + match.Length
                    ));
                }
                else if (match.Groups["IDENTIFIER"].Success)
                {
                    tokenList.Add(new Token(
                        TokenType.Identifier,
                        TokenOperator.None,
                        match.Groups["IDENTIFIER"].Value,
                        match.Index,
                        match.Index + match.Length
                    ));
                }
            }

            return tokenList;
        }

        private void ValidateTokens(List<Token> tokenList, string inline)
        {
            // Check for empty token list
            if (tokenList.Count == 0)
                throw new InvalidOperationException("No valid tokens found in the input.");

            // Check for balanced and properly ordered brackets
            var bracketStack = new Stack<Token>();
            foreach (var t in tokenList)
            {
                if (t.tokenType == TokenType.Bracket)
                {
                    var ch = t.value.ToString();
                    if (ch == "(" || ch == "{" || ch == "[")
                    {
                        bracketStack.Push(t);
                    }
                    else if (ch == ")" || ch == "}" || ch == "]")
                    {
                        if (bracketStack.Count == 0)
                            throw new InvalidOperationException($"Unmatched closing bracket '{ch}' at position {t.postion_start}.");
                        var opening = bracketStack.Pop().value.ToString();
                        if (!IsMatchingBracket(opening, ch))
                            throw new InvalidOperationException($"Mismatched brackets '{opening}' and '{ch}' at position {t.postion_start}.");
                    }
                }
            }
            if (bracketStack.Count > 0)
                throw new InvalidOperationException("Unmatched opening bracket(s).");

            // Check for invalid sequences: operator followed by operator, bracket sequences, etc.
            // We will ensure that no two operators occur consecutively (unless a bracket or identifier/number/string in between)
            // and that the expression doesn't start or end with a nonsensical token.
            Token prev = null;
            for (int i = 0; i < tokenList.Count; i++)
            {
                var current = tokenList[i];
                // Expression shouldn't start with an operator that isn't unary-friendly
                if (i == 0 && current.tokenType == TokenType.Operator && current.@operator != TokenOperator.Subtract)
                {
                    throw new InvalidOperationException($"Expression starts with an invalid operator '{current.value}' at position {current.postion_start}.");
                }

                // Check consecutive operators (e.g., "++", "+*", "&&/")
                if (prev != null && prev.tokenType == TokenType.Operator && current.tokenType == TokenType.Operator)
                {
                    throw new InvalidOperationException($"Invalid consecutive operators '{prev.value}' and '{current.value}' at position {current.postion_start}.");
                }

                // Check that after an operator, we get a valid token (not another operator or a closing bracket)
                if (prev != null && prev.tokenType == TokenType.Operator)
                {
                    // After an operator, we expect either a number, string, identifier, or an opening bracket
                    if (!(current.tokenType == TokenType.Number ||
                          current.tokenType == TokenType.String ||
                          current.tokenType == TokenType.Identifier ||
                          (current.tokenType == TokenType.Bracket && IsOpeningBracket(current.value.ToString()))))
                    {
                        throw new InvalidOperationException($"Invalid token '{current.value}' after operator '{prev.value}' at position {current.postion_start}.");
                    }
                }

                // Check that we don't have a closing bracket without a proper preceding token
                if (current.tokenType == TokenType.Bracket && IsClosingBracket(current.value.ToString()) && prev != null)
                {
                    // Before a closing bracket, we don't expect another operator (unless brackets handle it)
                    if (prev.tokenType == TokenType.Operator)
                    {
                        throw new InvalidOperationException($"Operator '{prev.value}' before closing bracket '{current.value}' at position {current.postion_start} is invalid.");
                    }
                }

                prev = current;
            }

            // The expression shouldn't end with an operator
            var lastToken = tokenList[^1];
            if (lastToken.tokenType == TokenType.Operator)
                throw new InvalidOperationException($"Expression ends with an operator '{lastToken.value}' which is invalid.");

        }

        private bool IsMatchingBracket(string opening, string closing)
        {
            return (opening == "(" && closing == ")") ||
                   (opening == "[" && closing == "]") ||
                   (opening == "{" && closing == "}");
        }

        private bool IsOpeningBracket(string bracket)
        {
            return bracket == "(" || bracket == "{" || bracket == "[";
        }

        private bool IsClosingBracket(string bracket)
        {
            return bracket == ")" || bracket == "}" || bracket == "]";
        }

        public string Parse()
        {
            object left_side = null;
            object right_side = null;
            TokenOperator opr = TokenOperator.None;
            object current_result = null;

            for (int i = 0; i < tokens.Count; i++)
            {
            retryleft:
                if (i + 1 >= tokens.Count)
                    continue;
                var left_token = tokens[i];
                _ = (left_token.tokenType) switch
                {
                    TokenType.String =>
                        left_side = left_token.value,
                    TokenType.Number =>
                           left_side = Convert.ToDouble(left_token.value),
                    TokenType.Identifier =>
                            left_side = GetVariable(left_token.value.ToString()!) ?? throw new InvalidOperationException(@$"invalid operation: ""{left_token.@operator}"" at {left_token.value}"),
                    TokenType.Bracket => i++,
                    TokenType.Operator when opr is TokenOperator.None && left_side is not null =>
                    opr = left_token.@operator,
                    TokenType.Operator when opr is not TokenOperator.None && left_side is null =>
                          throw new InvalidOperationException(@$"invalid operation: ""{left_token.@operator}"" at {left_token.value}"),
                    _ =>
                       throw new InvalidOperationException(@$"invalid operation: ""{left_token.@operator}"" at {left_token.value}"),

                };
                if (left_token.tokenType == TokenType.Bracket)
                {
                    goto retryleft;
                }
            retry_right:
                if (i + 1 <= tokens.Count && opr is not TokenOperator.None)
                {
                    var right_token = tokens[i + 1];

                    _ = (right_token.tokenType) switch
                    {
                        TokenType.String =>
                            right_side = right_token.value,
                        TokenType.Number =>
                               right_side = Convert.ToDouble(right_token.value),
                        TokenType.Identifier =>
                                right_side = GetVariable(right_token.value.ToString()!) ?? throw new InvalidOperationException(@$"invalid operation: ""{left_token.@operator}"" at {left_token.value}"),
                        TokenType.Bracket =>
                           i++,
                        _ =>
                           throw new InvalidOperationException(@$"invalid operation: ""{left_token.@operator}"" at {left_token.value}"),
                    };
                    if (right_token.tokenType == TokenType.Bracket)
                    {
                        goto retry_right;

                    }

                    current_result = Evaluate(current_result ?? left_side??default!, opr, right_side??default(object)!);
                    opr = TokenOperator.None;
                    i++;
                    continue;
                }
                else if (opr is not TokenOperator.None)
                {
                    throw new InvalidOperationException(@$"invalid operation: ""{left_token.@operator}"" at {left_token.value}");
                }
                current_result ??= left_side??"";

            }

            if (current_result is null)
                return _inline;
            return EditRange(_inline, tokens[0].postion_start, tokens[tokens.Count - 1].postion_end - 1, current_result!.ToString()!); ;
        }

        public static string EditRange(string input, int start, int end, string newValue)
        {
            if (start < 0 || end >= input.Length || start > end)
                throw new ArgumentOutOfRangeException("Invalid start or end positions.");

            // Replace the substring between start and end with newValue
            return input.Substring(0, start) + newValue + input.Substring(end + 1);
        }

        public static object Evaluate(object left, TokenOperator opar, object right)
        {
            if (opar == TokenOperator.None)
            {
                // If operator is None, just return the left value
                return left;
            }

            switch (opar)
            {
                case TokenOperator.Add:
                    {
                        try
                        {
                            return Convert.ToDouble(left) + Convert.ToDouble(right);
                        }
                        catch
                        {
                            return left.ToString() + right.ToString();
                        }
                    }

                case TokenOperator.Subtract:
                    return Convert.ToDouble(left) - Convert.ToDouble(right);

                case TokenOperator.Multiply:
                    return Convert.ToDouble(left) * Convert.ToDouble(right);

                case TokenOperator.Divide:
                    if (Convert.ToDouble(right) == 0)
                        throw new DivideByZeroException("Division by zero is not allowed.");
                    return Convert.ToDouble(left) / Convert.ToDouble(right);

                case TokenOperator.Modulus:
                    return Convert.ToDouble(left) % Convert.ToDouble(right);

                case TokenOperator.BitwiseAnd:
                    return Convert.ToInt32(left) & Convert.ToInt32(right);

                case TokenOperator.BitwiseOr:
                    return Convert.ToInt32(left) | Convert.ToInt32(right);

                case TokenOperator.LogicalAnd:
                    return Convert.ToBoolean(left) && Convert.ToBoolean(right);

                case TokenOperator.LogicalOr:
                    return Convert.ToBoolean(left) || Convert.ToBoolean(right);

                case TokenOperator.GreaterThan:
                    return Convert.ToDouble(left) > Convert.ToDouble(right);

                case TokenOperator.LessThan:
                    return Convert.ToDouble(left) < Convert.ToDouble(right);

                case TokenOperator.Equal:
                    return Equals(left, right);

                case TokenOperator.NotEqual:
                    return !Equals(left, right);

                default:
                    throw new NotImplementedException($"Operator '{opar}' is not implemented.");
            }
        }

        private TokenOperator ParseOperator(string op)
        {
            return op switch
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
                ">" => TokenOperator.GreaterThan,
                "<" => TokenOperator.LessThan,
                "==" => TokenOperator.Equal,
                "!=" => TokenOperator.NotEqual,
                _ => TokenOperator.None
            };
        }

    }
}
