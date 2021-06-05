using System;

namespace TddExample.Business
{
    public record BookLoan
    {
        public string MemberId { get; init; }

        public string Isbn { get; init; }

        public string CopyId { get; init; }

        public DateTime DueDate { get; init; }

        public bool WasReturned { get; init; }
    }
}
