namespace Tests {
    using FParsec;
    using FParsec.CSharp;
    using Microsoft.FSharp.Core;
    using Shouldly;
    using Xunit;
    using static FParsec.CSharp.CharParsersCS;
    using static FParsec.CSharp.PrimitivesCS;

    public class GlobPattern {
        private static readonly FSharpFunc<CharStream<Unit>, Reply<IState>> GlobParser =
            Many(OneOf(
                Skip('?').Map(NFA.MakeAnyChar),
                Skip('*').Map(NFA.MakeAnyChar).Map(NFA.MakeZeroOrMore),
                Between('[', AnyChar.And(Skip('-')).And(AnyChar), ']').Map(NFA.MakeCharRange),
                Skip('\\').And(AnyOf(@"?*[]\")).Map(NFA.MakeChar),
                AnyChar.Map(NFA.MakeChar)))
            .And(EOF)
            .Map(NFA.Concat)
            .Map(proto => proto(new Final()));

        [Fact]
        public void SingleChar() => GlobParser
            .ParseString("a").ShouldBeOk()
            .Matches("a")
            .ShouldBe(true);

        [Fact]
        public void MultipleChars() => GlobParser
            .ParseString("abc").ShouldBeOk()
            .Matches("abc")
            .ShouldBe(true);

        [Fact]
        public void AnyCharWildcard() => GlobParser
            .ParseString("a?c").ShouldBeOk()
            .Matches("abc")
            .ShouldBe(true);

        [Fact]
        public void AnyStringWildcard() => GlobParser
            .ParseString("a*d").ShouldBeOk()
            .Matches("abcd")
            .ShouldBe(true);

        [Fact]
        public void CharacterClass() => GlobParser
            .ParseString("[0-9]").ShouldBeOk()
            .Matches("3")
            .ShouldBe(true);

        [Fact]
        public void EscapedChar() => GlobParser
            .ParseString(@"a\[c").ShouldBeOk()
            .Matches("a[c")
            .ShouldBe(true);

        [Fact]
        public void All() => GlobParser
            .ParseString(@"The * syntax allows wildcards (\? and \*) and character classes (\[0-9\]). [A-Z]ackslash \\ is the escape character?").ShouldBeOk()
            .Matches(@"The glob syntax allows wildcards (? and *) and character classes ([0-9]). Hackslash \ is the escape character!")
            .ShouldBe(true);
    }
}
