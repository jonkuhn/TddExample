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
        [TestCase("memberId1", BookLibrary.MaxOutstandingLoans)]
        [TestCase("memberId2", BookLibrary.MaxOutstandingLoans)]
        public void TestCheckoutBookAsync_GivenMemberOverBookLimit_ThrowsTooManyCheckedOutBooksException(
            string memberId, int outstandingBookLoanCount)
        {
            var futureDueDate = DateTime.UtcNow + TimeSpan.FromDays(1);

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            SetupOutstandingBookLoansForMember(
                bookLoanRepository,
                memberId,
                CreateOutstandingBookLoans(memberId, outstandingBookLoanCount, futureDueDate));

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.ThrowsAsync<TooManyCheckedOutBooksException>(async () =>
                await bookLibrary.CheckoutBookAsync(memberId, "test-isbn"));
        }

        [TestCase("member1", BookLibrary.MaxOutstandingLoans - 1)]
        [TestCase("member2", BookLibrary.MaxOutstandingLoans - 2)]
        public void TestCheckoutBookAsync_GivenMemberNotOverBookLimit_Succeeds(
            string memberId, int outstandingBookLoanCount)
        {
            var futureDueDate = DateTime.UtcNow + TimeSpan.FromDays(1);

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            SetupOutstandingBookLoansForMember(
                bookLoanRepository,
                memberId,
                CreateOutstandingBookLoans(memberId, outstandingBookLoanCount, futureDueDate));
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(new[] { "available-copy-id" });
            bookLoanRepository.TryCreateBookLoanAsync(Arg.Any<BookLoan>())
                .Returns(true);

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.DoesNotThrowAsync(async () =>
                await bookLibrary.CheckoutBookAsync(memberId, "test-isbn"));
        }

        [Test]
        public void TestCheckoutBookAsync_GivenPastDueBookLoan_ThrowsBooksPastDueException()
        {
            const string memberId = "member-id-1";
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
                await bookLibrary.CheckoutBookAsync(memberId, "test-isbn"));
        }

        [Test]
        public void TestCheckoutBookAsync_GivenNoCopiesAvailable_ThrowsNoCopiesAvailableException()
        {
            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(Enumerable.Empty<string>());

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.ThrowsAsync<NoCopiesAvailableException>(async () =>
                await bookLibrary.CheckoutBookAsync("member-id", "test-isbn"));
        }

        [Test]
        public async Task TestCheckoutBookAsync_GivenCopyAvailable_CallsTryCreateBookLoanWithCorrectArgs()
        {
            const string memberId = "member-id-1";
            const string availableCopyId = "available-copy-id";
            const string isbnOfBookToCheckOut = "test-isbn";

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(new[] { availableCopyId });
            bookLoanRepository.TryCreateBookLoanAsync(Arg.Any<BookLoan>())
                .Returns(true);

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

        [Test]
        public void TestCheckoutBookAsync_GivenTryCreateBookLoanReturnsFalse_ThrowsNoCopiesAvailableException()
        {
            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(new[] { "available-copy-id" });
            bookLoanRepository.TryCreateBookLoanAsync(Arg.Any<BookLoan>())
                .Returns(false);

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.ThrowsAsync<NoCopiesAvailableException>(async () =>
                await bookLibrary.CheckoutBookAsync("member-id", "test-isbn"));
        }

        [Test]
        public void TestCheckoutBookAsync_Given3CopiesAndTryCreateBookLoanReturnsFalseFalseTrue_DoesNotThrow()
        {
            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(new[] { "avail-copy-id-1", "avail-copy-id-2", "avail-copy-id-3" });
            bookLoanRepository.TryCreateBookLoanAsync(Arg.Any<BookLoan>())
                .Returns(false, false, true);

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.DoesNotThrowAsync(async () =>
                await bookLibrary.CheckoutBookAsync("member-id", "test-isbn"));
        }

        [Test]
        public void TestCheckoutBookAsync_Given3CopiesAndTryCreateBookLoanReturnsFalseFalseFalse_ThrowsNoCopiesAvailableException()
        {
            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(new[] { "avail-copy-id-1", "avail-copy-id-2", "avail-copy-id-3" });
            bookLoanRepository.TryCreateBookLoanAsync(Arg.Any<BookLoan>())
                .Returns(false, false, false);

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.ThrowsAsync<NoCopiesAvailableException>(async () =>
                await bookLibrary.CheckoutBookAsync("member-id", "test-isbn"));
        }

        [Test]
        public async Task TestCheckoutBookAsync_Given3CopiesAndTryCreateBookLoanOnlyReturnsTrueForLast_CallsTryCreateBookLoanForEachCopy()
        {
            const string memberId = "member-id-1";
            const string isbnOfBookToCheckOut = "test-isbn";
            var availableCopyIds = new[] { "avail-copy-id-1", "avail-copy-id-2", "avail-copy-id-3" };

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(availableCopyIds);
            bookLoanRepository.TryCreateBookLoanAsync(Arg.Any<BookLoan>())
                .Returns(false, false, true);

            var bookLibrary = new BookLibrary(bookLoanRepository);

            await bookLibrary.CheckoutBookAsync(memberId, isbnOfBookToCheckOut);

            foreach (var copyId in availableCopyIds)
            {
                await bookLoanRepository.Received(1).TryCreateBookLoanAsync(
                    new BookLoan
                    {
                        MemberId = memberId,
                        Isbn = isbnOfBookToCheckOut,
                        CopyId = copyId,
                        DueDate = DateTime.UtcNow.Date + BookLibrary.LoanDuration,
                        WasReturned = false
                    });
            }
        }

        [Test]
        public async Task TestCheckoutBookAsync_Given3CopiesAndTryCreateBookLoanReturnsTrueOnFirst_CallsTryCreateBookLoanOnlyOnce()
        {
            const string memberId = "member-id-1";
            const string isbnOfBookToCheckOut = "test-isbn";
            var availableCopyIds = new[] { "avail-copy-id-1", "avail-copy-id-2", "avail-copy-id-3" };

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            bookLoanRepository.GetAvailableCopyIdsAsync(Arg.Any<string>())
                .Returns(availableCopyIds);
            bookLoanRepository.TryCreateBookLoanAsync(Arg.Any<BookLoan>())
                .Returns(true);

            var bookLibrary = new BookLibrary(bookLoanRepository);
            await bookLibrary.CheckoutBookAsync(memberId, isbnOfBookToCheckOut);

            await bookLoanRepository.Received(1).TryCreateBookLoanAsync(Arg.Any<BookLoan>());
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