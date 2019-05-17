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
        public static FSharpFunc<Chars, Reply<char>> AnyChar = anyChar<Unit>();

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
        /// Parses any UTF-16 uppercase letter char identified by `System.Char.IsUpper`. Returns
        /// the parsed char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Upper => upper<Unit>();

        /// <summary>
        /// Parses any UTF-16 lowercase letter char identified by `System.Char.IsLower`. Returns
        /// the parsed char.
        /// </summary>
        public static FSharpFunc<Chars, Reply<char>> Lower => lower<Unit>();

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

        #endregion String

        #region Number

        /// <summary>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// The parser fails
        /// without consuming input, if not at least one digit (including the '0' in the format
        ///     specifiers "0x" etc.) can be parsed,
        /// after consuming input, if no digit comes after an exponent marker or no hex digit comes
        ///     after a format specifier,
        /// after consuming input, if the value represented by the input string is greater than
        ///     `System.Int32.MaxValue` or less than `System.Int32.MinValue`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<int>> Int = pint32<Unit>();

        /// <summary>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// The parser fails
        /// without consuming input, if not at least one digit (including the '0' in the format
        ///     specifiers "0x" etc.) can be parsed,
        /// after consuming input, if no digit comes after an exponent marker or no hex digit comes
        ///     after a format specifier,
        /// after consuming input, if the value represented by the input string is greater than
        ///     `System.Int64.MaxValue` or less than `System.Int64.MinValue`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<long>> Long = pint64<Unit>();

        /// <summary>
        /// Parses a floating-point number in decimal or hexadecimal format.
        /// The special values NaN and Inf(inity)? (case insensitive) are also recognized.
        /// The parser fails
        /// without consuming input, if not at least one digit (including the '0' in "0x") can be
        ///     parsed,
        /// after consuming input, if no digit comes after an exponent marker or no hex digit comes
        ///     after "0x",
        /// after consuming input, if the value represented by the input string (after rounding) is
        ///     greater than `System.Double.MaxValue` or less than `System.Double.MinValue`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<double>> Float = pfloat<Unit>();

        #endregion Number

        #region Whitespace

        /// <summary>
        /// Skips over any sequence of *zero* or more whitespaces (space (' '), tab ('\t') or
        /// newline ("\n", "\r\n" or "\r")).
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Spaces = spaces<Unit>();

        /// <summary>Short form for `Spaces`.</summary>
        public static FSharpFunc<Chars, Reply<Unit>> WS = Spaces;

        /// <summary>
        /// Skips over any sequence of *one* or more whitespaces (space (' '), tab('\t') or
        /// newline ("\n", "\r\n" or "\r")).
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> Spaces1 = spaces1<Unit>();

        /// <summary>Short form for `Spaces1`.</summary>
        public static FSharpFunc<Chars, Reply<Unit>> WS1 = Spaces1;

        #endregion Whitespace

        #region EOF

        public static FSharpFunc<Chars, Reply<Unit>> EOF = eof<Unit>();

        #endregion EOF
    }
}
