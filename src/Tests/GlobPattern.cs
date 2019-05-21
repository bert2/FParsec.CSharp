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
            Many(Choice(
                Skip('?').Map(NFA.MakeAnyChar),
                Skip('*').Map(NFA.MakeAnyChar).Map(NFA.MakeZeroOrMore),
                Between('[', AnyChar.And(Skip('-')).And(AnyChar), ']').Lbl("character range").Map(NFA.MakeCharRange),
                Skip('\\').And(AnyOf(@"?*[]\").Lbl("meta character")).Map(NFA.MakeChar),
                AnyChar.Map(NFA.MakeChar)))
            .And(EOF)
            .Map(NFA.Concat)
            .Map(proto => proto(new Final()));

        [Fact]
        public void SingleChar() => GlobParser
            .ParseString("a").OkResult()
            .Matches("a")
            .ShouldBe(true);

        [Fact]
        public void MultipleChars() => GlobParser
            .ParseString("abc").OkResult()
            .Matches("abc")
            .ShouldBe(true);

        [Fact]
        public void AnyCharWildcard() => GlobParser
            .ParseString("a?c").OkResult()
            .Matches("abc")
            .ShouldBe(true);

        [Fact]
        public void AnyStringWildcard() => GlobParser
            .ParseString("a*d").OkResult()
            .Matches("abcd")
            .ShouldBe(true);

        [Fact]
        public void CharacterClass() => GlobParser
            .ParseString("[0-9]").OkResult()
            .Matches("3")
            .ShouldBe(true);

        [Fact]
        public void EscapedChar() => GlobParser
            .ParseString(@"a\[c").OkResult()
            .Matches("a[c")
            .ShouldBe(true);

        [Fact]
        public void All() => GlobParser
            .ParseString(@"The * syntax allows wildcards (\? and \*) and character classes (\[0-9\]). [A-Z]ackslash \\ is the escape character?").OkResult()
            .Matches(@"The glob syntax allows wildcards (? and *) and character classes ([0-9]). Hackslash \ is the escape character!")
            .ShouldBe(true);
    }
}
