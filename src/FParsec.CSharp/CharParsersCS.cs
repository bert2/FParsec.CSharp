namespace FParsec.CSharp {
    using System;
    using System.Collections.Generic;
    using Microsoft.FSharp.Core;
    using static CharParsers;
    using Chars = CharStream<Microsoft.FSharp.Core.Unit>;

    public static class CharParsersCS {
        #region Char

        /// <summary>
        /// `AnyChar` parses any single char or newline ("\n", "\r\n" or "\r").
        /// Returns the parsed char, or '\n' in case a newline was parsed.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> AnyChar => anyChar<Unit>();

        /// <summary>
        /// `CharP(c)` parses the char `c` and returns `c`. If `c = '\r'` or `c = '\n'` then
        /// `CharP(c)` will parse any one newline ("\n", "\r\n" or "\r") and return `c`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> CharP(char c) => pchar<Unit>(c);

        /// <summary>
        /// `CharP(f)` parses any one char or newline for which the predicate function `f` returns
        /// `true`. It returns the parsed char.
        /// Any newline ("\n", "\r\n" or "\r") is converted to the single char '\n'.
        /// Thus, to accept a newline `f('\n')` must return `true`. `f` will never be called
        /// with '\r' and `CharP(f)` will never return the result '\r'.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> CharP(Func<char, bool> pred)
            => satisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>
        /// `Skip(c)` parses the char `c` and skips it, i.e. returns `(Unit)null`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Skip(char c) => skipChar<Unit>(c);

        /// <summary>
        /// `AnyOf(s)` parses any char contained in the string `s`. It returns the parsed char. If
        /// `s` contains the char '\n', `AnyOf(s)` parses any newline ("\n", "\r\n" or "\r") and
        /// returns it as '\n'. Note that it does not make a difference whether or not `s`
        /// contains '\r'; `AnyOf(s)` will never return '\r'.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> AnyOf(IEnumerable<char> chars) => anyOf<Unit>(chars);

        /// <summary>
        /// `NoneOf(s)` parses any char not contained in the string `s`. It returns the parsed
        /// char. If `s` does not contain the char '\n', `NoneOf(s)` parses any newline
        /// ("\n", "\r\n" or "\r") and returns it as  as '\n'. Note that it does not make a 
        /// difference whether or not `s` contains '\r'; `NoneOf(s)` will never return '\r'.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> NoneOf(IEnumerable<char> chars) => noneOf<Unit>(chars);

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
        /// Skips over any sequence of *one* or more whitespaces (space (' '), tab('\t') or
        /// newline ("\n", "\r\n" or "\r")).
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Spaces1 => spaces1<Unit>();

        /// <summary>Short form for `Spaces1`.</summary>
        public static FSharpFunc<Chars, Reply<Unit>> WS1 => Spaces1;

        /// <summary>
        /// Parses a newline ("\n", "\r\n" or "\r"). Returns '\n'. Is equivalent to `CharP('\n')`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Newline => newline<Unit>();

        /// <summary>Short form for `Newline`.</summary>
        public static FSharpFunc<Chars, Reply<char>> NL => newline<Unit>();

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
