using ProArch.CodingTest.Summary;

namespace ProArch.CodingTest.Interfaces.Services
{
    public interface ISpendService
    {
        SpendSummary GetTotalSpend(int supplierId);
    }
}
