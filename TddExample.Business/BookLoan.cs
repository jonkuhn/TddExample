using System;

namespace TddExample.Business
{
    public class BookLoan
    {
        public string Id { get; set; }

        public string MemberId { get; set; }

        public string Isbn { get; set; }

        public string CopyId { get; set; }

        public DateTime DueDate { get; set; }

        public bool WasReturned { get; set; }
    }
}
