using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.FSharp.Core;
using static FParsec.CharParsers;
using static FParsec.Error;
using static FParsec.Primitives;

namespace FParsec.CSharp {
    /// <summary>Provides predefined char and string parsers.</summary>
    public static class CharParsersCS {
        #region Chars

        /// <summary>
        /// `AnyChar` parses any single char or newline ("\n", "\r\n" or "\r").
        /// Returns the parsed char, or '\n' in case a newline was parsed.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> AnyChar = anyChar<Unit>();

        /// <summary>`AnyCharU()` behaves like `AnyChar`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> AnyCharU<U>() => anyChar<U>();

        /// <summary>
        /// `SkipAnyChar` is an optimized implementation of `Skip(AnyChar)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipAnyChar = skipAnyChar<Unit>();

        /// <summary>
        /// `SkipAnyCharU()` behaves like `SkipAnyChar`, but supports user state.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipAnyCharU<U>() => skipAnyChar<U>();

        /// <summary>
        /// <para>`CharP(c)` parses the char `c` and returns `c`.</para>
        /// <para>
        /// If `c = '\r'` or `c = '\n'` then `CharP(c)` will parse any one newline ("\n", "\r\n",
        /// or "\r") and return `c`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> CharP(char c) => pchar<Unit>(c);

        /// <summary>`CharU(c)` behaves like `CharP(c)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> CharU<U>(char c) => pchar<U>(c);

        /// <summary>
        /// `CharP(c,x)` is an optimized implementation of `CharP(c).Return(x)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<T>> CharP<T>(char c, T x) => charReturn<T, Unit>(c, x);

        /// <summary>`CharU(c,x)` behaves like `CharP(c,x)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> CharU<U, T>(char c, T x) => charReturn<T, U>(c, x);

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
        public static FSharpFunc<CharStream<Unit>, Reply<char>> CharP(Func<char, bool> pred)
            => satisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>`CharU(f)` behaves like `CharP(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> CharU<U>(Func<char, bool> pred)
            => satisfy<U>(pred.ToFSharpFunc());

        /// <summary>
        /// `CharP(f,s)` is an optimized implementation of `CharP(f).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> CharP(Func<char, bool> pred, string label)
            => satisfyL<Unit>(pred.ToFSharpFunc(), label);

        /// <summary>`CharU(f,s)` behaves like `CharP(f,s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> CharU<U>(Func<char, bool> pred, string label)
            => satisfyL<U>(pred.ToFSharpFunc(), label);

        /// <summary>
        /// `Skip(c)` is an optimized implementation of `Skip(CharP(c))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> Skip(char c) => skipChar<Unit>(c);

        /// <summary>`SkipU(c)` behaves like `Skip(c)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipU<U>(char c) => skipChar<U>(c);

        /// <summary>
        /// `Skip(f)` is an optimized implementation of `Skip(CharP(f))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> Skip(Func<char, bool> pred)
            => skipSatisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>`SkipU(f)` behaves like `Skip(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipU<U>(Func<char, bool> pred)
            => skipSatisfy<U>(pred.ToFSharpFunc());

        /// <summary>
        /// `Skip(f,s)` is an optimized implementation of `Skip(f).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> Skip(Func<char, bool> pred, string label)
            => skipSatisfyL<Unit>(pred.ToFSharpFunc(), label);

        /// <summary>`SkipU(f,s)` behaves like `Skip(f,s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipU<U>(Func<char, bool> pred, string label)
            => skipSatisfyL<U>(pred.ToFSharpFunc(), label);

        /// <summary>
        /// <para>
        /// `AnyOf(s)` parses any char contained in the string `s`. It returns the parsed char.
        /// </para>
        /// <para>
        /// If `s` contains the char '\n', `AnyOf(s)` parses any newline ("\n", "\r\n" or "\r") and
        /// returns it as '\n'.
        /// </para>
        /// <para>
        /// Note that it does not make a difference whether or not `s` contains '\r'; `AnyOf(s)`
        /// will never return '\r'.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> AnyOf(IEnumerable<char> chars) => anyOf<Unit>(chars);

        /// <summary>`AnyOfU(s)` behaves like `AnyOf(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> AnyOfU<U>(IEnumerable<char> chars) => anyOf<U>(chars);

        /// <summary>
        /// `SkipAnyOf(s)` is an optimized implementation of `Skip(AnyOf(s))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipAnyOf(IEnumerable<char> chars) => skipAnyOf<Unit>(chars);

        /// <summary>`SkipAnyOfU(s)` behaves like `SkipAnyOf(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipAnyOfU<U>(IEnumerable<char> chars) => skipAnyOf<U>(chars);

        /// <summary>
        /// <para>
        /// `NoneOf(s)` parses any char not contained in the string `s`. It returns the parsed
        /// char.
        /// </para>
        /// <para>
        /// If `s` does not contain the char '\n', `NoneOf(s)` parses any newline ("\n", "\r\n", or
        /// "\r") and returns it as  as '\n'.
        /// </para>
        /// <para>
        /// Note that it does not make a difference whether or not `s` contains '\r'; `NoneOf(s)`
        /// will never return '\r'.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> NoneOf(IEnumerable<char> chars) => noneOf<Unit>(chars);

        /// <summary>`NoneOfU(s)` behaves like `NoneOf(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> NoneOfU<U>(IEnumerable<char> chars) => noneOf<U>(chars);

        /// <summary>
        /// `SkipNoneOf(s)` is an optimized implementation of `Skip(NoneOf(s))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipNoneOf(IEnumerable<char> chars) => skipNoneOf<Unit>(chars);

        /// <summary>`SkipNoneOfU(s)` behaves like `SkipNoneOf(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipNoneOfU<U>(IEnumerable<char> chars) => skipNoneOf<U>(chars);

        /// <summary>
        /// Parses any UTF-16 letter char identified by `System.Char.IsLetter`. Returns the parsed
        /// char.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> Letter = letter<Unit>();

        /// <summary>`LetterU()` behaves like `Letter`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> LetterU<U>() => letter<U>();

        /// <summary>
        /// Parses any char in the range 'a' - 'z' and 'A' - 'Z'. Returns the parsed char.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> AsciiLetter = asciiLetter<Unit>();

        /// <summary>`AsciiLetterU()` behaves like `AsciiLetter`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> AsciiLetterU<U>() => asciiLetter<U>();

        /// <summary>
        /// Parses any UTF-16 uppercase letter char identified by `System.Char.IsUpper`. Returns
        /// the parsed char.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> Upper = upper<Unit>();

        /// <summary>`UpperU()` behaves like `Upper`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> UpperU<U>() => upper<U>();

        /// <summary>
        /// Parses any char in the range 'A' - 'Z'. Returns the parsed char.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> AsciiUpper = asciiUpper<Unit>();

        /// <summary>`AsciiUpperU()` behaves like `AsciiUpper`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> AsciiUpperU<U>() => asciiUpper<U>();

        /// <summary>
        /// Parses any UTF-16 lowercase letter char identified by `System.Char.IsLower`. Returns
        /// the parsed char.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> Lower = lower<Unit>();

        /// <summary>`LowerU()` behaves like `Lower`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> LowerU<U>() => lower<U>();

        /// <summary>
        /// Parses any char in the range 'a' - 'z'. Returns the parsed char.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> AsciiLower = asciiLower<Unit>();

        /// <summary>`AsciiLowerU()` behaves like `AsciiLower`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> AsciiLowerU<U>() => asciiLower<U>();

        /// <summary>
        /// Parses any char in the range '0' - '9'. Returns the parsed char.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> Digit = digit<Unit>();

        /// <summary>`DigitU()` behaves like `Digit`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> DigitU<U>() => digit<U>();

        /// <summary>
        /// Parses any char in the range '0' - '9', 'a' - 'f' and 'A' - 'F'. Returns the parsed
        /// char.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> Hex = hex<Unit>();

        /// <summary>`HexU()` behaves like `Hex`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> HexU<U>() => hex<U>();

        /// <summary>
        /// Parses any char in the range '0' - '7'. Returns the parsed char.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> Octal = octal<Unit>();

        /// <summary>`OctalU()` behaves like `Octal`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> OctalU<U>() => octal<U>();

        /// <summary>
        /// `IsLetter(c)` is equivalent to `System.Char.IsLetter(c)`.
        /// </summary>
        public static bool IsLetter(char c) => isLetter(c);

        /// <summary>
        /// Returns `true` for any char in the range 'a' - 'z', 'A' - 'Z' and `false` for all other
        /// chars.
        /// </summary>
        public static bool IsAsciiLetter(char c) => isAsciiLetter(c);

        /// <summary>`IsUpper(c)` is equivalent to `System.Char.IsUpper(c)`.</summary>
        public static bool IsUpper(char c) => isUpper(c);

        /// <summary>
        /// Returns `true` for any char in the range 'A' - 'Z' and `false` for all other chars.
        /// </summary>
        public static bool IsAsciiUpper(char c) => isAsciiUpper(c);

        /// <summary>
        /// `IsLower(c)` is equivalent to `System.Char.IsLower(c)`.
        /// </summary>
        public static bool IsLower(char c) => isLower(c);

        /// <summary>
        /// Returns `true` for any char in the range 'a' - 'z' and `false` for all other chars.
        /// </summary>
        public static bool IsAsciiLower(char c) => isAsciiLower(c);

        /// <summary>
        /// Returns `true` for any char in the range '0' - '9' and `false` for all other chars.
        /// </summary>
        public static bool IsDigit(char c) => isDigit(c);

        /// <summary>
        /// Returns `true` for any char in the range '0' - '9', 'a' - 'f', 'A' - 'F' and `false`
        /// for all other chars.
        /// </summary>
        public static bool IsHex(char c) => isHex(c);

        /// <summary>
        /// Returns `true` for any char in the range '0' - '7' and `false` for all other chars.
        /// </summary>
        public static bool IsOctal(char c) => isOctal(c);

        #endregion Chars

        #region Strings

        /// <summary>
        /// The parser `Choice(strings)` is a short form for
        /// `Choice(StringP(s1), StringP(s2), StringP(...), StringP(sn))`, where `s1` ... `sn` are
        /// the strings in the sequence `strings`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> Choice(
            params string[] strings)
            => choice(strings.Select(s => StringP(s)));

        /// <summary>`ChoiceU(strings)` behaves like `Choice(strings)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ChoiceU<U>(
            params string[] strings)
            => choice(strings.Select(s => StringU<U>(s)));

        /// <summary>
        /// The parser `ChoiceL(label, strings)` is a short form for
        /// `Choice(label, StringP(s1), StringP(s2), StringP(...), StringP(sn))`, where `s1` ...
        /// `sn` are the strings in the sequence `strings`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> ChoiceL(
            string label,
            params string[] strings)
            => choiceL(strings.Select(s => StringP(s)), label);

        /// <summary>`ChoiceLU(label, strings)` behaves like `ChoiceL(label, strings)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ChoiceLU<U>(
            string label,
            params string[] strings)
            => choiceL(strings.Select(s => StringU<U>(s)), label);

        /// <summary>
        /// <para>`StringP(s)` parses the string `s` and returns `s`.</para>
        /// <para>
        /// It is an atomic parser: either it succeeds or it fails without consuming any input.
        /// </para>
        /// <para>`s` may not contain newline chars ('\n' or '\r').</para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> StringP(string s) => pstring<Unit>(s);

        /// <summary>`StringU(s)` behaves like `StringP(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> StringU<U>(string s) => pstring<U>(s);

        /// <summary>
        /// <para>
        /// `StringCI(s)` parses any string that case-insensitively matches the string `s`.
        /// It returns the *parsed* string.
        /// </para>
        /// <para>`s` may not contain newline chars ('\n' or '\r').</para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> StringCI(string s) => pstringCI<Unit>(s);

        /// <summary>`StringCIU(s)` behaves like `StringCI(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> StringCIU<U>(string s) => pstringCI<U>(s);

        /// <summary>
        /// `StringP(s,x)` is an optimized implementation of `StringP(s).Return(x)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<T>> StringP<T>(string s, T x) => stringReturn<T, Unit>(s, x);

        /// <summary>`StringU(s,x)` behaves like `StringP(s,x)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> StringU<U, T>(string s, T x) => stringReturn<T, U>(s, x);

        /// <summary>
        /// `StringCI(s,x)` is an optimized implementation of `StringCI(s).Return(x)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<T>> StringCI<T>(string s, T x) => stringCIReturn<T, Unit>(s, x);

        /// <summary>`StringCIU(s,x)` behaves like `StringCI(s,x)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> StringCIU<U, T>(string s, T x) => stringCIReturn<T, U>(s, x);

        /// <summary>
        /// `Skip(s)` parses the char `s` and skips it, i.e. returns `(Unit)null`. `s` may not
        /// contain newline chars ('\n' or '\r').
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> Skip(string s) => skipString<Unit>(s);

        /// <summary>`SkipU(s)` behaves like `Skip(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipU<U>(string s) => skipString<U>(s);

        /// <summary>
        /// `SkipCI(s)` parses any string that case-insensitively matches the string `s` and skips
        /// it , i.e. returns `(Unit)null`. `s` may not contain newline chars ('\n' or '\r').
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipCI(string s) => skipStringCI<Unit>(s);

        /// <summary>`SkipCIU(s)` behaves like `SkipCI(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipCIU<U>(string s) => skipStringCI<U>(s);

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
        public static FSharpFunc<CharStream<Unit>, Reply<string>> AnyString(int length) => anyString<Unit>(length);

        /// <summary>`AnyStringU(n)` behaves like `AnyString(n)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> AnyStringU<U>(int length) => anyString<U>(length);

        /// <summary>
        /// `SkipAnyString(n)` is an optimized implementation of `Skip(AnyString(n))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipAnyString(int length) => skipAnyString<Unit>(length);

        /// <summary>`SkipAnyStringU(n)` behaves like `SkipAnyString(n)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipAnyStringU<U>(int length) => skipAnyString<U>(length);

        /// <summary>
        /// <para>
        /// `RestOfLine(b)` parses any chars before the end of the line and, if `b` is `true`,
        /// skips to the beginning of the next line (if there is one).
        /// </para>
        /// <para>
        /// Returns the parsed chars before the end of the line as a string (without a newline).
        /// </para>
        /// <para>
        /// A line is terminated by a newline ("\n", "\r\n" or "\r") or the end of the input
        /// stream.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> RestOfLine(bool skipNewline = false)
            => restOfLine<Unit>(skipNewline);

        /// <summary>`RestOfLineU(b)` behaves like `RestOfLine(b)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> RestOfLineU<U>(bool skipNewline = false)
            => restOfLine<U>(skipNewline);

        /// <summary>
        /// `SkipRestOfLine(b)` is an optimized implementation of `Skip(RestOfLine(b))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipRestOfLine(bool skipNewline = false)
            => skipRestOfLine<Unit>(skipNewline);

        /// <summary>`SkipRestOfLineU(b)` behaves like `SkipRestOfLine(b)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipRestOfLineU<U>(bool skipNewline = false)
            => skipRestOfLine<U>(skipNewline);

        /// <summary>
        /// <para>
        /// `CharsTillString(s,n,b)` parses all chars before the first occurance of the string `s`
        /// and, if `b` is `true`, skips over `s`. It returns the parsed chars before the string.
        /// </para>
        /// <para>
        /// If more than `n` chars come before the first occurance of `s`, the parser *fails after
        /// consuming* `n` chars.
        /// </para>
        /// <para>
        /// Newlines ("\n", "\r\n" or "\r") are counted as single chars and in the returned string
        /// all newlines are normalized to "\n".
        /// </para>
        /// <para>
        /// `CharsTillString(s,n,b)` throws an `ArgumentOutOfRangeException` if `n` is negative.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> CharsTillString(
            string s,
            int maxCount,
            bool skipString = false)
            => charsTillString<Unit>(s, skipString, maxCount);

        /// <summary>`CharsTillStringU(s,n,b)` behaves like `CharsTillString(s,n,b)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> CharsTillStringU<U>(
            string s,
            int maxCount,
            bool skipString = false)
            => charsTillString<U>(s, skipString, maxCount);

        /// <summary>
        /// `SkipCharsTillString(s,n,b)` is an optimized implementation of
        /// `Skip(CharsTillString(s,n,b))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipCharsTillString(
            string s,
            int maxCount,
            bool skipString = false)
            => skipCharsTillString<Unit>(s, skipString, maxCount);

        /// <summary>`SkipCharsTillStringU(s,n,b)` behaves like `SkipCharsTillString(s,n,b)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipCharsTillStringU<U>(
            string s,
            int maxCount,
            bool skipString = false)
            => skipCharsTillString<U>(s, skipString, maxCount);

        /// <summary>
        /// <para>
        /// `CharsTillStringCI(s,n,b)` parses all chars before the first case-insensitive occurance
        /// of the string `s` and, if `b` is `true`, skips over it. It returns the parsed chars
        /// before the string.
        /// </para>
        /// <para>
        /// If more than `n` chars come before the first case-insensitive occurance of `s`, the
        /// parser *fails* after consuming `n` chars.
        /// </para>
        /// <para>
        /// Newlines ("\n", "\r\n" or "\r") are counted as single chars and in the returned string
        /// all newlines are normalized to "\n".
        /// </para>
        /// <para>
        /// `CharsTillStringCI(s,n,b)` throws an `ArgumentOutOfRangeException` if `n` is negative.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> CharsTillStringCI(
            string s,
            int maxCount,
            bool skipString = false)
            => charsTillStringCI<Unit>(s, skipString, maxCount);

        /// <summary>`CharsTillStringCIU(s,n,b)` behaves like `CharsTillStringCI(s,n,b)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> CharsTillStringCIU<U>(
            string s,
            int maxCount,
            bool skipString = false)
            => charsTillStringCI<U>(s, skipString, maxCount);

        /// <summary>
        /// `SkipCharsTillStringCI(s,n,b)` is an optimized implementation of
        /// `Skip(CharsTillStringCI(s,n,b))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipCharsTillStringCI(
            string s,
            int maxCount,
            bool skipString = false)
            => skipCharsTillStringCI<Unit>(s, skipString, maxCount);

        /// <summary>`SkipCharsTillStringCIU(s,n,b)` behaves like `SkipCharsTillStringCI(s,n,b)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipCharsTillStringCIU<U>(
            string s,
            int maxCount,
            bool skipString = false)
            => skipCharsTillStringCI<U>(s, skipString, maxCount);

        /// <summary>
        /// <para>
        /// `ManyChars(p)` parses a sequence of *zero* or more chars with the char parser `p`. It
        /// returns the parsed chars as a string.
        /// </para>
        /// <para>
        /// `ManyChars(p)` is an optimized implementation of `Many(Try(p))` that returns the
        /// chars as a string instead of a char list. The equivalence to `Many(Try(p))` instead of
        /// `Many(p)` implies that `ManyChars()` never fails.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyChars<U>(
            FSharpFunc<CharStream<U>, Reply<char>> p)
            => manyChars(p);

        /// <summary>
        /// `ManyChars(p1,p)` behaves like `ManyChars(p)`, except that it parses the first char
        /// with `p1` instead of `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyChars<U>(
            FSharpFunc<CharStream<U>, Reply<char>> p1,
            FSharpFunc<CharStream<U>, Reply<char>> p)
            => manyChars2(p1, p);

        /// <summary>
        /// <para>
        /// `Many1Chars(p)` parses a sequence of *one* or more chars with the char parser `p`. It
        /// returns the parsed chars as a string.
        /// </para>
        /// <para>
        /// `Many1Chars(p)` is an optimized implementation of `Many1(Try(p))` that returns the
        /// chars as a string instead of a char list. The equivalence to `Many1(Try(p))` instead of
        /// `Many1(p)` implies that `Many1Chars()` never fails after consuming input.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1Chars<U>(
            FSharpFunc<CharStream<U>, Reply<char>> p)
            => many1Chars(p);

        /// <summary>
        /// `Many1Chars(p1,p)` behaves like `Many1Chars(p)`, except that it parses the first char
        /// with `p1` instead of `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1Chars<U>(
            FSharpFunc<CharStream<U>, Reply<char>> p1, FSharpFunc<CharStream<U>, Reply<char>> p)
            => many1Chars2(p1, p);

        /// <summary>
        /// <para>
        /// `ManyChars(f)` parses a sequence of *zero* or more chars that satisfy the predicate
        /// function `f` (i.e.  chars for which `f` returns `true`). It returns the parsed chars as
        /// a string.
        /// </para>
        /// <para>
        /// Any newline ("\n", "\r\n" or "\r") is converted to the single char '\n'. Thus, to
        /// accept a newline `f('\n')` must return `true`. `f` will never be called with '\r' and
        /// the string returned by `ManyChars(f)` will never contain an '\r'.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> ManyChars(Func<char, bool> pred)
            => manySatisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>`ManyCharsU(f)` behaves like `ManyChars(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyCharsU<U>(Func<char, bool> pred)
            => manySatisfy<U>(pred.ToFSharpFunc());

        /// <summary>
        /// `SkipManyChars(f)` is an optimized implementation of `Skip(ManyChars(f))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipManyChars(Func<char, bool> pred)
            => skipManySatisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>`SkipManyCharsU(f)` behaves like `SkipManyChars(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipManyCharsU<U>(Func<char, bool> pred)
            => skipManySatisfy<U>(pred.ToFSharpFunc());

        /// <summary>
        /// `ManyChars(f1,f)` behaves like `ManyChars(f)`, except that the first char of the parsed
        /// string must satisfy `f1` instead of `f`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> ManyChars(
            Func<char, bool> pred1,
            Func<char, bool> pred)
            => manySatisfy2<Unit>(pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>`ManyCharsU(f1,f1)` behaves like `ManyChars(f1,f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyCharsU<U>(
            Func<char, bool> pred1,
            Func<char, bool> pred)
            => manySatisfy2<U>(pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>
        /// `SkipManyChars(f1,f)` is an optimized implementation of `Skip(ManyChars(f1,f))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipManyChars(
            Func<char, bool> pred1,
            Func<char, bool> pred)
            => skipManySatisfy2<Unit>(pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>`SkipManyCharsU(f1,f)` behaves like `SkipManyChars(f1,f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipManyCharsU<U>(
            Func<char, bool> pred1,
            Func<char, bool> pred)
            => skipManySatisfy2<U>(pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>
        /// <para>
        /// `Many1Chars(f)` parses a sequence of *one* or more chars that satisfy the predicate
        /// function `f` (i.e. chars for which `f` returns `true`). It returns the parsed chars as
        /// a string.
        /// </para>
        /// <para>
        /// If the first char does not satisfy `f`, this parser fails without consuming input.
        /// </para>
        /// <para>
        /// Any newline ("\n", "\r\n" or "\r") is converted to the single char '\n'. Thus, to
        /// accept a newline `f('\n')` must return `true`. `f` will never be called with '\r' and
        /// the string returned by `Many1Chars(f)` will never contain an '\r'.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> Many1Chars(Func<char, bool> pred)
            => many1Satisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>`Many1CharsU(f)` behaves like `Many1Chars(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1CharsU<U>(Func<char, bool> pred)
            => many1Satisfy<U>(pred.ToFSharpFunc());

        /// <summary>
        /// `Many1Chars(f,s)` is an optimized implementation of `Many1Chars(f).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> Many1Chars(
            Func<char, bool> pred,
            string label)
            => many1SatisfyL<Unit>(pred.ToFSharpFunc(), label);

        /// <summary>`Many1CharsU(f,s)` behaves like `Many1Chars(f,s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1CharsU<U>(
            Func<char, bool> pred,
            string label)
            => many1SatisfyL<U>(pred.ToFSharpFunc(), label);

        /// <summary>
        /// `SkipMany1Chars(f)` is an optimized implementation of `Skip(Many1Chars(f))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipMany1Chars(Func<char, bool> pred)
            => skipMany1Satisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>`SkipMany1CharsU(f)` behaves like `SkipMany1Chars(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany1CharsU<U>(Func<char, bool> pred)
            => skipMany1Satisfy<U>(pred.ToFSharpFunc());

        /// <summary>
        /// `SkipMany1Chars(f,s)` is an optimized implementation of `SkipMany1Chars(f).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipMany1Chars(
            Func<char, bool> pred,
            string label)
            => skipMany1SatisfyL<Unit>(pred.ToFSharpFunc(), label);

        /// <summary>`SkipMany1CharsU(f,s)` behaves like `SkipMany1Chars(f,s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany1CharsU<U>(
            Func<char, bool> pred,
            string label)
            => skipMany1SatisfyL<U>(pred.ToFSharpFunc(), label);

        /// <summary>
        /// `Many1Chars(f1,f)` behaves like `Many1Chars(f)`, except that the first char of the
        /// parsed string must satisfy `f1` instead of `f`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> Many1Chars(
            Func<char, bool> pred1,
            Func<char, bool> pred)
            => many1Satisfy2<Unit>(pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>`Many1CharsU(f1,f)` behaves like `Many1Chars(f1,f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1CharsU<U>(
            Func<char, bool> pred1,
            Func<char, bool> pred)
            => many1Satisfy2<U>(pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>
        /// `Many1Chars(f1,f,s)` is an optimized implementation of `Many1Chars(f1,f).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> Many1Chars(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            string label)
            => many1Satisfy2L<Unit>(pred1.ToFSharpFunc(), pred.ToFSharpFunc(), label);

        /// <summary>`Many1CharsU(f1,f,s)` behaves like `Many1Chars(f1,f,s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1CharsU<U>(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            string label)
            => many1Satisfy2L<U>(pred1.ToFSharpFunc(), pred.ToFSharpFunc(), label);

        /// <summary>
        /// `SkipMany1Chars(f1,f)` is an optimized implementation of `Skip(Many1Chars(f1,f))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipMany1Chars(
            Func<char, bool> pred1,
            Func<char, bool> pred)
            => skipMany1Satisfy2<Unit>(pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>`SkipMany1CharsU(f1,f)` behaves like `SkipMany1Chars(f1,f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany1CharsU<U>(
            Func<char, bool> pred1,
            Func<char, bool> pred)
            => skipMany1Satisfy2<U>(pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>
        /// `SkipMany1Chars(f1,f,s)` is an optimized implementation of
        /// `SkipMany1Chars(f1,f).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipMany1Chars(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            string label)
            => skipMany1Satisfy2L<Unit>(pred1.ToFSharpFunc(), pred.ToFSharpFunc(), label);

        /// <summary>`SkipMany1CharsU(f1,f,s)` behaves like `SkipMany1Chars(f1,f,s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany1CharsU<U>(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            string label)
            => skipMany1Satisfy2L<U>(pred1.ToFSharpFunc(), pred.ToFSharpFunc(), label);

        /// <summary>
        /// <para>
        /// `ManyChars(f,min,max)` parses a sequence of `min` or more chars that satisfy the
        /// predicate function `f` (i.e. chars for which `f` returns `true`), but not more than
        /// `max` chars. It returns the parsed chars as a string.
        /// </para>
        /// <para>
        /// This parser is atomic, i.e. if the first `min` chars do not all satisfy `f`, the parser
        /// fails without consuming any input.
        /// </para>
        /// <para>
        /// Any newline ("\n", "\r\n" or "\r") is converted to the single char '\n'. Thus, to
        /// accept a newline `f '\n'` must return `true`. `f` will never be called with '\r' and
        /// the string returned by `ManyChars(f,min,max)` will never contain an '\r'.
        /// </para>
        /// <para>`ManyChars(f,min,max)` throws an `ArgumentOutOfRangeException` if `max` is negative.</para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> ManyChars(
            Func<char, bool> pred,
            int minCount,
            int maxCount)
            => manyMinMaxSatisfy<Unit>(minCount, maxCount, pred.ToFSharpFunc());

        /// <summary>`ManyCharsU(f,min,max)` behaves like `ManyChars(f,min,max)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyCharsU<U>(
            Func<char, bool> pred,
            int minCount,
            int maxCount)
            => manyMinMaxSatisfy<U>(minCount, maxCount, pred.ToFSharpFunc());

        /// <summary>
        /// `ManyChars(f,min,max,s)` is an optimized implementation of
        /// `ManyChars(f,min,max).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> ManyChars(
            Func<char, bool> pred,
            int minCount,
            int maxCount,
            string label)
            => manyMinMaxSatisfyL<Unit>(minCount, maxCount, pred.ToFSharpFunc(), label);

        /// <summary>`ManyCharsU(f,min,max,s)` behaves like `ManyChars(f,min,max,s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyCharsU<U>(
            Func<char, bool> pred,
            int minCount,
            int maxCount,
            string label)
            => manyMinMaxSatisfyL<U>(minCount, maxCount, pred.ToFSharpFunc(), label);

        /// <summary>
        /// `SkipManyChars(f,min,max)` is an optimized implementation of
        /// `Skip(ManyChars(f,min,max))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipManyChars(
            Func<char, bool> pred,
            int minCount,
            int maxCount)
            => skipManyMinMaxSatisfy<Unit>(minCount, maxCount, pred.ToFSharpFunc());

        /// <summary>`SkipManyCharsU(f,min,max)` behaves like `SkipManyChars(f,min,max)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipManyCharsU<U>(
            Func<char, bool> pred,
            int minCount,
            int maxCount)
            => skipManyMinMaxSatisfy<U>(minCount, maxCount, pred.ToFSharpFunc());

        /// <summary>
        /// `SkipManyChars(f,min,max,s)` is an optimized implementation of
        /// `SkipManyChars(f,min,max).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipManyChars(
            Func<char, bool> pred,
            int minCount,
            int maxCount,
            string label)
            => skipManyMinMaxSatisfyL<Unit>(minCount, maxCount, pred.ToFSharpFunc(), label);

        /// <summary>`SkipManyCharsU(f,min,max,s)` behaves like `SkipManyChars(f,min,max,s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipManyCharsU<U>(
            Func<char, bool> pred,
            int minCount,
            int maxCount,
            string label)
            => skipManyMinMaxSatisfyL<U>(minCount, maxCount, pred.ToFSharpFunc(), label);

        /// <summary>
        /// `ManyChars(f1,f,min,max)` behaves like `ManyChars(f,min,max)`, except that the first
        /// char of the parsed string must satisfy `f1` instead of `f`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> ManyChars(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            int minCount,
            int maxCount)
            => manyMinMaxSatisfy2<Unit>(minCount, maxCount, pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>`ManyCharsU(f1,f,min,max)` behaves like `ManyChars(f1,f,min,max)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyCharsU<U>(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            int minCount,
            int maxCount)
            => manyMinMaxSatisfy2<U>(minCount, maxCount, pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>
        /// `ManyChars(f1,f,min,max,s)` is an optimized implementation of
        /// `ManyChars(f1,f,min,max).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> ManyChars(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            int minCount,
            int maxCount,
            string label)
            => manyMinMaxSatisfy2L<Unit>(minCount, maxCount, pred1.ToFSharpFunc(), pred.ToFSharpFunc(), label);

        /// <summary>`ManyCharsU(f1,f,min,max,s)` behaves like `ManyChars(f1,f,min,max,s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyCharsU<U>(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            int minCount,
            int maxCount,
            string label)
            => manyMinMaxSatisfy2L<U>(minCount, maxCount, pred1.ToFSharpFunc(), pred.ToFSharpFunc(), label);

        /// <summary>
        /// `SkipManyChars(f1,f,min,max)` is an optimized implementation of
        /// `Skip(ManyChars(f1,f,min,max))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipManyChars(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            int minCount,
            int maxCount)
            => skipManyMinMaxSatisfy2<Unit>(minCount, maxCount, pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>`SkipManyCharsU(f1,f,min,max)` behaves like `SkipManyChars(f1,f,min,max)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipManyCharsU<U>(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            int minCount,
            int maxCount)
            => skipManyMinMaxSatisfy2<U>(minCount, maxCount, pred1.ToFSharpFunc(), pred.ToFSharpFunc());

        /// <summary>
        /// `SkipManyChars(f1,f,min,max,s)` is an optimized implementation of
        /// `SkipManyChars(f1,f,min,max).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipManyChars(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            int minCount,
            int maxCount,
            string label)
            => skipManyMinMaxSatisfy2L<Unit>(minCount, maxCount, pred1.ToFSharpFunc(), pred.ToFSharpFunc(), label);

        /// <summary>`SkipManyCharsU(f1,f,min,max,s)` behaves like `SkipManyChars(f1,f,min,max,s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipManyCharsU<U>(
            Func<char, bool> pred1,
            Func<char, bool> pred,
            int minCount,
            int maxCount,
            string label)
            => skipManyMinMaxSatisfy2L<U>(minCount, maxCount, pred1.ToFSharpFunc(), pred.ToFSharpFunc(), label);

        /// <summary>
        /// `ManyCharsTill(p,endp)` parses chars with the char parser `p` until the parser `endp`
        /// succeeds. It stops after `endp` and returns the parsed chars as a string.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyCharsTill<U, TEnd>(
            FSharpFunc<CharStream<U>, Reply<char>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp)
            => manyCharsTill(p, endp);

        /// <summary>
        /// `ManyCharsTill(p1,p,endp)` behaves like `ManyCharsTill(p,endp)`, except that it parses
        /// the first char with `p1` instead of `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyCharsTill<U, TEnd>(
            FSharpFunc<CharStream<U>, Reply<char>> p1,
            FSharpFunc<CharStream<U>, Reply<char>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp)
            => manyCharsTill2(p1, p, endp);

        /// <summary>
        /// `ManyCharsTill(p,endp,f)` parses chars with the char parser `p` until the parser `endp`
        /// succeeds. It stops after `endp` and returns the result of the function application
        /// `f(s,b)`, where `s` is the parsed string and `b` is result returned by `endp`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> ManyCharsTill<U, T, TEnd>(
            FSharpFunc<CharStream<U>, Reply<char>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp,
            Func<string, TEnd, T> f)
            => manyCharsTillApply(p, endp, f.ToFSharpFunc());

        /// <summary>
        /// `ManyCharsTill(p1,p,endp,f)` behaves like `ManyCharsTill(p,endp,f)`, except that it
        /// parses the first char with `p1` instead of `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> ManyCharsTill<U, T, TEnd>(
            FSharpFunc<CharStream<U>, Reply<char>> p1,
            FSharpFunc<CharStream<U>, Reply<char>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp,
            Func<string, TEnd, T> f)
            => manyCharsTillApply2(p1, p, endp, f.ToFSharpFunc());

        /// <summary>
        /// <para>
        /// `Many1CharsTill(p,endp)` parses one char with the char parser `p`. Then it parses more
        /// chars with `p` until the parser `endp` succeeds. It stops after `endp` and returns the
        /// parsed chars as a string.
        /// </para>
        /// <para>
        /// `Many1CharsTill(p,endp)` is an optimized implementation of
        /// `Pipe(p, ManyCharsTill(p,endp), (c,s) => s.Prepend(c))`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1CharsTill<U, TEnd>(
            FSharpFunc<CharStream<U>, Reply<char>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp)
            => many1CharsTill(p, endp);

        /// <summary>
        /// `Many1CharsTill(p1,p,endp)` behaves like `Many1CharsTill(p,endp)`, except that it
        /// parses the first char with `p1` instead of `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1CharsTill<U, TEnd>(
            FSharpFunc<CharStream<U>, Reply<char>> p1,
            FSharpFunc<CharStream<U>, Reply<char>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp)
            => many1CharsTill2(p1, p, endp);

        /// <summary>
        /// `Many1CharsTill(p,endp,f)` parses one char with the char parser `p`. Then it parses
        /// more chars with `p` until the parser `endp` succeeds. It stops after `endp` and returns
        /// the result of the function application `f(s,b)`, where `s` is the parsed string and `b`
        /// is result returned by `endp`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Many1CharsTill<U, T, TEnd>(
            FSharpFunc<CharStream<U>, Reply<char>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp,
            Func<string, TEnd, T> f)
            => many1CharsTillApply(p, endp, f.ToFSharpFunc());

        /// <summary>
        /// `Many1CharsTill(p1,p,endp,f)` behaves like `Many1CharsTill(p,endp,f)`, except that it
        /// parses the first char with `p1` instead of `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Many1CharsTill<U, T, TEnd>(
            FSharpFunc<CharStream<U>, Reply<char>> p1,
            FSharpFunc<CharStream<U>, Reply<char>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp,
            Func<string, TEnd, T> f)
            => many1CharsTillApply2(p1, p, endp, f.ToFSharpFunc());

        /// <summary>
        /// <para>
        /// `ManyStrings(p)` parses a sequence of *zero* or more strings with the string parser
        /// `p`. It returns the strings in concatenated form.
        /// </para>
        /// <para>
        /// `ManyStrings(p)` is an optimized implementation of `Many(p, string.Concat, "")`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyStrings<U>(
            FSharpFunc<CharStream<U>, Reply<string>> p)
            => manyStrings(p);

        /// <summary>
        /// <para>
        /// `ManyStrings(p,sep)` parses *zero* or more occurrences of the string parser `p`
        /// separated by string parser `sep`.
        /// </para>
        /// <para>
        /// It returns the strings parsed by `p` *and* `sep` in concatenated form.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyStrings<U>(
            FSharpFunc<CharStream<U>, Reply<string>> p,
            FSharpFunc<CharStream<U>, Reply<string>> sep)
            => stringsSepBy(p, sep);

        /// <summary>
        /// <para>
        /// `ManyStrings(p,s)` parses *zero* or more occurrences of the string parser `p`
        /// separated by string `s`.
        /// </para>
        /// <para>
        /// It returns the strings parsed by `p` *and* the string `s` in concatenated form.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> ManyStrings<U>(
            FSharpFunc<CharStream<U>, Reply<string>> p,
            string sep)
            => stringsSepBy(p, pstring<U>(sep));

        /// <summary>
        /// <para>
        /// `Many1Strings(p)` parses a sequence of *one* or more strings with the string parser
        /// `p`. It returns the strings in concatenated form.
        /// </para>
        /// <para>
        /// Note that `Many1Strings(p)` does not require the first string to be non-empty.
        /// </para>
        /// <para>
        /// `Many1Strings(p)` is an optimized implementation of `Many1(p, string.Concat)`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1Strings<U>(
            FSharpFunc<CharStream<U>, Reply<string>> p)
            => many1Strings(p);

        /// <summary>
        /// <para>
        /// `Many1Strings(p,sep)` parses *one* or more occurrences of the string parser `p`
        /// separated by string parser `sep`.
        /// </para>
        /// <para>
        /// It returns the strings parsed by `p` *and* `sep` in concatenated form.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1Strings<U>(
            FSharpFunc<CharStream<U>, Reply<string>> p,
            FSharpFunc<CharStream<U>, Reply<string>> sep)
            => stringsSepBy(p, sep);

        /// <summary>
        /// <para>
        /// `Many1Strings(p,s)` parses *one* or more occurrences of the string parser `p`
        /// separated by string `s`.
        /// </para>
        /// <para>
        /// It returns the strings parsed by `p` *and* the string `s` in concatenated form.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> Many1Strings<U>(
            FSharpFunc<CharStream<U>, Reply<string>> p,
            string sep)
            => stringsSepBy(p, pstring<U>(sep));

        /// <summary>
        /// <para>
        /// `Regex(s)` matches the .NET regular expression given by the string `s` on the chars
        /// beginning at the current index in the input stream. It returns the string matched by
        /// the regular expression. If the regular expression does not match, the parser fails
        /// without consuming input.
        /// </para>
        /// <para>
        /// The `System.Text.RegularExpressions.Regex` object that is internally used to match the
        /// pattern is constructed with the `RegexOptions` `MultiLine` and `ExplicitCapture`. In
        /// order to ensure that the regular expression can only match at the beginning of a
        /// string, "\\A" is automatically prepended to the pattern.
        /// </para>
        /// <para>
        /// Newline chars ('\r' and '\n') in the pattern are interpreted literally. For example, an
        /// '\n' char in the pattern will only match "\n", not "\r" or "\r\n". However, in the
        /// returned string all newlines ("\n", "\r\n" or "\r") are normalized to "\n".
        /// </para>
        /// <para>
        /// For large files the regular expression is *not* applied to a string containing *all*
        /// the remaining chars in the stream. The number of chars that are guaranteed to be
        /// visible to the regular expression is specified during construction of the `CharStream`.
        /// If one of the `runParser` functions is used to run the parser, this number is 43690.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> Regex(string pattern) => regex<Unit>(pattern);

        /// <summary>`RegexU(s)` behaves like `Regex(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> RegexU<U>(string pattern) => regex<U>(pattern);

        /// <summary>
        /// `Regex(s,l)` is an optimized implementation of `Regex(s).Label(l)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<string>> Regex(string pattern, string label)=> regexL<Unit>(pattern, label);

        /// <summary>`RegexU(s,l)` behaves like `Regex(s,l)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> RegexU<U>(string pattern, string label) => regexL<U>(pattern, label);

        /// <summary>
        /// <para>
        /// `p.WithSkipped()` applies the skipping parser `p` and returns the chars skipped over by
        /// `p` as a string.
        /// </para>
        /// <para>All newlines ("\r\n", "\r" or "\n") are normalized to "\n".</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<string>> WithSkipped<U>(
            this FSharpFunc<CharStream<U>, Reply<Unit>> p)
            => skipped(p);

        /// <summary>
        /// `p.WithSkipped(f)` applies the parser `p` and returns the result of `f(s,x)`, where `s`
        /// is the string skipped over by `p` and `x` is the result returned by `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> WithSkipped<U, T, TResult>(
            this FSharpFunc<CharStream<U>, Reply<T>> p,
            Func<string, T, TResult> f)
            => withSkippedString(f.ToFSharpFunc(), p);

        /// <summary>
        /// `p.WithSkipped()` applies the parser `p` and returns the tuple `(s,x)`, where `s` is
        /// the string skipped over by `p` and `x` is the result returned by `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<(string, T)>> WithSkipped<U, T>(
            this FSharpFunc<CharStream<U>, Reply<T>> p)
            => withSkippedString(FSharpFunc.From<string, T, (string, T)>((s, x) => (s, x)), p);

        /// <summary>
        /// <para>
        /// `FoldCase(s)` returns a case-folded version of `s` with all chars mappend using the
        /// (non-Turkic) Unicode 1-to-1 case folding mappings for chars below 0x10000.
        /// </para>
        /// <para>If the argument is `null`, `null` is returned.</para>
        /// </summary>
        public static string FoldCase(string s) => foldCase(s);

        #endregion Strings

        #region Numbers

        /// <summary>
        /// <para>
        /// The parser `Natural` parses a sequence of one or more digits and converts them to an
        /// `int`.
        /// </para>
        /// <para>
        /// `Natural` fails if the value represented by the input string is greater than
        /// `System.Int32.MaxValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<int>> Natural = NaturalU<Unit>();

        /// <summary>`NaturalU()` behaves like `Natural`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<int>> NaturalU<U>() => FSharpFunc.From<CharStream<U>, Reply<int>>(chars =>
            many1Satisfy<U>(FSharpFunc.From<char, bool>(isDigit)).Invoke(chars) switch {
                (Ok, var r, _) => int.TryParse(r, out var n)
                    ? new Reply<int>(n)
                    : new Reply<int>(Primitives.Error, messageError("Number must be below 2147483648")),
                _ => new Reply<int>(Primitives.Error, expected("natural number"))
            });

        /// <summary>
        /// <para>
        /// Parses a floating-point number in decimal or hexadecimal format.
        /// The special values NaN and Inf(inity)? (case insensitive) are also recognized.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in "0x") can be
        /// parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        /// comes after "0x",
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string (after rounding) is
        /// greater than `System.Double.MaxValue` or less than `System.Double.MinValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<double>> Float = pfloat<Unit>();

        /// <summary>`FloatU()` behaves like `Float`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<double>> FloatU<U>() => pfloat<U>();

        /// <summary>
        /// <para>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in the format
        /// specifiers "0x" etc.) can be parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        /// comes after a format specifier,
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string is greater than
        /// `System.Int64.MaxValue` or less than `System.Int64.MinValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<long>> Long = pint64<Unit>();

        /// <summary>`LongU()` behaves like `Long`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<long>> LongU<U>() => pint64<U>();

        /// <summary>
        /// <para>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in the format
        /// specifiers "0x" etc.) can be parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        /// comes after a format specifier,
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string is greater than
        /// `System.UInt64.MaxValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<ulong>> ULong = puint64<Unit>();

        /// <summary>`ULongU()` behaves like `ULong`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<ulong>> ULongU<U>() => puint64<U>();

        /// <summary>
        /// <para>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in the format
        /// specifiers "0x" etc.) can be parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        /// comes after a format specifier,
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string is greater than
        /// `System.Int32.MaxValue` or less than `System.Int32.MinValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<int>> Int = pint32<Unit>();

        /// <summary>`IntU()` behaves like `Int`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<int>> IntU<U>() => pint32<U>();

        /// <summary>
        /// <para>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in the format
        /// specifiers "0x" etc.) can be parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        /// comes after a format specifier,
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string is greater than
        /// `System.UInt32.MaxValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<uint>> UInt = puint32<Unit>();

        /// <summary>`UIntU()` behaves like `UInt`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<uint>> UIntU<U>() => puint32<U>();

        /// <summary>
        /// <para>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in the format
        /// specifiers "0x" etc.) can be parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        /// comes after a format specifier,
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string is greater than
        /// `System.Int16.MaxValue` or less than `System.Int16.MinValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<short>> Short = pint16<Unit>();

        /// <summary>`ShortU()` behaves like `Short`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<short>> ShortU<U>() => pint16<U>();

        /// <summary>
        /// <para>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in the format
        /// specifiers "0x" etc.) can be parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        /// comes after a format specifier,
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string is greater than
        /// `System.UInt16.MaxValue`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<ushort>> UShort = puint16<Unit>();

        /// <summary>`UShortU()` behaves like `UShort`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<ushort>> UShortU<U>() => puint16<U>();

        /// <summary>
        /// <para>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in the format
        /// specifiers "0x" etc.) can be parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        /// comes after a format specifier,
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string is greater than
        /// 127 or less than -128.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<sbyte>> Byte = pint8<Unit>();

        /// <summary>`ByteU()` behaves like `Byte`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<sbyte>> ByteU<U>() => pint8<U>();

        /// <summary>
        /// <para>
        /// Parses an integer in decimal, hexadecimal ("0x" prefix), octal ("0o") or binary ("0b")
        /// format.
        /// </para>
        /// <para>The parser fails:</para>
        /// <para>
        /// - without consuming input, if not at least one digit (including the '0' in the format
        /// specifiers "0x" etc.) can be parsed,
        /// </para>
        /// <para>
        /// - after consuming input, if no digit comes after an exponent marker or no hex digit
        /// comes after a format specifier,
        /// </para>
        /// <para>
        /// - after consuming input, if the value represented by the input string is greater than
        /// 255.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<byte>> UByte = puint8<Unit>();

        /// <summary>`UByteU()` behaves like `UByte`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<byte>> UByteU<U>() => puint8<U>();

        /// <summary>
        /// Returns a hexadecimal string representation of the `double`.
        /// </summary>
        public static string DoubleToHexString(double x) => floatToHexString(x);

        /// <summary>
        /// <para>
        /// Returns the `double` value represented by the given string in hexadecimal format.
        /// </para>
        /// <para>
        /// Raises a `System.FormatException` in case the string representation is invalid.
        /// </para>
        /// <para>
        /// Raises a `System.OverflowException` if the (absolute) value is too large to be
        /// represented by a `double`.
        /// </para>
        /// </summary>
        public static double DoubleOfHexString(string s) => floatOfHexString(s);

        /// <summary>
        /// Returns a hexadecimal string representation of the `float`.
        /// </summary>
        public static string FloatToHexString(float x) => float32ToHexString(x);

        /// <summary>
        /// <para>
        /// Returns the `float` value represented by the given string in hexadecimal format.
        /// </para>
        /// <para>
        /// Raises a `System.FormatException` in case the string representation is invalid.
        /// </para>
        /// <para>
        /// Raises a `System.OverflowException` if the (absolute) value is too large to be
        /// represented by a `float`.
        /// </para>
        /// </summary>
        public static float FloatOfHexString(string s) => float32OfHexString(s);

        #endregion Numbers

        #region Whitespace

        /// <summary>
        /// Skips over any sequence of *zero* or more whitespaces (space (' '), tab ('\t') or
        /// newline ("\n", "\r\n" or "\r")).
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> Spaces = spaces<Unit>();

        /// <summary>`SpacesU()` behaves like `Spaces`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SpacesU<U>() => spaces<U>();

        /// <summary>Short form for `Spaces`.</summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> WS = Spaces;

        /// <summary>
        /// Skips over any sequence of *zero* or more unicode whitespaces and registers any unicode
        /// newline ("\n", "\r\n", "\r", "\u0085, "\u000C", "\u2028", or "\u2029") as a newline.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> UnicodeSpaces = unicodeSpaces<Unit>();

        /// <summary>`UnicodeSpacesU()` behaves like `UnicodeSpaces`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> UnicodeSpacesU<U>() => unicodeSpaces<U>();

        /// <summary>
        /// Skips over any sequence of *one* or more whitespaces (space (' '), tab('\t') or
        /// newline ("\n", "\r\n" or "\r")).
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> Spaces1 = spaces1<Unit>();

        /// <summary>`Spaces1U()` behaves like `Spaces1`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> Spaces1U<U>() => spaces1<U>();

        /// <summary>Short form for `Spaces1`.</summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> WS1 = Spaces1;

        /// <summary>
        /// Skips over any sequence of *one* or more unicode whitespaces and registers any unicode
        /// newline ("\n", "\r\n", "\r", "\u0085, "\u000C", "\u2028", or "\u2029") as a newline.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> UnicodeSpaces1 = unicodeSpaces1<Unit>();

        /// <summary>`UnicodeSpaces1U()` behaves like `UnicodeSpaces1`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> UnicodeSpaces1U<U>() => unicodeSpaces1<U>();

        /// <summary>
        /// Parses a newline ("\n", "\r\n" or "\r"). Returns '\n'. Is equivalent to `CharP('\n')`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> Newline = newline<Unit>();

        /// <summary>`NewlineU()` behaves like `Newline`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> NewlineU<U>() => newline<U>();

        /// <summary>Short form for `Newline`.</summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> NL = newline<Unit>();

        /// <summary>
        /// `NewlineReturn(x)` is an optimized implementation of `Newline.Return(x)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<T>> NewlineReturn<T>(T x) => newlineReturn<T, Unit>(x);

        /// <summary>`NewlineReturnU(x)` behaves like `NewlineReturn(x)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> NewlineReturnU<U, T>(T x) => newlineReturn<T, U>(x);

        /// <summary>
        /// `SkipNewline` is an optimized implementation of `Skip(Newline)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipNewline = skipNewline<Unit>();

        /// <summary>`SkipNewlineU()` behaves like `SkipNewline`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipNewlineU<U>() => skipNewline<U>();

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
        public static FSharpFunc<CharStream<Unit>, Reply<char>> UnicodeNewline = unicodeNewline<Unit>();

        /// <summary>`UnicodeNewlineU()` behaves like `UnicodeNewline`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> UnicodeNewlineU<U>() => unicodeNewline<U>();

        /// <summary>
        /// `UnicodeNewlineReturn(x)` is an optimized implementation of `UnicodeNewline.Return(x)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<T>> UnicodeNewlineReturn<T>(T x) => unicodeNewlineReturn<T, Unit>(x);

        /// <summary>`UnicodeNewlineReturnU(x)` behaves like `UnicodeNewlineReturn(x)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> UnicodeNewlineReturnU<U, T>(T x) => unicodeNewlineReturn<T, U>(x);

        /// <summary>
        /// `SkipUnicodeNewline` is an optimized implementation of `Skip(UnicodeNewline)`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> SkipUnicodeNewline = skipUnicodeNewline<Unit>();

        /// <summary>`SkipUnicodeNewlineU()` behaves like `SkipUnicodeNewline`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipUnicodeNewlineU<U>() => skipUnicodeNewline<U>();

        /// <summary>
        /// <para>Parses the tab char '\t' and returns '\t'.</para>
        /// <para>
        /// Note that a tab char is treated like any other non-newline char: the column number is
        /// incremented by (only) 1.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<char>> Tab = tab<Unit>();

        /// <summary>`TabU()` behaves like `Tab`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<char>> TabU<U>() => tab<U>();

        /// <summary>
        /// The parser `EOF` only succeeds at the end of the input. It never consumes input.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> EOF = eof<Unit>();

        /// <summary>`EOFU()` behaves like `EOF`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> EOFU<U>() => eof<U>();

        /// <summary>
        /// <para>
        /// `NormalizeNewlines(s)` returns a version of `s` with all occurances of "\r\n" and "\r"
        /// replaced by "\n".
        /// </para>
        /// <para>If the argument is `null`, `null` is returned.</para>
        /// </summary>
        public static string NormalizeNewlines(string s) => normalizeNewlines(s);

        #endregion Whitespace

        #region Conditional parsing

        /// <summary>
        /// `NotFollowedByEOF` is an optimized implementation of
        /// `NotFollowedBy(EOF, "end of input")`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> NotFollowedByEOF = notFollowedByEof<Unit>();

        /// <summary>`NotFollowedByEOFU()` behaves like `NotFollowedByEOF`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> NotFollowedByEOFU<U>() => notFollowedByEof<U>();

        /// <summary>
        /// `FollowedByNewline` is an optimized implementation of `FollowedBy(Newline, "newline")`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> FollowedByNewline = followedByNewline<Unit>();

        /// <summary>`FollowedByNewlineU()` behaves like `FollowedByNewline`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> FollowedByNewlineU<U>() => followedByNewline<U>();

        /// <summary>
        /// `NotFollowedByNewline` is an optimized implementation of
        /// `NotFollowedBy(Newline, "newline")`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> NotFollowedByNewline = notFollowedByNewline<Unit>();

        /// <summary>`NotFollowedByNewlineU()` behaves like `NotFollowedByNewline`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> NotFollowedByNewlineU<U>() => notFollowedByNewline<U>();

        /// <summary>
        /// `FollowedBy(s)` is an optimized implementation of `FollowedBy(StringP(s), $"'{s}'"))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> FollowedBy(string s) => followedByString<Unit>(s);

        /// <summary>`FollowedByU(s)` behaves like `FollowedBy(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> FollowedByU<U>(string s) => followedByString<U>(s);

        /// <summary>
        /// `NotFollowedBy(s)` is an optimized implementation of
        /// `NotFollowedBy(StringP(s), $"'{s}'"))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> NotFollowedBy(string s) => notFollowedByString<Unit>(s);

        /// <summary>`NotFollowedByU(s)` behaves like `NotFollowedBy(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> NotFollowedByU<U>(string s) => notFollowedByString<U>(s);

        /// <summary>
        /// `FollowedByCI(s)` is an optimized implementation of
        /// `FollowedBy(StringCI(s), $"'{s}'"))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> FollowedByCI(string s) => followedByStringCI<Unit>(s);

        /// <summary>`FollowedByCIU(s)` behaves like `FollowedByCI(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> FollowedByCIU<U>(string s) => followedByStringCI<U>(s);

        /// <summary>
        /// `NotFollowedByCI(s)` is an optimized implementation of
        /// `NotFollowedBy(StringCI(s), $"'{s}'"))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> NotFollowedByCI(string s) => notFollowedByStringCI<Unit>(s);

        /// <summary>`NotFollowedByCIU(s)` behaves like `NotFollowedByCI(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> NotFollowedByCIU<U>(string s) => notFollowedByStringCI<U>(s);

        /// <summary>
        /// `NextCharSatisfies(f)` is an optimized implementation of `FollowedBy(CharP(f))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> NextCharSatisfies(Func<char, bool> pred)
            => nextCharSatisfies<Unit>(pred.ToFSharpFunc());

        /// <summary>`NextCharSatisfiesU(f)` behaves like `NextCharSatisfies(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> NextCharSatisfiesU<U>(Func<char, bool> pred)
            => nextCharSatisfies<U>(pred.ToFSharpFunc());

        /// <summary>
        /// `NextCharSatisfiesNot(f)` is an optimized implementation of `NotFollowedBy(CharP(f))`.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> NextCharSatisfiesNot(Func<char, bool> pred)
            => nextCharSatisfiesNot<Unit>(pred.ToFSharpFunc());

        /// <summary>`NextCharSatisfiesNotU(f)` behaves like `NextCharSatisfiesNot(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> NextCharSatisfiesNotU<U>(Func<char, bool> pred)
            => nextCharSatisfiesNot<U>(pred.ToFSharpFunc());

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
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> Next2CharsSatisfy(Func<char, char, bool> pred)
            => next2CharsSatisfy<Unit>(pred.ToFSharpFunc());

        /// <summary>`Next2CharsSatisfyU(f)` behaves like `Next2CharsSatisfy(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> Next2CharsSatisfyU<U>(Func<char, char, bool> pred)
            => next2CharsSatisfy<U>(pred.ToFSharpFunc());

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
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> Next2CharsSatisfyNot(Func<char, char, bool> pred)
            => next2CharsSatisfyNot<Unit>(pred.ToFSharpFunc());

        /// <summary>`Next2CharsSatisfyNotU(f)` behaves like `Next2CharsSatisfyNot(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> Next2CharsSatisfyNotU<U>(Func<char, char, bool> pred)
            => next2CharsSatisfyNot<U>(pred.ToFSharpFunc());

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
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> PreviousCharSatisfies(Func<char, bool> pred)
            => previousCharSatisfies<Unit>(pred.ToFSharpFunc());

        /// <summary>`PreviousCharSatisfiesU(f)` behaves like `PreviousCharSatisfies(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> PreviousCharSatisfiesU<U>(Func<char, bool> pred)
            => previousCharSatisfies<U>(pred.ToFSharpFunc());

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
        public static FSharpFunc<CharStream<Unit>, Reply<Unit>> PreviousCharSatisfiesNot(Func<char, bool> pred)
            => previousCharSatisfiesNot<Unit>(pred.ToFSharpFunc());

        /// <summary>`PreviousCharSatisfiesNotU(f)` behaves like `PreviousCharSatisfiesNot(f)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> PreviousCharSatisfiesNotU<U>(Func<char, bool> pred)
            => previousCharSatisfiesNot<U>(pred.ToFSharpFunc());

        #endregion Conditional parsing
    }
}
