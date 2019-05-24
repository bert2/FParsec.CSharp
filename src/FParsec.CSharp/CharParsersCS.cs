namespace FParsec.CSharp {
    using System;
    using System.Collections.Generic;
    using Microsoft.FSharp.Core;
    using static CharParsers;
    using Chars = CharStream<Microsoft.FSharp.Core.Unit>;

    /// <summary>
    /// Provides predefined char and string parsers.
    /// </summary>
    public static class CharParsersCS {
        #region Char

        /// <summary>
        /// `AnyChar` parses any single char or newline ("\n", "\r\n" or "\r").
        /// Returns the parsed char, or '\n' in case a newline was parsed.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> AnyChar => anyChar<Unit>();

        /// <summary>
        /// `SkipAnyChar` is an optimized implementation of `Skip(AnyChar)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> SkipAnyChar => skipAnyChar<Unit>();

        /// <summary>
        /// `CharP(c)` parses the char `c` and returns `c`. If `c = '\r'` or `c = '\n'` then
        /// `CharP(c)` will parse any one newline ("\n", "\r\n" or "\r") and return `c`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> CharP(char c) => pchar<Unit>(c);

        /// <summary>
        /// `CharP(c,x)` is an optimized implementation of `CharP(c).Return(x)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> CharP<T>(char c, T x) => charReturn<T, Unit>(c, x);

        /// <summary>
        /// <para>
        /// `CharP(f)` parses any one char or newline for which the predicate function `f` returns
        /// `true`. It returns the parsed char.
        /// </para>
        /// <para>
        /// Any newline ("\n", "\r\n" or "\r") is converted to the single char '\n'.
        /// Thus, to accept a newline `f('\n')` must return `true`. `f` will never be called
        /// with '\r' and `CharP(f)` will never return the result '\r'.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> CharP(Func<char, bool> pred) => satisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>
        /// `CharP(f,s)` is an optimized implementation of `CharP(f).Label(s)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> CharP(Func<char, bool> pred, string label) => satisfyL<Unit>(pred.ToFSharpFunc(), label);

        /// <summary>
        /// `Skip(c)` is an optimized implementation of `Skip(CharP(c))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Skip(char c) => skipChar<Unit>(c);

        /// <summary>
        /// `Skip(f)` is an optimized implementation of `Skip(CharP(f))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Skip(Func<char, bool> pred) => skipSatisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>
        /// `Skip(f,s)` is an optimized implementation of `Skip(f).Label(s)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Skip(Func<char, bool> pred, string label) => skipSatisfyL<Unit>(pred.ToFSharpFunc(), label);

        /// <summary>
        /// `AnyOf(s)` parses any char contained in the string `s`. It returns the parsed char. If
        /// `s` contains the char '\n', `AnyOf(s)` parses any newline ("\n", "\r\n" or "\r") and
        /// returns it as '\n'. Note that it does not make a difference whether or not `s`
        /// contains '\r'; `AnyOf(s)` will never return '\r'.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> AnyOf(IEnumerable<char> chars) => anyOf<Unit>(chars);

        /// <summary>
        /// `SkipAnyOf(s)` is an optimized implementation of `Skip(AnyOf(s))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> SkipAnyOf(IEnumerable<char> chars) => skipAnyOf<Unit>(chars);

        /// <summary>
        /// `NoneOf(s)` parses any char not contained in the string `s`. It returns the parsed
        /// char. If `s` does not contain the char '\n', `NoneOf(s)` parses any newline
        /// ("\n", "\r\n" or "\r") and returns it as  as '\n'. Note that it does not make a 
        /// difference whether or not `s` contains '\r'; `NoneOf(s)` will never return '\r'.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> NoneOf(IEnumerable<char> chars) => noneOf<Unit>(chars);

        /// <summary>
        /// `SkipNoneOf(s)` is an optimized implementation of `Skip(NoneOf(s))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> SkipNoneOf(IEnumerable<char> chars) => skipNoneOf<Unit>(chars);

        /// <summary>
        /// Parses any UTF-16 letter char identified by `System.Char.IsLetter`. Returns the parsed
        /// char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Letter => letter<Unit>();

        /// <summary>
        /// Parses any char in the range 'a' - 'z' and 'A' - 'Z'. Returns the parsed char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> AsciiLetter => asciiLetter<Unit>();

        /// <summary>
        /// Parses any UTF-16 uppercase letter char identified by `System.Char.IsUpper`. Returns
        /// the parsed char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Upper => upper<Unit>();

        /// <summary>
        /// Parses any char in the range 'A' - 'Z'. Returns the parsed char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> AsciiUpper => asciiUpper<Unit>();

        /// <summary>
        /// Parses any UTF-16 lowercase letter char identified by `System.Char.IsLower`. Returns
        /// the parsed char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Lower => lower<Unit>();

        /// <summary>
        /// Parses any char in the range 'a' - 'z'. Returns the parsed char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> AsciiLower => asciiLower<Unit>();

        /// <summary>
        /// Parses any char in the range '0' - '9'. Returns the parsed char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Digit => digit<Unit>();

        /// <summary>
        /// Parses any char in the range '0' - '9', 'a' - 'f' and 'A' - 'F'. Returns the parsed
        /// char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Hex => hex<Unit>();

        /// <summary>
        /// Parses any char in the range '0' - '7'. Returns the parsed char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Octal => octal<Unit>();

        #endregion Char

        #region String

        /// <summary>
        /// `StringP(s)` parses the string `s` and returns `s`.
        /// It is an atomic parser: either it succeeds or it fails without consuming any input.
        /// `s` may not contain newline chars ('\n' or '\r').
        /// </summary>
        public static FSharpFunc<Chars, Reply<string>> StringP(string s) => pstring<Unit>(s);

        /// <summary>
        /// `StringCI(s)` parses any string that case-insensitively matches the string `s`.
        /// It returns the *parsed* string. `s` may not contain newline chars ('\n' or '\r').
        /// </summary>
        public static FSharpFunc<Chars, Reply<string>> StringCI(string s) => pstringCI<Unit>(s);

        /// <summary>
        /// `StringP(s,x)` is an optimized implementation of `StringP(s).Return(x)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> StringP<T>(string s, T x) => stringReturn<T, Unit>(s, x);

        /// <summary>
        /// `StringCI(s,x)` is an optimized implementation of `StringCI(s).Return(x)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> StringCI<T>(string s, T x) => stringCIReturn<T, Unit>(s, x);

        /// <summary>
        /// `Skip(s)` parses the char `s` and skips it, i.e. returns `(Unit)null`. `s` may not
        /// contain newline chars ('\n' or '\r').
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Skip(string s) => skipString<Unit>(s);

        /// <summary>
        /// `SkipCI(s)` parses any string that case-insensitively matches the string `s` and skips
        /// it , i.e. returns `(Unit)null`. `s` may not contain newline chars ('\n' or '\r').
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> SkipCI(string s) => skipStringCI<Unit>(s);

        /// <summary>
        /// <para>
        /// `AnyString(n)` parses any sequence of `n` chars or newlines ("\n", "\r\n" or "\r").
        /// It returns the parsed string.
        /// </para>
        /// <para>In the returned string all newlines are normalized to "\n".</para>
        /// <para>
        /// `AnyString(n)` is an atomic parser: either it succeeds or it fails without consuming
        /// any input.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<string>> AnyString(int length) => anyString<Unit>(length);

        /// <summary>
        /// `SkipAnyString(n)` is an optimized implementation of `Skip(AnyString(n))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> SkipAnyString(int length) => skipAnyString<Unit>(length);

        /// <summary>
        /// <para>
        /// `RestOfLine(skipNewline)` parses any chars before the end of the line and, if
        /// `skipNewline` is `true`, skips to the beginning of the next line (if there is one).
        /// </para>
        /// <para>
        /// Returns the parsed chars before the end of the line as a string (without a newline).
        /// </para>
        /// <para>A line is terminated by a newline ("\n", "\r\n" or "\r") or the end of the input stream.</para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<string>> RestOfLine(bool skipNewline = false) => restOfLine<Unit>(skipNewline);

        /// <summary>
        /// `SkipRestOfLine(skipNewline)` is an optimized implementation of `Skip(RestOfLine(skipNewline))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> SkipRestOfLine(bool skipNewline = false) => skipRestOfLine<Unit>(skipNewline);

        /// <summary>
        /// <para>
        /// `ManyChars(cp)` parses a sequence of *zero* or more chars with the char parser `cp`. It
        /// returns the parsed chars as a string.
        /// </para>
        /// <para>
        /// `ManyChars(cp)` is an optimized implementation of `Many(Try(cp))` that returns the
        /// chars as a string instead of a char list. The equivalence to `Many(Try(p))` instead of
        /// `Many(p)` implies that `ManyChars()` never fails.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<string>> ManyChars(
            FSharpFunc<Chars, Reply<char>> p)
            => manyChars(p);

        /// <summary>
        /// <para>
        /// `Many1Chars(cp)` parses a sequence of *one* or more chars with the char parser `cp`. It
        /// returns the parsed chars as a string.
        /// </para>
        /// <para>
        /// `Many1Chars(cp)` is an optimized implementation of `Many1(Try(cp))` that returns the
        /// chars as a string instead of a char list. The equivalence to `Many1(Try(p))` instead of
        /// `Many1(p)` implies that `Many1Chars()` never fails after consuming input.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<string>> Many1Chars(
            FSharpFunc<Chars, Reply<char>> p)
            => many1Chars(p);

        #endregion String

        #region Number

        /// <summary>
        /// <para>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in the format
        ///     specifiers "0x" etc.) can be parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        ///     comes after a format specifier,
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string is greater than
        ///     `System.Int32.MaxValue` or less than `System.Int32.MinValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<int>> Int => pint32<Unit>();

        /// <summary>
        /// <para>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in the format
        ///     specifiers "0x" etc.) can be parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        ///     comes after a format specifier,
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string is greater than
        ///     `System.Int64.MaxValue` or less than `System.Int64.MinValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<long>> Long => pint64<Unit>();

        /// <summary>
        /// <para>
        /// Parses a floating-point number in decimal or hexadecimal format.
        /// The special values NaN and Inf(inity)? (case insensitive) are also recognized.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in "0x") can be
        ///     parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        ///     comes after "0x",
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string (after rounding) is
        ///     greater than `System.Double.MaxValue` or less than `System.Double.MinValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<double>> Float => pfloat<Unit>();

        #endregion Number

        #region Whitespace

        /// <summary>
        /// Skips over any sequence of *zero* or more whitespaces (space (' '), tab ('\t') or
        /// newline ("\n", "\r\n" or "\r")).
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Spaces => spaces<Unit>();

        /// <summary>Short form for `Spaces`.</summary>
        public static FSharpFunc<Chars, Reply<Unit>> WS => Spaces;

        /// <summary>
        /// Skips over any sequence of *zero* or more unicode whitespaces and registers any unicode
        /// newline ("\n", "\r\n", "\r", "\u0085, "\u000C", "\u2028", or "\u2029") as a newline.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> UnicodeSpaces => unicodeSpaces<Unit>();

        /// <summary>
        /// Skips over any sequence of *one* or more whitespaces (space (' '), tab('\t') or
        /// newline ("\n", "\r\n" or "\r")).
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Spaces1 => spaces1<Unit>();

        /// <summary>Short form for `Spaces1`.</summary>
        public static FSharpFunc<Chars, Reply<Unit>> WS1 => Spaces1;

        /// <summary>
        /// Skips over any sequence of *one* or more unicode whitespaces and registers any unicode
        /// newline ("\n", "\r\n", "\r", "\u0085, "\u000C", "\u2028", or "\u2029") as a newline.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> UnicodeSpaces1 => unicodeSpaces1<Unit>();

        /// <summary>
        /// Parses a newline ("\n", "\r\n" or "\r"). Returns '\n'. Is equivalent to `CharP('\n')`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Newline => newline<Unit>();

        /// <summary>Short form for `Newline`.</summary>
        public static FSharpFunc<Chars, Reply<char>> NL => newline<Unit>();

        /// <summary>
        /// `NewlineReturn(x)` is an optimized implementation of `Newline.Return(x)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> NewlineReturn<T>(T x) => newlineReturn<T, Unit>(x);

        /// <summary>
        /// `SkipNewline` is an optimized implementation of `Skip(Newline)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> SkipNewline => skipNewline<Unit>();

        /// <summary>
        /// <para>
        /// Parses a unicode newline ("\n", "\r\n", "\r", "\u0085", "\u2028", or "\u2029") and
        /// returns '\n'.
        /// </para>
        /// <para>
        /// Note that this parser does not accept the formfeed char '\f' as a newline. In contrast
        /// to most other parsers in FParsec this parser also increments the internal line count
        /// for unicode newline characters other than '\n' and '\r'.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> UnicodeNewline => unicodeNewline<Unit>();

        /// <summary>
        /// `UnicodeNewlineReturn(x)` is an optimized implementation of `UnicodeNewline.Return(x)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> UnicodeNewlineReturn<T>(T x) => unicodeNewlineReturn<T, Unit>(x);

        /// <summary>
        /// `SkipUnicodeNewline` is an optimized implementation of `Skip(UnicodeNewline)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> SkipUnicodeNewline => skipUnicodeNewline<Unit>();

        /// <summary>
        /// Parses the tab char '\t' and returns '\t'. Note that a tab char is treated like any
        /// other non-newline char: the column number is incremented by (only) 1.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Tab => tab<Unit>();

        /// <summary>
        /// The parser `EOF` only succeeds at the end of the input. It never consumes input.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> EOF => eof<Unit>();

        #endregion Whitespace

        #region Conditional parsing

        /// <summary>
        /// `NotFollowedByEOF` is an optimized implementation of
        /// `NotFollowedBy(EOF, "end of input")`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> NotFollowedByEOF => notFollowedByEof<Unit>();

        /// <summary>
        /// `FollowedByNewline` is an optimized implementation of `FollowedBy(Newline, "newline")`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> FollowedByNewline => followedByNewline<Unit>();

        /// <summary>
        /// `NotFollowedByNewline` is an optimized implementation of
        /// `NotFollowedBy(Newline, "newline")`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> NotFollowedByNewline => notFollowedByNewline<Unit>();

        /// <summary>
        /// `FollowedBy(s)` is an optimized implementation of `FollowedBy(StringP(s), $"'{s}'"))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> FollowedBy(string s) => followedByString<Unit>(s);

        /// <summary>
        /// `NotFollowedBy(s)` is an optimized implementation of
        /// `NotFollowedBy(StringP(s), $"'{s}'"))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> NotFollowedBy(string s) => notFollowedByString<Unit>(s);

        /// <summary>
        /// `FollowedByCI(s)` is an optimized implementation of
        /// `FollowedBy(StringCI(s), $"'{s}'"))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> FollowedByCI(string s) => followedByStringCI<Unit>(s);

        /// <summary>
        /// `NotFollowedByCI(s)` is an optimized implementation of
        /// `NotFollowedBy(StringCI(s), $"'{s}'"))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> NotFollowedByCI(string s) => notFollowedByStringCI<Unit>(s);

        /// <summary>
        /// `NextCharSatisfies(f)` is an optimized implementation of `FollowedBy(CharP(f))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> NextCharSatisfies(
            Func<char, bool> pred)
            => nextCharSatisfies<Unit>(pred.ToFSharpFunc());

        /// <summary>
        /// `NextCharSatisfiesNot(f)` is an optimized implementation of `NotFollowedBy(CharP(f))`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> NextCharSatisfiesNot(
            Func<char, bool> pred)
            => nextCharSatisfiesNot<Unit>(pred.ToFSharpFunc());

        /// <summary>
        /// <para>
        /// `Next2CharsSatisfy(f)` succeeds if the predicate function `f` returns `true` when
        /// applied to the next two chars in the input stream, otherwise it fails.
        /// </para>
        /// <para>
        /// If there aren't two chars remaining in the input stream, this parser fails (as opposed
        /// to `Next2CharsSatisfyNot()`). This parser never changes the parser state.
        /// </para>
        /// <para>Any newline ("\n", "\r\n" or "\r") in the input is interpreted as a single char '\n'.</para>
        /// <para>
        /// If this parser fails, it returns no descriptive error message; hence it should only be
        /// used together with parsers that take care of a potential error.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Next2CharsSatisfy(
            Func<char, char, bool> pred)
            => next2CharsSatisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>
        /// <para>
        /// `Next2CharsSatisfyNot(f)` succeeds if the predicate function `f` returns `false` when
        /// applied to the next two chars in the input stream, otherwise it fails.
        /// </para>
        /// <para>
        /// If there aren't two chars remaining in the input stream, this parser succeeds (as
        /// opposed to `Next2CharsSatisfy()`). This parser never changes the parser state.
        /// </para>
        /// <para>Any newline ("\n", "\r\n" or "\r") in the input is interpreted as a single char '\n'.</para>
        /// <para>
        /// If this parser fails, it returns no descriptive error message; hence it should only be
        /// used together with parsers that take care of a potential error.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Next2CharsSatisfyNot(
            Func<char, char, bool> pred)
            => next2CharsSatisfyNot<Unit>(pred.ToFSharpFunc());

        /// <summary>
        /// <para>
        /// `PreviousCharSatisfies(f)` succeeds if the predicate function `f` returns `true` when
        /// applied to the previous char in the stream, otherwise it fails.
        /// </para>
        /// <para>
        /// If there is no previous char (because the stream is at the beginning), this parser
        /// fails (as opposed to `PreviousCharSatisfiesNot()`). This parser never changes the
        /// parser state.
        /// </para>
        /// <para>
        /// Any newline ("\n", "\r\n" or "\r") in the input is interpreted as a single char '\n'.
        /// </para>
        /// <para>
        /// If this parser fails, it returns no descriptive error message; hence it should only be
        /// used together with parsers that take care of a potential error.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> PreviousCharSatisfies(
            Func<char, bool> pred)
            => previousCharSatisfies<Unit>(pred.ToFSharpFunc());

        /// <summary>
        /// <para>
        /// `PreviousCharSatisfiesNot(f)` succeeds if the predicate function `f` returns `false`
        /// when applied to the previous char in the stream, otherwise it fails.
        /// </para>
        /// <para>
        /// If there is no previous char (because the stream is at the beginning), this parser
        /// succeeds (as opposed to `PreviousCharSatisfies()`). This parser never changes the
        /// parser state.
        /// </para>
        /// <para>
        /// Any newline ("\n", "\r\n" or "\r") in the input is interpreted as a single char '\n'.
        /// </para>
        /// <para>
        /// If this parser fails, it returns no descriptive error message; hence it should only be
        /// used together with parsers that take care of a potential error.
        /// </para>
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> PreviousCharSatisfiesNot(
            Func<char, bool> pred)
            => previousCharSatisfiesNot<Unit>(pred.ToFSharpFunc());

        #endregion Conditional parsing
    }
}
