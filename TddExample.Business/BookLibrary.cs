using System;
using System.Linq;
using System.Threading.Tasks;

namespace TddExample.Business
{
    public class BookLibrary
    {
        public const int MaxOutstandingLoans = 10;

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
        }
    }
}
