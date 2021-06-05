using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [TestCase("memberId1", BookLibrary.MaxOutstandingLoans)]
        [TestCase("memberId2", BookLibrary.MaxOutstandingLoans)]
        public void TestCheckoutBookAsync_GivenMemberOverBookLimit_ThrowsTooManyCheckedOutBooksException(
            string memberId, int outstandingBookLoanCount)
        {
            const string isbnOfBookToCheckOut = "test-isbn";
            var futureDueDate = DateTime.UtcNow + TimeSpan.FromDays(1);

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            SetupOutstandingBookLoansForMember(
                bookLoanRepository,
                memberId,
                CreateOutstandingBookLoans(memberId, outstandingBookLoanCount, futureDueDate));

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.ThrowsAsync<TooManyCheckedOutBooksException>(async () =>
                await bookLibrary.CheckoutBookAsync(memberId, isbnOfBookToCheckOut));
        }

        [TestCase("member1", BookLibrary.MaxOutstandingLoans - 1)]
        [TestCase("member2", BookLibrary.MaxOutstandingLoans - 2)]
        public void TestCheckoutBookAsync_GivenMemberNotOverBookLimit_Succeeds(
            string memberId, int outstandingBookLoanCount)
        {
            const string isbnOfBookToCheckOut = "test-isbn";
            var futureDueDate = DateTime.UtcNow + TimeSpan.FromDays(1);

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            SetupOutstandingBookLoansForMember(
                bookLoanRepository,
                memberId,
                CreateOutstandingBookLoans(memberId, outstandingBookLoanCount, futureDueDate));
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(new[] { "available-copy-id" });

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.DoesNotThrowAsync(async () =>
                await bookLibrary.CheckoutBookAsync(memberId, isbnOfBookToCheckOut));
        }

        [Test]
        public void TestCheckoutBookAsync_GivenPastDueBookLoan_ThrowsBooksPastDueException()
        {
            const string memberId = "member-1";
            const string isbnOfBookToCheckOut = "test-isbn";
            var pastDueDate = DateTime.UtcNow - TimeSpan.FromDays(1);
            var futureDueDate = DateTime.UtcNow + TimeSpan.FromDays(1);

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            SetupOutstandingBookLoansForMember(
                bookLoanRepository,
                memberId,
                CreateOutstandingBookLoans(memberId, 4, futureDueDate)
                .Concat(CreateOutstandingBookLoans(memberId, 1, pastDueDate)));

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.ThrowsAsync<PastDueBooksException>(async () =>
                await bookLibrary.CheckoutBookAsync(memberId, isbnOfBookToCheckOut));
        }

        [Test]
        public void TestCheckoutBookAsync_GivenNoCopiesAvailable_ThrowsNoCopiesAvailableException()
        {
            const string memberId = "member-1";
            const string isbnOfBookToCheckOut = "test-isbn";

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(Enumerable.Empty<string>());

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.ThrowsAsync<NoCopiesAvailableException>(async () =>
                await bookLibrary.CheckoutBookAsync(memberId, isbnOfBookToCheckOut));
        }

        [Test]
        public async Task TestCheckoutBookAsync_GivenCopyAvailable_CallsTryCreateBookLoanWithCorrectArgs()
        {
            const string memberId = "member-1";
            const string availableCopyId = "available-copy-id";
            const string isbnOfBookToCheckOut = "test-isbn";

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(new[] { availableCopyId });

            var bookLibrary = new BookLibrary(bookLoanRepository);
            await bookLibrary.CheckoutBookAsync(memberId, isbnOfBookToCheckOut);

            var expectedNewBookLoan = new BookLoan
            {
                MemberId = memberId,
                Isbn = isbnOfBookToCheckOut,
                CopyId = availableCopyId,
                DueDate = DateTime.UtcNow.Date + BookLibrary.LoanDuration,
                WasReturned = false
            };
            await bookLoanRepository.Received(1).TryCreateBookLoanAsync(expectedNewBookLoan);
        }

        private static void SetupOutstandingBookLoansForMember(
            IBookLoanRepository mockBookLoanRepository,
            string memberId,
            IEnumerable<BookLoan> outstandingBookLoans)
        {
            // Stub GetOutstandingBookLoansForMember to return the given
            // list of book loans for the given member
            var stubOutstandingLoans = new List<BookLoan>();
            mockBookLoanRepository.GetOutstandingBookLoansForMemberAsync(memberId)
                .Returns(outstandingBookLoans);
        }

        private static IEnumerable<BookLoan> CreateOutstandingBookLoans(
            string memberId,
            int count,
            DateTime dueDate)
        {
            var bookLoans = new List<BookLoan>();
            for (var i = 0; i < count; i++)
            {
                bookLoans.Add(
                    new BookLoan
                    {
                        MemberId = memberId,
                        Isbn = $"isbn-{i}",
                        CopyId = $"copy-id-for-book-{i}",
                        DueDate = dueDate,
                        // Note: all _outstanding_ BookLoans would have WasReturned set to false
                        WasReturned = false
                    });
            }
            return bookLoans;
        }
    }
}