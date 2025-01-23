using System.Xml.Linq;
using FParsec;
using FParsec.CSharp;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Xunit;
using static FParsec.CSharp.CharParsersCS;
using static FParsec.CSharp.PrimitivesCS;

namespace Tests;

using StringParser = FSharpFunc<CharStream<Unit>, Reply<string>>;
using XElContentParser = FSharpFunc<CharStream<Unit>, Reply<object>>;
using XElParser = FSharpFunc<CharStream<Unit>, Reply<XElement>>;

public class SimpleXml {
    #region Parser definition

    // This XML parser mimics the behavior of `XElement.Parse(string)`.
    private static readonly XElParser SimpleXmlParser;

    static SimpleXml() {
        var nameStart = Choice(Letter, CharP('_'));
        var nameChar = Choice(Letter, Digit, AnyOf("-_."));
        var name = Many1Chars(nameStart, nameChar).And(WS);

        var quotedString = Between('"', ManyChars(NoneOf("\"")), '"');
        var attribute = name.And(Skip('=')).And(WS).And(quotedString).And(WS)
            .Lbl_("attribute")
            .Map((attrName, attrVal) => new XAttribute(attrName, attrVal));
        var attributes = Many(attribute);

        XElParser? element = null;

        var elementStart = Skip('<').AndTry(name.Lbl("tag name")).And(attributes);

        static StringParser closingTag(string tagName) => Between("</", StringP(tagName).And(WS), ">")
            .Lbl_($"closing tag '</{tagName}>'");

        XElContentParser textContent(string leadingWS) => NotEmpty(ManyChars(NoneOf("<"))
            .Map(text => leadingWS + text)
            .Map(x => (object)x)
            .Lbl_("text content"));

        var childElement = Rec(() => element).Map(x => (object)x).Lbl_("child element");

        static object EmptyContentToEmptyString(FSharpList<object> xs) => xs.IsEmpty ? (object)"" : xs;

        var elementContent = Many(WS.WithSkipped().AndTry(ws => Choice(textContent(ws), childElement)))
            .Map(EmptyContentToEmptyString);

        XElParser elementEnd(string elName, FSharpList<XAttribute> elAttrs) =>
            Choice(
                Skip("/>").Return((object?)null),
                Skip(">").And(elementContent).And(WS).AndL(closingTag(elName)).Map(x => (object?)x))
            .Map(elContent => new XElement(elName, elContent, elAttrs));

        element = elementStart.And(elementEnd);

        SimpleXmlParser = element.And(WS).And(EOF);
    }

    #endregion Parser definition

    #region Tests

    [Fact]
    public void Root() =>
        SimpleXmlParser
        .Run("<root/>").GetResult()
        .ShouldBe(new XElement("root"));

    [Fact]
    public void TextContent() =>
        SimpleXmlParser
        .Run("<root>text</root>").GetResult()
        .ShouldBe(new XElement("root", "text"));

    [Fact]
    public void NoContent() =>
        SimpleXmlParser
        .Run("<root></root>").GetResult()
        .ShouldBe(new XElement("root", ""));

    [Fact]
    public void WhitespaceContent() =>
        SimpleXmlParser
        .Run("<root>   \t  \n </root>").GetResult()
        .ShouldBe(new XElement("root", ""));

    [Fact]
    public void ChildElements() =>
        SimpleXmlParser
        .Run("<root><child1/><child2></child2></root>").GetResult()
        .ShouldBe(new XElement("root",
            new XElement("child1"),
            new XElement("child2", "")));

    [Fact]
    public void TextContentAndChildElementsMix() =>
        SimpleXmlParser
        .Run("<root> text1 <child1/> <child2></child2> text2 </root>").GetResult()
        .ShouldBe(new XElement("root",
            " text1 ",
            new XElement("child1"),
            new XElement("child2", ""),
            " text2 "));

    [Fact]
    public void Attributes() =>
        SimpleXmlParser
        .Run("<root attr1=\"1\" attr2=\"2\"></root>").GetResult()
        .ShouldBe(new XElement("root",
            new XAttribute("attr1", "1"),
            new XAttribute("attr2", "2"),
            ""));

    [Fact]
    public void WithWhitespace() =>
        SimpleXmlParser
        .Run(
            """
            <root    attr1  =  "1"
                            attr2  =  "2"   >
                        <child1  />
                        <child2   ></child2   >
                    </root   >       
            """)
        .GetResult()
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
