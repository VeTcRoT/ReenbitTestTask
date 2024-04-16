using ProArch.CodingTest.Summary;
using ProArch.CodingTest.Suppliers;
using System.Linq;

namespace ProArch.CodingTest.Interfaces.Services
{
    public interface IYearAmountsService
    {
        IQueryable<YearAmount> GetYearAmountsBySupplier(Supplier supplier);
    }
}
