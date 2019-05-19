namespace FParsec.CSharp {
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.FSharp.Collections;
    using Microsoft.FSharp.Core;

    public static class Extensions {
        /// <summary>Applies the parser `p` to the input string.</summary>
        public static Reply<TResult> ParseString<TResult>(
            this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
            string chars)
            => p.Invoke(new CharStream<Unit>(chars, 0, chars.Length));

        /// <summary>Applies the parser `p` to the input file. Auto-detects encoding from BOM.</summary>
        public static Reply<TResult> ParseFile<TResult>(
            this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
            string path)
            => p.Invoke(new CharStream<Unit>(path, encoding: null, detectEncodingFromByteOrderMarks: true));

        /// <summary>Applies the parser `p` to the input `CharStream`.</summary>
        public static Reply<TResult> Parse<TResult>(
           this FSharpFunc<CharStream<Unit>, Reply<TResult>> p,
           CharStream<Unit> chars)
           => p.Invoke(chars);

        public static FSharpList<T> ToFSharpList<T>(this IEnumerable<T> source) => ListModule.OfSeq(source);

        public static T GetValueOrDefault<T>(this FSharpOption<T> opt) => FSharpOption<T>.get_IsSome(opt)? opt.Value : default;

        public static bool IsOk<T>(this Reply<T> reply) => reply.Status == ReplyStatus.Ok;

        public static IEnumerable<ErrorMessage> AsEnumerable(this ErrorMessageList errs) {
            if (errs == null) yield break;

            yield return errs.Head;

            foreach (var e in errs.Tail.AsEnumerable())
                yield return e;
        }

        /// <summary>Workaround to access the bugged property `FParsec.ErrorMessage.Expected.String`.</summary>
        public static string GetString(this ErrorMessage e)
            => (string)messageField.GetValue(e);

        private static readonly FieldInfo messageField = typeof(ErrorMessage)
            .GetField("String", BindingFlags.Instance | BindingFlags.NonPublic);
    }
}
