using ProArch.CodingTest.Suppliers;

namespace ProArch.CodingTest.Interfaces.Services
{
    public interface ISupplierService
    {
        Supplier GetById(int id);
    }
}