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
            string memberId, int outstandingBookLoans)
        {
            const string isbnOfBookToCheckOut = "test-isbn";

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();

            var stubOutstandingLoans = new List<BookLoan>();

            // Stub GetOutstandingBookLoansForMember to return exactly
            // the maximum number of outstanding BookLoans
            for (var i = 0; i < outstandingBookLoans; i++)
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
            bookLoanRepository.GetOutstandingBookLoansForMemberAsync(memberId)
                .Returns(stubOutstandingLoans);

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.ThrowsAsync<TooManyCheckedOutBooksException>(async () =>
                await bookLibrary.CheckoutBookAsync(memberId, isbnOfBookToCheckOut));
        }

        [TestCase("member1", BookLibrary.MaxOutstandingLoans - 1)]
        [TestCase("member2", BookLibrary.MaxOutstandingLoans - 2)]
        public void TestCheckoutBookAsync_GivenMemberNotOverBookLimit_Succeeds(
            string memberId, int outstandingBookLoans)
        {
            const string isbnOfBookToCheckOut = "test-isbn";

            var bookLoanRepository = Substitute.For<IBookLoanRepository>();

            var stubOutstandingLoans = new List<BookLoan>();

            // Stub GetOutstandingBookLoansForMember to return one less
            // that the maximum number of outstanding loans so this checkout
            // should be allowed
            for (var i = 0; i < outstandingBookLoans; i++)
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
            bookLoanRepository.GetOutstandingBookLoansForMemberAsync(memberId)
                .Returns(stubOutstandingLoans);

            var bookLibrary = new BookLibrary(bookLoanRepository);

            Assert.DoesNotThrowAsync(async () =>
                await bookLibrary.CheckoutBookAsync(memberId, isbnOfBookToCheckOut));
        }
    }
}