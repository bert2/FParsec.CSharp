namespace Tests {
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using FParsec;
    using FParsec.CSharp;
    using Newtonsoft.Json.Linq;
    using Shouldly;

    internal static class Extensions {
        internal static void ShouldBe<TResult>(this Reply<TResult> reply, TResult result)
            => reply.Result.ShouldBe(result, reply.Error.Print());

        internal static TResult ShouldBeOk<TResult>(this Reply<TResult> reply) {
            reply.IsOk().ShouldBeTrue(reply.Error.Print());
            return reply.Result;
        }

        internal static void ShouldBe(this Reply<XElement> reply, XElement result) {
            reply.IsOk().ShouldBeTrue(reply.Error.Print());
            XNode.DeepEquals(reply.Result, result)
                .ShouldBeTrue($"\nExpected:\t{result}\nActual:\t\t{reply.Result}");
        }

        internal static void ShouldBe<T, TError>(this Reply<T> reply, string message)
            where TError : ErrorMessage
            => reply.Error.AsEnumerable()
                .ShouldHaveSingleItem()
                .ShouldBeOfType<TError>()
                .GetString().ShouldBe(message);

        internal static void ShouldBe<TError>(this Reply<char> reply, string message)
            where TError : ErrorMessage
            => reply.ShouldBe<char, TError>(message);

        internal static void ShouldBe<TError>(this Reply<int> reply, string message)
            where TError : ErrorMessage
            => reply.ShouldBe<int, TError>(message);

        internal static void ShouldBe<TError>(this Reply<JObject> reply, string message)
            where TError : ErrorMessage
            => reply.ShouldBe<JObject, TError>(message);

        internal static void ShouldBe<TError>(this Reply<XElement> reply, string message)
            where TError : ErrorMessage
            => reply.ShouldBe<XElement, TError>(message);

        internal static string Print(this ErrorMessageList errors) => string.Join(", ", errors
            .AsEnumerable()
            .Select(e => (string)getDebuggerDisplayMethod.Invoke(e, new object[0])));

        private static readonly MethodInfo getDebuggerDisplayMethod = typeof(ErrorMessage)
            .GetMethod("GetDebuggerDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
    }
}
