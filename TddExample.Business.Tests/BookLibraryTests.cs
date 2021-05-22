using NSubstitute;
using NUnit.Framework;

namespace TddExample.Business.Tests
{
    public class BookLibraryTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestCheckoutBookAsync_GivenMemberOverBookLimit_ThrowsTooManyCheckedOutBooksException()
        {
            const string memberId = "test-member-id";
            const string isbn = "test-isbn";

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            var bookLibrary = new BookLibrary(bookLoanRepository);

            // TODO: This test doesn't actually do what it says yet because it
            // doesn't actually set up the situation where the member is over
            // the book limit.   However, this is still a good time to try to
            // run it and get the classes built and compiling.

            Assert.ThrowsAsync<TooManyCheckedOutBooksException>(async () =>
                await bookLibrary.CheckoutBookAsync(memberId, isbn));
        }
    }
}