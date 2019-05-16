namespace Tests {
    using FParsec;
    using FParsec.CSharp;
    using Microsoft.FSharp.Core;
    using Xunit;
    using static FParsec.CSharp.CharParsersCS;
    using static FParsec.CSharp.PrimitivesCS;
    using Chars = FParsec.CharStream<Microsoft.FSharp.Core.Unit>;
    using static FParsec.CharParsers;

    public class BasicExamples {
        #region Chars

        [Fact]
        public void ParseAnyChar() =>
            AnyChar
            .ParseString("x")
            .ShouldBe('x');

        [Fact]
        public void SingleChar() =>
            CharP('a')
            .ParseString("a")
            .ShouldBe('a');

        [Fact]
        public void CharThatSatisfiesPredicate() =>
            CharP(char.IsDigit)
            .ParseString("1")
            .ShouldBe('1');

        [Fact]
        public void ParseAndSkipChar() =>
            Skip('a')
            .ParseString("a")
            .ShouldBe(null);

        [Fact]
        public void MatchAnyOfList() =>
            AnyOf("0123456789")
            .ParseString("7")
            .ShouldBe('7');

        [Fact]
        public void InvertedMatch() =>
            NoneOf("abc")
            .ParseString("d")
            .ShouldBe('d');

        [Fact]
        public void ParseLetter() =>
            Letter
            .ParseString("A")
            .ShouldBe('A');

        [Fact]
        public void ParseDigit() =>
            Digit
            .ParseString("7")
            .ShouldBe('7');

        #endregion Chars

        #region Strings

        [Fact]
        public void ParseString() =>
            StringP("hello parser")
            .ParseString("hello parser")
            .ShouldBe("hello parser");

        [Fact]
        public void ParseStringIgnoreCase() =>
            StringCI("hello parser")
            .ParseString("Hello PARSER")
            .ShouldBe("Hello PARSER");

        [Fact]
        public void ParseAndSkipString() =>
            Skip("abc")
            .ParseString("abc")
            .ShouldBe(null);

        [Fact]
        public void ParseAndSkipStringIgnoreCase() =>
            SkipCI("hello parser")
            .ParseString("Hello PARSER")
            .ShouldBe(null);

        #endregion Strings

        #region Numbers

        [Fact]
        public void ParseInt() =>
            Int
            .ParseString("13")
            .ShouldBe(13);

        [Fact]
        public void ParseLong() =>
            Long
            .ParseString("9223372036854775807")
            .ShouldBe(9223372036854775807);

        [Fact]
        public void ParseFloat() =>
            Float
            .ParseString("13.45")
            .ShouldBe(13.45);

        #endregion Numbers

        #region White space

        [Fact]
        public void ZeroOrMoreWhitespaces() =>
            Spaces
            .ParseString("\n \t \r\n \r")
            .ShouldBe(null);

        [Fact]
        public void OneOreMoreWhitespaces() =>
            Spaces1
            .ParseString("\n \t \r\n \r")
            .ShouldBe(null);

        #endregion White space

        #region End of input

        [Fact]
        public void EndOfInput() =>
            EOF
            .ParseString("")
            .ShouldBe(null);

        #endregion End of input

        #region Combinators (sequence)

        [Fact]
        public void TwoChars() =>
            CharP('a').And(CharP('b'))
            .ParseString("ab")
            .ShouldBe(('a', 'b'));

        [Fact]
        public void ThreeChars() =>
            CharP('a').And(CharP('b')).And(CharP('c'))
            .ParseString("abc")
            .ShouldBe((('a', 'b'), 'c'));

        [Fact]
        public void AnyCharFollowedByTheSameChar() =>
            AnyChar.And(CharP)
            .ParseString("xx")
            .ShouldBe('x');

        [Fact]
        public void SkipLeftChar() =>
            Skip('a').And(CharP('b'))
            .ParseString("ab")
            .ShouldBe('b');

        [Fact]
        public void SkipRightChar() =>
            CharP('a').And(Skip('b'))
            .ParseString("ab")
            .ShouldBe('a');

        [Fact]
        public void BetweenChars() =>
            Between('(', Int, ')')
            .ParseString("(1)")
            .ShouldBe(1);

        [Fact]
        public void BetweenStrings() =>
            Between("[begin]", Int, "[end]")
            .ParseString("[begin]1[end]")
            .ShouldBe(1);

        [Fact]
        public void BetweenParsers() =>
            Between(CharP('(').And(WS), Int.And(WS), CharP(')'))
            .ParseString("( 1 )")
            .ShouldBe(1);

        #endregion Combinators (sequence)

        #region Combinators (choice)

        [Fact]
        public void EitherOneOfTwoChars() =>
            CharP('a').Or(CharP('b'))
            .ParseString("b")
            .ShouldBe('b');

        [Fact]
        public void OneOfMultipleChars() =>
            OneOf(CharP('a'), CharP('b'), CharP('c'))
            .ParseString("b")
            .ShouldBe('b');

        #endregion Combinators

        #region Combinators (special)

        [Fact]
        public void ApplyFuncToResult() =>
            Int.Map(i => i + 1)
            .ParseString("1")
            .ShouldBe(2);

        [Fact]
        public void ApplyParameterlessFuncToUnitResult() =>
            Skip('V').Map(() => 5)
            .ParseString("V")
            .ShouldBe(5);

        [Fact]
        public void AlwaysReturnValue() =>
            Return(13)
            .ParseString("whatever")
            .ShouldBe(13);

        [Fact]
        public void ParseAndReturnValue() =>
            Int.Return(int.MaxValue)
            .ParseString("0")
            .ShouldBe(int.MaxValue);

        [Fact]
        public void RecursiveGrammer() {
            FSharpFunc<Chars, Reply<char>> expr = null;
            var parenthesised = Skip('(').And(Rec(() => expr)).And(Skip(')'));
            expr = CharP(char.IsDigit).Or(parenthesised);

            var r = expr.ParseString("((0))");

            r.ShouldBe('0');
        }

        [Fact]
        public void FlattenNestedParserReplies() =>
            StringP("foo").And(CharP('=')).And(Int).Map(Flat)
            .ParseString("foo=1")
            .ShouldBe(("foo", '=', 1));

        #endregion Combinators (special)

        #region Combinators (backtracking)

        [Fact]
        public void BacktrackFailedAlternatives() =>
            Try(Digit.And(Letter)).Or(Digit.And(Digit))
            .ParseString("12")
            .ShouldBe(('1', '2'));

        #endregion Combinators (backtracking)

        #region Repetitions

        [Fact]
        public void ZeroOrMoreOfChar() =>
            Many(CharP('a'))
            .ParseString("aaa")
            .ShouldBe("aaa".ToFSharpList());

        [Fact]
        public void OneOrMoreOfChar() =>
            Many1(CharP('a'))
            .ParseString("aaa")
            .ShouldBe("aaa".ToFSharpList());

        [Fact]
        public void CommaSeparatedChars() =>
            Many(AnyChar, sep: ',')
            .ParseString("a,b,c")
            .ShouldBe("abc".ToFSharpList());

        [Fact]
        public void StringSeparatedChars() =>
            Many(AnyChar, sep: " - ")
            .ParseString("a - b - c")
            .ShouldBe("abc".ToFSharpList());

        [Fact]
        public void ParserSeparatedChars() =>
            Many(AnyChar, sep: CharP(char.IsDigit))
            .ParseString("a1b2c")
            .ShouldBe("abc".ToFSharpList());

        [Fact]
        public void CommaSeparatedCharsAtLeastOne() =>
            Many1(AnyChar, sep: ',')
            .ParseString("a,b,c")
            .ShouldBe("abc".ToFSharpList());

        #endregion Repetitions

        #region Parse errors

        [Fact]
        public void UnexpectedChar() =>
            CharP('a')
            .ParseString("b")
            .ShouldBe<ErrorMessage.ExpectedString>("a");

        #endregion Parse errors

        #region Labels

        [Fact]
        public void LabelAsErrorMsg() =>
            CharP('a').Label("the first letter of the alphabet")
            .ParseString("x")
            .ShouldBe<ErrorMessage.Expected>("the first letter of the alphabet");

        [Fact]
        public void LabelEvenWhenParserAlreadyConsumed() =>
            CharP('a').And(CharP('b')).Label_("the first two letters of the alphabet")
            .ParseString("ax")
            .ShouldBe<ErrorMessage.CompoundError>("the first two letters of the alphabet");

        #endregion Labels
    }
}
