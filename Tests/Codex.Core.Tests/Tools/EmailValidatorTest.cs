using Codex.Core.Tools;
using Codex.Tests.Framework;
using Xunit;

namespace Codex.Core.Tests
{
    public class EmailValidatorTest : IClassFixture<Fixture>
    {
        public EmailValidatorTest()
        {
        }

        [Fact]
        public void EmailValid()
        {
            bool result = EmailValidator.EmailValid("test@gmail.com");

            Assert.True(result);
        }

        [Fact]
        public void EmailValid_valid_1()
        {
            bool result = EmailValidator.EmailValid("test@gmailcom");

            Assert.True(result);
        }

        [Fact]
        public void EmailValid_Invalid_Email_1()
        {
            bool result = EmailValidator.EmailValid("@gmail.com");

            Assert.False(result);
        }

        [Fact]
        public void EmailValid_Invalid_Email_2()
        {
            bool result = EmailValidator.EmailValid("testgmail.com");

            Assert.False(result);
        }

        [Fact]
        public void EmailValid_Invalid_Email_3()
        {
            bool result = EmailValidator.EmailValid("test@.com");

            Assert.False(result);
        }

        [Fact]
        public void EmailValid_Empty_Email()
        {
            bool result = EmailValidator.EmailValid("");

            Assert.False(result);
        }
    }
}
