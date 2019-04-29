# FParsec.CSharp

[![Build status](https://ci.appveyor.com/api/projects/status/282vojx52ole5lww?svg=true)](https://ci.appveyor.com/project/bert2/fparsec-csharp) [![NuGet](https://img.shields.io/nuget/v/FParsec.CSharp.svg)](https://www.nuget.org/packages/FParsec.CSharp)

This is a C# wrapper for the parser combinator library [FParsec](https://github.com/stephan-tolksdorf/fparsec) which is written in optimized F#.

## Why Parser Combinators?

Easy: because they are the most elegant way to define parsers.

Check out the [tests](https://github.com/bert2/FParsec.CSharp/tree/master/src/Tests) and see for yourself.

## Why FParsec.CSharp?

While using `FParsec` from C# is entirely possible in theory, it is very awkward in practice. Most of `FParsec`'s elegance is lost in translation due to C#'s inferior type inference and its lack of custom operators.

`FParsec.CSharp` tries to alleviate that by wrapping `FParsec`'s operators as extension functions.

`FParsec.CSharp` does not try to hide any types from `FParsec` or `FSharp.Core`--the wrapper is thin and also avoids name collisions. That way you can always fallback to `FParsec` anytime you need some functionality not (yet) implemented by `FParsec.CSharp`.

Based on the current implementation it should be easy to extend the wrapper yourself. Pull requests are always welcome!

## Example

This is what a basic JSON parser looks like with `FParsec.CSharp`:

```C#
FSharpFunc<CharStream<Unit>, Reply<object>> jvalue = null;

var jnull = StringCI("null").Return((object)null);
var jnum = Int.Map(i => (object)i);
var jbool = StringCI("true").Or(StringCI("false"))
    .Map(b => (object)bool.Parse(b));
var quotedString = Skip('"').And(Many(CharP(c => c != '"'))).And(Skip('"'))
    .Map(string.Concat);
var jstring = quotedString.Map(s => (object)s);
var jarray = Skip('[').And(WS).And(Many(Rec(() => jvalue), sep: CharP(',').And(WS))).And(Skip(']'))
    .Map(elems => (object)new JArray(elems));
var jidentifier = quotedString;
var jprop = jidentifier.And(WS).And(Skip(':')).And(WS).And(Rec(() => jvalue))
    .Map(x => new JProperty(x.Item1, x.Item2));
var jobject = Skip('{').And(WS).And(Many(jprop, sep: CharP(',').And(WS))).And(Skip('}'))
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
    .ShouldBe(new JObject(
        new JProperty("prop1", "val"),
        new JProperty("prop2", new JArray(false, 13, null)),
        new JProperty("prop3", new JObject())));
```

You can find more examples in the [test project](https://github.com/bert2/FParsec.CSharp/tree/master/src/Tests).

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
