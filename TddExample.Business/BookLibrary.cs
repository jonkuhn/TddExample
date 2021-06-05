using System;
using System.Linq;
using System.Threading.Tasks;

namespace TddExample.Business
{
    public class BookLibrary
    {
        public const int MaxOutstandingLoans = 10;
        public static TimeSpan LoanDuration => TimeSpan.FromDays(14);

        private IBookLoanRepository _bookLoanRepository;

        public BookLibrary(IBookLoanRepository bookLoanRepository)
        {
            _bookLoanRepository = bookLoanRepository;
        }
        public async Task CheckoutBookAsync(string memberId, string isbn)
        {
            var outstandingBookLoans =
                (await _bookLoanRepository.GetOutstandingBookLoansForMemberAsync(memberId))
                .ToList();

            if ((outstandingBookLoans.Count + 1) > MaxOutstandingLoans)
            {
                throw new TooManyCheckedOutBooksException();
            }

            if (outstandingBookLoans.Any(x => x.DueDate <= DateTime.UtcNow))
            {
                throw new PastDueBooksException();
            }

            var availableCopies = await _bookLoanRepository.GetAvailableCopyIdsAsync(isbn);
            if (!availableCopies.Any())
            {
                throw new NoCopiesAvailableException();
            }

            await _bookLoanRepository.TryCreateBookLoanAsync(
                new BookLoan
                {
                    MemberId = memberId,
                    Isbn = isbn,
                    CopyId = availableCopies.First(),
                    DueDate = DateTime.UtcNow.Date + LoanDuration,
                    WasReturned = false
                });
        }
    }
}
