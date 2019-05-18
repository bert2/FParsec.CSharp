# FParsec.CSharp

[![Build status](https://ci.appveyor.com/api/projects/status/282vojx52ole5lww?svg=true)](https://ci.appveyor.com/project/bert2/fparsec-csharp) [![NuGet](https://img.shields.io/nuget/v/FParsec.CSharp.svg)](https://www.nuget.org/packages/FParsec.CSharp)

FParsec.CSharp is a C# wrapper for the F# package [FParsec](https://github.com/stephan-tolksdorf/fparsec). FParsec is a parser combinator library with which you can implement parsers declaratively.

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

### Using FParsec.CSharp and FParsec together

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

FParsec supports parsing inputs with custom user state, wich is reflected by most of its functions taking a user state type variable. FParsec.CSharp however does not support user state, so you will have to specify `Microsoft.FSharp.Core.Unit` as the user state type when it can not be inferred:

```C#
var p = restOfLine<Unit>(true);
```

Otherwise you won't be able to use the combinators from FParsec.CSharp on it.

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

jvalue = OneOf(jnum, jbool, jnull, jstring, jarray, jobject).And(WS);

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
    Many(OneOf(
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
    .WithTerms(term => OneOf(Integer, Between('(', term, ')')))
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
        .WithTerms(term => OneOf(
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
            return OneOf(
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
  * Not as fast as `FParsec`.
* [Sprache](https://github.com/sprache/Sprache)
  * Is not as optimized as `Pidgin`.
