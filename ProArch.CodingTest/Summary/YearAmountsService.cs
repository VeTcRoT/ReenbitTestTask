using System.Linq;
using ProArch.CodingTest.Interfaces.Repositories;
using ProArch.CodingTest.Interfaces.Services;
using ProArch.CodingTest.Suppliers;

namespace ProArch.CodingTest.Summary
{
    public class YearAmountsService : IYearAmountsService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IExternalInvoiceService _externalInvoiceService;

        public YearAmountsService(IInvoiceRepository invoiceRepository, IExternalInvoiceService externalInvoiceService)
        {
            _invoiceRepository = invoiceRepository;
            _externalInvoiceService = externalInvoiceService;
        }

        public IQueryable<YearAmount> GetYearAmountsBySupplier(Supplier supplier)
        {
            if (supplier.IsExternal)
            {
                return _externalInvoiceService.GetInvoices(supplier.Id)
                    .AsQueryable()
                    .Select(x => new YearAmount(x.Year, x.TotalAmount));
            }

            return _invoiceRepository.Get()
                .Where(x => x.SupplierId == supplier.Id)
                .Select(x => new YearAmount(x.InvoiceDate.Year, x.Amount));
        }
    }
}