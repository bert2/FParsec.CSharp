namespace Tests {
    using System.Linq;
    using System.Reflection;
    using FParsec;
    using FParsec.CSharp;
    using Newtonsoft.Json.Linq;
    using Shouldly;

    internal static class Extensions {
        internal static void ShouldBe<TResult>(this Reply<TResult> reply, TResult result)
            => reply.Result.ShouldBe(result, reply.Error.Print());

        internal static void ShouldBe<T, TError>(this Reply<T> reply, string message)
            where TError : ErrorMessage
            => reply.Error.AsEnumerable()
                .ShouldHaveSingleItem()
                .ShouldBeOfType<TError>()
                .GetString().ShouldBe(message);

        internal static void ShouldBe<TError>(this Reply<char> reply, string message)
            where TError : ErrorMessage
            => reply.ShouldBe<char, TError>(message);

        internal static void ShouldBe<TError>(this Reply<JObject> reply, string message)
            where TError : ErrorMessage
            => reply.ShouldBe<JObject, TError>(message);

        internal static string Print(this ErrorMessageList errors)
            => (string)getDebuggerDisplayMethod
                .Invoke(null, new[] { errors });

        private static readonly MethodInfo getDebuggerDisplayMethod = typeof(ErrorMessageList)
            .GetMethod("GetDebuggerDisplay", BindingFlags.Static | BindingFlags.NonPublic);
    }
}
