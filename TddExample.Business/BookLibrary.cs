using System.Threading.Tasks;

namespace TddExample.Business
{
    public class BookLibrary
    {
        public const int MaxOutstandingLoans = 10;

        public BookLibrary(IBookLoanRepository bookLoanRepository)
        {

        }
        public async Task CheckoutBookAsync(string memberId, string isbn)
        {
            await Task.CompletedTask;
            throw new TooManyCheckedOutBooksException();
        }
    }
}
