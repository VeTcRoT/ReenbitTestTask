using System.Collections.Generic;
using System.Linq;

namespace ProArch.CodingTest.Invoices
{
    public class InvoiceRepository
    {
        public IQueryable<Invoice> Get()
        {
            return new List<Invoice>().AsQueryable();
        }
    }
}
