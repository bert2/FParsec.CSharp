# FParsec.CSharp

[![Build status](https://ci.appveyor.com/api/projects/status/282vojx52ole5lww?svg=true)](https://ci.appveyor.com/project/bert2/fparsec-csharp) [![NuGet](https://img.shields.io/nuget/v/FParsec.CSharp.svg)](https://www.nuget.org/packages/FParsec.CSharp)

FParsec.CSharp is a C# wrapper for the F# package [FParsec](https://github.com/stephan-tolksdorf/fparsec). FParsec is a parser combinator library with which you can implement parsers declaratively.

- [Why FParsec.CSharp?](#why-fparseccsharp)
- [Getting started](#getting-started)
- [Executing parsers](#executing-parsers)
  * [Running the parser function and getting the `Reply`](#running-the-parser-function-and-getting-the-reply)
  * [Getting nicer error messages with `ParserResult`](#getting-nicer-error-messages-with-parserresult)
- [Handling parser results](#handling-parser-results)
  * [Getting the result or an exception](#getting-the-result-or-an-exception)
  * [Getting the result with custom error handling](#getting-the-result-with-custom-error-handling)
  * [Safe unwrapping](#safe-unwrapping)
- [Using FParsec.CSharp and FParsec together](#using-fparseccsharp-and-fparsec-together)
  * [Working with FParsec parsers directly](#working-with-fparsec-parsers-directly)
  * [Specifying user state](#specifying-user-state)
  * [Passing lambdas to FParsec](#passing-lambdas-to-fparsec)
- [Examples](#examples)
  * [Simple JSON](#simple-json)
  * [Simple XML](#simple-xml)
  * [Glob patterns](#glob-patterns)
  * [Arithmetic expressions](#arithmetic-expressions)
  * [Simple regular expressions](#simple-regular-expressions)
- [Hints](#hints)
  * [Debugging](#debugging)
  * [Aliasing awkward types](#aliasing-awkward-types)
- [Alternatives](#alternatives)
- [Where is the FParsec function `x`?](#where-is-the-fparsec-function-x)
- [TODO](#todo)


## Why FParsec.CSharp?

While using FParsec from C# is entirely possible in theory, it is very awkward in practice. Most of FParsec's elegance is lost in translation due to C#'s inferior type inference and its lack of custom operators.

FParsec.CSharp tries to alleviate that by wrapping FParsec's operators as extension functions.

FParsec.CSharp does not try to hide any types from `FParsec` or `FSharp.Core`--the wrapper is thin and also avoids name collisions. That way you can always fallback to FParsec anytime you need some functionality not (yet) implemented by FParsec.CSharp.

Based on the current implementation it should be easy to extend the wrapper yourself if needed. Pull requests are always welcome!

## Getting started

Import the combinators, pre-defined parsers, and helper functions:

```C#
using FParsec.CSharp; // extension functions (combinators & helpers)
using static FParsec.CSharp.PrimitivesCS; // combinator functions
using static FParsec.CSharp.CharParsersCS; // pre-defined parsers
```

Now you can write some parsers:

```C#
var p = AnyChar.And(Digit);
var r = p.ParseString("a1");
System.Diagnostics.Debug.Assert(r.Result == ('a', '1'));
```

## Executing parsers

An FParsec parser is a function that takes a `CharStream<T>` and returns a `Reply<T>`. In C# such parsers are represented by the type `FSharpFunction<CharStream<TUserState>, Reply<TResult>>` and can be executed with the method `Reply<TResult>> Invoke(CharStream<TUserState>)`.

FParsec.CSharp comes with extensions to make things easier for you.

### Running the parser function and getting the `Reply`

You have already seen one way to execute a parser in the previous section: `ParseString()`.

`ParseString(string)` is just a wrapper for `FSharpFunc<CharStream<Unit>, Reply<T>>.Invoke()`. It constructs the `CharStream` for you from the given string.

`ParseFile(string)` will do the same for a given file path.

If you need maximum control then `Parse(CharStream<Unit>)` is your best option. You can configure the `CharStream` yourself and inspect all the details of the `Reply` after parsing.

### Getting nicer error messages with `ParserResult`

Using `Invoke()`, `Parse()`, or any of its variants is generally not recommended. FParsec provides a better way to execute parsers that generates nicely formatted error messages: `Run()`.

The extension `ParserResult<T, Unit> Run(string)` does the same as `Reply<T> ParseString(string)`, but also builds an error message from the `Reply`'s errors and the parser postion:

```C#
var result = Many1(Digit).AndR(Upper).Run("123a");

// Don't be shocked: in the next section we will learn how to improve this.
var msg = ((ParserResult<char, Unit>.Failure)result).Item1;

Console.WriteLine(msg);
```

The above will print the following detailed parsing failure message:

```
Error in Ln: 1 Col: 4
123a
   ^
Expecting: decimal digit or uppercase letter
```

`Run(string)` is actually just a short form for `RunOnString(string)`.

Additionally, there are three more `Run...()` functions: `RunOnString(string, int, int)` (parse a substring), `RunOnStream()` (parse a `Stream`), and `RunOnFile()` (parse... well you get the idea).

## Handling parser results

FParsec's `ParserResult` is an F# discriminated union, which are awkward to work with in C#. FParsec.CSharp comes with extensions methods that hide those ugly details.

### Getting the result or an exception

The easiest way to get the parser result is to use `GetResult()`:

```C#
var one = Digit.Run("1").GetResult();
Debug.Assert(one == '1');
```

`GetResult()` will throw an `InvalidOperationException` in case the parser failed. The exception will contain the detailed parsing failure message.

### Getting the result with custom error handling

If you need more graceful error handling you can use `GetResult<T>(Func<string, T>)` which delegates error handling to the caller and provides the failure message to it:

```C#
// provide fallback value
var d1 = Digit.Run("a").GetResult(_ => default);
Debug.Assert(d1 == '\0');

// throw your own exception
var d2 = Digit.Run("a").GetResult(msg => throw new Exception($"whoops... {msg}"));

// do anything as long as you return a fallback value or throw
var d3 = Digit.Run("a").GetResult(_ => {
    Console.WriteLine("oof");
    return GetRandomChar();
});
```

Additionally, the extensions `GetResultOrError<T>(Func<ParserError, T>)` and `GetResultOrFailure<T>(Func<ParserResult<T, Unit>.Failure, T>)` let you inspect the `ParserError` or `Failure` objects during error handling.

### Safe unwrapping

Alternatively, you can can safely unwrap a `ParserResult<T, Unit>` into a tuple `(T result, string message)` using `UnwrapResult()` (which will never throw):

```C#
var (res, msg) = Digit.Run("a").UnwrapResult();
Console.WriteLine(msg ?? $"Parser succeeded: {res}");
```

In case parsing succeeded the left side of the tuple will hold the parser's return value and the right side will be `null`.

In case parsing failed the left side of the tuple will hold the return value type's `default` value and the right side will hold the detailed parser error message.

Hence the safest way to check if the parser result tuple indicates failure is to check whether the right side is `null`.

With C# 8.0 you can do that quite nicely using a `switch` expression and recursive patterns:

```C#
var response = Digit.Run("a").UnwrapResult() switch {
    (var r, null) => $"Parser succeeded: {r}",
    (_,    var m) => $"Parser failed: {m}"
};
```

`UnwrapWithError()` and `UnwrapWithFailure()` work the same way, but return the `ParserError` or `ParserResult<TResult, Unit>.Failure` instance in the right side of the tuple.

## Using FParsec.CSharp and FParsec together

### Working with FParsec parsers directly

In case you need one of FParsec's more specialized parsers you can easily import their namespace:

```C#
using static FParsec.CharParsers;
```

In the example below we are using `FParsec.CharParsers.many1Chars2()`. As you can see it integrates seemlessly with FParsec.CSharp:

```C#
var first = Letter.Or(CharP('_'));
var rest = Letter.Or(CharP('_')).Or(Digit);
var identifier = many1Chars2(first, rest);
var p = identifier.And(Skip('=')).And(Int);

var r = p.ParseString("my_1st_var=13");

System.Diagnostics.Debug.Assert(r.Result == ("my_1st_var", 13));
```

### Specifying user state

FParsec supports parsing inputs with custom user state, wich is reflected by most of its functions taking a user state type variable. FParsec.CSharp however does not support user state, so you will have to specify `Microsoft.FSharp.Core.Unit` as the user state type when it can not be inferred:

```C#
var p = restOfLine<Unit>(true);
```

Otherwise you won't be able to use the combinators from FParsec.CSharp on it.

### Passing lambdas to FParsec

Some of FParsec's parsers take anonymous functions. But since they expect curried `FSharpFunc`s they won't accept C# lambdas. FParsec.CSharp comes with a little helper to create `FSharpFunc`s from `Func` objects:

```C#
// convert lambda with factory method
var fsfunc1 = FSharpFunc.From<char, bool>(c => c == 'x' || c == 'y');

// convert Func object with extension method
Func<char, bool> func = c => c == '1' || c == '2';
var fsfunc2 = func.ToFSharpFunc();

var p = manySatisfy<Unit>(fsfunc1).And(manySatisfy<Unit>(fsfunc2));

var r = p.ParseString("xyxyyy212221212");
```

## Examples

You can find lots of examples in the [test project](https://github.com/bert2/FParsec.CSharp/tree/master/src/Tests). Below are some parser definitions from there.

### Simple JSON

```C#
FSharpFunc<CharStream<Unit>, Reply<object>> jvalue = null;

var jnull = StringCI("null").Return((object)null);
var jnum = Int.Map(i => (object)i);
var jbool = StringCI("true").Or(StringCI("false"))
    .Map(b => (object)bool.Parse(b));
var quotedString = Between('"', Many(NoneOf("\"")), '"')
    .Map(string.Concat);
var jstring = quotedString.Map(s => (object)s);

var arrItems = Many(Rec(() => jvalue), sep: CharP(',').And(WS));
var jarray = Between(CharP('[').And(WS), arrItems, CharP(']'))
    .Map(elems => (object)new JArray(elems));

var jidentifier = quotedString;
var jprop = jidentifier.And(WS).And(Skip(':')).And(WS).And(Rec(() => jvalue))
    .Map((name, value) => new JProperty(name, value));
var objProps = Many(jprop, sep: CharP(',').And(WS));
var jobject = Between(CharP('{').And(WS), objProps, CharP('}'))
    .Map(props => (object)new JObject(props));

jvalue = Choice(jnum, jbool, jnull, jstring, jarray, jobject).And(WS);

var simpleJsonParser = WS.And(jobject).And(WS).And(EOF).Map(o => (JObject)o);
```

### Simple XML

```C#
var nameStart = Letter.Or(CharP('_'));
var nameChar = Letter.Or(Digit).Or(AnyOf("-_."));
var name = nameStart.And(Many(nameChar))
    .Map((first, rest) => string.Concat(rest.Prepend(first)));

var quotedString = Between('"', Many(NoneOf("\"")), '"')
    .Map(string.Concat);
var attribute = WS1.And(name).And(WS).And(Skip('=')).And(WS).And(quotedString)
    .Map((attrName, attrVal) => new XAttribute(attrName, attrVal));
var attributes = Many(Try(attribute));

FSharpFunc<CharStream<Unit>, Reply<XElement>> element = null;

var emptyElement = Between("<", name.And(attributes).And(WS), "/>")
    .Map((elName, attrs) => new XElement(elName, attrs));

var openingTag = Between('<', name.And(attributes).And(WS), '>');
FSharpFunc<CharStream<Unit>, Reply<string>> closingTag(string tagName) => Between("</", StringP(tagName).And(WS), ">");
var childElements = Many1(Try(WS.And(Rec(() => element)).And(WS)))
    .Map(attrs => (object)attrs);
var text = Many(NoneOf("<"))
    .Map(t => (object)string.Concat(t));
var content = childElements.Or(text);
var parentElement = openingTag.And(content).Map(Flat).And(x => closingTag(x.Item1).Return(x))
    .Map((elName, elAttrs, elContent) => new XElement(elName, elAttrs, elContent));

element = Try(emptyElement).Or(parentElement);

var simpleXmlParser = element.And(WS).And(EOF);
```

### Glob patterns

```C#
var globParser =
    Many(Choice(
        Skip('?').Map(NFA.MakeAnyChar),
        Skip('*').Map(NFA.MakeAnyChar).Map(NFA.MakeZeroOrMore),
        Between('[', AnyChar.And(Skip('-')).And(AnyChar), ']').Map(NFA.MakeCharRange),
        Skip('\\').And(AnyOf(@"?*[]\")).Map(NFA.MakeChar),
        AnyChar.Map(NFA.MakeChar)))
    .And(EOF)
    .Map(NFA.Concat)
    .Map(proto => proto(new Final()));
```

This example contructs a non-deterministic finite automaton (NFA) during parsing and can be used for matching:

```C#
[Fact] public void CanParseAndMatchGlobPattern() => globParser
    .ParseString("The * syntax is easy?").Result
    .Matches("The glob syntax is easy!")
    .ShouldBe(true);
```

### Arithmetic expressions

FParsec.CSharp comes with a builder to construct `FParsec.OperatorPrecedenceParser`s:

```C#
var basicExprParser = new OPPBuilder<int, Unit>()
    .WithOperators(ops => ops
        .AddInfix("+", 1, (x, y) => x + y)
        .AddInfix("*", 2, (x, y) => x * y))
    .WithTerms(Integer)
    .Build()
    .ExpressionParser;

var recursiveExprParser = new OPPBuilder<int, Unit>()
    .WithOperators(ops => ops
        .AddInfix("+", 1, (x, y) => x + y)
        .AddInfix("*", 2, (x, y) => x * y))
    .WithTerms(term => Choice(Integer, Between('(', term, ')')))
    .Build()
    .ExpressionParser;
```

It also supports implicit operators:

```C#
var exprParser =
    WS.And(new OPPBuilder<int, Unit>()
        .WithOperators(ops => ops
            .AddInfix("+", 10, WS, (x, y) => x + y)
            .AddInfix("-", 10, WS, (x, y) => x - y)
            .AddInfix("*", 20, WS, (x, y) => x * y)
            .AddInfix("/", 20, WS, (x, y) => x / y)
            .AddPrefix("-", 20, x => -x)
            .AddInfix("^", 30, Associativity.Right, WS, (x, y) => (int)Math.Pow(x, y))
            .AddPostfix("!", 40, Factorial))
        .WithImplicitOperator(20, (x, y) => x * y)
        .WithTerms(term => Choice(
            Integer.And(WS),
            Between(CharP('(').And(WS), term, CharP(')').And(WS))))
        .Build()
        .ExpressionParser);
```

### Simple regular expressions

Armed with the `OPPBuilder` and the NFA implementation used for the glob parser above we can even build a simple regex parser & matcher:

```C#
var simpleRegexParser =
    Many(new OPPBuilder<NFA.ProtoState, Unit>()
        .WithImplicitOperator(2, NFA.Connect)
        .WithOperators(ops => ops
            .AddPostfix("*", 3, NFA.MakeZeroOrMore)
            .AddPostfix("+", 3, NFA.MakeOneOrMore)
            .AddPostfix("?", 3, NFA.MakeZeroOrOne)
            .AddInfix("|", 1, NFA.MakeAlternation))
        .WithTerms(matchExpr => {
            var group = Between('(', Many(matchExpr), ')');
            var wildcard = Skip('.');
            var charMatch = NoneOf("*+?|()");
            return Choice(
                group.Map(NFA.Concat),
                wildcard.Map(NFA.MakeAnyChar),
                charMatch.Map(NFA.MakeChar));
        })
        .Build()
        .ExpressionParser)
    .And(EOF)
    .Map(NFA.Concat)
    .Map(proto => proto(new Final()));
```

```C#
[Fact] public void CanParseAndMatchRegex() => simpleRegexParser
    .ParseString("The( simple)? .+ syntax is .*more tricky( and (complex|difficult|involved))+.").Result
    .Matches("The simple regex syntax is only a little bit more tricky and complex and involved!")
    .ShouldBe(true);
```

## Hints

### Debugging

When you need to debug into your parser chain, use the `Debug()` combinator on any of your chain's parsers.

It takes two `Action`s:

* `Action<CharStream<Unit>> before`: is run before the parser is applied,
* `Action<CharStream<Unit>, Reply<T>> after`: is run after the parser was applied.

For instance, you can use empty `Action`s in order to place break points:

```C#
var p = Digit.Debug(cs => {}, (cs, r) => {})
        .And(
            Letter.Debug(cs => {}, (cs, r) => {}))
        .Debug(cs => {}, (cs, r) => {});
```

### Aliasing awkward types

In order to simplify the types shown in IntelliSense you can use type aliases:

```C#
using Chars = FParsec.CharStream<Microsoft.FSharp.Core.Unit>;
```

Unfortunately C# does not support type aliases with open generics. Hence if you want to simplify the type of a parser you will have to do it for each of the possible `Reply<T>`s you are using:

```C#
using StringParser = Microsoft.FSharp.Core.FSharpFunc<FParsec.CharStream<Microsoft.FSharp.Core.Unit>, FParsec.Reply<string>>;
using JsonParser = Microsoft.FSharp.Core.FSharpFunc<FParsec.CharStream<Microsoft.FSharp.Core.Unit>, FParsec.Reply<JObject>>;
// ...
```

## Alternatives

FParsec.CSharp does not wrap all of FParsec's features yet. If you need an all-in-one solution then have a look at the following alternatives:

* [Pidgin](https://github.com/benjamin-hodgson/Pidgin)
  * Can also parse binary data.
  * Not as fast as FParsec.
* [Sprache](https://github.com/sprache/Sprache)
  * Is not as optimized as `Pidgin`.

## Where is the FParsec function `x`?

FParsec.CSharp does not mirror all of FParsec's functions exactly. Some are not (yet) implemented and some are just named differently.

Below is a table that maps FParsec's parser functions, combinators, and helper functions to their FParsec.CSharp equivalent.

The type `FSharpFunc<CharStream<Unit>, Reply<T>>` is shortened to `P<T>` for brewity.

| FParsec | FParsec.CSharp |
| :--- | :--- |
| `preturn` | `P<T> Return(T)` |
| `pzero` | `P<T> Zero<T>()` |
| `(>>=)` | `P<T2> P<T1>.And(Func<T1, P<T2>>)` |
| `(>>%)` | `P<T2> P<T1>.Return(T2)` |
| `(>>.)` | `P<T2> P<T1>.AndR(P<T2>)` skips left explicitly, <br>`P<T> P<Unit>.And(P<T>)` skips left implicitly when it returns `Unit` |
| `(.>>)` | `P<T1> P<T1>.AndL(P<T2>)` skips right explicitly, <br>`P<T> P<T>.And(P<Unit>)` skips right implicitly when it returns `Unit` |
| `(.>>.)` | `P<(T1,T2)> P<T1>.And(P<T2>)` if neither side returns `Unit`,<br>`P<(Unit,Unit)> P<Unit>.And_(P<Unit>)` if any side returns `Unit` |
| `(\|>>)` | `P<T2> P<T1>.Map(Func<T1, T2>)` |
| `between` | `P<T2> Between(P<T1>, P<T2>, P<T3>)` (different argument order) |
| `pipe2` | `P<TR> Pipe(P<T1>, P<T2>, Func<T1, T2, TR>)` |
| `pipe3` | `P<TR> Pipe(P<T1>, P<T2>, P<T3>, Func<T1, T2, T3, TR>)` |
| `pipe4` | `P<TR> Pipe(P<T1>, P<T2>, P<T3>, P<T4>, Func<T1, T2, T3, T4, TR>)` |
| `pipe5` | `P<TR> Pipe(P<T1>, P<T2>, P<T3>, P<T4>, P<T5>, Func<T1, T2, T3, T4, T5, TR>)` |
| `(<\|>)` | `P<T> P<T>.Or(P<T>)` |
| `choice` | `P<T> Choice(params P<T>[])` |
| `choiceL` | `P<T> Choice(string, params P<T>[])` |
| `(<\|>%)` | `P<T> P<T>.Or(T)` |
| `opt` | `P<FSharpOption<T>> Opt_(P<T>)`,<br>`P<T> Opt(P<T>)` unwraps the `FSharpOption<T>`,<br>`P<T> Opt(P<T>, T)` unwraps the `FSharpOption<T>` with default value |
| `optional` | `P<Unit> Optional(P<T>)` |
| `attempt` | `P<T> Try(P<T>)` |
| `(>>=?)` | `P<T2> P<T1>.AndTry(Func<T1, P<T2>>)` |
| `(>>?)` | `P<T2> P<T1>.AndRTry(P<T2>)` skips left explicitly, <br>`P<T> P<Unit>.AndTry(P<T>)` skips left implicitly when it returns `Unit` |
| `(.>>?)` | `P<T1> P<T1>.AndLTry(P<T2>)` skips right explicitly, <br>`P<T> P<T>.AndTry(P<Unit>)` skips right implicitly when it returns `Unit` |
| `(.>>.?)` | `P<(T1,T2)> P<T1>.AndTry(P<T2>)` if neither side returns `Unit`,<br>`P<(Unit,Unit)> P<Unit>.AndTry_(P<Unit>)` if any side returns `Unit` |
| `notEmpty` | `P<T> NotEmpty(P<T>)` |
| `followedBy` | `P<Unit> FollowedBy(P<T>)` |
| `followedByL` | `P<Unit> FollowedBy(P<T>, string)` |
| `notFollowedBy` | `P<Unit> NotFollowedBy(P<T>)` |
| `notFollowedByL` | `P<Unit> NotFollowedBy(P<T>, string)` |
| `lookAhead` | `P<T> LookAhead(P<T>)` |
| `(<?>)` | `P<T> P<T>.Label(string)` |
| `(<??>)` | `P<T> P<T>.Label_(string)` |
| `fail` | `P<T> Fail(string)` |
| `failFatally` | `P<T> FailFatally(string)` |
| `tuple2` | `P<(T1,T2)> Tuple(P<T1>, P<T2>)` |
| `tuple3` | `P<(T1,T2,T3)> Tuple(P<T1>, P<T2>, P<T3>)` |
| `tuple4` | `P<(T1,T2,T3,T4)> Tuple(P<T1>, P<T2>, P<T3>, P<T4>)` |
| `tuple5` | `P<(T1,T2,T3,T4,T5)> Tuple(P<T1>, P<T2>, P<T3>, P<T4>, P<T5>)` |
| `parray` | `P<T[]> Array(int, P<T>)` |
| `skipArray` | `P<Unit> SkipArray(int, P<T>)` |
| `many` | `P<FSharpList<T>> Many(P<T>)` |
| `skipMany` | `P<Unit> SkipMany(P<T>)` |
| `many1` | `P<FSharpList<T>> Many1(P<T>)` |
| `skipMany1` | `P<Unit> SkipMany1(P<T>)` |
| `sepBy` | `P<FSharpList<T>> Many(P<T>, P<TSep>)` |
| `skipSepBy` | `P<Unit> SkipMany(P<T>, P<TSep>)` |
| `sepBy1` | `P<FSharpList<T>> Many1(P<T>, P<TSep>)` |
| `skipSepBy1` | `P<Unit> SkipMany1(P<T>, P<TSep>)` |
| `sepEndBy` | `P<FSharpList<T>> Many(P<T>, P<TSep>, canEndWithSep: true)` |
| `skipSepEndBy` | `P<Unit> SkipMany(P<T>, P<TSep>, canEndWithSep: true)` |
| `sepEndBy1` | `P<FSharpList<T>> Many1(P<T>, P<TSep>, canEndWithSep: true)` |
| `skipSepEndBy1` | `P<Unit> SkipMany1(P<T>, P<TSep>, canEndWithSep: true)` |
| `manyTill` | `P<FSharpList<T>> ManyTill(P<T>, P<TEnd>)` |
| `skipManyTill` | `P<Unit> SkipManyTill(P<T>, P<TEnd>)` |
| `many1Till` | `P<FSharpList<T>> Many1Till(P<T>, P<TEnd>)` |
| `skipMany1Till` | `P<Unit> SkipMany1Till(P<T>, P<TEnd>)` |
| `chainl1` | `P<T> ChainL(P<T>, P<Func<T, T, T>>)` |
| `chainl` | `P<T> ChainL(P<T>, P<Func<T, T, T>>, T)` |
| `chainr1` | `P<T> ChainR(P<T>, P<Func<T, T, T>>)` |
| `chainr` | `P<T> ChainR(P<T>, P<Func<T, T, T>>, T)` |
| `runParserOnString` | `ParserResult<T> P<T>.RunOnString(string, string)` |
| `runParserOnSubstring` | `ParserResult<T> P<T>.RunOnString(string, int, int, string)` |
| `runParserOnStream` | `ParserResult<T> P<T>.RunOnStream(Stream, Encoding, string)` |
| `runParserOnFile` | `ParserResult<T> P<T>.RunOnFile(string, Encoding)` |
| `run` | `ParserResult<T> P<T>.Run(string)` |
| `getPosition` | `P<Position> PositionP` |
| `getUserState` | not implemented |
| `setUserState` | not implemented |
| `updateUserState` | not implemented |
| `userStateSatisfies` | not implemented |
| `pchar` | `P<char> CharP(char)` |
| `skipChar` | `P<Unit> Skip(char)` |
| `charReturn` | `P<T> CharP(char, T)` |
| `anyChar` | `P<char> AnyChar` |
| `skipAnyChar` | `P<Unit> SkipAnyChar` |
| `satisfy` | `P<char> CharP(Func<char, bool>)` |
| `skipSatisfy` | `P<Unit> CharP(Func<char, bool>)` |
| `satisfyL` | `P<char> CharP(Func<char, bool>, string)` |
| `skipSatisfyL` | `P<Unit> CharP(Func<char, bool>, string)` |
| `anyOf` | `P<char> AnyOf(IEnumerable<char>)` |
| `skipAnyOf` | `P<Unit> SkipAnyOf(IEnumerable<char>)` |
| `noneOf` | `P<char> NoneOf(IEnumerable<char>)` |
| `skipNoneOf` | `P<Unit> SkipNoneOf(IEnumerable<char>)` |
| `asciiUpper` | `P<char> AsciiUpper` |
| `asciiLower` | `P<char> AsciiLower` |
| `asciiLetter` | `P<char> AsciiLetter` |
| `upper` | `P<char> Upper` |
| `lower` | `P<char> Lower` |
| `letter` | `P<char> Letter` |
| `digit` | `P<char> Digit` |
| `hex` | `P<char> Hex` |
| `octal` | `P<char> Octal` |
| `isAnyOf` | not implemented |
| `isNoneOf` | not implemented |
| `isAsciiUpper` | `bool IsAsciiUpper(char)` |
| `isAsciiLower` | `bool IsAsciiLower(char)` |
| `isAsciiLetter` | `bool IsAsciiLetter(char)` |
| `isUpper` | `bool IsUpper(char)` |
| `isLower` | `bool IsLower(char)` |
| `isLetter` | `bool IsLetter(char)` |
| `isDigit` | `bool IsDigit(char)` |
| `isHex` | `bool IsHex(char)` |
| `isOctal` | `bool IsOctal(char)` |
| `tab` | `P<char> Tab` |
| `newline` | `P<char> Newline` |
| `skipNewline` | `P<Unit> SkipNewline` |
| `newlineReturn` | `P<T> NewlineReturn(T)` |
| `unicodeNewline` | `P<Unit> UnicodeNewline` |
| `skipUnicodeNewline` | `P<Unit> SkipUnicodeNewline` |
| `unicodeNewlineReturn` | `P<T> UnicodeNewlineReturn(T x)` |
| `spaces` | `P<Unit> Spaces` |
| `spaces1` | `P<Unit> Spaces1` |
| `unicodeSpaces` | `P<Unit> UnicodeSpaces` |
| `unicodeSpaces1` | `P<Unit> UnicodeSpaces1` |
| `eof` | `P<Unit> EOF` |
| `pstring` | `P<string> StringP(string)` |
| `skipString` | `P<Unit> Skip(string)` |
| `stringReturn` | `P<T> StringP(string, T)` |
| `pstringCI` | `P<string> StringCI(string)` |
| `skipStringCI` | `P<Unit> SkipCI(string)` |
| `stringCIReturn` | `P<T> StringP(string, T)` |
| `anyString` | `P<string> AnyString(int)` |
| `skipAnyString` | `P<Unit> SkipAnyString(int)` |
| `restOfLine` | `P<string> RestOfLine(bool)` |
| `skipRestOfLine` | `P<Unit> SkipRestOfLine(bool)` |
| `charsTillString` | `Reply<string> CharsTillString(string, int, bool)` |
| `skipCharsTillString` | `Reply<Unit> SkipCharsTillString(string, int, bool)` |
| `charsTillStringCI` | `Reply<string> CharsTillStringCI(string, int, bool)` |
| `skipCharsTillStringCI` | `Reply<Unit> SkipCharsTillStringCI(string, int, bool)` |
| `manySatisfy` | `P<string> ManyChars(Func<char, bool>)` |
| `manySatisfy2` | not yet implemented |
| `skipManySatisfy` | not yet implemented |
| `skipManySatisfy2` | not yet implemented |
| `many1Satisfy` | `P<string> Many1Chars(Func<char, bool>)` |
| `many1Satisfy2` | not yet implemented |
| `skipMany1Satisfy` | not yet implemented |
| `skipMany1Satisfy2` | not yet implemented |
| `many1SatisfyL` | not yet implemented |
| `many1Satisfy2L` | not yet implemented |
| `skipMany1SatisfyL` | not yet implemented |
| `skipMany1Satisfy2L` | not yet implemented |
| `manyMinMaxSatisfy` | `P<string> ManyChars(Func<char, bool>, int, int)` |
| `manyMinMaxSatisfy2` | not yet implemented |
| `skipManyMinMaxSatisfy` | not yet implemented |
| `skipManyMinMaxSatisfy2` | not yet implemented |
| `manyMinMaxSatisfyL` | not yet implemented |
| `manyMinMaxSatisfy2L` | not yet implemented |
| `skipManyMinMaxSatisfyL` | not yet implemented |
| `skipManyMinMaxSatisfy2L` | not yet implemented |
| `regex` | `Reply<string> Regex(string)` |
| `regexL` | not yet implemented |
| `identifier` | not implemented |
| `manyChars` | `P<string> ManyChars(P<char>)` |
| `manyChars2` | not yet implemented |
| `many1Chars` | `P<string> Many1Chars(P<char>)` |
| `many1Chars2` | not yet implemented |
| `manyCharsTill` | `P<string> ManyCharsTill(P<char>, P<T>)` |
| `manyCharsTill2` | not yet implemented |
| `manyCharsTillApply` | not yet implemented |
| `manyCharsTillApply2` | not yet implemented |
| `many1CharsTill` | `P<string> Many1CharsTill(P<char>, P<T>)` |
| `many1CharsTill2` | not yet implemented |
| `many1CharsTillApply` | not yet implemented |
| `manyStrings` | `P<string> ManyStrings(P<string>)` |
| `manyStrings2` | not yet implemented |
| `many1Strings` | `P<string> Many1Strings(P<string>)` |
| `many1Strings2` | not yet implemented |
| `stringsSepBy` | `ManyStrings(P<string>, P<String>)` |
| `stringsSepBy1` | `Many1Strings(P<string>, P<String>)` |
| `skipped` | `P<string> P<Unit>.WithSkipped()` |
| `withSkippedString` | `P<T2> P<T1>.WithSkipped(Func<string, T1, T2>)`,<br>`P<(string,T)> P<T>.WithSkipped()` |
| `numberLiteral` | not implemented |
| `numberLiteralE` | not implemented |
| `pfloat` | `P<double> Float` |
| `pint64` | `P<long> Long` |
| `pint32` | `P<int> Int` |
| `pint16` | `P<short> Short` |
| `pint8` | `P<sbyte> Byte` |
| `puint64` | `P<ulong> ULong` |
| `puint32` | `P<uint> UInt` |
| `puint16` | `P<ushort> UShort` |
| `puint8` | `P<byte> UByte` |
| `notFollowedByEof` | `P<Unit> NotFollowedByEOF` |
| `followedByNewline` | `P<Unit> FollowedByNewline` |
| `notFollowedByNewline` | `P<Unit> NotFollowedByNewline` |
| `followedByString` | `P<Unit> FollowedBy(string)` |
| `followedByStringCI` | `P<Unit> FollowedByCI(foo)` |
| `notFollowedByString` | `P<Unit> NotFollowedBy(string)` |
| `notFollowedByStringCI` | `P<Unit> NotFollowedByCI(string)` |
| `nextCharSatisfies` | `P<Unit> NextCharSatisfies(Func<char, bool>)` |
| `nextCharSatisfiesNot` | `P<Unit> NextCharSatisfiesNot(Func<char, bool>)` |
| `next2CharsSatisfy` | `P<Unit> Next2CharsSatisfy(Func<char, char, bool>)` |
| `next2CharsSatisfyNot` | `P<Unit> Next2CharsSatisfyNot(Func<char, char, bool>)` |
| `previousCharSatisfies` | `P<Unit> PreviousCharSatisfies(Func<char, bool>)` |
| `previousCharSatisfiesNot` | `P<Unit> PreviousCharSatisfiesNot(Func<char, bool>)` |
| `foldCase` | `string FoldCase(string)` |
| `normalizeNewlines` | `string NormalizeNewlines(string)` |
| `floatToHexString` | `string DoubleToHexString(double)` |
| `floatOfHexString` | `double DoubleOfHexString(string)` |
| `float32ToHexString` | `string FloatToHexString(double)` |
| `float32OfHexString` | `double FloatOfHexString(string)` |

## TODO

* Wrap remaining string parsers
* Add more extensions to work with `Reply<T>`?
* Rework examples to use the new parsers and combinators
* Implement small script language example
* Wrap `numberLiteral` and `identifier` parsers?
* Add [source link](https://github.com/dotnet/sourcelink) support?
* Wrap `FSharpFunc<...>` with own type?
  * Implement implicit conversions between the two
  * Overload operators like `+` and `|` to use them as combinators
* Support user state?
