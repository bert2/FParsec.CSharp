namespace Tests {
    using System.Linq;
    using System.Xml.Linq;
    using FParsec;
    using FParsec.CSharp;
    using Xunit;
    using static FParsec.CSharp.CharParsersCS;
    using static FParsec.CSharp.PrimitivesCS;
    using StringParser = Microsoft.FSharp.Core.FSharpFunc<FParsec.CharStream<Microsoft.FSharp.Core.Unit>, FParsec.Reply<string>>;
    using XElParser = Microsoft.FSharp.Core.FSharpFunc<FParsec.CharStream<Microsoft.FSharp.Core.Unit>, FParsec.Reply<System.Xml.Linq.XElement>>;

    public class SimpleXml {
        #region Parser definition

        private static readonly XElParser SimpleXmlParser;

        static SimpleXml() {
            var nameStart = Letter.Or(CharP('_'));
            var nameChar = Letter.Or(Digit).Or(AnyOf("-_."));
            var name = nameStart.And(Many(nameChar))
                .Map((first, rest) => string.Concat(rest.Prepend(first)));

            var quotedString = Skip('"').And(Many(NoneOf("\""))).And(Skip('"'))
                .Map(string.Concat);
            var attribute = WS1.And(name).And(WS).And(Skip('=')).And(WS).And(quotedString)
                .Map((attrName, attrVal) => new XAttribute(attrName, attrVal));
            var attributes = Many(Try(attribute));

            XElParser element = null;

            var emptyElement = Skip('<').And(name).And(attributes).And(WS).And(Skip("/>"))
                .Map((elName, attrs) => new XElement(elName, attrs));

            var openingTag = Skip('<').And(name).And(attributes).And(WS).And(Skip(">"));
            StringParser closingTag(string tagName) => Skip("</").And(StringP(tagName)).And(WS).And(Skip('>'));
            var childElements = Many1(Try(WS.And(Rec(() => element)).And(WS)))
                .Map(attrs => (object)attrs);
            var text = Many(NoneOf("<"))
                .Map(t => (object)string.Concat(t));
            var content = childElements.Or(text);
            var parentElement = openingTag.And(content).Map(Flat).And(x => closingTag(x.Item1).Return(x))
                .Map((elName, elAttrs, elContent) => new XElement(elName, elAttrs, elContent));

            element = Try(emptyElement).Or(parentElement);

            SimpleXmlParser = element.And(WS).And(EOF);
        }

        #endregion Parser definition

        #region Tests

        [Fact]
        public void Root() =>
            SimpleXmlParser
            .ParseString("<root/>")
            .ShouldBe(new XElement("root"));

        [Fact]
        public void TextContent() =>
            SimpleXmlParser
            .ParseString("<root>text</root>")
            .ShouldBe(new XElement("root", "text"));

        [Fact]
        public void NoContent() =>
            SimpleXmlParser
            .ParseString("<root></root>")
            .ShouldBe(new XElement("root", ""));

        [Fact]
        public void ChildElements() =>
            SimpleXmlParser
            .ParseString("<root><child1/><child2></child2></root>")
            .ShouldBe(new XElement("root",
                new XElement("child1"),
                new XElement("child2", "")));

        [Fact]
        public void Attributes() =>
            SimpleXmlParser
            .ParseString("<root attr1=\"1\" attr2=\"2\"></root>")
            .ShouldBe(new XElement("root",
                new XAttribute("attr1", "1"),
                new XAttribute("attr2", "2"),
                ""));

        [Fact]
        public void WithWhitespace() =>
            SimpleXmlParser
            .ParseString(@"<root    attr1  =  ""1""
                                    attr2  =  ""2""   >
                               <child1  />
                               <child2   ></child2   >
                           </root   >       ")
            .ShouldBe(new XElement("root",
                new XAttribute("attr1", 1),
                new XAttribute("attr2", 2),
                new XElement("child1"),
                new XElement("child2", "")));

        [Fact]
        public void ParsesWholeInput() =>
            SimpleXmlParser
            .ParseString("<root/> this should cause an error")
            .ShouldBe<ErrorMessage.Expected>("end of input");

        #endregion Tests
    }
}
