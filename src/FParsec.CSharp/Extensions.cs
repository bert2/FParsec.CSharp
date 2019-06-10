using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using static FParsec.CharParsers;
using static FParsec.Error;

namespace FParsec.CSharp {
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
        /// `p.RunOnString(s,u,name)` runs the parser `p` directly on the content of the string
        /// `s`, starting with the initial user state `u`.
        /// </para>
        /// <para>
        /// The `name` is used in error messages to describe the source of the input (e.g. a file
        /// path) and may be `null` or empty.
        /// </para>
        /// <para>The parser's `Reply` is captured and returned as a `ParserResult` value.</para>
        /// </summary>
        public static ParserResult<TResult, U> RunOnString<U, TResult>(
            this FSharpFunc<CharStream<U>, Reply<TResult>> p,
            string chars,
            U userState = default,
            string streamName = null)
            => runParserOnString(p, userState, streamName, chars);

        /// <summary>
        /// <para>
        /// `p.RunOnString(s,index,count,u,name)` runs the parser `p` directly on the content of
        /// the string `s` between the indices `index` (inclusive) and `index + count` (exclusive),
        /// starting with the initial user state `u`.
        /// </para>
        /// <para>
        /// The `name` is used in error messages to describe the source of the input (e.g. a file
        /// path) and may be `null` or empty.
        /// </para>
        /// <para>The parser's `Reply` is captured and returned as a `ParserResult` value.</para>
        /// </summary>
        public static ParserResult<TResult, U> RunOnString<U, TResult>(
            this FSharpFunc<CharStream<U>, Reply<TResult>> p,
            string chars,
            int index,
            int length,
            U userState = default,
            string streamName = null)
            => runParserOnSubstring(p, userState, streamName, chars, index, length);

        /// <summary>
        /// <para>
        /// `p.RunOnStream(stream,u,enc,name)` runs the parser `p` on the content of the
        /// `System.IO.Stream` `stream`, starting with the initial user state `u`.
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
        public static ParserResult<TResult, U> RunOnStream<U, TResult>(
            this FSharpFunc<CharStream<U>, Reply<TResult>> p,
            Stream byteStream,
            U userState = default,
            Encoding encoding = null,
            string streamName = null)
            => runParserOnStream(p, userState, streamName, byteStream, encoding ?? Encoding.Default);

        /// <summary>
        /// <para>
        /// `p.RunOnFile(path,u,enc)` runs the parser `p` on the content of the file at the given
        /// `path`, starting with the initial user state `u`.
        /// </para>
        /// <para>
        /// In case no unicode byte order mark is found, the file data is assumed to be encoded
        /// with the given `enc`. `Encoding.Default` will be used if `enc` was not specified.
        /// </para>
        /// <para>The parser's `Reply` is captured and returned as a `ParserResult` value.</para>
        /// </summary>
        public static ParserResult<TResult, U> RunOnFile<U, TResult>(
            this FSharpFunc<CharStream<U>, Reply<TResult>> p,
            string path,
            U userState = default,
            Encoding encoding = null)
            => runParserOnFile(p, userState, path, encoding ?? Encoding.Default);

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
        public static (TResult result, string message) UnwrapResult<U, TResult>(this ParserResult<TResult, U> result) {
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
        public static (TResult result, ParserError error) UnwrapWithError<U, TResult>(this ParserResult<TResult, U> result) {
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
        public static (TResult result, ParserResult<TResult, U>.Failure failure) UnwrapWithFailure<U, TResult>(
            this ParserResult<TResult, U> result)
            => result.IsSuccess
                ? (result.ToSuccess().Result(), (ParserResult<TResult, U>.Failure)null)
                : (default, result.ToFailure());

        /// <summary>
        /// <para>Gets the result of the parser reply.</para>
        /// <para>
        /// When the parser result is an instance of `Failure` then `GetResult()` will throw an
        /// `InvalidOperationException` with the detailed parser error message.
        /// </para>
        /// </summary>
        public static TResult GetResult<U, TResult>(this ParserResult<TResult, U> result)
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
        public static TResult GetResult<U, TResult>(
            this ParserResult<TResult, U> result,
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
        public static TResult GetResultOrError<U, TResult>(
            this ParserResult<TResult, U> result,
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
        public static TResult GetResultOrFailure<U, TResult>(
            this ParserResult<TResult, U> result,
            Func<ParserResult<TResult, U>.Failure, TResult> handleFailure)
            => result.IsFailure
                ? handleFailure(result.ToFailure())
                : result.ToSuccess().Result();

        /// <summary>Wrapper for `ParserResult.Success.Item1`.</summary>
        public static TResult Result<U, TResult>(this ParserResult<TResult, U>.Success success) => success.Item1;

        /// <summary>Wrapper for `ParserResult.Success.Item2`.</summary>
        public static U UserState<U, TResult>(this ParserResult<TResult, U>.Success success) => success.Item2;

        /// <summary>Wrapper for `ParserResult.Success.Item3`.</summary>
        public static Position Position<U, TResult>(this ParserResult<TResult, U>.Success success) => success.Item3;

        /// <summary>Wrapper for `ParserResult.Failure.Item1`.</summary>
        public static string Message<U, TResult>(this ParserResult<TResult, U>.Failure failure) => failure.Item1;

        /// <summary>Wrapper for `ParserResult.Failure.Item2`.</summary>
        public static ParserError Error<U, TResult>(this ParserResult<TResult, U>.Failure failure) => failure.Item2;

        /// <summary>Wrapper for `ParserResult.Failure.Item3`.</summary>
        public static U UserState<U, TResult>(this ParserResult<TResult, U>.Failure failure) => failure.Item3;

        /// <summary>Unsafely cast the `ParserResult` to `ParserResult.Success`.</summary>
        public static ParserResult<TResult, U>.Success ToSuccess<U, TResult>(this ParserResult<TResult, U> result)
            => (ParserResult<TResult, U>.Success)result;

        /// <summary>Unsafely cast the `ParserResult` to `ParserResult.Failure`.</summary>
        public static ParserResult<TResult, U>.Failure ToFailure<U, TResult>(this ParserResult<TResult, U> result)
           => (ParserResult<TResult, U>.Failure)result;

        /// <summary>Safely cast the `ParserResult` to `ParserResult.Success`.</summary>
        public static ParserResult<TResult, U>.Success AsSuccess<U, TResult>(this ParserResult<TResult, U> result)
            => result as ParserResult<TResult, U>.Success;

        /// <summary>Safely cast the `ParserResult` to `ParserResult.Failure`.</summary>
        public static ParserResult<TResult, U>.Failure AsFailure<U, TResult>(this ParserResult<TResult, U> result)
            => result as ParserResult<TResult, U>.Failure;

        #endregion Handling `ParserResult`s

        #region Handling `Reply`s

        /// <summary>Indicates whether the parser `Reply` has status `Ok`.</summary>
        public static bool IsOk<TResult>(this Reply<TResult> reply) => reply.Status == ReplyStatus.Ok;

        /// <summary>Turns the `ErrorMessageList` into an `IEnumarable` of `ErrorMessage`s.</summary>
        public static IEnumerable<ErrorMessage> AsEnumerable(this ErrorMessageList errs) {
            if (errs == null) yield break;

            yield return errs.Head;

            foreach (var e in errs.Tail.AsEnumerable())
                yield return e;
        }

        /// <summary>
        /// Workaround to access the bugged properties
        /// `FParsec.ErrorMessage.ExpectedString.String` and
        /// `FParsec.ErrorMessage.UnexpectedString.String`.
        /// </summary>
        public static string GetString(this ErrorMessage e) => (string)messageField.GetValue(e);

        private static readonly FieldInfo messageField = typeof(ErrorMessage)
            .GetField("String", BindingFlags.Instance | BindingFlags.NonPublic);

        #endregion Handling `Reply`s

        #region Deconstructors

        /// <summary>Deconstructs a `Success` result.</summary>
        public static void Deconstruct<U, TResult>(
            this ParserResult<TResult, U>.Success success,
            out TResult result,
            out U userState,
            out Position position) {
            result = success.Result();
            userState = success.UserState();
            position = success.Position();
        }

        /// <summary>Deconstructs a `Failure` result.</summary>
        public static void Deconstruct<U, TResult>(
            this ParserResult<TResult, U>.Failure failure,
            out string message,
            out ParserError error,
            out U userState) {
            message = failure.Message();
            error = failure.Error();
            userState = failure.UserState();
        }

        /// <summary>Deconstructs a `Reply`.</summary>
        public static void Deconstruct<TResult>(
            this Reply<TResult> reply,
            out ReplyStatus status,
            out TResult result,
            out ErrorMessageList error) {
            status = reply.Status;
            result = reply.Result;
            error = reply.Error;
        }

        /// <summary>Deconstructs a `ParserError`.</summary>
        public static void Deconstruct(
            this ParserError error,
            out ErrorMessageList messages,
            out Position position) {
            messages = error.Messages;
            position = error.Position;
        }

        /// <summary>Deconstructs an `ErrorMessageList`.</summary>
        public static void Deconstruct(
            this ErrorMessageList error,
            out ErrorMessage head,
            out ErrorMessageList tail) {
            head = error.Head;
            tail = error.Tail;
        }

        /// <summary>Deconstructs an `ErrorMessage`.</summary>
        public static void Deconstruct(this ErrorMessage error, out ErrorMessageType type) => type = error.Type;

        /// <summary>Deconstructs an `ErrorMessage.Expected` error.</summary>
        public static void Deconstruct(this ErrorMessage.Expected error, out string label) => label = error.Label;

        /// <summary>Deconstructs an `ErrorMessage.ExpectedString` error.</summary>
        public static void Deconstruct(this ErrorMessage.ExpectedString error, out string @string) => @string = error.GetString();

        /// <summary>Deconstructs an `ErrorMessage.ExpectedCaseInsensitiveString` error.</summary>
        public static void Deconstruct(this ErrorMessage.ExpectedCaseInsensitiveString error, out string caseInsensitiveString)
            => caseInsensitiveString = error.CaseInsensitiveString;

        /// <summary>Deconstructs an `ErrorMessage.Unexpected` error.</summary>
        public static void Deconstruct(this ErrorMessage.Unexpected error, out string label) => label = error.Label;

        /// <summary>Deconstructs an `ErrorMessage.UnexpectedString` error.</summary>
        public static void Deconstruct(this ErrorMessage.UnexpectedString error, out string @string) => @string = error.GetString();

        /// <summary>Deconstructs an `ErrorMessage.ExpectedCaseInsensitiveString` error.</summary>
        public static void Deconstruct(this ErrorMessage.UnexpectedCaseInsensitiveString error, out string caseInsensitiveString)
            => caseInsensitiveString = error.CaseInsensitiveString;

        /// <summary>Deconstructs an `ErrorMessage.Message` error.</summary>
        public static void Deconstruct(this ErrorMessage.Message error, out string @string) => @string = error.String;

        /// <summary>Deconstructs an `ErrorMessage.NestedError` error.</summary>
        public static void Deconstruct(
            this ErrorMessage.NestedError error,
            out Position position,
            out ErrorMessageList messages) {
            position = error.Position;
            messages = error.Messages;
        }

        /// <summary>Deconstructs an `ErrorMessage.CompoundError` error.</summary>
        public static void Deconstruct(
            this ErrorMessage.CompoundError error,
            out string labelOfCompound,
            out Position nestedErrorPosition,
            out ErrorMessageList nestedErrorMessages) {
            labelOfCompound = error.LabelOfCompound;
            nestedErrorPosition = error.NestedErrorPosition;
            nestedErrorMessages = error.NestedErrorMessages;
        }

        /// <summary>Deconstructs an `ErrorMessage.Other` error.</summary>
        public static void Deconstruct(this ErrorMessage.Other error, out object data) => data = error.Data;

        /// <summary>Deconstructs a `Position`.</summary>
        public static void Deconstruct(
            this Position position,
            out long index,
            out long line,
            out long column,
            out string streamName) {
            index = position.Index;
            line = position.Line;
            column = position.Column;
            streamName = position.StreamName;
        }

        #endregion Deconstructors

        /// <summary>Creates an `FSharpList` from the `IEnumerable`.</summary>
        public static FSharpList<T> ToFSharpList<T>(this IEnumerable<T> source) => ListModule.OfSeq(source);

        /// <summary>Unwraps the `FSharpOption` using an optional default value.</summary>
        public static T GetValueOrDefault<T>(
            this FSharpOption<T> opt,
            T defaultValue = default)
            => FSharpOption<T>.get_IsSome(opt) ? opt.Value : defaultValue;
    }
}
