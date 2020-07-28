using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class LambdaPattern : ConsumingPattern
{
   private const int PRIORITY = 19;

   private const int PARAMETERS = 0;
   private const int OPERATOR = 1;
   private const int BODY = 3;

   // Examples:
   // (a: num, b) => a + b - 10
   // x => x * x
   // y: System => y.start()
   // (z) { if z > 0 { => 1 } else => -1 }
   public LambdaPattern() : base(
      TokenType.CONTENT | TokenType.IDENTIFIER | TokenType.DYNAMIC,
      TokenType.OPERATOR,
      TokenType.END | TokenType.OPTIONAL,
      TokenType.CONTENT | TokenType.OPTIONAL
   ) {}

   public override bool Passes(Context context, List<Token> tokens)
   {
      if (tokens[OPERATOR].To<OperatorToken>().Operator != Operators.RETURN)
      {
         return false;
      }

      if (tokens[PARAMETERS].Type == TokenType.DYNAMIC && tokens[PARAMETERS].To<DynamicToken>().Node.Is(NodeType.VARIABLE_NODE) ||
            tokens[PARAMETERS].Type == TokenType.IDENTIFIER)
      {
         // Examples:
         // x => x * x
         // y: System => y.start()

         // Since this is a short lambda, there can not be curly brackets
         return tokens[BODY].Type == TokenType.NONE;
      }

      return tokens[BODY].Type == TokenType.CONTENT && 
               tokens[BODY].To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS;
   }

   private static ContentToken GetParameterTokens(List<Token> tokens)
   {
      return tokens[PARAMETERS].Type == TokenType.CONTENT 
         ? tokens[PARAMETERS].To<ContentToken>()
         : new ContentToken(tokens[PARAMETERS]);
   }

   public override Node? Build(Context context, List<Token> tokens, ConsumptionState state)
   {
      List<Token>? body;

      if (tokens[BODY].Type != TokenType.CONTENT)
      {
         // Consume the code if there is no curly brackets
			if (!state.IsConsumed)
			{
				// Consume only tokens which represent the following patterns
				state.Consume(new List<System.Type>
				{
					typeof(ArrayAllocationPattern),
					typeof(CastPattern),
					typeof(LinkPattern),
					typeof(NotPattern),
					typeof(OperatorPattern),
					typeof(PreIncrementAndDecrementPattern),
					typeof(UnarySignPattern),
				});
				
				return new Node();
			}

         body = new List<Token> { tokens[OPERATOR] };
         body.AddRange(tokens.GetRange(BODY + 1, tokens.Count - BODY - 1));

         if (body.Count <= 1)
         {
            throw Errors.Get(tokens[PARAMETERS].Position, "Short function doesn't have a body");
         }
      }
      else
      {
         // Example:
         // (a, b) { => a % b }
         body = tokens[BODY].To<ContentToken>().GetTokens();
      }

      var name = context.GetNextLambda().ToString(CultureInfo.InvariantCulture);

      // Create a function token manually since it contains some useful helper functions
      var function = new FunctionToken(
         new IdentifierToken(name),
         GetParameterTokens(tokens)
      );

      var lambda = new Function(
         context,
         AccessModifier.PUBLIC,
         name,
         body

      ) { Prefix = "Lambda" };

      // Lambdas usually capture variables from the parent context
      lambda.Link(lambda.Parent!);
      context.Declare(lambda);

      lambda.Parameters = function.GetParameters(lambda);

      if (lambda.Parameters.All(p => p.Type != null && !p.Type.IsUnresolved))
      {
         lambda.Implement(lambda.Parameters.Select(p => p.Type!));
      }

      return new LambdaNode(lambda);
   }

   public override int GetPriority(List<Token> tokens)
   {
      return PRIORITY;
   }
}