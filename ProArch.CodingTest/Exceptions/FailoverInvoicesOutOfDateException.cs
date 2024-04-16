using System;

namespace ProArch.CodingTest.Exceptions
{
    [Serializable]
    public class FailoverInvoicesOutOfDateException : Exception
    {
        public FailoverInvoicesOutOfDateException(DateTime timespan, Exception inner) : base(CreateMessage(timespan), inner)
        {
        }

        private static string CreateMessage(DateTime timespan) => $"Failover invoices has out of date invoices from {timespan}";
    }
}