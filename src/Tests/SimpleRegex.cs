namespace Tests {
    using FParsec;
    using FParsec.CSharp;
    using Microsoft.FSharp.Core;
    using Shouldly;
    using Xunit;
    using static FParsec.CSharp.CharParsersCS;
    using static FParsec.CSharp.PrimitivesCS;

    public class SimpleRegex {
        private static readonly FSharpFunc<CharStream<Unit>, Reply<IState>> SimpleRegexParser =
            Many(new OPPBuilder<NFA.ProtoState, Unit>()
                .WithImplicitOperator(2, NFA.Connect)
                .WithOperators(ops => ops
                    .AddPostfix("*", 3, NFA.MakeZeroOrMore)
                    .AddPostfix("+", 3, NFA.MakeOneOrMore)
                    .AddPostfix("?", 3, NFA.MakeZeroOrOne)
                    .AddInfix("|", 1, NFA.MakeAlternation))
                .WithTerms(matchExpr => {
                    var group = Between('(', Many(matchExpr), ')');
                    var wildcard = Skip('.');
                    var charMatch = NoneOf("*+?|()");
                    return Choice(
                        group.Map(NFA.Concat),
                        wildcard.Map(NFA.MakeAnyChar),
                        charMatch.Map(NFA.MakeChar));
                })
                .Build()
                .ExpressionParser)
            .And(EOF)
            .Map(NFA.Concat)
            .Map(proto => proto(new Final()));

        [Fact]
        public void SingleChar() => SimpleRegexParser
            .ParseString("a").OkResult()
            .Matches("a")
            .ShouldBe(true);

        [Fact]
        public void MultipleChars() => SimpleRegexParser
            .ParseString("abc").OkResult()
            .Matches("abc")
            .ShouldBe(true);

        [Fact]
        public void AnyCharMatch() => SimpleRegexParser
            .ParseString("a.c").OkResult()
            .Matches("axc")
            .ShouldBe(true);

        [Fact]
        public void Grouping() => SimpleRegexParser
            .ParseString("()a((b)c)").OkResult()
            .Matches("abc")
            .ShouldBe(true);

        [Fact]
        public void KleeneStar() => SimpleRegexParser
            .ParseString("(ab)*c*").OkResult()
            .Matches("ababab")
            .ShouldBe(true);

        [Fact]
        public void KleenePlus() => SimpleRegexParser
            .ParseString("(ab)+c+").OkResult()
            .Matches("abababc")
            .ShouldBe(true);

        [Fact]
        public void Option() => SimpleRegexParser
            .ParseString("ab?c?").OkResult()
            .Matches("ac")
            .ShouldBe(true);

        [Fact]
        public void Alternation() => SimpleRegexParser
            .ParseString("a|b|c").OkResult()
            .Matches("c")
            .ShouldBe(true);

        [Fact]
        public void All() => SimpleRegexParser
            .ParseString("(a|b|c)+d*(ef|gh(.)+)?").OkResult()
            .Matches("abcabcghxxx")
            .ShouldBe(true);
    }
}
