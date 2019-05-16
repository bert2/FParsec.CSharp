namespace FParsec.CSharp {
    using System;
    using Microsoft.FSharp.Collections;
    using Microsoft.FSharp.Core;
    using static CharParsers;
    using static Primitives;
    using Chars = CharStream<Microsoft.FSharp.Core.Unit>;

    public static class PrimitivesCS {
        #region Sequence

        /// <summary>
        /// The parser `p1.And(p2)` applies the parsers `p1` and `p2` in sequence and returns
        /// the results in a tuple.
        /// </summary>
        public static FSharpFunc<Chars, Reply<(T1, T2)>> And<T1, T2>(
            this FSharpFunc<Chars, Reply<T1>> p1,
            FSharpFunc<Chars, Reply<T2>> p2)
            => op_DotGreaterGreaterDot(p1, p2).Map(x => x.ToValueTuple());

        /// <summary>
        /// The parser `p1.And(p2)` applies the parsers `p1` and `p2` in sequence and returns
        /// the result of `p1`. Since `p2` is a skipping parser that returns `Unit` its result 
        /// will not be returned.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T1>> And<T1>(
            this FSharpFunc<Chars, Reply<T1>> p1,
            FSharpFunc<Chars, Reply<Unit>> p2)
            => op_DotGreaterGreater(p1, p2);

        /// <summary>
        /// The parser `p1.And(p2)` applies the parsers `p1` and `p2` in sequence and returns
        /// the result of `p2`. Since `p1` is a skipping parser that returns `Unit` its result 
        /// will not be returned.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T2>> And<T2>(
            this FSharpFunc<Chars, Reply<Unit>> p1,
            FSharpFunc<Chars, Reply<T2>> p2)
            => op_GreaterGreaterDot(p1, p2);

        /// <summary>
        /// The parser `p1.And(p2)` applies the parsers `p1` and `p2` in sequence and returns
        /// the result of `p2`. Since `p1` is a skipping parser that returns `Unit` its result 
        /// will not be returned. Although `p2` is skipping parser as well, its result will still
        /// be returned instead of creating a new `Reply&lt;Unit&gt;`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<Unit>> And(
            this FSharpFunc<Chars, Reply<Unit>> p1,
            FSharpFunc<Chars, Reply<Unit>> p2)
            => op_GreaterGreaterDot(p1, p2);

        /// <summary>
        /// The parser `p1.And_(p2)` applies the parsers `p1` and `p2` in sequence and returns
        /// the results in a tuple. `p1.And_(p2)` behaves like `p1.And(p2)` except that it will
        /// always return both parser results even if either of them returns `Unit`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<(T1, T2)>> And_<T1, T2>(
            this FSharpFunc<Chars, Reply<T1>> p1,
            FSharpFunc<Chars, Reply<T2>> p2)
            => op_DotGreaterGreaterDot(p1, p2).Map(x => x.ToValueTuple());

        /// <summary>
        /// The parser `p.And(f)` first applies the parser `p` to the input, then applies the
        /// function `f` to the result returned by `p` and finally applies the parser returned by
        /// `f` to the input.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T2>> And<T1, T2>(
           this FSharpFunc<Chars, Reply<T1>> p1,
           Func<T1, FSharpFunc<Chars, Reply<T2>>> p2)
           => op_GreaterGreaterEquals(p1, p2.ToFSharpFunc());

        /// <summary>
        /// The parser `Between(pOpen, p, pClose)` applies the parsers `pOpen`, `p` and `pClose` in
        /// sequence. It returns the result of `p`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> Between<TOpen, T, TClose>(
            FSharpFunc<Chars, Reply<TOpen>> open,
            FSharpFunc<Chars, Reply<T>> p,
            FSharpFunc<Chars, Reply<TClose>> close)
            => between(open, close, p);

        /// <summary>
        /// The parser `Between(cOpen, p, cClose)` skips the char `cOpen`, then applies parser `p`,
        /// and then skips the char `cClose` in sequence. It returns the result of `p`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> Between<T>(
            char open,
            FSharpFunc<Chars, Reply<T>> p,
            char close)
            => between(pchar<Unit>(open), pchar<Unit>(close), p);

        /// <summary>
        /// The parser `Between(sOpen, p, sClose)` skips the string `sOpen`, then applies parser `p`,
        /// and then skips the string `sClose` in sequence. It returns the result of `p`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> Between<T>(
            string open,
            FSharpFunc<Chars, Reply<T>> p,
            string close)
            => between(pstring<Unit>(open), pstring<Unit>(close), p);

        #endregion Sequence

        #region Choice

        /// <summary>
        /// The parser `p1.Or(p2)` first applies the parser `p1`.
        /// If `p1` succeeds, the result of `p1` is returned.
        /// If `p1` fails with a non-fatal error and *without changing the parser state*,
        /// the parser `p2` is applied.
        /// Note: The stream position is part of the parser state, so if `p1` fails after consuming input,
        /// `p2` will not be applied.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> Or<T>(
            this FSharpFunc<Chars, Reply<T>> p1,
            FSharpFunc<Chars, Reply<T>> p2)
            => op_LessBarGreater(p1, p2);

        /// <summary>
        /// The parser `OneOf(ps)` is an optimized implementation of `p1.Or(p2).Or(...).Or(pn)`,
        /// where `p1` ... `pn` are the parsers in the sequence `ps`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> OneOf<T>(
            params FSharpFunc<Chars, Reply<T>>[] ps)
            => choice(ps);

        #endregion Choice

        #region Repetition

        /// <summary>
        /// The parser `Many(p)` repeatedly applies the parser `p` until `p` fails.
        /// It returns a list of the results returned by `p`.
        /// At the end of the sequence `p` must fail without changing the parser state and without
        /// signalling a `FatalError`, otherwise `Many(p)` will fail with the error reported by `p`.
        /// `Many(p)` tries to guard against an infinite loop by throwing an exception
        /// if `p` succeeds without changing the parser state.
        /// </summary>
        public static FSharpFunc<Chars, Reply<FSharpList<T>>> Many<T>(
            FSharpFunc<Chars, Reply<T>> p)
            => many(p);

        /// <summary>
        /// The parser `Many1(p)` behaves like `Many(p)`, except that it requires `p` to succeed at
        /// least one time.
        /// </summary>
        public static FSharpFunc<Chars, Reply<FSharpList<T>>> Many1<T>(
            FSharpFunc<Chars, Reply<T>> p)
            => many1(p);

        /// <summary>
        /// The parser `Many(p, c)` parses *zero* or more occurrences of `p` separated by the char
        /// `c` (in EBNF notation: `(p (c p)*)?`).
        /// </summary>
        public static FSharpFunc<Chars, Reply<FSharpList<T>>> Many<T>(
            FSharpFunc<Chars, Reply<T>> p,
            char sep)
            => sepBy(p, pchar<Unit>(sep));

        /// <summary>
        /// The parser `Many(p, s)` parses *zero* or more occurrences of `p` separated by the
        /// string `s` (in EBNF notation: `(p (s p)*)?`).
        /// </summary>
        public static FSharpFunc<Chars, Reply<FSharpList<T>>> Many<T>(
            FSharpFunc<Chars, Reply<T>> p,
            string sep)
            => sepBy(p, pstring<Unit>(sep));

        /// <summary>
        /// The parser `Many(p, sep)` parses *zero* or more occurrences of `p` separated by the
        /// parser `sep` (in EBNF notation: `(p (sep p)*)?`).
        /// </summary>
        public static FSharpFunc<Chars, Reply<FSharpList<T>>> Many<T, TSep>(
            FSharpFunc<Chars, Reply<T>> p,
            FSharpFunc<Chars, Reply<TSep>> sep)
            => sepBy(p, sep);

        /// <summary>
        /// The parser `Many1(p, c)` parses *one* or more occurrences of `p` separated by the
        /// char `c` (in EBNF notation: `p (c p)*`).
        /// </summary>
        public static FSharpFunc<Chars, Reply<FSharpList<T>>> Many1<T>(
            FSharpFunc<Chars, Reply<T>> p,
            char sep)
            => sepBy1(p, pchar<Unit>(sep));

        /// <summary>
        /// The parser `Many1(p, s)` parses *one* or more occurrences of `p` separated by the
        /// string `s` (in EBNF notation: `p (s p)*`).
        /// </summary>
        public static FSharpFunc<Chars, Reply<FSharpList<T>>> Many1<T>(
            FSharpFunc<Chars, Reply<T>> p,
            string sep)
            => sepBy1(p, pstring<Unit>(sep));

        /// <summary>
        /// The parser `Many1(p, sep)` parses *one* or more occurrences of `p` separated by the
        /// parser `sep` (in EBNF notation: `p (sep p)*`).
        /// </summary>
        public static FSharpFunc<Chars, Reply<FSharpList<T>>> Many1<T>(
            FSharpFunc<Chars, Reply<T>> p,
            FSharpFunc<Chars, Reply<T>> sep)
            => sepBy1(p, sep);

        #endregion Repetition

        #region Backtracking

        /// <summary>
        /// The parser `Try(p)` applies the parser `p`. If `p` fails after changing the parser
        /// state or with a fatal error, `Try(p)` will backtrack to the original parser state and
        /// report a non-fatal error.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> Try<T>(
            FSharpFunc<Chars, Reply<T>> p)
            => attempt(p);

        #endregion Backtracking

        #region Special

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f(x)`, where `x`
        /// is the result returned by `p`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<TResult>> Map<T, TResult>(
            this FSharpFunc<Chars, Reply<T>> p,
            Func<T, TResult> map)
            => op_BarGreaterGreater(p, map.ToFSharpFunc());

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f()`. Hence the 
        /// result of the `Unit`-returning parser `p` will be ignored.
        /// </summary>
        public static FSharpFunc<Chars, Reply<TResult>> Map<TResult>(
            this FSharpFunc<Chars, Reply<Unit>> p,
            Func<TResult> map)
            => op_BarGreaterGreater(p, FSharpFunc.From<Unit, TResult>(_ => map()));

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f(x, y)`, where
        /// `x` and `y` are items of the tuple result `(x,y)` returned by `p`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<TResult>> Map<T1, T2, TResult>(
            this FSharpFunc<Chars, Reply<(T1, T2)>> p,
            Func<T1, T2, TResult> map)
            => op_BarGreaterGreater(p, FSharpFunc.From<(T1, T2), TResult>(x => map(x.Item1, x.Item2)));

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f(x, y, z)`, where
        /// `x`, `y` and `z` are items of the tuple result `(x,y,z)` returned by `p`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<TResult>> Map<T1, T2, T3, TResult>(
            this FSharpFunc<Chars, Reply<(T1, T2, T3)>> p,
            Func<T1, T2, T3, TResult> map)
            => op_BarGreaterGreater(p, FSharpFunc.From<(T1, T2, T3), TResult>(x => map(x.Item1, x.Item2, x.Item3)));

        /// <summary>
        /// The parser `Return(x)` always succeeds with the result `x` (without changing the parser
        /// state).
        /// </summary>
        public static FSharpFunc<Chars, Reply<TResult>> Return<TResult>(
            TResult result)
            => preturn<TResult, Unit>(result);

        /// <summary>
        /// The parser `p.Return(x)` applies the parser `p` and returns the result `x`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<TResult>> Return<T, TResult>(
            this FSharpFunc<Chars, Reply<T>> p,
            TResult result)
            => op_GreaterGreaterPercent(p, result);

        /// <summary>
        /// The parser `Rec(() => p)` is needed for recursive grammars (e.g. JSON, where objects
        /// can be nested). When parsers `p1` and `p2` depend on each other (directly or 
        /// indirectly) then the parser that is defined first has to reference the parser defined 
        /// last using `Rec(() => ...)`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> Rec<T>(
            Func<FSharpFunc<Chars, Reply<T>>> p)
            => FSharpFunc.From((Chars cs) => p().Invoke(cs));

        /// <summary>
        /// Flattens the nested tuple `((a,b),c)` to `(a,b,c)`.
        /// </summary>
        public static (T1, T2, T3) Flat<T1, T2, T3>(((T1, T2), T3) x) => (x.Item1.Item1, x.Item1.Item2, x.Item2);

        /// <summary>
        /// Flattens the nested tuple `(a,(b,c))` to `(a,b,c)`.
        /// </summary>
        public static (T1, T2, T3) Flat<T1, T2, T3>((T1, (T2, T3)) x) => (x.Item1, x.Item2.Item1, x.Item2.Item2);

        #endregion Special

        #region Labels

        /// <summary>
        /// The parser `p.Label(s)` applies the parser `p`. If `p` does not change the parser state
        /// (usually because `p` failed), the error messages are replaced with `expected s`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> Label<T>(
            this FSharpFunc<Chars, Reply<T>> p,
            string label)
            => op_LessQmarkGreater(p, label);

        /// <summary>Short form for `Label()`.</summary>
        public static FSharpFunc<Chars, Reply<T>> Lbl<T>(
            this FSharpFunc<Chars, Reply<T>> p,
            string label)
            => op_LessQmarkGreater(p, label);

        /// <summary>
        /// The parser `p.Label_(s)` behaves like `p.Label(s)`, except that when `p` fails after
        /// changing the parser state (for example, because `p` consumes input before it fails),
        /// a `CompoundError` message is generated with both the given string `s` and the error 
        /// messages generated by `p`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<T>> Label_<T>(
            this FSharpFunc<Chars, Reply<T>> p,
            string label)
            => op_LessQmarkQmarkGreater(p, label);

        /// <summary>Short form for `Label_()`.</summary>
        public static FSharpFunc<Chars, Reply<T>> Lbl_<T>(
            this FSharpFunc<Chars, Reply<T>> p,
            string label)
            => op_LessQmarkQmarkGreater(p, label);

        #endregion Labels
    }
}
