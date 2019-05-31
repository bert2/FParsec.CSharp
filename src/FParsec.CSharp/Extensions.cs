namespace FParsec.CSharp {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Microsoft.FSharp.Collections;
    using Microsoft.FSharp.Core;
    using static FParsec.CharParsers;
    using static FParsec.Error;

    /// <summary>Provides helpers to work with types from `FParsec` and `FSharpCore`.</summary>
    public static class Extensions {
        #region Executing parsers

        /// <summary>
        /// `p.Run(s)` is a convenient abbreviation for `p.RunOnString(s,name)`.
        /// </summary>
        public static ParserResult<TResult, Unit> Run<TResult>(
            this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
            string chars)
            => run(p, chars);

        /// <summary>
        /// <para>
        /// `p.RunOnString(s,name)` runs the parser `p` directly on the content of the string `s`.
        /// </para>
        /// <para>
        /// The `name` is used in error messages to describe the source of the input (e.g. a file
        /// path) and may be `null` or empty.
        /// </para>
        /// <para>The parser's `Reply` is captured and returned as a `ParserResult` value.</para>
        /// </summary>
        public static ParserResult<TResult, Unit> RunOnString<TResult>(
            this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
            string chars,
            string streamName = null)
            => runParserOnString(p, null, streamName, chars);

        /// <summary>
        /// <para>
        /// `p.RunOnString(s,index,count,name)` runs the parser `p` directly on the content of the
        /// string `s` between the indices `index` (inclusive) and `index + count` (exclusive)
        /// </para>
        /// <para>
        /// The `name` is used in error messages to describe the source of the input (e.g. a file
        /// path) and may be `null` or empty.
        /// </para>
        /// <para>The parser's `Reply` is captured and returned as a `ParserResult` value.</para>
        /// </summary>
        public static ParserResult<TResult, Unit> RunOnString<TResult>(
            this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
            string chars,
            int index,
            int length,
            string streamName = null)
            => runParserOnSubstring(p, null, streamName, chars, index, length);

        /// <summary>
        /// <para>
        /// `p.RunOnStream(stream,enc,name)` runs the parser `p` on the content of the
        /// `System.IO.Stream` `stream`.
        /// </para>
        /// <para>
        /// The `name` is used in error messages to describe the source of the input (e.g. a file
        /// path) and may be `null` or empty.
        /// </para>
        /// <para>
        /// In case no unicode byte order mark is found, the stream data is assumed to be encoded
        /// with the given `enc`. `Encoding.Default` will be used if `enc` was not specified.
        /// </para>
        /// <para>The parser's `Reply` is captured and returned as a `ParserResult` value.</para>
        /// </summary>
        public static ParserResult<TResult, Unit> RunOnStream<TResult>(
            this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
            Stream byteStream,
            Encoding encoding = null,
            string streamName = null)
            => runParserOnStream(p, null, streamName, byteStream, encoding ?? Encoding.Default);

        /// <summary>
        /// <para>
        /// `p.RunOnFile(path,enc)` runs the parser `p` on the content of the file at the given
        /// `path`.
        /// </para>
        /// <para>
        /// In case no unicode byte order mark is found, the file data is assumed to be encoded
        /// with the given `enc`. `Encoding.Default` will be used if `enc` was not specified.
        /// </para>
        /// <para>The parser's `Reply` is captured and returned as a `ParserResult` value.</para>
        /// </summary>
        public static ParserResult<TResult, Unit> RunOnFile<TResult>(
            this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
            string path,
            Encoding encoding = null)
            => runParserOnFile(p, null, path, encoding ?? Encoding.Default);

        /// <summary>Applies the parser `p` to the input string.</summary>
        public static Reply<TResult> ParseString<TResult>(
            this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
            string chars)
            => p.Invoke(new CharStream<Unit>(chars, 0, chars.Length));

        /// <summary>
        /// <para>Applies the parser `p` to the input file.</para>
        /// <para>
        /// In case no unicode byte order mark is found, the file data is assumed to be encoded
        /// with the given `encoding`. `Encoding.Default` will be used if `encoding` was not
        /// specified.
        /// </para>
        /// </summary>
        public static Reply<TResult> ParseFile<TResult>(
            this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
            string path,
            Encoding encoding = null)
            => p.Invoke(new CharStream<Unit>(path, encoding ?? Encoding.Default, detectEncodingFromByteOrderMarks: true));

        /// <summary>Applies the parser `p` to the input `CharStream`.</summary>
        public static Reply<TResult> Parse<TResult>(
           this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
           CharStream<Unit> chars)
           => p.Invoke(chars);

        #endregion Executing parsers

        #region Handling `ParserResult`s

        /// <summary>
        /// <para>
        /// Unwraps the `ParserResult` and returns the tuple `(r,m)`, where `r` is the parser
        /// result and `m` is the detailed parser error message.
        /// </para>
        /// <para>`m` will be `null` if parsing succeeded.</para>
        /// <para>`r` will be `default(TResult)` if parsing failed.</para>
        /// </summary>
        public static (TResult result, string message) UnwrapResult<TResult>(this ParserResult<TResult, Unit> result) {
            var (r, f) = result.UnwrapWithFailure();
            return (r, f?.Message());
        }

        /// <summary>
        /// <para>
        /// Unwraps the `ParserResult` and returns the tuple `(r,e)`, where `r` is the parser
        /// result and `e` is the `ParserError` instance.
        /// </para>
        /// <para>`e` will be `null` if parsing succeeded.</para>
        /// <para>`r` will be `default(TResult)` if parsing failed.</para>
        /// </summary>
        public static (TResult result, ParserError error) UnwrapWithError<TResult>(this ParserResult<TResult, Unit> result) {
            var (r, f) = result.UnwrapWithFailure();
            return (r, f?.Error());
        }

        /// <summary>
        /// <para>
        /// Unwraps the `ParserResult` and returns the tuple `(r,f)`, where `r` is the parser
        /// result and `f` is this `ParserResult` instance, but casted to `ParserResult.Failure`.
        /// </para>
        /// <para>`f` will be `null` if parsing succeeded.</para>
        /// <para>`r` will be `default(TResult)` if parsing failed.</para>
        /// </summary>
        public static (TResult result, ParserResult<TResult, Unit>.Failure failure) UnwrapWithFailure<TResult>(
            this ParserResult<TResult, Unit> result)
            => result.IsSuccess
                ? (result.ToSuccess().Result(), (ParserResult<TResult, Unit>.Failure)null)
                : (default, result.ToFailure());

        /// <summary>
        /// <para>Gets the result of the parser reply.</para>
        /// <para>
        /// When the parser result is an instance of `Failure` then `GetResult()` will throw an
        /// `InvalidOperationException` with the detailed parser error message.
        /// </para>
        /// </summary>
        public static TResult GetResult<TResult>(this ParserResult<TResult, Unit> result)
            => result.GetResult(msg => throw new InvalidOperationException(msg));

        /// <summary>
        /// <para>
        /// Gets the result of the parser reply and delegates handling of parser failures.
        /// </para>
        /// <para>
        /// `handleMessage` will receive the detailed parser error message and must either return a
        /// fallback result value or throw an exception.
        /// </para>
        /// </summary>
        public static TResult GetResult<TResult>(
            this ParserResult<TResult, Unit> result,
            Func<string, TResult> handleMessage)
            => result.GetResultOrFailure(f => handleMessage(f.Message()));

        /// <summary>
        /// <para>
        /// Gets the result of the parser reply and delegates handling of parser failures.
        /// </para>
        /// <para>
        /// `handleError` will receive the `ParserError` instance and must either return a fallback
        /// result value or throw an exception.
        /// </para>
        /// </summary>
        public static TResult GetResultOrError<TResult>(
            this ParserResult<TResult, Unit> result,
            Func<ParserError, TResult> handleError)
            => result.GetResultOrFailure(f => handleError(f.Error()));

        /// <summary>
        /// <para>
        /// Gets the result of the parser reply and delegates handling of parser failures.
        /// </para>
        /// <para>
        /// `handleFailure` will receive the `Failure` instance and must either return a fallback
        /// result value or throw an exception.
        /// </para>
        /// </summary>
        public static TResult GetResultOrFailure<TResult>(
            this ParserResult<TResult, Unit> result,
            Func<ParserResult<TResult, Unit>.Failure, TResult> handleFailure)
            => result.IsFailure
                ? handleFailure(result.ToFailure())
                : result.ToSuccess().Result();

        /// <summary>Wrapper for `ParserResult.Success.Item1`.</summary>
        public static TResult Result<TResult>(this ParserResult<TResult, Unit>.Success success) => success.Item1;

        /// <summary>Wrapper for `ParserResult.Success.Item3`.</summary>
        public static Position Position<TResult>(this ParserResult<TResult, Unit>.Success success) => success.Item3;

        /// <summary>Wrapper for `ParserResult.Failure.Item1`.</summary>
        public static string Message<TResult>(this ParserResult<TResult, Unit>.Failure failure) => failure.Item1;

        /// <summary>Wrapper for `ParserResult.Failure.Item2`.</summary>
        public static ParserError Error<TResult>(this ParserResult<TResult, Unit>.Failure failure) => failure.Item2;

        /// <summary>Unsafely cast the `ParserResult` to `ParserResult.Success`.</summary>
        public static ParserResult<TResult, Unit>.Success ToSuccess<TResult>(this ParserResult<TResult, Unit> result)
            => (ParserResult<TResult, Unit>.Success)result;

        /// <summary>Unsafely cast the `ParserResult` to `ParserResult.Failure`.</summary>
        public static ParserResult<TResult, Unit>.Failure ToFailure<TResult>(this ParserResult<TResult, Unit> result)
           => (ParserResult<TResult, Unit>.Failure)result;

        /// <summary>Safely cast the `ParserResult` to `ParserResult.Success`.</summary>
        public static ParserResult<TResult, Unit>.Success AsSuccess<TResult>(this ParserResult<TResult, Unit> result)
            => result as ParserResult<TResult, Unit>.Success;

        /// <summary>Safely cast the `ParserResult` to `ParserResult.Failure`.</summary>
        public static ParserResult<TResult, Unit>.Failure AsFailure<TResult>(this ParserResult<TResult, Unit> result)
            => result as ParserResult<TResult, Unit>.Failure;

        #endregion Handling `ParserResult`s

        #region Handling `Reply`s

        /// <summary>Deconstructs a `Reply`.</summary>
        public static void Deconstruct<T>(this Reply<T> r, out ReplyStatus status, out T result, out ErrorMessageList error) {
            status = r.Status;
            result = r.Result;
            error = r.Error;
        }

        /// <summary>Indicates whether the parser `Reply` has status `Ok`.</summary>
        public static bool IsOk<T>(this Reply<T> reply) => reply.Status == ReplyStatus.Ok;

        /// <summary>Turns the `ErrorMessageList` into an `IEnumarable` of `ErrorMessage`s.</summary>
        public static IEnumerable<ErrorMessage> AsEnumerable(this ErrorMessageList errs) {
            if (errs == null) yield break;

            yield return errs.Head;

            foreach (var e in errs.Tail.AsEnumerable())
                yield return e;
        }

        /// <summary>Deconstructs an `ErrorMessageList`.</summary>
        public static void Deconstruct(this ErrorMessageList error, out ErrorMessage head, out ErrorMessageList tail) {
            head = error.Head;
            tail = error.Tail;
        }

        /// <summary>Workaround to access the bugged property `FParsec.ErrorMessage.Expected.String`.</summary>
        public static string GetString(this ErrorMessage e)
            => (string)messageField.GetValue(e);

        private static readonly FieldInfo messageField = typeof(ErrorMessage)
            .GetField("String", BindingFlags.Instance | BindingFlags.NonPublic);

        #endregion Handling `Reply`s

        /// <summary>Creates an `FSharpList` from the `IEnumerable`.</summary>
        public static FSharpList<T> ToFSharpList<T>(this IEnumerable<T> source) => ListModule.OfSeq(source);

        /// <summary>Unwraps the `FSharpOption` using an optional default value.</summary>
        public static T GetValueOrDefault<T>(
            this FSharpOption<T> opt,
            T defaultValue = default)
            => FSharpOption<T>.get_IsSome(opt) ? opt.Value : defaultValue;
    }
}
