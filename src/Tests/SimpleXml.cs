namespace Tests {
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
            var nameStart = Choice(Letter, CharP('_'));
            var nameChar = Choice(Letter, Digit, AnyOf("-_."));
            var name = ManyChars(nameStart, nameChar);

            var quotedString = Between('"', ManyChars(NoneOf("\"")), '"');
            var attribute = WS1.And(name).And(WS).And(Skip('=')).And(WS).And(quotedString)
                .Map((attrName, attrVal) => new XAttribute(attrName, attrVal));
            var attributes = Many(Try(attribute));

            var nameWithAttrs = name.And(attributes).And(WS);

            XElParser element = null;

            var emptyElement = Between("<", nameWithAttrs, "/>")
                .Map((elName, attrs) => new XElement(elName, attrs));

            var openingTag = Between('<', nameWithAttrs, '>');
            StringParser closingTag(string tagName) => Between("</", StringP(tagName).And(WS), ">");
            var childElements = Many1(Try(WS.And(Rec(() => element)).And(WS)))
                .Map(els => (object)els);
            var text = ManyChars(NoneOf("<"))
                .Map(t => (object)t);
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
