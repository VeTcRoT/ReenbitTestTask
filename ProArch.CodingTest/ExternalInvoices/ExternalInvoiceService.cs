using System;
using System.Linq;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using ProArch.CodingTest.Exceptions;
using ProArch.CodingTest.External;
using ProArch.CodingTest.Interfaces.Services;

namespace ProArch.CodingTest.ExternalInvoices
{
    public class ExternalInvoiceService : IExternalInvoiceService
    {
        private readonly IFailoverInvoiceService _failoverInvoiceService;

        private readonly RetryPolicy _retryPolicy = Policy
            .Handle<Exception>()
            .Retry(3);

        private readonly CircuitBreakerPolicy _circuitBreakerPolicy;

        public ExternalInvoiceService(IFailoverInvoiceService failoverInvoiceService, ExternalInvoiceServiceOptions options)
        {
            _failoverInvoiceService = failoverInvoiceService;

            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreaker(1, options.CircuitBreakDuration);
        }

        public ExternalInvoice[] GetInvoices(int supplierId)
        {
            var executeAndCapture = _circuitBreakerPolicy.ExecuteAndCapture(() => _retryPolicy.Execute(() => External.ExternalInvoiceService.GetInvoices(supplierId.ToString())));

            if (executeAndCapture.Outcome == OutcomeType.Failure)
            {
                var invoiceCollection = _failoverInvoiceService.GetInvoices(supplierId);
                if (invoiceCollection.Timestamp <= DateTime.Today.AddDays(-28))
                {
                    throw new FailoverInvoicesOutOfDateException(invoiceCollection.Timestamp,
                        executeAndCapture.FinalException);
                }

                return invoiceCollection
                    .Invoices.Select(x => new ExternalInvoice
                    {
                        Year = x.Year,
                        TotalAmount = x.TotalAmount
                    })
                    .ToArray();
            }

            return executeAndCapture.Result;
        }
    }
}