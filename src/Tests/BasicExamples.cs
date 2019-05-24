namespace Tests {
    using FParsec;
    using FParsec.CSharp;
    using Microsoft.FSharp.Core;
    using Xunit;
    using static FParsec.CSharp.CharParsersCS;
    using static FParsec.CSharp.PrimitivesCS;
    using Chars = FParsec.CharStream<Microsoft.FSharp.Core.Unit>;

    public class BasicExamples {
        #region Chars

        [Fact] public void ParseAnyChar() => AnyChar.ParseString("x").ShouldBe('x');

        [Fact] public void SingleChar() => CharP('a').ParseString("a").ShouldBe('a');

        [Fact] public void CharThatSatisfiesPredicate() => CharP(char.IsDigit).ParseString("1").ShouldBe('1');

        [Fact] public void ParseAndSkipChar() => Skip('a').ParseString("a").ShouldBe(null);

        [Fact] public void MatchAnyOfList() => AnyOf("0123456789").ParseString("7").ShouldBe('7');

        [Fact] public void InvertedMatch() => NoneOf("abc").ParseString("d").ShouldBe('d');

        [Fact] public void ParseLetter() => Letter.ParseString("A").ShouldBe('A');

        [Fact] public void UpperCaseLetter() => Upper.ParseString("X").ShouldBe('X');

        [Fact] public void LowerCaseLetter() => Lower.ParseString("x").ShouldBe('x');

        [Fact] public void ParseDigit() => Digit.ParseString("7").ShouldBe('7');

        [Fact] public void HexDigit() => Hex.ParseString("F").ShouldBe('F');

        [Fact] public void OctalDigit() => Octal.ParseString("7").ShouldBe('7');

        #endregion Chars

        #region Strings

        [Fact] public void ParseString() => StringP("hello parser").ParseString("hello parser").ShouldBe("hello parser");

        [Fact] public void StringIgnoreCase() => StringCI("hello parser").ParseString("Hello PARSER").ShouldBe("Hello PARSER");

        [Fact] public void SkipString() => Skip("abc").ParseString("abc").ShouldBe(null);

        [Fact] public void SkipStringIgnoreCase() => SkipCI("hello parser").ParseString("Hello PARSER").ShouldBe(null);

        [Fact] public void StringWithCharParser() => ManyChars(Upper).ParseString("HELLO").ShouldBe("HELLO");

        [Fact] public void StringWithCharParserAtLeastOnce() => Many1Chars(Lower).ParseString("hello").ShouldBe("hello");

        [Fact] public void StringWithPredicate() => ManyChars(char.IsUpper).ParseString("HELLO").ShouldBe("HELLO");

        [Fact] public void StringWithPredicateMinToMaxTimes() => ManyChars(char.IsUpper, 1, 9).ParseString("HELLO").ShouldBe("HELLO");

        [Fact] public void AnyStringOfLengthN() => AnyString(3).ParseString("abcdef").ShouldBe("abc");

        [Fact] public void RestOfTheLine() => Skip('a').And(RestOfLine()).ParseString("abc\ndef").ShouldBe("bc");

        [Fact] public void StringWithRegularExpression() => Regex(@"\w\d").ParseString("a1").ShouldBe("a1");

        [Fact] public void StringUntilEnd() => ManyCharsTill(AnyChar, NL).ParseString("abc\ndef").ShouldBe("abc");

        #endregion Strings

        #region Numbers

        [Fact] public void ParseInt() => Int.ParseString("13").ShouldBe(13);

        [Fact] public void ParseLong() => Long.ParseString("9223372036854775807").ShouldBe(9223372036854775807);

        [Fact] public void ParseFloat() => Float.ParseString("13.45").ShouldBe(13.45);

        #endregion Numbers

        #region White space

        [Fact] public void ZeroOrMoreWhitespaces() => Spaces.ParseString("\n \t \r\n \r").ShouldBe(null);

        [Fact] public void OneOreMoreWhitespaces() => Spaces1.ParseString("\n \t \r\n \r").ShouldBe(null);

        [Fact] public void EndOfLine() => Newline.ParseString("\r\n").ShouldBe('\n');

        [Fact] public void TabChar() => Tab.ParseString("\t").ShouldBe('\t');

        [Fact] public void EndOfInput() => EOF.ParseString("").ShouldBe(null);

        #endregion White space

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

        [Fact]
        public void ParserSequenceToArray() =>
            Array(4, Digit)
            .ParseString("1234")
            .ShouldBe(new[] { '1', '2', '3', '4' });

        [Fact]
        public void ParserSequenceToTuple() =>
            Tuple(Int.And(WS), Float.And(WS), Many1Chars(Digit))
            .ParseString("12 34.5 28")
            .ShouldBe((12, 34.5, "28"));

        [Fact]
        public void ParserSequenceWithMapping() =>
            Pipe(
                Int.And(WS),
                Float.And(WS),
                Many1Chars(Digit),
                (x, y, z) => x + y + int.Parse(z))
            .ParseString("12 34.5 28")
            .ShouldBe(74.5);

        #endregion Combinators (sequence)

        #region Combinators (choice)

        [Fact]
        public void EitherOneOfTwoChars() =>
            CharP('a').Or(CharP('b'))
            .ParseString("b")
            .ShouldBe('b');

        [Fact]
        public void OneOfMultipleChars() =>
            Choice(CharP('a'), CharP('b'), CharP('c'))
            .ParseString("b")
            .ShouldBe('b');

        [Fact]
        public void AlternativeValue() =>
            Digit.Or('\0')
            .ParseString("a")
            .ShouldBe('\0');

        [Fact]
        public void SkipOverOptionalValue() =>
            Optional(CharP(';'))
            .ParseString("")
            .ShouldBe(null);

        [Fact]
        public void ParseOptionalValue() =>
            Opt(CharP('a'))
            .ParseString("a")
            .ShouldBe('a');

        [Fact]
        public void ParseMissingOptional() =>
            Opt(CharP('a'))
            .ParseString("")
            .ShouldBe('\0');

        [Fact]
        public void ParseMissingOptionalWithDefault() =>
            Opt(CharP('a'), defaultValue: 'x')
            .ParseString("")
            .ShouldBe('x');

        [Fact]
        public void ParseOptionalValueWithoutUnwrapping() =>
            Opt_(CharP(';'))
            .ParseString("")
            .ShouldBe(FSharpOption<char>.None);

        #endregion Combinators

        #region Repetitions

        [Fact]
        public void ZeroOrMoreOfChar() =>
            Many(CharP('a'))
            .ParseString("aaa")
            .ShouldBe('a', 'a', 'a');

        [Fact]
        public void OneOrMoreOfChar() =>
            Many1(CharP('a'))
            .ParseString("aaa")
            .ShouldBe('a', 'a', 'a');

        [Fact]
        public void CommaSeparatedChars() =>
            Many(AnyChar, sep: ',')
            .ParseString("a,b,c")
            .ShouldBe('a', 'b', 'c');

        [Fact]
        public void StringSeparatedChars() =>
            Many(AnyChar, sep: " - ")
            .ParseString("a - b - c")
            .ShouldBe('a', 'b', 'c');

        [Fact]
        public void ParserSeparatedChars() =>
            Many(AnyChar, sep: CharP(char.IsDigit))
            .ParseString("a1b2c")
            .ShouldBe('a', 'b', 'c');

        [Fact]
        public void CommaSeparatedCharsAtLeastOne() =>
            Many1(AnyChar, sep: ',')
            .ParseString("a,b,c")
            .ShouldBe('a', 'b', 'c');

        [Fact]
        public void CommaSeparatedWithTrailingComma() =>
            Many(AnyChar, sep: ',', canEndWithSep: true)
            .ParseString("a,b,c,")
            .ShouldBe('a', 'b', 'c');

        [Fact]
        public void ParseUntilEndingIsParsed() =>
            ManyTill(NoneOf(".!?"), AnyOf(".!?")).Map(string.Concat)
            .ParseString("Hello world!")
            .ShouldBe("Hello world");

        [Fact]
        public void SkipManyCommaSeparated() =>
            SkipMany(Letter, sep: ',')
            .ParseString("a,b,c")
            .ShouldBe(null);

        [Fact]
        public void ReduceMany() =>
            Many(Digit.Map(c => c - '0'), (sum, x) => sum + x, 0)
            .ParseString("01232456789")
            .ShouldBe(47);

        [Fact]
        public void ReduceManyAtLeastOne() =>
            Many1(AnyString(3).And(WS), string.Concat)
            .ParseString("abc def ghi")
            .ShouldBe("abcdefghi");

        [Fact]
        public void ComputeExpressionDuringParsing() {
            var op = Choice(
                CharP('+').Return((int x, int y) => x + y),
                CharP('-').Return((int x, int y) => x - y));

            ChainL(Int, op)
            .ParseString("1+2+3-4+5-6")
            .ShouldBe(1);
        }

        #endregion Repetitions

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
            var parenthesized = Between('(', Rec(() => expr), ')');
            expr = CharP(char.IsDigit).Or(parenthesized);

            var r = expr.ParseString("((0))");

            r.ShouldBe('0');
        }

        [Fact]
        public void FlattenNestedParserReplies() =>
            StringP("foo").And(CharP('=')).And(Int).Map(Flat)
            .ParseString("foo=1")
            .ShouldBe(("foo", '=', 1));

        [Fact]
        public void ParserMustChangeState() =>
            NotEmpty(Many(AnyChar))
            .ParseString("")
            .ShouldBe<ErrorMessage.Expected>("any char");

        #endregion Combinators (special)

        #region Combinators (backtracking, looking ahead & conditional parsing)

        [Fact]
        public void BacktrackFailure() =>
            Try(Digit.And(Letter)).Or(Digit.And(Digit))
            .ParseString("12")
            .ShouldBe(('1', '2'));

        [Fact]
        public void LookAheadAndBacktrack() {
            var keepLowerSkipUpper = LookAhead(Letter).And(c => char.IsLower(c)
                ? ManyChars(Lower)
                : ManyChars(Upper).Return(""));

            Many(keepLowerSkipUpper, sep: ',')
            .ParseString("you,CELLS,are,WITHIN,not,even,CELLS,close,to,INTERLINKED,baseline")
            .ShouldBe("you", "", "are", "", "not", "even", "", "close", "to", "", "baseline");
        }

        [Fact]
        public void LookAheadAndBacktrackWithoutResult() =>
            FollowedBy(Upper, "start of sentence")
                .And(Many(NoneOf(".!?")))
                .And(FollowedBy(AnyOf(".!?"), "end of sentence"))
                .Map(string.Concat)
            .ParseString("Is this a sentence?")
            .ShouldBe("Is this a sentence");

        [Fact]
        public void NegativelyLookAheadAndBacktrackWithoutResult() =>
            Many1(Digit).And(NotFollowedBy(CharP('.'), "floating point"))
            .ParseString("123")
            .ShouldBe('1', '2', '3');

        [Fact]
        public void PeekNextChar() {
            FSharpFunc<Chars, Reply<char>> NextIsSmallerOrEOF(char c) =>
                NextCharSatisfies(n => n < c).Lbl($"char smaller than '{c}'")
                .Or(EOF)
                .Return(c);

            Many(AnyChar.And(NextIsSmallerOrEOF))
            .ParseString("cba")
            .ShouldBe('c', 'b', 'a');
        }

        [Fact]
        public void PeekNextTwoChars() {
            FSharpFunc<Chars, Reply<char>> IsSumOfNext2(char c) =>
                Next2CharsSatisfy((a, b) => a + b == c).Lbl("reverse Fibonacci chars")
                .Or(FollowedBy(AnyChar.And(EOF)))
                .Or(EOF)
                .Return(c);

            Many(AnyChar.And(IsSumOfNext2))
            .ParseString("\x8\x5\x3\x2\x1\x1\x0")
            .ShouldBe('\x8', '\x5', '\x3', '\x2', '\x1', '\x1', '\x0');
        }

        [Fact]
        public void LookBehind() {
            var tagType = Choice(
                PreviousCharSatisfies(c => c == '/').Return("self-closing tag"),
                Return("opening tag"));

            var tagContent = Many1(NoneOf(">"));

            Between('<', Skip(tagContent).And(tagType), '>')
            .ParseString("<tag />")
            .ShouldBe("self-closing tag");
        }

        #endregion Combinators (backtracking, looking ahead & conditional parsing)

        #region Parse errors

        [Fact] public void UnexpectedChar() => CharP('a').ParseString("b").ShouldBe<ErrorMessage.ExpectedString>("a");

        [Fact] public void AlwaysFailingParser() => Zero<char>().ParseString("abc").ShouldBeErrors();

        [Fact] public void FailWithMessage() =>
            Fail<char>("my error")
            .ParseString("abc")
            .ShouldBe<ErrorMessage.Message>("my error");

        [Fact] public void FailFatallyWithMessage() =>
            FailFatally<char>("my error")
            .ParseString("abc")
            .ShouldBe(ReplyStatus.FatalError);

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
