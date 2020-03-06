using System;
using System.Linq;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using static FParsec.CharParsers;
using static FParsec.Primitives;

namespace FParsec.CSharp {
    /// <summary>Provides combinator functions.</summary>
    public static class PrimitivesCS {
        #region Sequence

        /// <summary>
        /// The parser `p1.And(p2)` applies the parsers `p1` and `p2` in sequence and returns
        /// the results in a tuple.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<(T1, T2)>> And<U, T1, T2>(
            this FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => op_DotGreaterGreaterDot(p1, p2).Map(x => x.ToValueTuple());

        /// <summary>
        /// <para>
        /// The parser `p1.And(p2)` applies the parsers `p1` and `p2` in sequence and returns
        /// the result of `p1`.
        /// </para>
        /// <para>
        /// Since `p2` is a skipping parser that returns `Unit`, its result will not be returned.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T1>> And<U, T1>(
            this FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<Unit>> p2)
            => op_DotGreaterGreater(p1, p2);

        /// <summary>
        /// <para>
        /// The parser `p1.And(p2)` applies the parsers `p1` and `p2` in sequence and returns
        /// the result of `p2`.
        /// </para>
        /// <para>
        /// Since `p1` is a skipping parser that returns `Unit`, its result will not be returned.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T2>> And<U, T2>(
            this FSharpFunc<CharStream<U>, Reply<Unit>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => op_GreaterGreaterDot(p1, p2);

        /// <summary>
        /// `p1.And_(p2)` behaves like `p1.And(p2)` except that it will always return both parser
        /// results even if either of them returns `Unit`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<(T1, T2)>> And_<U, T1, T2>(
            this FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => op_DotGreaterGreaterDot(p1, p2).Map(x => x.ToValueTuple());

        /// <summary>
        /// The parser `p1.AndL(p2)` applies the parsers `p1` and `p2` in sequence and returns the
        /// result of `p1`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T1>> AndL<U, T1, T2>(
            this FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => op_DotGreaterGreater(p1, p2);

        /// <summary>
        /// The parser `p1.AndR(p2)` applies the parsers `p1` and `p2` in sequence and returns the
        /// result of `p2`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T2>> AndR<U, T1, T2>(
            this FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => op_GreaterGreaterDot(p1, p2);

        /// <summary>
        /// The parser `p.And(f)` first applies the parser `p` to the input, then applies the
        /// function `f` to the result returned by `p` and finally applies the parser returned by
        /// `f` to the input.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T2>> And<U, T1, T2>(
           this FSharpFunc<CharStream<U>, Reply<T1>> p,
           Func<T1, FSharpFunc<CharStream<U>, Reply<T2>>> f)
           => op_GreaterGreaterEquals(p, f.ToFSharpFunc());

        /// <summary>
        /// The parser `p.And(f)` first applies the skipping parser `p` to the input, then executes
        /// the parameterless function `f` and finally applies the parser returned by `f` to the
        /// input.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> And<U, TResult>(
           this FSharpFunc<CharStream<U>, Reply<Unit>> p,
           Func<FSharpFunc<CharStream<U>, Reply<TResult>>> f)
           => op_GreaterGreaterEquals(p, FSharpFunc.From<Unit, FSharpFunc<CharStream<U>, Reply<TResult>>>(_
               => f()));

        /// <summary>
        /// The parser `p.And(f)` first applies the parser `p` to the input, then applies the
        /// function `f` to both values of the tuple returned by `p` and finally applies the parser
        /// returned by `f` to the input.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> And<U, T1, T2, TResult>(
           this FSharpFunc<CharStream<U>, Reply<(T1, T2)>> p,
           Func<T1, T2, FSharpFunc<CharStream<U>, Reply<TResult>>> f)
           => op_GreaterGreaterEquals(p, FSharpFunc.From<(T1, T2), FSharpFunc<CharStream<U>, Reply<TResult>>>(x
               => f(x.Item1, x.Item2)));

        /// <summary>
        /// The parser `p.And(f)` first applies the parser `p` to the input, then applies the
        /// function `f` to all values of the 3-tuple returned by `p` and finally applies the
        /// parser returned by `f` to the input.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> And<U, T1, T2, T3, TResult>(
           this FSharpFunc<CharStream<U>, Reply<(T1, T2, T3)>> p,
           Func<T1, T2, T3, FSharpFunc<CharStream<U>, Reply<TResult>>> f)
           => op_GreaterGreaterEquals(p, FSharpFunc.From<(T1, T2, T3), FSharpFunc<CharStream<U>, Reply<TResult>>>(x
               => f(x.Item1, x.Item2, x.Item3)));

        /// <summary>
        /// The parser `p.And(f)` first applies the parser `p` to the input, then applies the
        /// function `f` to all values of the 4-tuple returned by `p` and finally applies the
        /// parser returned by `f` to the input.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> And<U, T1, T2, T3, T4, TResult>(
           this FSharpFunc<CharStream<U>, Reply<(T1, T2, T3, T4)>> p,
           Func<T1, T2, T3, T4, FSharpFunc<CharStream<U>, Reply<TResult>>> f)
           => op_GreaterGreaterEquals(p, FSharpFunc.From<(T1, T2, T3, T4), FSharpFunc<CharStream<U>, Reply<TResult>>>(x
               => f(x.Item1, x.Item2, x.Item3, x.Item4)));

        /// <summary>
        /// The parser `p.And(f)` first applies the parser `p` to the input, then applies the
        /// function `f` to all values of the 5-tuple returned by `p` and finally applies the
        /// parser returned by `f` to the input.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> And<U, T1, T2, T3, T4, T5, TResult>(
           this FSharpFunc<CharStream<U>, Reply<(T1, T2, T3, T4, T5)>> p,
           Func<T1, T2, T3, T4, T5, FSharpFunc<CharStream<U>, Reply<TResult>>> f)
           => op_GreaterGreaterEquals(p, FSharpFunc.From<(T1, T2, T3, T4, T5), FSharpFunc<CharStream<U>, Reply<TResult>>>(x
               => f(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5)));

        /// <summary>
        /// The parser `Between(pOpen, p, pClose)` applies the parsers `pOpen`, `p` and `pClose` in
        /// sequence. It returns the result of `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Between<U, TOpen, T, TClose>(
            FSharpFunc<CharStream<U>, Reply<TOpen>> open,
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<TClose>> close)
            => between(open, close, p);

        /// <summary>
        /// The parser `Between(cOpen, p, cClose)` skips the char `cOpen`, then applies parser `p`,
        /// and then skips the char `cClose` in sequence. It returns the result of `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Between<U, T>(
            char open,
            FSharpFunc<CharStream<U>, Reply<T>> p,
            char close)
            => between(pchar<U>(open), pchar<U>(close), p);

        /// <summary>
        /// The parser `Between(sOpen, p, sClose)` skips the string `sOpen`, then applies parser `p`,
        /// and then skips the string `sClose` in sequence. It returns the result of `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Between<U, T>(
            string open,
            FSharpFunc<CharStream<U>, Reply<T>> p,
            string close)
            => between(pstring<U>(open), pstring<U>(close), p);

        /// <summary>
        /// <para>
        /// The parser `Array(n,p)` parses `n` occurences of `p` and returns the results in an
        /// array.
        /// </para>
        /// <para>
        /// For example, `Array(3,p)` is equivalent to `Pipe(p,p,p,(a,b,c) => new[] {a,b,c})`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T[]>> Array<U, T>(
            int n,
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => parray(n, p);

        /// <summary>
        /// The parser `SkipArray(n,p)` is an optimized implementation of `Skip(Array(n,p))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipArray<U, T>(
            int n,
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => skipArray(n, p);

        /// <summary>
        /// <para>
        /// The parser `Tuple(p1,p2)` applies the parsers `p1` and `p2` in sequence and returns
        /// the results in a tuple.
        /// </para>
        /// <para>`Tuple(p1,p2)` is equivalent to `p1.And(p2)`.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<(T1, T2)>> Tuple<U, T1, T2>(
            FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => tuple2(p1, p2).Map(x => x.ToValueTuple());

        /// <summary>
        /// <para>
        /// The parser `Tuple(p1,p2,p3)` applies the parsers `p1`, `p2` and `p3` in sequence and
        /// returns the results in a tuple.
        /// </para>
        /// <para>`Tuple(p1,p2,p3)` is equivalent to `p1.And(p2).And(p3).Map(Flat)`.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<(T1, T2, T3)>> Tuple<U, T1, T2, T3>(
            FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2,
            FSharpFunc<CharStream<U>, Reply<T3>> p3)
            => tuple3(p1, p2, p3).Map(x => x.ToValueTuple());

        /// <summary>
        /// The parser `Tuple(p1,p2,p3,p4)` applies the parsers `p1`, `p2`, `p3` and `p4` in
        /// sequence and returns the results in a tuple.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<(T1, T2, T3, T4)>> Tuple<U, T1, T2, T3, T4>(
            FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2,
            FSharpFunc<CharStream<U>, Reply<T3>> p3,
            FSharpFunc<CharStream<U>, Reply<T4>> p4)
            => tuple4(p1, p2, p3, p4).Map(x => x.ToValueTuple());

        /// <summary>
        /// The parser `Tuple(p1,p2,p3,p4,p5)` applies the parsers `p1`, `p2`, `p3`, `p4` and `p5`
        /// in sequence and returns the results in a tuple.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<(T1, T2, T3, T4, T5)>> Tuple<U, T1, T2, T3, T4, T5>(
            FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2,
            FSharpFunc<CharStream<U>, Reply<T3>> p3,
            FSharpFunc<CharStream<U>, Reply<T4>> p4,
            FSharpFunc<CharStream<U>, Reply<T5>> p5)
            => tuple5(p1, p2, p3, p4, p5).Map(x => x.ToValueTuple());

        /// <summary>
        /// The parser `Pipe(p1,p2,f)` applies the parsers `p1` and `p2` in sequence. It returns
        /// the result of the function application `f(a,b)`, where `a`, `b` and `c` are the results
        /// returned by `p1` and `p2`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Pipe<U, T1, T2, TResult>(
            FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2,
            Func<T1, T2, TResult> f)
            => pipe2(p1, p2, f.ToFSharpFunc());

        /// <summary>
        /// The parser `Pipe(p1,p2,p3,f)` applies the parsers `p1`, `p2` and `p3` in sequence. It
        /// returns the result of the function application `f(a,b,c)`, where `a`, `b` and `c` are
        /// the results returned by `p1`, `p2` and `p3`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Pipe<U, T1, T2, T3, TResult>(
            FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2,
            FSharpFunc<CharStream<U>, Reply<T3>> p3,
            Func<T1, T2, T3, TResult> f)
            => pipe3(p1, p2, p3, f.ToFSharpFunc());

        /// <summary>
        /// The parser `Pipe(p1,p2,p3,p4,f)` applies the parsers `p1`, `p2`, `p3` and `p4` in
        /// sequence. It returns the result of the function application `f(a,b,c,d)`, where `a`,
        /// `b`, `c` and `d` are the results returned by `p1`, `p2`, `p3` and `p4`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Pipe<U, T1, T2, T3, T4, TResult>(
            FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2,
            FSharpFunc<CharStream<U>, Reply<T3>> p3,
            FSharpFunc<CharStream<U>, Reply<T4>> p4,
            Func<T1, T2, T3, T4, TResult> f)
            => pipe4(p1, p2, p3, p4, f.ToFSharpFunc());

        /// <summary>
        /// The parser `Pipe(p1,p2,p3,p4,p5,f)` applies the parsers `p1`, `p2`, `p3`, `p4` and `p5`
        /// in sequence. It returns the result of the function application `f(a,b,c,d,e)`, where
        /// `a`, `b`, `c`, `d` and `e` are the results returned by `p1`, `p2`, `p3`, `p4` and `p5`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Pipe<U, T1, T2, T3, T4, T5, TResult>(
            FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2,
            FSharpFunc<CharStream<U>, Reply<T3>> p3,
            FSharpFunc<CharStream<U>, Reply<T4>> p4,
            FSharpFunc<CharStream<U>, Reply<T5>> p5,
            Func<T1, T2, T3, T4, T5, TResult> f)
            => pipe5(p1, p2, p3, p4, p5, f.ToFSharpFunc());

        #endregion Sequence

        #region Choice

        /// <summary>
        /// <para>The parser `p1.Or(p2)` first applies the parser `p1`.</para>
        /// <para>If `p1` succeeds, the result of `p1` is returned.</para>
        /// <para>
        /// If `p1` fails with a non-fatal error and *without changing the parser state*,
        /// the parser `p2` is applied.
        /// </para>
        /// <para>
        /// Note: The stream position is part of the parser state, so if `p1` fails after consuming input,
        /// `p2` will not be applied.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Or<U, T>(
            this FSharpFunc<CharStream<U>, Reply<T>> p1,
            FSharpFunc<CharStream<U>, Reply<T>> p2)
            => op_LessBarGreater(p1, p2);

        /// <summary>
        /// The parser `Choice(ps)` is an optimized implementation of `p1.Or(p2).Or(...).Or(pn)`,
        /// where `p1` ... `pn` are the parsers in the sequence `ps`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Choice<U, T>(
            params FSharpFunc<CharStream<U>, Reply<T>>[] ps)
            => choice(ps);

        /// <summary>
        /// The parser `Choice(s,ps)` is an optimized implementation of `Choice(ps).Label(s)`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Choice<U, T>(
            string label,
            params FSharpFunc<CharStream<U>, Reply<T>>[] ps)
            => choiceL(ps, label);

        /// <summary>
        /// The parser `p.Or(x)` is an optimized implementation of `p.Or(Return(x))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Or<U, T>(
            this FSharpFunc<CharStream<U>, Reply<T>> p,
            T x)
            => op_LessBarGreaterPercent(p, x);

        /// <summary>
        /// The parser `Optional(p)` skips over an optional occurrence of `p`. `Optional(p)` is an
        /// optimized implementation of `p.Return((Unit)null).Or((Unit)null)`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> Optional<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => optional(p);

        /// <summary>
        /// The parser `Opt(p)` behaves like `Opt_(p)` but also unwraps the `FSharpOption` value.
        /// In case `Opt(p)` did not parse anything the result type's `default` value is returned.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Opt<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => opt(p).Map(x => x.GetValueOrDefault());

        /// <summary>
        /// The parser `Opt(p,d)` behaves like `Opt_(p)` but also unwraps the `FSharpOption` value.
        /// In case `Opt(p,d)` did not parse anything then `d` is returned.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Opt<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            T defaultValue)
            => opt(p).Map(x => x.GetValueOrDefault(defaultValue));

        /// <summary>
        /// <para>The parser `Opt_(p)` parses an optional occurrence of `p` as an `FSharpOption` value.</para>
        /// <para>
        /// `Opt_(p)` is an optimized implementation of
        /// `p.Map(FSharpOption.Some).Or(FSharpOption.None)`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpOption<T>>> Opt_<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => opt(p);

        #endregion Choice

        #region Repetition

        /// <summary>
        /// <para>
        /// The parser `Many(p)` repeatedly applies the parser `p` until `p` fails.
        /// Returns a list of the results returned by `p`.
        /// </para>
        /// <para>
        /// At the end of the sequence `p` must fail without changing the parser state and without
        /// signalling a `FatalError`, otherwise `Many(p)` will fail with the error reported by `p`.
        /// </para>
        /// <para>
        /// `Many(p)` tries to guard against an infinite loop by throwing an exception
        /// if `p` succeeds without changing the parser state.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpList<T>>> Many<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => many(p);

        /// <summary>
        /// <para>
        /// The parser `Many(p,c)` parses *zero* or more occurrences of `p` separated by the char
        /// `c` (in EBNF notation: `(p (c p)*)?`).
        /// </para>
        /// <para>
        /// The parser `Many(p,c,true)` parses *zero* or more occurrences of `p` separated and
        /// optionally ended by `c` (in EBNF notation: `(p (c p)* c?)?`).
        /// </para>
        /// <para>Returns a list of the results returned by `p`.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpList<T>>> Many<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            char sep,
            bool canEndWithSep = false)
            => canEndWithSep ? sepEndBy(p, pchar<U>(sep)) : sepBy(p, pchar<U>(sep));

        /// <summary>
        /// <para>
        /// The parser `Many(p,s)` parses *zero* or more occurrences of `p` separated by the string
        /// `s` (in EBNF notation: `(p (s p)*)?`).
        /// </para>
        /// <para>
        /// The parser `Many(p,s,true)` parses *zero* or more occurrences of `p` separated and
        /// optionally ended by `s` (in EBNF notation: `(p (s p)* s?)?`).
        /// </para>
        /// <para>Returns a list of the results returned by `p`.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpList<T>>> Many<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            string sep,
            bool canEndWithSep = false)
            => canEndWithSep ? sepEndBy(p, pstring<U>(sep)) : sepBy(p, pstring<U>(sep));

        /// <summary>
        /// <para>
        /// The parser `Many(p,sep)` parses *zero* or more occurrences of `p` separated by the
        /// parser `sep` (in EBNF notation: `(p (sep p)*)?`).
        /// </para>
        /// <para>
        /// The parser `Many(p,sep,true)` parses *zero* or more occurrences of `p` separated and
        /// optionally ended by `sep` (in EBNF notation: `(p (sep p)* sep?)?`).
        /// </para>
        /// <para>Returns a list of the results returned by `p`.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpList<T>>> Many<U, T, TSep>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<TSep>> sep,
            bool canEndWithSep = false)
            => canEndWithSep ? sepEndBy(p, sep) : sepBy(p, sep);

        /// <summary>
        /// The parser `SkipMany(p)` is an optimized implementation of `Skip(Many(p))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => skipMany(p);

        /// <summary>
        /// The parser `SkipMany(p,c)` is an optimized implementation of `Skip(Many(p,c))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            char sep,
            bool canEndWithSep = false)
            => canEndWithSep ? skipSepEndBy(p, pchar<U>(sep)) : skipSepBy(p, pchar<U>(sep));

        /// <summary>
        /// The parser `SkipMany(p,s)` is an optimized implementation of `Skip(Many(p,s))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            string sep,
            bool canEndWithSep = false)
            => canEndWithSep ? skipSepEndBy(p, pstring<U>(sep)) : skipSepBy(p, pstring<U>(sep));

        /// <summary>
        /// The parser `SkipMany(p,sep)` is an optimized implementation of `Skip(Many(p,sep))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany<U, T, TSep>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<TSep>> sep,
            bool canEndWithSep = false)
            => canEndWithSep ? skipSepEndBy(p, sep) : skipSepBy(p, sep);

        /// <summary>
        /// The parser `Many1(p)` behaves like `Many(p)`, except that it requires `p` to succeed at
        /// least one time.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpList<T>>> Many1<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => many1(p);

        /// <summary>
        /// <para>
        /// The parser `Many1(p,c)` parses *one* or more occurrences of `p` separated by the char
        /// `c` (in EBNF notation: `p (c p)*`).
        /// </para>
        /// <para>
        /// The parser `Many1(p,c,true)` parses *one* or more occurrences of `p` separated and
        /// optionally ended by `c` (in EBNF notation: `p (c p)* c?`).
        /// </para>
        /// <para>Returns a list of the results returned by `p`.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpList<T>>> Many1<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            char sep,
            bool canEndWithSep = false)
            => canEndWithSep ? sepEndBy1(p, pchar<U>(sep)) : sepBy1(p, pchar<U>(sep));

        /// <summary>
        /// <para>
        /// The parser `Many1(p,s)` parses *one* or more occurrences of `p` separated by the string
        /// `s` (in EBNF notation: `p (s p)*`).
        /// </para>
        /// <para>
        /// The parser `Many1(p,s,true)` parses *one* or more occurrences of `p` separated and
        /// optionally ended by `s` (in EBNF notation: `p (s p)* s?`).
        /// </para>
        /// <para>Returns a list of the results returned by `p`.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpList<T>>> Many1<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            string sep,
            bool canEndWithSep = false)
            => canEndWithSep ? sepEndBy1(p, pstring<U>(sep)) : sepBy1(p, pstring<U>(sep));

        /// <summary>
        /// <para>
        /// The parser `Many1(p,sep)` parses *one* or more occurrences of `p` separated by the
        /// parser `sep` (in EBNF notation: `p (sep p)*`).
        /// </para>
        /// <para>
        /// The parser `Many1(p,sep,true)` parses *one* or more occurrences of `p` separated and
        /// optionally ended by `sep` (in EBNF notation: `p (sep p)* sep?`).
        /// </para>
        /// <para>Returns a list of the results returned by `p`.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpList<T>>> Many1<U, T, TSep>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<TSep>> sep,
            bool canEndWithSep = false)
            => canEndWithSep ? sepEndBy1(p, sep) : sepBy1(p, sep);

        /// <summary>
        /// The parser `SkipMany1(p)` is an optimized implementation of `Skip(Many1(p))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany1<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => skipMany1(p);

        /// <summary>
        /// The parser `SkipMany1(p,c)` is an optimized implementation of `Skip(Many1(p,c))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany1<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            char sep,
            bool canEndWithSep = false)
            => canEndWithSep ? skipSepEndBy1(p, pchar<U>(sep)) : skipSepBy1(p, pchar<U>(sep));

        /// <summary>
        /// The parser `SkipMany1(p,s)` is an optimized implementation of `Skip(Many1(p,s))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany1<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            string sep,
            bool canEndWithSep = false)
            => canEndWithSep ? skipSepEndBy1(p, pstring<U>(sep)) : skipSepBy1(p, pstring<U>(sep));

        /// <summary>
        /// The parser `SkipMany1(p,sep)` is an optimized implementation of `Skip(Many1(p,sep))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany1<U, T, TSep>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<TSep>> sep,
            bool canEndWithSep = false)
            => canEndWithSep ? skipSepEndBy1(p, sep) : skipSepBy1(p, sep);

        /// <summary>
        /// <para>
        /// The parser `ManyTill(p,endp)` repeatedly applies the parser `p` for as long as `endp`
        /// fails (without changing the parser state).
        /// </para>
        /// <para>Returns a list of the results returned by `p`.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpList<T>>> ManyTill<U, T, TEnd>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> end)
            => manyTill(p, end);

        /// <summary>
        /// The parser `SkipManyTill(p,endp)` is an optimized implementation of
        /// `Skip(ManyTill(p,endp))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipManyTill<U, T, TEnd>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp)
            => skipManyTill(p, endp);

        /// <summary>
        /// <para>
        /// The parser `Many1Till(p,endp)` behaves like `manyTill(p,endp)`, except that it requires
        /// `p` to succeed at least one time.
        /// </para>
        /// <para>
        /// `Many1Till(p,endp)` is an optimized implementation of
        /// `Pipe(p, ManyTill(p,endp), FSharpList.Cons)`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<FSharpList<T>>> Many1Till<U, T, TEnd>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp)
            => many1Till(p, endp);

        /// <summary>
        /// The parser `SkipMany1Till(p,endp)` is an optimized implementation of
        /// `Skip(Many1Till(p,endp))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SkipMany1Till<U, T, TEnd>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<TEnd>> endp)
            => skipMany1Till(p, endp);

        /// <summary>
        /// `Many(p,f,x)` is a short form for `Many(p).Map(xs => xs.Aggregate(x, f))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Many<U, T, TResult>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            Func<TResult, T, TResult> aggregate,
            TResult seed)
            => many(p).Map(xs => xs.Aggregate(seed, aggregate));

        /// <summary>
        /// `Many(p,f)` is a short form for `Many1(p).Map(xs => xs.Aggregate(f))`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Many1<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            Func<T, T, T> aggregate)
            => many1(p).Map(xs => xs.Aggregate(aggregate));

        /// <summary>
        /// <para>
        /// The parser `ChainL(p,op)` parses one or more occurrences of `p` separated by `op` (in
        /// EBNF notation: `p (op p)*`).
        /// </para>
        /// <para>
        /// It returns the value obtained by *left* associative application of all functions
        /// returned by `op` to the results returned by `p`, i.e.
        /// `f_n(... f_2(f_1(x_1,x_2),x_3) ..., x_n+1)`, where `f_1` to `f_n` are the functions
        /// returned by the parser `op` and `x_1` to `x_n+1` are the values returned by `p`.
        /// </para>
        /// <para>
        /// If only a single occurance of `p` and no occurance of `op` is parsed, the result of `p`
        /// is returned directly.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> ChainL<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<Func<T, T, T>>> op)
            => chainl1(p, op.Map(f => f.ToFSharpFunc()));

        /// <summary>
        /// The parser `ChainL(p,op,x)` is equivalent to `ChainL(p,op).Or(x)`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> ChainL<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<Func<T, T, T>>> op,
            T defaultVal)
            => chainl(p, op.Map(f => f.ToFSharpFunc()), defaultVal);

        /// <summary>
        /// <para>
        /// The parser `ChainR(p,op)` parses one or more occurrences of `p` separated by `op` (in
        /// EBNF notation: `p (op p)*`).
        /// </para>
        /// <para>
        /// It returns the value obtained by *right* associative application of all functions
        /// returned by `op` to the results returned by `p`, i.e.
        /// `f_1(x_1, f_2(x_2, ... f_n(x_n, x_n+1) ...))`, where `f_1` to `f_n` are the functions
        /// returned by the parser `op` and `x_1` to `x_n+1` are the values returned by `p`.
        /// </para>
        /// <para>
        /// If only a single occurance of `p` and no occurance of `op` is parsed, the result of `p`
        /// is returned directly.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> ChainR<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<Func<T, T, T>>> op)
            => chainr1(p, op.Map(f => f.ToFSharpFunc()));

        /// <summary>
        /// The parser `ChainR(p,op,x)` is equivalent to `ChainR(p,op).Or(x)`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> ChainR<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            FSharpFunc<CharStream<U>, Reply<Func<T, T, T>>> op,
            T defaultValue)
            => chainr(p, op.Map(f => f.ToFSharpFunc()), defaultValue);

        #endregion Repetition

        #region Backtracking

        /// <summary>
        /// The parser `Try(p)` applies the parser `p`. If `p` fails after changing the parser
        /// state or with a fatal error, `Try(p)` will backtrack to the original parser state and
        /// report a non-fatal error.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Try<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => attempt(p);

        /// <summary>
        /// The parser `p1.AndTry(p2)` behaves like `p1.And(p2)`, except that it will backtrack to
        /// the beginning if `p2` fails with a non-fatal error and without changing the parser
        /// state, even if `p1` has changed the parser state.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<(T1, T2)>> AndTry<U, T1, T2>(
            this FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => op_DotGreaterGreaterDotQmark(p1, p2).Map(x => x.ToValueTuple());

        /// <summary>
        /// <para>
        /// The parser `p1.AndTry(p2)` behaves like `p1.And(p2)`, except that it will backtrack to
        /// the beginning if `p2` fails with a non-fatal error and without changing the parser
        /// state, even if `p1` has changed the parser state.
        /// </para>
        /// <para>
        /// Since `p2` is a skipping parser that returns `Unit`, its result will not be returned.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T1>> AndTry<U, T1>(
            this FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<Unit>> p2)
            => op_DotGreaterGreaterQmark(p1, p2);

        /// <summary>
        /// <para>
        /// The parser `p1.AndTry(p2)` behaves like `p1.And(p2)`, except that it will backtrack to
        /// the beginning if `p2` fails with a non-fatal error and without changing the parser
        /// state, even if `p1` has changed the parser state.
        /// </para>
        /// <para>
        /// Since `p1` is a skipping parser that returns `Unit`, its result will not be returned.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T2>> AndTry<U, T2>(
            this FSharpFunc<CharStream<U>, Reply<Unit>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => op_GreaterGreaterQmark(p1, p2);

        /// <summary>
        /// `p1.AndTry_(p2)` behaves like `p1.AndTry(p2)` except that it will always return both parser
        /// results even if either of them returns `Unit`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<(T1, T2)>> AndTry_<U, T1, T2>(
            this FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => op_DotGreaterGreaterDotQmark(p1, p2).Map(x => x.ToValueTuple());

        /// <summary>
        /// The parser `p1.AndLTry(p2)` behaves like `p1.AndL(p2)`, except that it will backtrack
        /// to the beginning if `p2` fails with a non-fatal error and without changing the parser
        /// state, even if `p1` has changed the parser state.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T1>> AndLTry<U, T1, T2>(
            this FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => op_DotGreaterGreaterQmark(p1, p2);

        /// <summary>
        /// The parser `p1.AndRTry(p2)` behaves like `p1.AndR(p2)`, except that it will backtrack
        /// to the beginning if `p2` fails with a non-fatal error and without changing the parser
        /// state, even if `p1` has changed the parser state.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T2>> AndRTry<U, T1, T2>(
            this FSharpFunc<CharStream<U>, Reply<T1>> p1,
            FSharpFunc<CharStream<U>, Reply<T2>> p2)
            => op_GreaterGreaterQmark(p1, p2);

        /// <summary>
        /// The parser `p.AndTry(f)` behaves like `p.And(f)`, except that it will backtrack to the
        /// beginning if the parser returned by `f` fails with a non-fatal error and without
        /// changing the parser state, even if `p` has changed the parser state.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T2>> AndTry<U, T1, T2>(
           this FSharpFunc<CharStream<U>, Reply<T1>> p1,
           Func<T1, FSharpFunc<CharStream<U>, Reply<T2>>> p2)
           => op_GreaterGreaterEqualsQmark(p1, p2.ToFSharpFunc());

        /// <summary>
        /// <para>The parser `LookAhead(p)` parses `p` and restores the original parse state afterwards.</para>
        /// <para>
        /// In case `p` fails after changing the parser state, the error messages are wrapped in a
        /// `NestedError`. If it succeeds, any error messages are discarded. Fatal errors are
        /// turned into normal errors.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> LookAhead<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => lookAhead(p);

        /// <summary>
        /// <para>
        /// The parser `FollowedBy(p)` succeeds if the parser `p` succeeds at the current position.
        /// Otherwise it fails with a non-fatal error.
        /// </para>
        /// <para>This parser never changes the parser state.</para>
        /// <para>
        /// If the parser `FollowedBy(p)` fails, it returns no descriptive error message. Hence it
        /// should only be used together with other parsers that take care of a potential error.
        /// Alternatively, `FollowedBy(p,s)` can be used to ensure a more descriptive error
        /// message.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> FollowedBy<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => followedBy(p);

        /// <summary>
        /// The parser `FollowedBy(p,s)` behaves like `FollowedBy(p)`, except that it returns an
        /// `Expected s` error message when the parser `p` fails.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> FollowedBy<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            string label)
            => followedByL(p, label);

        /// <summary>
        /// <para>
        /// The parser `NotFollowedBy(p)` succeeds if the parser `p` fails to parse at the current
        /// position. Otherwise it fails with a non-fatal error.
        /// </para>
        /// <para>This parser never changes the parser state.</para>
        /// <para>
        /// If the parser `NotFollowedBy(p)` fails, it returns no descriptive error message. Hence
        /// it should only be used together with other parsers that take care of a potential error.
        /// Alternatively, `NotFollowedBy(p,s)` can be used to ensure a more descriptive error
        /// message.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> NotFollowedBy<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => notFollowedBy(p);

        /// <summary>
        /// The parser `NotFollowedBy(p,s)` behaves like `NotFollowedBy(p)`, except that it returns
        /// an `Unexpected s` error message when the parser `p` fails.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> NotFollowedBy<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p,
            string label)
            => notFollowedByL(p, label);

        #endregion Backtracking

        #region Special

        /// <summary>
        /// The parser `Zero()` always fails with an empty error message list, i.e. an unspecified
        /// error.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<T>> Zero<T>() => pzero<T, Unit>();

        /// <summary>`ZeroU()` behaves like `Zero()`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> ZeroU<U, T>() => pzero<T, U>();

        /// <summary>
        /// The parser `Return(x)` always succeeds with the result `x` (without changing the parser
        /// state).
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<TResult>> Return<TResult>(
            TResult result)
            => preturn<TResult, Unit>(result);

        /// <summary>`ReturnU(x)` behaves like `Return(x)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> ReturnU<U, TResult>(
            TResult result)
            => preturn<TResult, U>(result);

        /// <summary>
        /// The parser `p.Return(x)` applies the parser `p` and returns the result `x`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Return<U, T, TResult>(
            this FSharpFunc<CharStream<U>, Reply<T>> p,
            TResult result)
            => op_GreaterGreaterPercent(p, result);

        /// <summary>
        /// <para>
        /// The parser `p.Return(f)` applies the parser `p` and returns the binary operation `f`.
        /// </para>
        /// <para>`p.Return(f)` is a helper for `ChainL()` and `ChainR()`.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Func<TOp, TOp, TOp>>> Return<U, T, TOp>(
            this FSharpFunc<CharStream<U>, Reply<T>> p,
            Func<TOp, TOp, TOp> result)
            => op_GreaterGreaterPercent(p, result);

        /// <summary>
        /// The parser `Fail(s)` always fails with a `messageError s`. The error message will be
        /// displayed together with other error messages generated for the same input position.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<T>> Fail<T>(string error) => fail<T, Unit>(error);

        /// <summary>`FailU(s)` behaves like `Fail(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> FailU<U, T>(string error) => fail<T, U>(error);

        /// <summary>
        /// The parser `FailFatally(s)` always fails with a `messageError s`. It signals a
        /// FatalError, so that no error recovery is attempted (except via backtracking
        /// constructs).
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<T>> FailFatally<T>(string error) => failFatally<T, Unit>(error);

        /// <summary>`FailFatallyU(s)` behaves like `FailFatally(s)`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> FailFatallyU<U, T>(string error) => failFatally<T, U>(error);

        /// <summary>
        /// <para>
        /// The parser `Purify(p)` applies the parser `p` to the input and discards any errors
        /// messages if `p` succeeds.
        /// </para>
        /// <para>
        /// All error messages will be preserved in case `p` fails.
        /// </para>
        /// <para>
        /// `Purify(p)` can be used to ignore the errors generated by `Many()` and its variants.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Purify<U, T>(FSharpFunc<CharStream<U>, Reply<T>> p) =>
            FSharpFunc.From<CharStream<U>, Reply<T>>(chars =>
                p.Invoke(chars) switch {
                    (Ok, var result, _) => new Reply<T>(result),
                    var reply => reply
                });

        /// <summary>
        /// The parser `Skip(p)` applies the parser `p` and skips its result, i.e. returns
        /// `(Unit)null`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> Skip<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => p.Return((Unit)null!);

        /// <summary>
        /// <para>
        /// The parser `Rec(() => p)` delays reading the value of `p` until the parent parser is
        /// run. `Rec()` is needed for recursive grammars (e.g. JSON, where objects can be nested).
        /// </para>
        /// <para>
        /// When parsers `p1` and `p2` depend on each other (directly or
        /// indirectly) then the parser that is defined last needs to be declared first and
        /// initialized with `null`. The parser that is defined first can then reference the parser
        /// defined last using `Rec(() => ...)`.
        /// </para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Rec<U, T>(
            Func<FSharpFunc<CharStream<U>, Reply<T>>?> p)
            => FSharpFunc.From((CharStream<U> cs) => p()!.Invoke(cs));

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f(x)`, where `x`
        /// is the result returned by `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Map<U, T, TResult>(
            this FSharpFunc<CharStream<U>, Reply<T>> p,
            Func<T, TResult> map)
            => op_BarGreaterGreater(p, map.ToFSharpFunc());

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f()`. Hence the 
        /// result of the `Unit`-returning parser `p` will be ignored.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Map<U, TResult>(
            this FSharpFunc<CharStream<U>, Reply<Unit>> p,
            Func<TResult> map)
            => op_BarGreaterGreater(p, FSharpFunc.From<Unit, TResult>(_ => map()));

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f(x,y)`, where `x`
        /// and `y` are items of the tuple result `(x,y)` returned by `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Map<U, T1, T2, TResult>(
            this FSharpFunc<CharStream<U>, Reply<(T1, T2)>> p,
            Func<T1, T2, TResult> map)
            => op_BarGreaterGreater(p, FSharpFunc.From<(T1, T2), TResult>(x => map(x.Item1, x.Item2)));

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f(x, y, z)`, where
        /// `x`, `y` and `z` are items of the tuple result `(x,y,z)` returned by `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Map<U, T1, T2, T3, TResult>(
            this FSharpFunc<CharStream<U>, Reply<(T1, T2, T3)>> p,
            Func<T1, T2, T3, TResult> map)
            => op_BarGreaterGreater(p, FSharpFunc.From<(T1, T2, T3), TResult>(x => map(x.Item1, x.Item2, x.Item3)));

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f(w, x, y, z)`,
        /// where `w`, `x`, `y` and `z` are items of the tuple result `(w,x,y,z)` returned by `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Map<U, T1, T2, T3, T4, TResult>(
            this FSharpFunc<CharStream<U>, Reply<(T1, T2, T3, T4)>> p,
            Func<T1, T2, T3, T4, TResult> map)
            => op_BarGreaterGreater(p, FSharpFunc.From<(T1, T2, T3, T4), TResult>(x => map(x.Item1, x.Item2, x.Item3, x.Item4)));

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f(v, w, x, y, z)`,
        /// where `v`, `w`, `x`, `y` and `z` are items of the tuple result `(v,w,x,y,z)` returned
        /// by `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<TResult>> Map<U, T1, T2, T3, T4, T5, TResult>(
            this FSharpFunc<CharStream<U>, Reply<(T1, T2, T3, T4, T5)>> p,
            Func<T1, T2, T3, T4, T5, TResult> map)
            => op_BarGreaterGreater(p, FSharpFunc.From<(T1, T2, T3, T4, T5), TResult>(x => map(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5)));

        /// <summary>
        /// The parser `NotEmpty(p)` behaves like `p`, except that it fails when `p` succeeds
        /// without consuming input or changing the parser state in any other way.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> NotEmpty<U, T>(
            FSharpFunc<CharStream<U>, Reply<T>> p)
            => notEmpty(p);

        /// <summary>
        /// <para>
        /// The parser `p.Debug(before,after)` passes the `CharStream` input of `p` to the `before`
        /// action before applying `p`. Afterwards it passes the reply and the input stream to the
        /// `after` action, before finally returning the reply.
        /// </para>
        /// <para>
        /// This combinator should only be used for debugging. Hook into your combinator chain at
        /// arbitrary positions in order to observe stream state and parser replies.
        /// </para>
        /// <para>
        /// You can use empty actions to place break points:
        /// `Letter.Debug(cs => {}, (cs, r) => {}).And(Digit)`
        /// </para>
        /// <para>Both `before` and `after` can be null.</para>
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Debug<U, T>(
            this FSharpFunc<CharStream<U>, Reply<T>> p,
            Action<CharStream<U>>? before = null,
            Action<CharStream<U>, Reply<T>>? after = null)
            => FSharpFunc.From((CharStream<U> cs) => {
                before?.Invoke(cs);
                var r = p.Invoke(cs);
                after?.Invoke(cs, r);
                return r;
            });

        /// <summary>
        /// Flattens the nested tuple `((a,b),c)` to `(a,b,c)`.
        /// </summary>
        public static (T1, T2, T3) Flat<T1, T2, T3>(((T1, T2), T3) x)
            => (x.Item1.Item1, x.Item1.Item2, x.Item2);

        /// <summary>
        /// Flattens the nested tuple `(((a,b),c),d)` to `(a,b,c,d)`.
        /// </summary>
        public static (T1, T2, T3, T4) Flat<T1, T2, T3, T4>((((T1, T2), T3), T4) x)
            => (x.Item1.Item1.Item1, x.Item1.Item1.Item2, x.Item1.Item2, x.Item2);

        /// <summary>
        /// Flattens the nested tuple `((((a,b),c),d),e)` to `(a,b,c,d,e)`.
        /// </summary>
        public static (T1, T2, T3, T4, T5) Flat<T1, T2, T3, T4, T5>(((((T1, T2), T3), T4), T5) x)
            => (x.Item1.Item1.Item1.Item1, x.Item1.Item1.Item1.Item2, x.Item1.Item1.Item2, x.Item1.Item2, x.Item2);

        /// <summary>
        /// The parser `PositionP` returns the current position in the input stream.
        /// </summary>
        public static FSharpFunc<CharStream<Unit>, Reply<Position>> PositionP = getPosition<Unit>();

        /// <summary>`PositionU()` behaves like `PositionP()`, but supports user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Position>> PositionU<U>() => getPosition<U>();

        /// <summary>The parser `GetUserState()` returns the current user state.</summary>
        public static FSharpFunc<CharStream<U>, Reply<U>> GetUserState<U>() => getUserState<U>();

        /// <summary>The parser `SetUserState(u)` sets the user state to `u`.</summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> SetUserState<U>(U x) => setUserState(x);

        /// <summary>
        /// The parser `UpdateUserState(f)` sets the user state to the result of the function
        /// application `f(u)`, where `u` is the current user state.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> UpdateUserState<U>(Func<U, U> f) => updateUserState(f.ToFSharpFunc());

        /// <summary>
        /// The parser `UserStateSatisfies(f)` succeeds if the predicate function `f` returns
        /// `true` when applied to the current user state.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<Unit>> UserStateSatisfies<U>(Func<U, bool> pred) => userStateSatisfies(pred.ToFSharpFunc());

        #endregion Special

        #region Labels

        /// <summary>
        /// The parser `p.Label(s)` applies the parser `p`. If `p` does not change the parser state
        /// (usually because `p` failed), the error messages are replaced with `expected s`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Label<U, T>(
            this FSharpFunc<CharStream<U>, Reply<T>> p,
            string label)
            => op_LessQmarkGreater(p, label);

        /// <summary>Short form for `Label()`.</summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Lbl<U, T>(
            this FSharpFunc<CharStream<U>, Reply<T>> p,
            string label)
            => op_LessQmarkGreater(p, label);

        /// <summary>
        /// The parser `p.Label_(s)` behaves like `p.Label(s)`, except that when `p` fails after
        /// changing the parser state (for example, because `p` consumes input before it fails),
        /// a `CompoundError` message is generated with both the given string `s` and the error 
        /// messages generated by `p`.
        /// </summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Label_<U, T>(
            this FSharpFunc<CharStream<U>, Reply<T>> p,
            string label)
            => op_LessQmarkQmarkGreater(p, label);

        /// <summary>Short form for `Label_()`.</summary>
        public static FSharpFunc<CharStream<U>, Reply<T>> Lbl_<U, T>(
            this FSharpFunc<CharStream<U>, Reply<T>> p,
            string label)
            => op_LessQmarkQmarkGreater(p, label);

        #endregion Labels
    }
}
