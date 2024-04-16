using Moq;
using Xunit;
using FluentAssertions;
using AutoFixture;
using ProArch.CodingTest.Summary;
using ProArch.CodingTest.External;
using ProArch.CodingTest.Suppliers;
using ProArch.CodingTest.Interfaces.Repositories;
using ProArch.CodingTest.Interfaces.Services;
using ProArch.CodingTest.Invoices;

namespace ProArch.CodingTest.Tests
{
    public class YearAmountsServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private readonly Mock<IExternalInvoiceService> _externalInvoiceServiceMock;
        private readonly YearAmountsService _yearAmountsService;

        public YearAmountsServiceTests()
        {
            _fixture = new Fixture();
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _externalInvoiceServiceMock = new Mock<IExternalInvoiceService>();
            _yearAmountsService = new YearAmountsService(_invoiceRepositoryMock.Object, _externalInvoiceServiceMock.Object);
        }

        [Fact]
        public void YearAmountsService_GetYearAmountsBySupplier_ShouldReturnYearAmountsFromExternalService()
        {
            // Arrange
            var supplierId = 1;
            var supplier = new Supplier { Id = supplierId, IsExternal = true };
            var externalInvoices = _fixture.CreateMany<ExternalInvoice>(3).ToArray();
            _externalInvoiceServiceMock.Setup(x => x.GetInvoices(supplierId)).Returns(externalInvoices);

            // Act
            var yearAmounts = _yearAmountsService.GetYearAmountsBySupplier(supplier).ToList();

            // Assert
            yearAmounts.Should().HaveCount(3);
            yearAmounts.Should().OnlyContain(x => externalInvoices.Any(y => y.Year == x.Year && y.TotalAmount == x.Amount));
        }

        [Fact]
        public void YearAmountsService_GetYearAmountsBySupplier_ShouldReturnYearAmountsFromRepository()
        {
            // Arrange
            var supplierId = 1;
            var supplier = new Supplier { Id = supplierId, IsExternal = false };
            var invoices = _fixture.CreateMany<Invoice>(3).ToList();
            invoices.ForEach(x => x.SupplierId = supplierId);
            _invoiceRepositoryMock.Setup(x => x.Get()).Returns(invoices.AsQueryable());

            // Act
            var yearAmounts = _yearAmountsService.GetYearAmountsBySupplier(supplier).ToList();

            // Assert
            yearAmounts.Should().HaveCount(3);
            yearAmounts.Should().OnlyContain(x => invoices.Any(y => y.InvoiceDate.Year == x.Year && y.Amount == x.Amount));
        }
    }
}
