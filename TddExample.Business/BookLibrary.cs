using System;
using System.Linq;
using System.Threading.Tasks;

namespace TddExample.Business
{
    public class BookLibrary
    {
        public const int MaxOutstandingLoans = 10;
        public static TimeSpan LoanDuration => TimeSpan.FromDays(14);

        private readonly IBookLoanRepository _bookLoanRepository;
        private readonly IBookLoanReminderService _bookLoanReminderService;

        public BookLibrary(
            IBookLoanRepository bookLoanRepository,
            IBookLoanReminderService bookDueReminderService)
        {
            _bookLoanRepository = bookLoanRepository;
            _bookLoanReminderService = bookDueReminderService;
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

            var bookLoan = await CreateBookLoanForFirstAvailableCopy(memberId, isbn);
        }

        private async Task<BookLoan> CreateBookLoanForFirstAvailableCopy(string memberId, string isbn)
        {
            var availableCopyIds = await _bookLoanRepository.GetAvailableCopyIdsAsync(isbn);
            if (!availableCopyIds.Any())
            {
                throw new NoCopiesAvailableException();
            }

            foreach (var copyId in availableCopyIds)
            {
                var bookLoan = new BookLoan
                {
                    MemberId = memberId,
                    Isbn = isbn,
                    CopyId = copyId,
                    DueDate = DateTime.UtcNow.Date + LoanDuration,
                    WasReturned = false
                };

                await _bookLoanReminderService.ScheduleRemindersAsync(bookLoan);

                if (await _bookLoanRepository.TryCreateBookLoanAsync(bookLoan))
                {
                    return bookLoan;
                }
            }

            throw new NoCopiesAvailableException();
        }
    }
}
