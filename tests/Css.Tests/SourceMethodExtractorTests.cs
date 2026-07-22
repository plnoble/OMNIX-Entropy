using FluentAssertions;

namespace Css.Tests;

public sealed class SourceMethodExtractorTests
{
    [Fact]
    public void Extract_returns_only_the_requested_method()
    {
        const string source = """
            private void First()
            {
                Call();
            }

            private void Second()
            {
                Other();
            }
            """;

        var method = SourceMethodExtractor.Extract(source, "private void First()");

        method.Should().Contain("Call();").And.NotContain("Second").And.NotContain("Other");
    }

    [Fact]
    public void Extract_ignores_braces_inside_strings_characters_and_comments()
    {
        const string source = """
            private string Example()
            {
                // }
                var value = "}";
                var character = '{';
                return value;
            }
            """;

        var method = SourceMethodExtractor.Extract(source, "private string Example()");

        method.Should().Contain("return value;").And.EndWith("}");
    }

    [Fact]
    public void Extract_refuses_a_missing_or_bare_symbol()
    {
        FluentActions.Invoking(() => SourceMethodExtractor.Extract("private void A() { }", "A"))
            .Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => SourceMethodExtractor.Extract("private void A() { }", "private void B()"))
            .Should().Throw<InvalidOperationException>();
    }
}
