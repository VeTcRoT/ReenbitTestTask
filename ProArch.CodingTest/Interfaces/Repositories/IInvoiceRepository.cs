using System.Linq;
using ProArch.CodingTest.Invoices;

namespace ProArch.CodingTest.Interfaces.Repositories
{
    public interface IInvoiceRepository
    {
        IQueryable<Invoice> Get();
    }
}