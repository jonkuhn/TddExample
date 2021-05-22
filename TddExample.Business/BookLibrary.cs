using System.Threading.Tasks;

namespace TddExample.Business
{
    public class BookLibrary
    {
        public BookLibrary(IBookLoanRepository bookLoanRepository)
        {

        }
        public async Task CheckoutBookAsync(string memberId, string isbn)
        {
            throw new TooManyCheckedOutBooksException();
        }
    }
}
