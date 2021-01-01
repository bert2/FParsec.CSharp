# FParsec.CSharp

[![build](https://img.shields.io/appveyor/build/bert2/fparsec-csharp/master?logo=appveyor)](https://ci.appveyor.com/project/bert2/fparsec-csharp/branch/master) [![tests](https://img.shields.io/appveyor/tests/bert2/fparsec-csharp/master?compact_message&logo=appveyor)](https://ci.appveyor.com/project/bert2/fparsec-csharp/branch/master) [![coverage](https://img.shields.io/codecov/c/github/bert2/FParsec.CSharp/master?logo=codecov)](https://codecov.io/gh/bert2/FParsec.CSharp) [![CodeFactor](https://www.codefactor.io/repository/github/bert2/fparsec.csharp/badge)](https://www.codefactor.io/repository/github/bert2/fparsec.csharp) ![last commit](https://img.shields.io/github/last-commit/bert2/FParsec.CSharp/master?logo=github) [![nuget package](https://img.shields.io/nuget/v/FParsec.CSharp.svg?logo=nuget)](https://www.nuget.org/packages/FParsec.CSharp) [![nuget downloads](https://img.shields.io/nuget/dt/FParsec.CSharp?color=blue&logo=nuget)](https://www.nuget.org/packages/FParsec.CSharp)

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
  * [Deconstructing parser results (C# 8.0)](#deconstructing-parser-results-c-80)
- [Working with user state](#working-with-user-state)
- [Using FParsec.CSharp and FParsec together](#using-fparseccsharp-and-fparsec-together)
  * [Working with FParsec parsers directly](#working-with-fparsec-parsers-directly)
  * [Passing lambdas to FParsec](#passing-lambdas-to-fparsec)
- [Examples](#examples)
  * [Simple JSON](#simple-json)
  * [Simple XML](#simple-xml)
  * [Glob patterns](#glob-patterns)
  * [Arithmetic expressions](#arithmetic-expressions)
  * [Simple regular expressions](#simple-regular-expressions)
  * [Simple script language](#simple-script-language)
- [Hints](#hints)
  * [Debugging](#debugging)
  * [Aliasing awkward types](#aliasing-awkward-types)
- [Where is the FParsec function `x`?](#where-is-the-fparsec-function-x)
- [Credits](#credits)
- [TODO](#todo)

## Why FParsec.CSharp?

While using FParsec from C# is entirely possible in theory, it is very awkward in practice. Most of FParsec's elegance is lost in translation due to C#'s inferior type inference and its lack of custom operators.

FParsec.CSharp tries to alleviate that by wrapping FParsec's operators as extension functions.

FParsec.CSharp does not try to hide any types from `FParsec` or `FSharp.Core`--the wrapper is thin and also avoids name collisions. That way you can always fallback to FParsec anytime you need some functionality not implemented by FParsec.CSharp.

Based on the current implementation it should be easy to extend the wrapper yourself if needed. Pull requests are always welcome!

Note that the documentation assumes prior knowledge on FParsec or other parser combinator libraries.

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

### Deconstructing parser results (C# 8.0)

FParsec.CSharp extends the types involved with parser results with deconstructors so you can make use of C# 8.0's recursive patterns inside `switch` statements/expressions:

```C#
var response = Digit.Run("1") switch {
    ParserResult<char, Unit>.Success(var c, _, _) => $"Parsed '{c}'.",
    ParserResult<char, Unit>.Failure(_, (_, (_, _, var col, _)), _) => $"Some error at column {col}."
};

```

```C#
var response = Digit.ParseString("a") switch {
    (ReplyStatus.Ok, var c, _) => $"Parsed '{c}'.",
    (ReplyStatus.Error, _, (ErrorMessage.Expected err, _)) => $"Expected a {err.Label}.",
    _ => "oof."
};
```

You will need to import some of FParsec's namespaces for this to work:

```
using FParsec; // contains `ReplyStatus` and `ErrorMessage`
using static FParsec.CharParsers; // contains `ParserResult`
```
## Working with user state

FParsec.CSharp, like FParsec, supports parsing with user state. This is reflected by the type parameter `U` in the signatures:

```C#
public FSharpFunc<CharStream<U>, Reply<(T1, T2)>> And<U, T1, T2>(
    this FSharpFunc<CharStream<U>, Reply<T1>> p1,
    FSharpFunc<CharStream<U>, Reply<T2>> p2);
```

If a combinator/parser supports user state then it will always have `U` as the first type parameter.

For the combinators from `FParsec.CSharp.PrimitivesCS` this will be transparent most of the time, because C# is able to infer the user state type of the combinator from the user state type of the parser argument(s).

Unfortunately C# is not able to infer the user state type retrospectively from later bindings and hence forces you to explicitly specify the user state type on parsers that have no parser parameters. In the case of the predefined parsers from `FParsec.CSharp.CharParsersCS` (which usually don't take other parsers as arguments) this restriction would be cause for much annoyance.

That's why all parsers/combinators that have no parser parameters have two variants: one assuming a user state type of `Unit` and another one expecting the explicit type argument `U`. The names of the latter ones are always suffixed with the letter "U":

```
var parserWithoutUserState = Digit.And(Letter);

var parserWithUserState = DigitU<int>().And(LetterU<int>());
```

Below are example test cases to demonstrate working with user state:

```C#
[Fact] public void SimpleSet() {
    switch (SetUserState(12).RunOnString("", 0)) {
        case ParserResult<Unit, int>.Success(_, 12, _):
            break;
        default:
            throw new Exception();
    }
}

[Fact] public void CountParsedLetters() {
    var countedLetter = LetterU<int>().And(UpdateUserState<int>(cnt => cnt + 1));

    SkipMany(countedLetter).And(GetUserState<int>())
    .RunOnString("abcd", 0).GetResult()
    .ShouldBe(4);
}

[Fact] public void CheckNestingLevel() {
    FSharpFunc<CharStream<int>, Reply<Unit>> expr = null;
    var parens = Between('(', Rec(() => expr), ')');
    var empty = ReturnU<int, Unit>(null);
    expr = Choice(
        parens.AndR(UpdateUserState<int>(depth => depth + 1)),
        empty);

    expr.AndR(UserStateSatisfies<int>(depth => depth < 3))
    .RunOnString("((()))", 0)
    .IsFailure
    .ShouldBeTrue();
}
```

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
FSharpFunc<CharStream<Unit>, Reply<JToken>> jvalue = null;

var jnull = StringCI("null", (JToken)null).Lbl("null");

var jnum = Float.Map(i => (JToken)i).Lbl("number");

var jbool = StringCI("true").Or(StringCI("false"))
    .Map(b => (JToken)bool.Parse(b))
    .Lbl("bool");

var quotedString = Between('"', ManyChars(NoneOf("\"")), '"');

var jstring = quotedString.Map(s => (JToken)s).Lbl("string");

var arrItems = Many(Rec(() => jvalue), sep: CharP(',').And(WS));
var jarray = Between(CharP('[').And(WS), arrItems, CharP(']'))
    .Map(elems => (JToken)new JArray(elems))
    .Lbl("array");

var jidentifier = quotedString.Lbl("identifier");
var jprop = jidentifier.And(WS).And(Skip(':')).And(WS).And(Rec(() => jvalue))
    .Map((name, value) => new JProperty(name, value));
var objProps = Many(jprop, sep: CharP(',').And(WS));
var jobject = Between(CharP('{').And(WS), objProps, CharP('}'))
    .Map(props => (JToken)new JObject(props))
    .Lbl("object");

jvalue = Choice(jnum, jbool, jnull, jstring, jarray, jobject).And(WS);

var simpleJsonParser = WS.And(jobject).And(WS).And(EOF).Map(o => (JObject)o);
```

### Simple XML

```C#
var nameStart = Choice(Letter, CharP('_'));
var nameChar = Choice(Letter, Digit, AnyOf("-_."));
var name = Many1Chars(nameStart, nameChar).And(WS);

var quotedString = Between('"', ManyChars(NoneOf("\"")), '"');
var attribute = name.And(Skip('=')).And(WS).And(quotedString).And(WS)
    .Lbl_("attribute")
    .Map((attrName, attrVal) => new XAttribute(attrName, attrVal));
var attributes = Many(attribute);

FSharpFunc<CharStream<Unit>,Reply<XElement>> element = null;

var elementStart = Skip('<').AndTry(name.Lbl("tag name")).And(attributes);

FSharpFunc<CharStream<Unit>, Reply<string>> closingTag(string tagName) => Between("</", StringP(tagName).And(WS), ">")
    .Lbl_($"closing tag '</{tagName}>'");

FSharpFunc<CharStream<Unit>, Reply<object>> textContent(string leadingWS) => NotEmpty(ManyChars(NoneOf("<"))
    .Map(text => leadingWS + text)
    .Map(x => (object)x)
    .Lbl_("text content"));

var childElement = Rec(() => element).Map(x => (object)x).Lbl_("child element");

object EmptyContentToEmptyString(FSharpList<object> xs) => xs.IsEmpty ? (object)"" : xs;

var elementContent = Many(WS.WithSkipped().AndTry(ws => Choice(textContent(ws), childElement)))
    .Map(EmptyContentToEmptyString);

FSharpFunc<CharStream<Unit>,Reply<XElement>> elementEnd(string elName, FSharpList<XAttribute> elAttrs) =>
    Choice(
        Skip("/>").Return((object)null),
        Skip(">").And(elementContent).And(WS).AndL(closingTag(elName)))
    .Map(elContent => new XElement(elName, elContent, elAttrs));

element = elementStart.And(elementEnd);

var simpleXmlParser = WS.And(element).And(WS).And(EOF);
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
var basicExprParser = new OPPBuilder<Unit, int, Unit>()
    .WithOperators(ops => ops
        .AddInfix("+", 1, (x, y) => x + y)
        .AddInfix("*", 2, (x, y) => x * y))
    .WithTerms(Natural)
    .Build()
    .ExpressionParser;

var recursiveExprParser = new OPPBuilder<Unit, int, Unit>()
    .WithOperators(ops => ops
        .AddInfix("+", 1, (x, y) => x + y)
        .AddInfix("*", 2, (x, y) => x * y))
    .WithTerms(term => Choice(Natural, Between('(', term, ')')))
    .Build()
    .ExpressionParser;
```

It also supports implicit operators:

```C#
var exprParser =
    WS.And(new OPPBuilder<Unit, int, Unit>()
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
            Natural.And(WS),
            Between(CharP('(').And(WS), term, CharP(')').And(WS))))
        .Build()
        .ExpressionParser);
```

### Simple regular expressions

Armed with the `OPPBuilder` and the NFA implementation used for the glob parser above we can even build a simple regex parser & matcher:

```C#
var simpleRegexParser =
    Many(new OPPBuilder<Unit, NFA.ProtoState, Unit>()
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

### Simple script language

This example implements a simple functional script language. It only knows one type (`int`) and is super inefficient, but it has lots of functional fu (e.g. lazy evaluation, partial application, lambdas, higher order functions, and function composition).

```C#
var number = Natural.Lbl("number");

static StringParser notReserved(string id) => id == "let" || id == "in" || id == "match" ? Zero<string>() : Return(id);
var identifier1 = Choice(Letter, CharP('_'));
var identifierRest = Choice(Letter, CharP('_'), CharP('\''), Digit);
var identifier = Purify(Many1Chars(identifier1, identifierRest)).AndTry(notReserved).Lbl("identifier");

var parameters = Many(identifier, sep: WS1, canEndWithSep: true).Lbl("parameter list");

ScriptParser? expression = null;

var letBinding =
    Skip("let").AndR(WS1)
    .And(identifier).And(WS)
    .And(parameters)
    .And(Skip('=')).And(WS)
    .And(Rec(() => expression).Lbl("'let' definition expression"))
    .And(Skip("in")).And(WS1)
    .And(Rec(() => expression).Lbl("'let' body expression"))
    .Map(Flat)
    .Lbl("'let' binding");

var lambda =
    Skip('\\')
    .And(parameters)
    .And(Skip("->")).And(WS)
    .And(Rec(() => expression).Lbl("lambda body"))
    .Lbl("lambda");

var defaultCase = Skip('_').AndRTry(NotFollowedBy(identifierRest)).AndR(WS).Return(ScriptB.AlwaysMatches);
var caseValueExpr = Rec(() => expression).Map(ScriptB.Matches);
var caseExpr =
    Skip('|').AndR(WS)
    .And(defaultCase.Or(caseValueExpr).Lbl("case value expression"))
    .And(Skip("=>")).And(WS)
    .And(Rec(() => expression).Lbl("case result expression"))
    .Lbl("match case");
var matchExpr =
    Skip("match").AndR(WS1)
    .And(Rec(() => expression).Lbl("match value expression"))
    .And(Many1(caseExpr))
    .Lbl("'match' expression");

expression = new OPPBuilder<Unit, Script, Unit>()
    .WithOperators(ops => ops
        .AddInfix("+", 10, WS, ScriptB.Lift2((x, y) => x + y))
        .AddInfix("-", 10, WS, ScriptB.Lift2((x, y) => x - y))
        .AddInfix("*", 20, WS, ScriptB.Lift2((x, y) => x * y))
        .AddInfix("/", 20, WS, ScriptB.Lift2((x, y) => x / y))
        .AddInfix("%", 20, WS, ScriptB.Lift2((x, y) => x % y))
        .AddPrefix("-", 20, ScriptB.Lift(x => -x))
        .AddInfix(".", 30, Associativity.Right, WS, ScriptB.Compose))
    .WithImplicitOperator(50, ScriptB.Apply)
    .WithTerms(term =>
        Choice(
            letBinding.Map(ScriptB.BindVar),
            matchExpr.Map(ScriptB.Match),
            Between(CharP('(').And(WS), term, CharP(')').And(WS)),
            number.And(WS).Map(ScriptB.Return),
            identifier.And(WS).Map(ScriptB.Resolve),
            lambda.Map(ScriptB.Lambda))
        .Lbl("expression"))
    .Build()
    .ExpressionParser;

var scriptParser = WS.And(expression).And(EOF);
```

This parser builds a function that can be invoked (with an empty arguments list and an empty "runtime environment") to execute the script:

```C#
[Fact] public void FibonacciNumber() => scriptParser
    .Run(@"
        let fib n = match n
            | 0 => 0
            | 1 => 1
            | _ => fib (n-1) + fib (n-2)
        in fib 7")
    .GetResult()
    .Invoke(FSharpList<Script>.Empty, new Dictionary<string, Script>())
    .ShouldBe(13);
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

Also make sure to place your import `usings` _outside_, and your alias `using`s _inside_ your namespace declaration. This will greatly reduce namespace noise in IntelliSense and simplify your alias definitions:

```C#
using System.Xml.Linq;
using FParsec;
using Microsoft.FSharp.Core;

namespace Tests {
    using XElParser = FSharpFunc<CharStream<Unit>, Reply<XElement>>;
    // ...
```

## Where is the FParsec function `x`?

FParsec.CSharp does not mirror all of FParsec's functions exactly. A few are not wrapped and some are just named differently.

Below is a table that maps FParsec's parser functions, combinators, and helper functions to their FParsec.CSharp equivalent.

The type `FSharpFunc<CharStream<U>, Reply<T>>` is shortened to `P<T>` for brewity.

Keep in mind that many predefined parsers and some of the combinators have a variant that supports parsing with user state. Those variants always have a `U` suffix in their name and are not listed in this table.

| FParsec | FParsec.CSharp |
| :--- | :--- |
| `preturn` | `P<T> Return(T)` |
| `pzero` | `P<T> Zero<T>()` |
| `(>>=)` | `P<TR> P<T1>.And(Func<T1, P<TR>>)`,<br>`P<TR> P<Unit>.And(Func<P<TR>>)` if left side returns `Unit`,<br>`P<TR> P<(T1,T2)>.And(Func<T1, T2, P<TR>>)` deconstructs left tuple result,<br>`P<TR> P<(T1,T2,T3)>.And(Func<T1, T2, T3, P<TR>>)` deconstructs left 3-tuple result |
| `(>>%)` | `P<T2> P<T1>.Return(T2)` |
| `(>>.)` | `P<T2> P<T1>.AndR(P<T2>)` skips left explicitly, <br>`P<T> P<Unit>.And(P<T>)` skips left implicitly when it returns `Unit` |
| `(.>>)` | `P<T1> P<T1>.AndL(P<T2>)` skips right explicitly, <br>`P<T> P<T>.And(P<Unit>)` skips right implicitly when it returns `Unit` |
| `(.>>.)` | `P<(T1,T2)> P<T1>.And(P<T2>)` if neither side returns `Unit`,<br>`P<(Unit,Unit)> P<Unit>.And_(P<Unit>)` if any side returns `Unit` |
| `(\|>>)` | `P<TR> P<T1>.Map(Func<T1, TR>)`,<br>`P<TR> P<Unit>.Map(Func<TR>)` if left side returns `Unit`,<br>`P<TR> P<(T1,T2)>.Map(Func<T1, T2, TR>)` deconstructs left tuple result,<br>`P<TR> P<(T1,T2,T3)>.Map(Func<T1, T2, T3, TR>)` deconstructs left 3-tuple result |
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
| `getUserState` | `P<U> GetUserState<U>()` |
| `setUserState` | `P<Unit> SetUserState<U>(U)` |
| `updateUserState` | `P<Unit> UpdateUserState<U>(Func<U, U>)` |
| `userStateSatisfies` | `P<Unit> UserStateSatisfies<U>(Func<U, bool>)` |
| `pchar` | `P<char> CharP(char)` |
| `skipChar` | `P<Unit> Skip(char)` |
| `charReturn` | `P<T> CharP(char, T)` |
| `anyChar` | `P<char> AnyChar` |
| `skipAnyChar` | `P<Unit> SkipAnyChar` |
| `satisfy` | `P<char> CharP(Func<char, bool>)` |
| `skipSatisfy` | `P<Unit> Skip(Func<char, bool>)` |
| `satisfyL` | `P<char> CharP(Func<char, bool>, string)` |
| `skipSatisfyL` | `P<Unit> Skip(Func<char, bool>, string)` |
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
| `manySatisfy2` | `P<string> ManyChars(Func<char, bool>, Func<char, bool>)` |
| `skipManySatisfy` | `P<Unit> SkipManyChars(Func<char, bool>)` |
| `skipManySatisfy2` | `P<Unit> SkipManyChars(Func<char, bool>, Func<char, bool>)` |
| `many1Satisfy` | `P<string> Many1Chars(Func<char, bool>)` |
| `many1Satisfy2` | `P<string> Many1Chars(Func<char, bool>, Func<char, bool>)` |
| `skipMany1Satisfy` | `P<Unit> SkipMany1Chars(Func<char, bool>)` |
| `skipMany1Satisfy2` | `P<Unit> SkipMany1Chars(Func<char, bool>, Func<char, bool>)` |
| `many1SatisfyL` | `P<string> Many1Chars(Func<char, bool>, string)` |
| `many1Satisfy2L` | `P<string> Many1Chars(Func<char, bool>, Func<char, bool>, string)` |
| `skipMany1SatisfyL` | `P<Unit> SkipMany1Chars(Func<char, bool>, string)` |
| `skipMany1Satisfy2L` | `P<Unit> SkipMany1Chars(Func<char, bool>, Func<char, bool>, string)` |
| `manyMinMaxSatisfy` | `P<string> ManyChars(Func<char, bool>, int, int)` |
| `manyMinMaxSatisfy2` | `P<string> ManyChars(Func<char, bool>, Func<char, bool>, int, int)` |
| `skipManyMinMaxSatisfy` | `P<Unit> SkipManyChars(Func<char, bool>, int, int)` |
| `skipManyMinMaxSatisfy2` | `P<Unit> SkipManyChars(Func<char, bool>, Func<char, bool>, int, int)` |
| `manyMinMaxSatisfyL` | `P<string> ManyChars(Func<char, bool>, int, int, string)` |
| `manyMinMaxSatisfy2L` | `P<string> ManyChars(Func<char, bool>, Func<char, bool>, int, int, string)` |
| `skipManyMinMaxSatisfyL` | `P<Unit> SkipManyChars(Func<char, bool>, int, int, string)` |
| `skipManyMinMaxSatisfy2L` | `P<Unit> SkipManyChars(Func<char, bool>, Func<char, bool>, int, int, string)` |
| `regex` | `Reply<string> Regex(string)` |
| `regexL` | `Reply<string> Regex(string, string)` |
| `identifier` | not implemented |
| `manyChars` | `P<string> ManyChars(P<char>)` |
| `manyChars2` | `P<string> ManyChars(P<char>, P<char>)` |
| `many1Chars` | `P<string> Many1Chars(P<char>)` |
| `many1Chars2` | `P<string> Many1Chars(P<char>, P<char>)` |
| `manyCharsTill` | `P<string> ManyCharsTill(P<char>, P<TEnd>)` |
| `manyCharsTill2` | `P<string> ManyCharsTill(P<char>, P<char>, P<TEnd>)` |
| `manyCharsTillApply` | `P<T> ManyCharsTill(P<char>, P<TEnd>, Func<string, TEnd, T>)` |
| `manyCharsTillApply2` | `P<T> ManyCharsTill(P<char>, P<char>, P<TEnd>, Func<string, TEnd, T>)` |
| `many1CharsTill` | `P<string> Many1CharsTill(P<char>, P<TEnd>)` |
| `many1CharsTill2` | `P<string> Many1CharsTill(P<char>, P<char>, P<TEnd>)` |
| `many1CharsTillApply` | `P<T> Many1CharsTill(P<char>, P<TEnd>, Func<string, TEnd, T>)` |
| `many1CharsTillApply2` | `P<T> Many1CharsTill(P<char>, P<char>, P<TEnd>, Func<string, TEnd, T>)` |
| `manyStrings` | `P<string> ManyStrings(P<string>)` |
| `manyStrings2` | not implemented |
| `many1Strings` | `P<string> Many1Strings(P<string>)` |
| `many1Strings2` | not implemented |
| `stringsSepBy` | `ManyStrings(P<string>, P<String>)` |
| `stringsSepBy1` | `Many1Strings(P<string>, P<String>)` |
| `skipped` | `P<string> P<Unit>.WithSkipped()` |
| `withSkippedString` | `P<T2> P<T1>.WithSkipped(Func<string, T1, T2>)`,<br>`P<(string,T)> P<T>.WithSkipped()` |
| `numberLiteral` | `P<NumberLiteral> NumberLiteral(NumberLiteralOptions, string)` |
| `numberLiteralE` | `Reply<NumberLiteral> NumberLiteralE(NumberLiteralOptions, ErrorMessageList, CharStream<Unit>)` |
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

## Credits

This library is based on the following works:

* Obviously [FParsec](https://github.com/stephan-tolksdorf/fparsec), because FParsec.CSharp only _wraps_ FParsec and barely implements any logic on its own. FParsec is also where I stole most of the XML documentation from.
* [Pidgin](https://github.com/benjamin-hodgson/Pidgin) gave me the whole idea of thinking about a parser combinator API in C#. However, I'm not smart enough to build my own implementation and just wrapped FParsec instead.
* The OPP's implicit operator implementation was stolen from [StackOverflow](https://stackoverflow.com/questions/29322892).
* The idea for the parser `Purify()` was also stolen from [StackOverflow](https://stackoverflow.com/a/56364809/1025555).
* The NFA implementation for the glob/regex parser example was inspired by [Russ Cox' fantastic article on efficient regex matching](https://swtch.com/~rsc/regexp/regexp1.html).

## TODO

* Wrap `identifier` parsers?
* Add [source link](https://github.com/dotnet/sourcelink) support?
