using ProArch.CodingTest.External;

namespace ProArch.CodingTest.Interfaces.Services
{
    public interface IExternalInvoiceService
    {
        ExternalInvoice[] GetInvoices(int supplierId);
    }
}