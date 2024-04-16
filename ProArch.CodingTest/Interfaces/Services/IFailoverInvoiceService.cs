using ProArch.CodingTest.Invoices;

namespace ProArch.CodingTest.Interfaces.Services
{
    public interface IFailoverInvoiceService
    {
        FailoverInvoiceCollection GetInvoices(int supplierId);
    }
}