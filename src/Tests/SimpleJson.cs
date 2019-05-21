namespace Tests {
    using FParsec;
    using FParsec.CSharp;
    using Microsoft.FSharp.Core;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using static FParsec.CSharp.CharParsersCS;
    using static FParsec.CSharp.PrimitivesCS;

    public class SimpleJson {
        #region Parser definition

        private static readonly FSharpFunc<CharStream<Unit>, Reply<JObject>> SimpleJsonParser;

        static SimpleJson() {
            FSharpFunc<CharStream<Unit>, Reply<object>> jvalue = null;

            var jnull = StringCI("null").Return((object)null).Lbl("null");

            var jnum = Int.Map(i => (object)i).Lbl("integer");

            var jbool = StringCI("true").Or(StringCI("false"))
                .Map(b => (object)bool.Parse(b))
                .Lbl("bool");

            var quotedString = Between('"', Many(NoneOf("\"")), '"')
                .Map(string.Concat);

            var jstring = quotedString.Map(s => (object)s).Lbl("string");

            var arrItems = Many(Rec(() => jvalue), sep: CharP(',').And(WS));
            var jarray = Between(CharP('[').And(WS), arrItems, CharP(']'))
                .Map(elems => (object)new JArray(elems))
                .Lbl("array");

            var jidentifier = quotedString.Lbl("identifier");
            var jprop = jidentifier.And(WS).And(Skip(':')).And(WS).And(Rec(() => jvalue))
                .Map((name, value) => new JProperty(name, value));
            var objProps = Many(jprop, sep: CharP(',').And(WS));
            var jobject = Between(CharP('{').And(WS), objProps, CharP('}'))
                .Map(props => (object)new JObject(props))
                .Lbl("object");

            jvalue = Choice(jnum, jbool, jnull, jstring, jarray, jobject).And(WS);

            SimpleJsonParser = WS.And(jobject).And(WS).And(EOF).Map(o => (JObject)o);
        }

        #endregion Parser definition

        #region Tests

        [Fact]
        public void EmptyObject() =>
            SimpleJsonParser
            .ParseString("{}")
            .ShouldBe(new JObject());

        [Fact]
        public void BoolProperty() =>
            SimpleJsonParser
            .ParseString("{\"bool\":tRuE}")
            .ShouldBe(new JObject(new JProperty("bool", true)));

        [Fact]
        public void StringProperty() =>
            SimpleJsonParser
            .ParseString("{\"str\":\"value\"}")
            .ShouldBe(new JObject(new JProperty("str", "value")));

        [Fact]
        public void NullProperty() =>
            SimpleJsonParser
            .ParseString("{\"nothing\":null}")
            .ShouldBe(new JObject(new JProperty("nothing", null)));

        [Fact]
        public void ArrayProperty() =>
            SimpleJsonParser
            .ParseString("{\"arr\":[\"test\",2,null,false,{}]}")
            .ShouldBe(
                new JObject(
                    new JProperty("arr", new JArray(
                        "test",
                        2,
                        null,
                        false,
                        new JObject()))));

        [Fact]
        public void ObjectProperty() =>
            SimpleJsonParser
            .ParseString("{\"obj\":{\"x\":1}}")
            .ShouldBe(
                new JObject(
                    new JProperty("obj", new JObject(
                        new JProperty("x", 1)))));

        [Fact]
        public void MultipleProperties() =>
            SimpleJsonParser
            .ParseString("{\"prop1\":\"val1\",\"prop2\":false,\"prop3\":null,\"prop4\":[1]}")
            .ShouldBe(
                new JObject(
                    new JProperty("prop1", "val1"),
                    new JProperty("prop2", false),
                    new JProperty("prop3", null),
                    new JProperty("prop4", new[] { 1 })));

        [Fact]
        public void WithWithspace() =>
            SimpleJsonParser
            .ParseString(@"
                {
                    ""my-property"" : ""my value"",
                    ""prop1""       : 1           ,
                    ""prop2""       : [
                                          false   ,
                                          true    ,
                                          null
                                      ]           ,
                    ""prop3""       : { }
                }                                 ")
            .ShouldBe(
                new JObject(
                    new JProperty("my-property", "my value"),
                    new JProperty("prop1", 1),
                    new JProperty("prop2", new JArray(false, true, null)),
                    new JProperty("prop3", new JObject())));

        [Fact]
        public void ParsesWholeInput() =>
            SimpleJsonParser
            .ParseString("{} this should cause an error")
            .ShouldBe<ErrorMessage.Expected>("end of input");

        [Fact]
        public void HelpfulErrorWhenParsingProperty() =>
            SimpleJsonParser
            .ParseString("{ x }")
            .ShouldBeErrors(
                new ErrorMessage.Expected("identifier"),
                new ErrorMessage.ExpectedString("}"));

        [Fact]
        public void HelpfulErrorWhenParsingValue() =>
            SimpleJsonParser
            .ParseString("{ \"prop\": x }")
            .ShouldBeErrors(
                new ErrorMessage.Expected("null"),
                new ErrorMessage.Expected("integer"),
                new ErrorMessage.Expected("string"),
                new ErrorMessage.Expected("bool"),
                new ErrorMessage.Expected("array"),
                new ErrorMessage.Expected("object"));

        #endregion Tests
    }
}
