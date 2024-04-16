using System;

namespace ProArch.CodingTest.ExternalInvoices
{
    public class ExternalInvoiceServiceOptions
    {
        public TimeSpan CircuitBreakDuration { get; }
        public static ExternalInvoiceServiceOptions Default { get; } =
            new ExternalInvoiceServiceOptions(TimeSpan.FromMinutes(1));

        public ExternalInvoiceServiceOptions(TimeSpan circuitBreakDuration)
        {
            CircuitBreakDuration = circuitBreakDuration;
        }
    }
}