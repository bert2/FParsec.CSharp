﻿using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using FParsec;
using FParsec.CSharp;
using Microsoft.FSharp.Collections;
using Newtonsoft.Json.Linq;
using Shouldly;

namespace Tests;

internal static class Extensions {
    internal static TResult OkResult<TResult>(this Reply<TResult> reply) {
        reply.IsOk().ShouldBeTrue(reply.Error.Print());
        return reply.Result;
    }

    internal static void ShouldBe<TResult>(this Reply<TResult> reply, TResult result)
        => reply.OkResult().ShouldBe(result, reply.Error.Print());

    internal static void ShouldBe<TResult>(this Reply<FSharpList<TResult>> reply, params TResult[] results)
        => reply.OkResult().ShouldBe(results.ToFSharpList(), reply.Error.Print());

    internal static void ShouldBe<T>(this Reply<T> reply, ReplyStatus status)
        => reply.Status.ShouldBe(status, reply.Error.Print());

    internal static void ShouldBe<T, TError>(this Reply<T> reply, string message)
        where TError : ErrorMessage
        => reply.Error.AsEnumerable()
            .ShouldHaveSingleItem()
            .ShouldBeOfType<TError>()
            .Map(err => err switch
            {
                ErrorMessage.Expected(var msg) => msg,
                ErrorMessage.ExpectedString(var msg) => msg,
                ErrorMessage.ExpectedCaseInsensitiveString(var msg) => msg,
                ErrorMessage.Unexpected(var msg) => msg,
                ErrorMessage.UnexpectedString(var msg) => msg,
                ErrorMessage.UnexpectedCaseInsensitiveString(var msg) => msg,
                ErrorMessage.Message(var msg) => msg,
                ErrorMessage.CompoundError(var msg, _, _) => msg,
                _ => throw new ArgumentException($"Unsupported error type 'ErrorMessage.{typeof(TError).Name}'", nameof(TError))
            })
            .ShouldBe(message);

    internal static void ShouldBeErrors<T>(this Reply<T> reply, params ErrorMessage[] errors) {
        reply.Error.AsEnumerable().Count().ShouldBe(errors.Length, reply.Error.Print());
        reply.Error.AsEnumerable().ShouldBe(errors, ignoreOrder: true, reply.Error.Print());
    }

    #region Explicit wrappers for ShouldBe<T, TError> to avoid specifying T

    internal static void ShouldBe<TError>(this Reply<char> reply, string message)
        where TError : ErrorMessage
        => reply.ShouldBe<char, TError>(message);

    internal static void ShouldBe<TError>(this Reply<(char, char)> reply, string message)
        where TError : ErrorMessage
        => reply.ShouldBe<(char, char), TError>(message);

    internal static void ShouldBe<TError>(this Reply<string> reply, string message)
        where TError : ErrorMessage
        => reply.ShouldBe<string, TError>(message);

    internal static void ShouldBe<TError>(this Reply<int> reply, string message)
        where TError : ErrorMessage
        => reply.ShouldBe<int, TError>(message);

    internal static void ShouldBe<TError>(this Reply<FSharpList<char>> reply, string message)
        where TError : ErrorMessage
        => reply.ShouldBe<FSharpList<char>, TError>(message);

    internal static void ShouldBe<TError>(this Reply<JObject?> reply, string message)
        where TError : ErrorMessage
        => reply.ShouldBe<JObject?, TError>(message);

    internal static void ShouldBe<TError>(this Reply<XElement> reply, string message)
        where TError : ErrorMessage
        => reply.ShouldBe<XElement, TError>(message);

    #endregion Explicit wrappers for ShouldBe<T, TError> to avoid specifying T

    internal static void ShouldBe(this Reply<XElement> reply, XElement result) => XNode
        .DeepEquals(reply.OkResult(), result)
        .ShouldBeTrue($"\nExpected:\t{result}\nActual:\t\t{reply.Result}");

    internal static void ShouldBe(this XElement actual, XElement expected) => XNode
        .DeepEquals(actual, expected)
        .ShouldBeTrue($"\nExpected:\t{expected}\nActual:\t\t{actual}");

    internal static string Print(this ErrorMessageList errors) => string.Join(", ", errors
        .AsEnumerable()
        .Select(e => (string?)getDebuggerDisplayMethod?.Invoke(e, [])));

    private static readonly MethodInfo? getDebuggerDisplayMethod = typeof(ErrorMessage)
        .GetMethod("GetDebuggerDisplay", BindingFlags.Instance | BindingFlags.NonPublic);

    internal static T Debug<T>(this T x, Action<T> f) { f(x); return x; }

    private static TResult Map<T, TResult>(this T x, Func<T, TResult> f) => f(x);
}
