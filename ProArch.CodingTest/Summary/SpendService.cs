using System.Linq;
using ProArch.CodingTest.Interfaces.Services;

namespace ProArch.CodingTest.Summary
{
    public class SpendService : ISpendService
    {
        private readonly ISupplierService _supplierService;
        private readonly IYearAmountsService _yearAmountsService;

        public SpendService(ISupplierService supplierService, IYearAmountsService yearAmountsQueryHandler)
            => (_supplierService, _yearAmountsService) =
                (supplierService, yearAmountsQueryHandler);

        public SpendSummary GetTotalSpend(int supplierId)
        {
            var supplier = _supplierService.GetById(supplierId);

            var yearAmounts = _yearAmountsService.GetYearAmountsBySupplier(supplier);

            var years = yearAmounts
                .GroupBy(x => x.Year, (year, yearAmount)
                    => new SpendDetail { Year = year, TotalSpend = yearAmount.Sum(x => x.Amount) })
                .ToList();

            return new SpendSummary
            {
                Name = supplier.Name,
                Years = years
            };
        }
    }
}