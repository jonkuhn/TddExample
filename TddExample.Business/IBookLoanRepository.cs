using System.Collections.Generic;
using System.Threading.Tasks;

namespace TddExample.Business
{
    public interface IBookLoanRepository
    {
        Task<IEnumerable<BookLoan>> GetOutstandingBookLoansForMemberAsync(string memberId);
        Task<IEnumerable<string>> GetAvailableCopyIdsAsync(string isbn);
        Task<bool> TryCreateBookLoanAsync(BookLoan bookLoan);
    }
}
