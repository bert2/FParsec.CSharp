using System;
using System.Collections.Generic;
using FParsec;
using FParsec.CSharp;
using Microsoft.FSharp.Core;
using Shouldly;
using Xunit;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;
using RTE = System.Collections.Generic.Dictionary<string, Tests.Script>;
using Microsoft.FSharp.Collections;
using System.Linq;

namespace Tests {
    using StringParser = FSharpFunc<CharStream<Unit>, Reply<string>>;
    using ScriptParser = FSharpFunc<CharStream<Unit>, Reply<Script>>;

    // This example implements a simple functional script language. It only knows one type (`int`)
    // and is super inefficient, but it has lots of functional fu (e.g. lazy evaluation, partial
    // application, lambdas, higher order functions, and function composition).
    public delegate int Script(FSharpList<Script> args, RTE rte);

    public class SimpleScriptParser {
        #region Parser definition

        private static readonly FSharpFunc<CharStream<Unit>, Reply<Script>> ScriptParser;

        static SimpleScriptParser() {
            var number = Natural.Lbl("number");

            static StringParser notReserved(string id) => id == "let" || id == "in" || id == "match" ? Zero<string>() : Return(id);
            var identifier1 = Choice(Letter, CharP('_'));
            var identifierRest = Choice(Letter, CharP('_'), CharP('\''), Digit);
            var identifier = Purify(Many1Chars(identifier1, identifierRest)).AndTry(notReserved).Lbl("identifier");

            var parameters = Many(identifier, sep: WS1, canEndWithSep: true).Lbl("parameter list");

            ScriptParser? expression = null;

            var letBinding =
                Skip("let").AndR(WS1)
                .And(identifier).And(WS)
                .And(parameters)
                .And(Skip('=')).And(WS)
                .And(Rec(() => expression).Lbl("'let' definition expression"))
                .And(Skip("in")).And(WS1)
                .And(Rec(() => expression).Lbl("'let' body expression"))
                .Map(Flat)
                .Lbl("'let' binding");

            var lambda =
                Skip('\\')
                .And(parameters)
                .And(Skip("->")).And(WS)
                .And(Rec(() => expression).Lbl("lambda body"))
                .Lbl("lambda");

            var defaultCase = Skip('_').AndRTry(NotFollowedBy(identifierRest)).AndR(WS).Return(ScriptB.AlwaysMatches);
            var caseValueExpr = Rec(() => expression).Map(ScriptB.Matches);
            var caseExpr =
                Skip('|').AndR(WS)
                .And(defaultCase.Or(caseValueExpr).Lbl("case value expression"))
                .And(Skip("=>")).And(WS)
                .And(Rec(() => expression).Lbl("case result expression"))
                .Lbl("match case");
            var matchExpr =
                Skip("match").AndR(WS1)
                .And(Rec(() => expression).Lbl("match value expression"))
                .And(Many1(caseExpr))
                .Lbl("'match' expression");

            expression = new OPPBuilder<Unit, Script, Unit>()
                .WithOperators(ops => ops
                    .AddInfix("+", 10, WS, ScriptB.Lift2((x, y) => x + y))
                    .AddInfix("-", 10, WS, ScriptB.Lift2((x, y) => x - y))
                    .AddInfix("*", 20, WS, ScriptB.Lift2((x, y) => x * y))
                    .AddInfix("/", 20, WS, ScriptB.Lift2((x, y) => x / y))
                    .AddInfix("%", 20, WS, ScriptB.Lift2((x, y) => x % y))
                    .AddPrefix("-", 20, ScriptB.Lift(x => -x))
                    .AddInfix(".", 30, Associativity.Right, WS, ScriptB.Compose))
                .WithImplicitOperator(50, ScriptB.Apply)
                .WithTerms(term =>
                    Choice(
                        letBinding.Map(ScriptB.BindVar),
                        matchExpr.Map(ScriptB.Match),
                        Between(CharP('(').And(WS), term, CharP(')').And(WS)),
                        number.And(WS).Map(ScriptB.Return),
                        identifier.And(WS).Map(ScriptB.Resolve),
                        lambda.Map(ScriptB.Lambda))
                    .Lbl("expression"))
                .Build()
                .ExpressionParser;

            ScriptParser = WS.And(expression).And(EOF);
        }

        #endregion Parser definition

        #region Tests

        #region Examples

        [Fact] public void HelloWorld() => ScriptParser
            .Run("42")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(42);

        [Fact] public void Variable() => ScriptParser
            .Run(@"
                let x = 6 in
                x * 7")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(42);

        [Fact] public void Function() => ScriptParser
            .Run(@"
                let negateOdd n =
                    match n % 2
                    | 0 => n
                    | 1 => -n
                in
                negateOdd 7")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(-7);

        [Fact] public void FibonacciNumber() => ScriptParser
            .Run(@"
                let fib n = match n
                    | 0 => 0
                    | 1 => 1
                    | _ => fib (n-1) + fib (n-2)
                in fib 7")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(13);

        [Fact] public void SkkReducesToI() => ScriptParser
            .Run(@"
                let s f g x = f x (g x) in
                let k x y   = x         in
                let skk     = s k k     in

                let f x     = x * x     in
                let f'      = skk f     in
                f' 4")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(16);

        [Fact] public void YCombinator() => ScriptParser
            .Run(@"
                let y f = (\x -> f (x x)) (\x -> f (x x)) in
                
                let factGen fact n = match n
                                     | 0 => 1
                                     | _ => n * fact (n-1)
                in let fact = y factGen
                in fact 7")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(5040);

        [Fact] public void YCombinatorDefinedRecursively() => ScriptParser
            .Run(@"
                let y f = f (y f) in
                
                let factGen fact n = match n
                                     | 0 => 1
                                     | _ => n * fact (n-1)
                in let fact = y factGen
                in fact 7")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(5040);

        [Fact] public void YCombinatorDefinedWithSK() => ScriptParser
            .Run(@"
                let s f g x = f x (g x)                  in
                let k x y   = x                          in
                let ss      = s s                        in
                let ssk     = ss k                       in
                let y       = ssk (s (k (ss (s ssk))) k) in

                let factGen fact n = match n
                                     | 0 => 1
                                     | _ => n * fact (n-1)
                in let fact = y factGen
                in fact 7")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(5040);

        [Fact] public void FixPointCombinator() => ScriptParser
            .Run(@"
                let fix f = f (\x -> (fix f) x) in
                
                let fact = fix (\fact n -> match n
                                           | 0 => 1
                                           | _ => n * fact (n-1))
                in fact 7")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(5040);

        #endregion Examples

        #region Unbound expressions

        [Fact] public void UnboundValue() => ScriptParser
            .Run("-25")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(-25);

        [Fact] public void UnboundArithmeticExpr() => ScriptParser
            .Run("-2 + 3 - (2 - (3 - 4))")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(-2);

        #endregion Unbound expressions

        #region 'let' bindings

        [Fact] public void LetBinding() => ScriptParser
            .Run("let x = 3 in x")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(3);

        [Fact] public void LetBindingsCanBeInsideArithmeticExpr() => ScriptParser
            .Run("1 + (let x = 2 in x + 3) * 4")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(21);

        [Fact] public void LetBindingsCanBeChained() => ScriptParser
            .Run(@"
                let x = 4 in
                let y = x in
                y")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(4);

        [Fact] public void LetBindingsCanBeNested() => ScriptParser
            .Run(@"
                let x =
                    let y = 5
                    in  y
                in x")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(5);

        [Fact] public void DuplicateChainedLetBindingShadowsOriginal() => ScriptParser
            .Run(@"
                let x = 1 in
                let x = 2 in
                x")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(2);

        [Fact] public void DuplicateNestedLetBindingShadowsOriginal() => ScriptParser
            .Run(@"
                let x = 1 in
                let y = let x = 2 in x
                in y")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(2);

        [Fact] public void DuplicateNestedLetBindingDoesNotLeakToOuterScope() => ScriptParser
            .Run(@"
                let x = 1 in
                let y = let x = 2 in x
                in x + y")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(3);

        [Fact] public void VarsMustBeBound() => new Action(() => ScriptParser
            .Run("x")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE()))
            .ShouldThrow<KeyNotFoundException>();

        [Fact] public void VarsMustBeBoundBeforeUsage() => new Action(() => ScriptParser
            .Run(@"
                let y = x in
                let x = 2 in
                y")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE()))
            .ShouldThrow<KeyNotFoundException>();

        [Fact] public void NestedBindingCanAccessOuterScope() => ScriptParser
            .Run(@"
                let x = 6
                in let y =
                    let z = x in z
                in y")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(6);

        [Fact] public void NestedBindingCannotAccessVariableFromOuterScopeThatsBoundAfterIt() => new Action(() => ScriptParser
            .Run(@"
                let y =
                    let z = x in z
                in let x = 7
                in y")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE()))
            .ShouldThrow<KeyNotFoundException>();

        [Fact] public void NestedBindingsDontLeakToOuterScope() => new Action(() => ScriptParser
            .Run(@"
                let x =
                    let y = 8
                    in  y
                in x + y")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE()))
            .ShouldThrow<KeyNotFoundException>();

        #endregion 'let' bindings

        #region Functions

        [Fact] public void FunctionWithArgValue() => ScriptParser
            .Run(@"
                let inc x = x + 1 in
                inc 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(3);

        [Fact] public void FunctionWithArgExpr() => ScriptParser
            .Run(@"
                let inc x = x + 1 in
                inc (1 * 2)")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(3);

        [Fact] public void FunctionWithTwoParameters() => ScriptParser
            .Run(@"
                let sub x y = x - y in
                sub 5 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(2);

        [Fact] public void FunctionCallingOtherFunction() => ScriptParser
            .Run(@"
                let add a b = a + b in
                let inc x = add 1 x in
                inc 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(3);

        [Fact] public void FunctionCallingNestedFunction() => ScriptParser
            .Run(@"
                let inc x =
                    let add a b = a + b in
                    add 1 x
                in inc 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(3);

        [Fact] public void RecursiveFunction() => ScriptParser
            .Run(@"
                let toZero x = match x
                    | 0 => 0
                    | _ => toZero (x - 1)
                in toZero 7")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(0);

        [Fact] public void ApplyingTooManyArguments() => new Action(() => ScriptParser
            .Run(@"
                let add x y = x + y in
                add 1 2 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE()))
            .ShouldThrow<InvalidOperationException>();

        [Fact] public void ApplyingTooFewArguments() => new Action(() => ScriptParser
            .Run(@"
                let add x y = x + y in
                add 1")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE()))
            .ShouldThrow<InvalidOperationException>();

        [Fact] public void ApplyingWithoutSpace() => ScriptParser
            .Run(@"
                let inc x = x + 1 in
                inc(2)")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(3);

        [Fact] public void DuplicateParameterShadowsOriginalFromOuterScope() => ScriptParser
            .Run(@"
                let x = 1 in
                let add x y = x + y in
                add 2 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(5);

        [Fact] public void DuplicateParameterShadowsOriginal() => ScriptParser
            .Run(@"
                let add x x = x + x in
                add 1 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(4);

        [Fact] public void ParameterScopesOfChainedFunctionsAreSeparated() => ScriptParser
            .Run(@"
                let add x y = x + y in
                let inc x = add 1 x in
                inc 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(3);

        [Fact] public void ParameterScopesOfNestedFunctionsAreSeparated() => ScriptParser
            .Run(@"
                let inc x =
                    let add x y = x + y in
                    add 1 x
                in inc 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(3);

        [Fact] public void PartialApplication() => ScriptParser
            .Run(@"
                let add x y = x + y in
                let inc = add 1 in
                inc 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(3);

        [Fact] public void FunctionComposition() => ScriptParser
            .Run(@"
                let inc x = x + 1 in
                let square x = x * x in
                let incSquared x = inc (square x) in
                incSquared 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(10);

        [Fact] public void FunctionCompositionWithInfixOperator() => ScriptParser
            .Run(@"
                let inc x = x + 1 in
                let square x = x * x in
                (inc . square) 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(10);

        [Fact] public void FunctionCompositionWithInfixOperatorAndPartialApplication() => ScriptParser
            .Run(@"
                let inc x = x + 1 in
                let square x = x * x in
                let incSquared = inc . square in
                incSquared 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(10);

        [Fact] public void FunctionCompositionWithInfixOperatorAndInlinePartialApplication() => ScriptParser
            .Run(@"
                let add x y = x + y in
                let square x = x * x in
                (add 1 . square) 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(10);

        [Fact] public void ApplicationHasHigherPrecedenceThanMul() => ScriptParser
            .Run(@"
                let inc x = x + 1 in
                inc 1 * 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(4);

        [Fact] public void CannotApplyArgumentToConstant() => new Action(() => ScriptParser
            .Run("1 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE()))
            .ShouldThrow<InvalidOperationException>();

        [Fact] public void CannotApplyArgumentToValueVariable() => new Action(() => ScriptParser
             .Run("let x = 1 in x 2")
             .GetResult()
             .Invoke(FSharpList<Script>.Empty, new RTE()))
            .ShouldThrow<InvalidOperationException>();

        [Fact] public void HigherOrderFunction() => ScriptParser
            .Run(@"
                let map f x = f x in
                let square x = x * x in
                map square 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(9);

        #endregion Functions

        #region 'match' expressions

        [Fact] public void MatchCase() => ScriptParser
            .Run("match 0 | 1 => 1 | 0 => 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(2);

        [Fact] public void MatchDefaultCase() => ScriptParser
            .Run("match 0 | 1 => 1 | _ => 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(2);

        [Fact] public void MatchVar() => ScriptParser
            .Run(@"
                let x = 2 in
                match 2
                | x => 1
                | _ => 0")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(1);

        [Fact] public void MatchAnExpression() => ScriptParser
            .Run(@"
                let x = 1 in
                match 2
                | x+1 => 1
                | _   => 0")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(1);

        [Fact] public void NoMatchFound() => new Action(() => ScriptParser
            .Run("match 1 | 0 => 0")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE()))
            .ShouldThrow<InvalidOperationException>();

        #endregion 'match' expressions

        #region Lambdas

        [Fact] public void FunctionAsLambda() => ScriptParser
            .Run(@"let f = \x -> x * x
                   in f 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(9);

        [Fact] public void MultiParamFunctionAsNestedLambda() => ScriptParser
            .Run(@"let f = \x -> \y -> x + y
                   in f 3 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(5);

        [Fact] public void MultiParamFunctionAsMultiParamLambda() => ScriptParser
            .Run(@"let f = \x y -> x + y
                   in f 3 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(5);

        [Fact]
        public void LambdaCanAccessOuterScope() => ScriptParser
            .Run(@"let f x = \y -> x + y
                   in f 3 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(5);

        [Fact] public void LambdaParamShadowsOuterScope() => ScriptParser
            .Run(@"let f x = \x -> x
                   in f 3 2")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(2);

        [Fact] public void LambdaAsArg() => ScriptParser
            .Run(@"let map f x = f x
                   in map (\x -> x * x) 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(9);

        [Fact] public void InlineDefinitionOfAppliedFunction() => ScriptParser
            .Run(@"(\x -> x * x) 3")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(9);

        #endregion Lambdas

        #region Misc

        [Fact] public void ExpressionsAreEvaluatedLazy() => ScriptParser
            .Run("let x = z in 9")
            .GetResult()
            .Invoke(FSharpList<Script>.Empty, new RTE())
            .ShouldBe(9);

        [Fact] public void KeywordLetIsReserved() => ScriptParser
            .Run("let let = 1 in 1")
            .ToFailure().Error().Messages.AsEnumerable()
            .ShouldContain(new ErrorMessage.Expected("identifier"));

        [Fact] public void KeywordInIsReserved() => ScriptParser
            .Run("let in = 1 in 1")
            .ToFailure().Error().Messages.AsEnumerable()
            .ShouldContain(new ErrorMessage.Expected("identifier"));

        [Fact] public void KeywordMatchIsReserved() => ScriptParser
            .Run("let match = 1 in 1")
            .ToFailure().Error().Messages.AsEnumerable()
            .ShouldContain(new ErrorMessage.Expected("identifier"));

        #endregion Misc

        #region Parser errors

        [Fact] public void EmptyProgram() => new Action(() => ScriptParser
            .Run("").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 1\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: expression or prefix operator\r\n");

        [Fact] public void IncompleteArithmeticExpr() => new Action(() => ScriptParser
            .Run("1 +").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 4\r\n1 +\r\n   ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: expression or prefix operator\r\n");

        [Fact] public void IncompleteLetBinding1() => new Action(() => ScriptParser
            .Run("let").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 4\r\nlet\r\n   ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: whitespace\r\n");

        [Fact] public void IncompleteLetBinding2() => new Action(() => ScriptParser
            .Run("let ").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 5\r\nlet \r\n    ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: identifier\r\n");

        [Fact] public void IncompleteLetBinding3() => new Action(() => ScriptParser
            .Run("let x").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 6\r\nlet x\r\n     ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: parameter list or '='\r\n");

        [Fact] public void IncompleteLetBinding31() => new Action(() => ScriptParser
            .Run("let x a").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 8\r\nlet x a\r\n       ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: whitespace or '='\r\n");

        [Fact] public void IncompleteLetBinding4() => new Action(() => ScriptParser
            .Run("let x =").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 8\r\nlet x =\r\n       ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: 'let' definition expression\r\n");

        [Fact] public void IncompleteLetBinding5() => new Action(() => ScriptParser
            .Run("let x = 1").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 10\r\nlet x = 1\r\n         ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: expression, infix operator or 'in'\r\n");

        [Fact] public void IncompleteLetBinding6() => new Action(() => ScriptParser
            .Run("let x = 1 ").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 11\r\nlet x = 1 \r\n          ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: expression, infix operator or 'in'\r\n");

        [Fact] public void IncompleteLetBinding7() => new Action(() => ScriptParser
            .Run("let x = 1 i").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 12\r\nlet x = 1 i\r\n           ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: expression, infix operator or 'in'\r\n");

        [Fact] public void IncompleteLetBinding8() => new Action(() => ScriptParser
            .Run("let x = 1 in").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 13\r\nlet x = 1 in\r\n            ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: whitespace\r\n");

        [Fact] public void IncompleteLetBinding9() => new Action(() => ScriptParser
            .Run("let x = 1 in ").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 14\r\nlet x = 1 in \r\n             ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: 'let' body expression\r\n");

        [Fact] public void IncompleteMatchExpr1() => new Action(() => ScriptParser
            .Run("match").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 6\r\nmatch\r\n     ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: whitespace\r\n");

        [Fact] public void IncompleteMatchExpr2() => new Action(() => ScriptParser
            .Run("match 1").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 8\r\nmatch 1\r\n       ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: expression, infix operator or match case\r\n");

        [Fact] public void IncompleteMatchExpr3() => new Action(() => ScriptParser
            .Run("match 1 |").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 10\r\nmatch 1 |\r\n         ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: case value expression\r\n");

        [Fact] public void IncompleteMatchExpr4() => new Action(() => ScriptParser
            .Run("match 1 | 1").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 12\r\nmatch 1 | 1\r\n           ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: expression, infix operator or '=>'\r\n");

        [Fact] public void IncompleteMatchExpr5() => new Action(() => ScriptParser
            .Run("match 1 | 1 =").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 13\r\nmatch 1 | 1 =\r\n            ^\r\nExpecting: expression, infix operator or '=>'\r\n");

        [Fact] public void IncompleteMatchExpr6() => new Action(() => ScriptParser
            .Run("match 1 | 1 =>").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 15\r\nmatch 1 | 1 =>\r\n              ^\r\nNote: The error occurred at the end of the input stream.\r\nExpecting: case result expression\r\n");

        [Fact] public void ReservedWordAsIdentifier() => new Action(() => ScriptParser
            .Run("let let = 1").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 5\r\nlet let = 1\r\n    ^\r\nExpecting: identifier\r\n");

        [Fact] public void ReservedWordAsParameterName() => new Action(() => ScriptParser
            .Run("let x let = 1").GetResult())
            .ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("Error in Ln: 1 Col: 7\r\nlet x let = 1\r\n      ^\r\nExpecting: parameter list or '='\r\n");

        #endregion Parser errors

        #endregion Tests
    }

    #region Script builder

    public static class ScriptB {
        public static Script Return(int x) => new Script((args, _) => args.IsEmpty ? x : Throw($"Cannot apply argument to value '{x}'"));

        public static Func<Script, Script> Lift(Func<int, int> f) => x => new Script((args, rte) => f(x(args, rte)));

        public static Func<Script, Script, Script> Lift2(Func<int, int, int> f) => (x, y) => new Script((args, rte) => f(x(args, rte), y(args, rte)));

        public static Script Compose(Script f, Script g) => new Script((args, rte) => f(args.Tail.Prepend(Return(g(args, rte))), rte));

        public static Script Lambda(FSharpList<string> @params, Script body) => new Script((args, rte) => body.BindArgs(@params)(args, rte));

        public static Script BindVar(string id, FSharpList<string> @params, Script def, Script body) => new Script((args, rte) => {
            RTE? _rte = null;
            _rte = rte.CloneWith(id, def.BindArgs(@params).Capture(() => _rte));
            return body(args, _rte);
        });

        public static Script BindArgs(this Script def, FSharpList<string> @params) => @params
            .Reverse()
            .Aggregate(def, (s, param) => new Script((args, rte) => s(args.Tail, rte.CloneWith(param, args.Head))));

        public static Script Resolve(string id) => new Script((arg, rte) => rte[id](arg, rte));

        public static Script Apply(Script func, Script arg) => new Script((args, rte) => func(args.Prepend(arg.Capture(() => rte)), rte));

        public static Func<Script, FSharpList<Script>, RTE, bool> Matches(Script x) => (y, args, rte) => x(args, rte) == y(args, rte);

        public static Func<Script, FSharpList<Script>, RTE, bool> AlwaysMatches = (_, __, ___) => true;

        public static Script Match(Script value, FSharpList<(Func<Script, FSharpList<Script>, RTE, bool> matches, Script caseResult)> cases)
            => new Script((args, rte) => cases
                .First(c => c.matches(value, args, rte))
                .caseResult(args, rte));

        public static Script Capture(this Script s, RTE rte) => new Script((arg, _) => s(arg, rte));

        public static Script Capture(this Script s, Func<RTE?> rte) => new Script((arg, _) => s(arg, rte()!.AssertNotNull()));

        public static RTE CloneWith(this RTE rte, string id, Script s) => new RTE(rte) { [id] = s };

        private static int Throw(string message) => throw new InvalidOperationException(message);

        private static T AssertNotNull<T>(this T x) => x != null ? x : throw new InvalidOperationException();
    }

    #endregion Script builder
}
