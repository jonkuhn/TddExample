using System;
using System.Collections.Generic;
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

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            SetupOutstandingBookLoansForMember(bookLoanRepository, memberId, outstandingBookLoanCount);

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
            var bookLoanRepository = Substitute.For<IBookLoanRepository>();
            SetupOutstandingBookLoansForMember(bookLoanRepository, memberId, outstandingBookLoanCount);

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.DoesNotThrowAsync(async () =>
                await bookLibrary.CheckoutBookAsync(memberId, isbnOfBookToCheckOut));
        }

        private static void SetupOutstandingBookLoansForMember(
            IBookLoanRepository mockBookLoanRepository,
            string memberId,
            int count)
        {
            // Stub GetOutstandingBookLoansForMember to return a given
            // number of outstanding book loans for the given member
            var stubOutstandingLoans = new List<BookLoan>();
            for (var i = 0; i < count; i++)
            {
                stubOutstandingLoans.Add(
                    new BookLoan
                    {
                        Id = $"test-book-loan-id-{i}",
                        MemberId = memberId,
                        Isbn = $"isbn-{i}",
                        CopyId = $"copy-id-for-book-{i}",
                        DueDate = new DateTime(2021, 1, 1, 0, 0, 0),
                        WasReturned = false
                    });
            }
            mockBookLoanRepository.GetOutstandingBookLoansForMemberAsync(memberId)
                .Returns(stubOutstandingLoans);
        }
    }
}