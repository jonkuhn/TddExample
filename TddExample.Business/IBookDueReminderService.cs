using System;
using System.Threading.Tasks;

namespace TddExample.Business
{
    public interface IBookLoanReminderService
    {
        Task ScheduleRemindersAsync(BookLoan bookLoan);
    }
}
