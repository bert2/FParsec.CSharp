namespace Tests {
    using System;
    using System.Linq;
    using FParsec;
    using FParsec.CSharp;
    using Microsoft.FSharp.Core;
    using Xunit;
    using static FParsec.CSharp.CharParsersCS;
    using static FParsec.CSharp.PrimitivesCS;

    public class ArithmeticExpression {
        #region Parser definitions

        public static FSharpFunc<CharStream<Unit>, Reply<int>> Integer = Many1(Digit).Map(string.Concat).Map(int.Parse);

        private static readonly FSharpFunc<CharStream<Unit>, Reply<int>> BasicExprParser = new OPPBuilder<int, Unit>()
            .WithOperators(ops => ops
                .AddInfix("+", 1, (x, y) => x + y)
                .AddInfix("*", 2, (x, y) => x * y))
            .WithTerms(Integer)
            .Build()
            .ExpressionParser;

        private static readonly FSharpFunc<CharStream<Unit>, Reply<int>> RecursiveExprParser = new OPPBuilder<int, Unit>()
            .WithOperators(ops => ops
                .AddInfix("+", 1, (x, y) => x + y)
                .AddInfix("*", 2, (x, y) => x * y))
            .WithTerms(term => OneOf(Integer, Between('(', term, ')')))
            .Build()
            .ExpressionParser;

        private static readonly FSharpFunc<CharStream<Unit>, Reply<int>> ExprParser =
            WS.And(new OPPBuilder<int, Unit>()
                .WithOperators(ops => ops
                    .AddInfix("+", 10, WS, (x, y) => x + y)
                    .AddInfix("-", 10, WS, (x, y) => x - y)
                    .AddInfix("*", 20, WS, (x, y) => x * y)
                    .AddInfix("/", 20, WS, (x, y) => x / y)
                    .AddPrefix("-", 20, x => -x)
                    .AddInfix("^", 30, Associativity.Right, WS, (x, y) => (int)Math.Pow(x, y))
                    .AddPostfix("!", 40, Factorial))
                .WithImplicitOperator(20, (x, y) => x * y)
                .WithTerms(term => OneOf(
                    Integer.And(WS),
                    Between(CharP('(').And(WS), term, CharP(')').And(WS))))
                .Build()
                .ExpressionParser);

        private static int Factorial(int n) => Enumerable.Range(1, n).Aggregate(1, (fac, x) => fac * x);

        #endregion Parser definitions

        #region Tests

        [Fact]
        public void Add() =>
            BasicExprParser
            .ParseString("1+1")
            .ShouldBe(2);

        [Fact]
        public void Mul() =>
            BasicExprParser
            .ParseString("2*3")
            .ShouldBe(6);

        [Fact]
        public void ChainingAdds() =>
            BasicExprParser
            .ParseString("1+2+3")
            .ShouldBe(6);

        [Fact]
        public void MulPrecedesAdd() =>
            BasicExprParser
            .ParseString("1+2*3")
            .ShouldBe(7);

        [Fact]
        public void Parentheses() =>
            RecursiveExprParser
            .ParseString("((1+2)*(3))")
            .ShouldBe(9);

        [Fact]
        public void ImplicitMul() => ExprParser
            .ParseString("3 4")
            .ShouldBe(12);

        [Fact]
        public void ImplicitMulOfParentheses() => ExprParser
            .ParseString("(2 3)(4 - 2)")
            .ShouldBe(12);

        [Fact]
        public void Negation() =>
            ExprParser
            .ParseString("-3")
            .ShouldBe(-3);

        [Fact]
        public void NegPrecedesAdd() =>
            ExprParser
            .ParseString("-3+2")
            .ShouldBe(-1);

        [Fact]
        public void NegOnRightSideOfSub() =>
            ExprParser
            .ParseString("3--2")
            .ShouldBe(5);

        [Fact]
        public void NegNotAssociative() =>
            ExprParser
            .ParseString("--3")
            .ShouldBe<ErrorMessage.Message>("The prefix operator '-' (precedence: 20, non-associative) conflicts with the prefix operator '-' (precedence: 20, non-associative) on the same line at column 1.");

        [Fact]
        public void Exponent() =>
            ExprParser
            .ParseString("2^3")
            .ShouldBe(8);

        [Fact]
        public void ExpPrecedesNeg() =>
            ExprParser
            .ParseString("-3^2")
            .ShouldBe(-9);

        [Fact]
        public void ExpIsRightAssociative() =>
            ExprParser
            .ParseString("4^3^2")
            .ShouldBe(262144);

        [Fact]
        public void Fact() =>
            ExprParser
            .ParseString("10!")
            .ShouldBe(3628800);

        [Fact]
        public void FactIsNotAssociative() =>
            ExprParser
            .ParseString("10!!")
            .ShouldBe<ErrorMessage.Message>("The postfix operator '!' (precedence: 40, non-associative) conflicts with the postfix operator '!' (precedence: 40, non-associative) on the same line at column 3.");

        [Fact]
        public void FactPrecedesExp() =>
            ExprParser
            .ParseString("2^3!")
            .ShouldBe(64);

        [Fact]
        public void WithWhitespace() =>
            ExprParser
            .ParseString(" ( 1 + -2 ) * 3! ")
            .ShouldBe(-6);

        #endregion Tests
    }
}
