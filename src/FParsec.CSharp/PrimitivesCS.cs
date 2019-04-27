namespace FParsec.CSharp {
    using System;
    using LambdaConvert;
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

        #region Special

        /// <summary>
        /// The parser `p.Map(f)` applies the parser `p` and returns the result `f x`,  where `x`
        /// is the result returned by `p`.
        /// </summary>
        public static FSharpFunc<Chars, Reply<TResult>> Map<T, TResult>(
            this FSharpFunc<Chars, Reply<T>> p,
            Func<T, TResult> map)
            => op_BarGreaterGreater(p, map.ToFSharpFunc());

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

        #endregion Special
    }
}
