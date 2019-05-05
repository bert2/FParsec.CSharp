# FParsec.CSharp

[![Build status](https://ci.appveyor.com/api/projects/status/282vojx52ole5lww?svg=true)](https://ci.appveyor.com/project/bert2/fparsec-csharp) [![NuGet](https://img.shields.io/nuget/v/FParsec.CSharp.svg)](https://www.nuget.org/packages/FParsec.CSharp)

`FParsec.CSharp` is a C# wrapper for the F# package [FParsec](https://github.com/stephan-tolksdorf/fparsec). `FParsec` is a parser combinator library with which you can implement parsers declaratively and efficiently.

## Why FParsec.CSharp?

While using `FParsec` from C# is entirely possible in theory, it is very awkward in practice. Most of `FParsec`'s elegance is lost in translation due to C#'s inferior type inference and its lack of custom operators.

`FParsec.CSharp` tries to alleviate that by wrapping `FParsec`'s operators as extension functions.

`FParsec.CSharp` does not try to hide any types from `FParsec` or `FSharp.Core`--the wrapper is thin and also avoids name collisions. That way you can always fallback to `FParsec` anytime you need some functionality not (yet) implemented by `FParsec.CSharp`.

Based on the current implementation it should be easy to extend the wrapper yourself. Pull requests are always welcome!

## Examples

You can find lots of examples in the [test project](https://github.com/bert2/FParsec.CSharp/tree/master/src/Tests). Below are some parser definitions from there.

### Simple JSON

This is what a basic JSON parser looks like with `FParsec.CSharp`:

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

Run it like so:

```C#
[Fact] public void CanParseJson() => simpleJsonParser
    .ParseString(@"{
        ""prop1"" : ""val"",
        ""prop2"" : [false, 13, null],
        ""prop3"" : { }
    }")
    .Result
    .ShouldBe(new JObject(
        new JProperty("prop1", "val"),
        new JProperty("prop2", new JArray(false, 13, null)),
        new JProperty("prop3", new JObject())));
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
    .ParseString(@"The * syntax is easy?").Result
    .Matches("The glob syntax is easy!")
    .ShouldBe(true);
```

### Arithmetic expressions

`FParsec.CSharp` also comes with a builder to construct `FParsec.OperatorPrecedenceParser`s:

```C#
var basicExprParser = new OPPBuilder<int, Unit>()
    .WithOperators(ops => ops
        .AddInfix("+", 1, Associativity.Left, (x, y) => x + y)
        .AddInfix("*", 2, Associativity.Left, (x, y) => x * y))
    .WithTerms(Integer)
    .Build()
    .ExpressionParser;

var recursiveExprParser = new OPPBuilder<int, Unit>()
    .WithOperators(ops => ops
        .AddInfix("+", 1, Associativity.Left, (x, y) => x + y)
        .AddInfix("*", 2, Associativity.Left, (x, y) => x * y))
    .WithTerms(term => OneOf(Integer, Between('(', term, ')')))
    .Build()
    .ExpressionParser;

var exprParser =
    WS.And(new OPPBuilder<int, Unit>()
        .WithOperators(ops => ops
            .AddInfix("+", 10, Associativity.Left, WS, (x, y) => x + y)
            .AddInfix("-", 10, Associativity.Left, WS, (x, y) => x - y)
            .AddInfix("*", 20, Associativity.Left, WS, (x, y) => x * y)
            .AddInfix("/", 20, Associativity.Left, WS, (x, y) => x / y)
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

## Hints

In order to simplify the types shown in IntelliSense you can use type aliases:

```C#
using Chars = FParsec.CharStream<Microsoft.FSharp.Core.Unit>;
```

Unfortunately C# does not support type aliases with open generics. Hence if you want to simplify the type of parser you will have to do it for each of the possible `Reply<T>`s you are using:

```C#
using StringParser = Microsoft.FSharp.Core.FSharpFunc<FParsec.CharStream<Microsoft.FSharp.Core.Unit>, FParsec.Reply<string>>;
using JsonParser = Microsoft.FSharp.Core.FSharpFunc<FParsec.CharStream<Microsoft.FSharp.Core.Unit>, FParsec.Reply<JObject>>;
// ...
```

## Alternatives

`FParsec.CSharp` does not wrap all of `FParsec`'s features yet. If you need an all-in-one solution then have a look at the following alternatives:

* [Pidgin](https://github.com/benjamin-hodgson/Pidgin)
  * Can also parse binary data.
  * Not as fast as `FParsec`.
* [Sprache](https://github.com/sprache/Sprache)
  * Is not as optimized as `Pidgin`.
