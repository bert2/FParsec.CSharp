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
                    return OneOf(
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
            .ParseString("a").ShouldBeOk()
            .Matches("a")
            .ShouldBe(true);

        [Fact]
        public void MultipleChars() => SimpleRegexParser
            .ParseString("abc").ShouldBeOk()
            .Matches("abc")
            .ShouldBe(true);

        [Fact]
        public void AnyCharMatch() => SimpleRegexParser
            .ParseString("a.c").ShouldBeOk()
            .Matches("axc")
            .ShouldBe(true);

        [Fact]
        public void Grouping() => SimpleRegexParser
            .ParseString("()a((b)c)").ShouldBeOk()
            .Matches("abc")
            .ShouldBe(true);

        [Fact]
        public void KleeneStar() => SimpleRegexParser
            .ParseString("(ab)*c*").ShouldBeOk()
            .Matches("ababab")
            .ShouldBe(true);

        [Fact]
        public void KleenePlus() => SimpleRegexParser
            .ParseString("(ab)+c+").ShouldBeOk()
            .Matches("abababc")
            .ShouldBe(true);

        [Fact]
        public void Option() => SimpleRegexParser
            .ParseString("ab?c?").ShouldBeOk()
            .Matches("ac")
            .ShouldBe(true);

        [Fact]
        public void Alternation() => SimpleRegexParser
            .ParseString("a|b|c").ShouldBeOk()
            .Matches("c")
            .ShouldBe(true);

        [Fact]
        public void All() => SimpleRegexParser
            .ParseString("(a|b|c)+d*(ef|gh(.)+)?").ShouldBeOk()
            .Matches("abcabcghxxx")
            .ShouldBe(true);
    }
}
