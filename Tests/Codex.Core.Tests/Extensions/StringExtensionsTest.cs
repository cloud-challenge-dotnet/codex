using Codex.Tests.Framework;


namespace Codex.Core.Tests.Extensions
{
    public class StringExtensionsTest : IClassFixture<Fixture>
    {
        public StringExtensionsTest()
        {
        }

        [Fact]
        public void ToCamelCase()
        {
            string value = "MyTestData";

            string result = value.ToCamelCase();

            Assert.Equal("myTestData", result);
        }

        [Fact]
        public void ToCamelCase_Empty_Value()
        {
            string value = "";

            string result = value.ToCamelCase();

            Assert.Equal("", result);
        }

        [Fact]
        public void ToCamelCase_One_Char_Value()
        {
            string value = "A";

            string result = value.ToCamelCase();

            Assert.Equal("a", result);
        }

        [Fact]
        public void ToNullableCamelCase()
        {
            string? value = "MyTestData";

            string? result = value.ToNullableCamelCase();

            Assert.Equal("myTestData", result);
        }


        [Fact]
        public void ToNullableCamelCase_Null_Value()
        {
            string? value = null;

            string? result = value.ToNullableCamelCase();

            Assert.Null(result);
        }
    }
}
