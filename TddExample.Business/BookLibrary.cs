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
            var numberOfOutstandingLoans =
                (await _bookLoanRepository.GetOutstandingBookLoansForMemberAsync(memberId))
                .Count();

            if ((numberOfOutstandingLoans + 1) > MaxOutstandingLoans)
            {
                throw new TooManyCheckedOutBooksException();
            }
        }
    }
}
