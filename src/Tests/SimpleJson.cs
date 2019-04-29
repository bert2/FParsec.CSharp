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

            var jnull = StringCI("null").Return((object)null);

            var jnum = Int.Map(i => (object)i);

            var jbool = StringCI("true").Or(StringCI("false"))
                .Map(b => (object)bool.Parse(b));

            var quotedString = Skip('"').And(Many(NoneOf("\""))).And(Skip('"'))
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

        #endregion Tests
    }
}
