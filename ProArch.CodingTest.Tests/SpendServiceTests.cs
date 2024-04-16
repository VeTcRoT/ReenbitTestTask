using AutoFixture;
using FluentAssertions;
using Moq;
using ProArch.CodingTest.External;
using ProArch.CodingTest.Interfaces.Services;
using ProArch.CodingTest.Invoices;
using ProArch.CodingTest.Summary;
using ProArch.CodingTest.Suppliers;
using Xunit;

namespace ProArch.CodingTest.Tests
{
    public class SpendServiceTests
    {
        private readonly Mock<ISupplierService> _supplierServiceMock;
        private readonly Mock<IExternalInvoiceService> _externalInvoiceServiceWrapperMock;
        private readonly Mock<IYearAmountsService> _yearAmountsServiceMock;
        private readonly Fixture _fixture;

        public SpendServiceTests()
        {
            _supplierServiceMock = new Mock<ISupplierService>();
            _externalInvoiceServiceWrapperMock = new Mock<IExternalInvoiceService>();
            _yearAmountsServiceMock = new Mock<IYearAmountsService>();
            _fixture = new Fixture();
        }

        [Fact]
        public void SpendService_GetTotalSpend_ShouldReturnSummaryOfASingleInvoiceForInternalSuppliers()
        {
            // Arrange
            var supplier = new Supplier { Id = 1, Name = "Supplier 1" };
            var invoices = new[] { new Invoice { SupplierId = supplier.Id, Amount = 100, InvoiceDate = new DateTime(2024, 1, 1) } };
            _supplierServiceMock.Setup(x => x.GetById(supplier.Id)).Returns(supplier);
            _yearAmountsServiceMock.Setup(x => x.GetYearAmountsBySupplier(supplier)).Returns(invoices.Select(inv => new YearAmount(inv.InvoiceDate.Year, inv.Amount)).AsQueryable());

            var spendService = new SpendService(_supplierServiceMock.Object, _yearAmountsServiceMock.Object);

            // Act
            var spendSummary = spendService.GetTotalSpend(supplier.Id);

            // Assert
            spendSummary.Should().BeEquivalentTo(new
            {
                Name = supplier.Name,
                Years = new[]
                {
                    new
                    {
                        Year = 2024,
                        TotalSpend = 100
                    }
                }
            });
        }

        [Fact]
        public void SpendService_GetTotalSpend_ShouldReturnSummaryOfOnlySuppliedSupplierForInternalSuppliers()
        {
            // Arrange
            var supplier1 = new Supplier { Id = 1, Name = "Supplier 1" };
            var supplier2 = new Supplier { Id = 2, Name = "Supplier 2" };
            var invoices = new[]
            {
                new Invoice { SupplierId = supplier1.Id, Amount = 50, InvoiceDate = new DateTime(2000, 1, 1) },
                new Invoice { SupplierId = supplier2.Id, Amount = 150, InvoiceDate = new DateTime(2024, 1, 1) }
            };
            _supplierServiceMock.Setup(x => x.GetById(supplier1.Id)).Returns(supplier1);
            _supplierServiceMock.Setup(x => x.GetById(supplier2.Id)).Returns(supplier2);
            _yearAmountsServiceMock.Setup(x => x.GetYearAmountsBySupplier(supplier2)).Returns(invoices.Where(inv => inv.SupplierId == supplier2.Id).Select(inv => new YearAmount(inv.InvoiceDate.Year, inv.Amount)).AsQueryable());

            var spendService = new SpendService(_supplierServiceMock.Object, _yearAmountsServiceMock.Object);

            // Act
            var spendSummary = spendService.GetTotalSpend(supplier2.Id);

            // Assert
            spendSummary.Should().BeEquivalentTo(new
            {
                Name = supplier2.Name,
                Years = new[]
                {
                    new
                    {
                        Year = 2024,
                        TotalSpend = 150
                    }
                }
            });
        }

        [Fact]
        public void SpendService_GetTotalSpend_ShouldReturnSummaryOfYearsSummedForInternalSuppliers()
        {
            // Arrange
            var supplier = new Supplier { Id = 1, Name = "Supplier 1" };
            var invoices = new[]
            {
                new Invoice { SupplierId = supplier.Id, Amount = 10, InvoiceDate = new DateTime(2000, 1, 1) },
                new Invoice { SupplierId = supplier.Id, Amount = 20, InvoiceDate = new DateTime(2000, 1, 1) },
                new Invoice { SupplierId = supplier.Id, Amount = 30, InvoiceDate = new DateTime(2001, 1, 1) },
                new Invoice { SupplierId = supplier.Id, Amount = 40, InvoiceDate = new DateTime(2001, 1, 1) },
                new Invoice { SupplierId = supplier.Id, Amount = 50, InvoiceDate = new DateTime(2002, 1, 1) }
            };
            _supplierServiceMock.Setup(x => x.GetById(supplier.Id)).Returns(supplier);
            _yearAmountsServiceMock.Setup(x => x.GetYearAmountsBySupplier(supplier)).Returns(invoices.Select(inv => new YearAmount(inv.InvoiceDate.Year, inv.Amount)).AsQueryable());

            var spendService = new SpendService(_supplierServiceMock.Object, _yearAmountsServiceMock.Object);

            // Act
            var spendSummary = spendService.GetTotalSpend(supplier.Id);

            // Assert
            spendSummary.Should().BeEquivalentTo(new
            {
                Name = supplier.Name,
                Years = new[]
                {
                    new
                    {
                        Year = 2000,
                        TotalSpend = 30
                    },
                    new
                    {
                        Year = 2001,
                        TotalSpend = 70
                    },
                    new
                    {
                        Year = 2002,
                        TotalSpend = 50
                    }
                }
            });
        }

        [Fact]
        public void SpendService_GetTotalSpend_ShouldReturnSummaryOfASingleInvoiceForExternalSuppliers()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var externalInvoice = _fixture.Create<ExternalInvoice>();
            var yearAmountsForSupplier = new List<YearAmount>
            {
                new YearAmount(externalInvoice.Year, externalInvoice.TotalAmount)
            };

            _yearAmountsServiceMock.Setup(x => x.GetYearAmountsBySupplier(supplier)).Returns(yearAmountsForSupplier.AsQueryable());
            var spendService = new SpendService(_supplierServiceMock.Object, _yearAmountsServiceMock.Object);
            _supplierServiceMock.Setup(x => x.GetById(supplier.Id)).Returns(supplier);
            _externalInvoiceServiceWrapperMock.Setup(x => x.GetInvoices(supplier.Id)).Returns(new[] { externalInvoice });

            // Act
            var spendSummary = spendService.GetTotalSpend(supplier.Id);

            // Assert
            spendSummary.Should().BeEquivalentTo(new
            {
                Name = supplier.Name,
                Years = new[]
                {
                    new
                    {
                        Year = externalInvoice.Year,
                        TotalSpend = externalInvoice.TotalAmount
                    }
                }
            });
        }

        [Fact]
        public void SpendService_GetTotalSpend_ShouldReturnSummaryOfOnlySuppliedSupplierForExternalSuppliers()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var externalInvoice = _fixture.Create<ExternalInvoice>();

            _supplierServiceMock.Setup(x => x.GetById(supplier.Id)).Returns(supplier);
            _externalInvoiceServiceWrapperMock.Setup(x => x.GetInvoices(supplier.Id)).Returns(new[] { externalInvoice });

            var yearAmountsForSupplier = new List<YearAmount>
            {
                new YearAmount(externalInvoice.Year, externalInvoice.TotalAmount)
            };

            _yearAmountsServiceMock.Setup(x => x.GetYearAmountsBySupplier(supplier)).Returns(yearAmountsForSupplier.AsQueryable());

            var spendService = new SpendService(_supplierServiceMock.Object, _yearAmountsServiceMock.Object);

            // Act
            var spendSummary = spendService.GetTotalSpend(supplier.Id);

            // Assert
            spendSummary.Should().BeEquivalentTo(new
            {
                Name = supplier.Name,
                Years = new[]
                {
                    new
                    {
                        Year = externalInvoice.Year,
                        TotalSpend = externalInvoice.TotalAmount
                    }
                }
            });
        }

        [Fact]
        public void SpendService_GetTotalSpend_ShouldReturnSummaryOfYearsSummedForExternalSuppliers()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var externalInvoice1 = _fixture.Build<ExternalInvoice>().With(x => x.Year, 2000).Create();
            var externalInvoice2 = _fixture.Build<ExternalInvoice>().With(x => x.Year, 2001).Create();
            var externalInvoice3 = _fixture.Build<ExternalInvoice>().With(x => x.Year, 2001).Create();

            var yearAmountsForSupplier = new List<YearAmount>
            {
                new YearAmount(externalInvoice1.Year, externalInvoice1.TotalAmount),
                new YearAmount(externalInvoice2.Year, externalInvoice2.TotalAmount + externalInvoice3.TotalAmount)
            };

            _yearAmountsServiceMock.Setup(x => x.GetYearAmountsBySupplier(supplier)).Returns(yearAmountsForSupplier.AsQueryable());
            var spendService = new SpendService(_supplierServiceMock.Object, _yearAmountsServiceMock.Object);
            _supplierServiceMock.Setup(x => x.GetById(supplier.Id)).Returns(supplier);
            _externalInvoiceServiceWrapperMock.Setup(x => x.GetInvoices(supplier.Id)).Returns(new[] { externalInvoice1, externalInvoice2, externalInvoice3 });

            // Act
            var spendSummary = spendService.GetTotalSpend(supplier.Id);

            // Assert
            spendSummary.Should().BeEquivalentTo(new
            {
                Name = supplier.Name,
                Years = new[]
                {
                    new
                    {
                        Year = 2000,
                        TotalSpend = externalInvoice1.TotalAmount
                    },
                    new
                    {
                        Year = 2001,
                        TotalSpend = externalInvoice2.TotalAmount + externalInvoice3.TotalAmount
                    }
                }
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void SpendService_GetTotalSpend_ShouldReturnInvoicesUpTo3ConsecutiveErrors(int errorCount)
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var externalInvoice = _fixture.Create<ExternalInvoice>();
            _supplierServiceMock.Setup(x => x.GetById(supplier.Id)).Returns(supplier);
            _externalInvoiceServiceWrapperMock.Setup(x => x.GetInvoices(supplier.Id)).Returns(new[] { externalInvoice });
            _yearAmountsServiceMock.Setup(x => x.GetYearAmountsBySupplier(supplier)).Returns(new[] { new YearAmount(externalInvoice.Year, externalInvoice.TotalAmount) }.AsQueryable());

            var spendService = new SpendService(_supplierServiceMock.Object, _yearAmountsServiceMock.Object);

            for (var i = 0; i < errorCount; i++)
            {
                _externalInvoiceServiceWrapperMock.Setup(x => x.GetInvoices(supplier.Id)).Throws(new TimeoutException("Boom!"));
            }

            // Act
            var spendSummary = spendService.GetTotalSpend(supplier.Id);

            // Assert
            spendSummary.Should().BeEquivalentTo(new
            {
                Name = supplier.Name,
                Years = new[]
                {
                    new
                    {
                        Year = externalInvoice.Year,
                        TotalSpend = externalInvoice.TotalAmount
                    }
                }
            });
        }

        [Fact]
        public void SpendService_GetTotalSpend_ShouldReturnInvoicesFromFailoverInvoicesAfter3Errors()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var externalInvoices = new[]
            {
                new ExternalInvoice { TotalAmount = 5, Year = 2023 },
                new ExternalInvoice { TotalAmount = 6, Year = 2024 },
                new ExternalInvoice { TotalAmount = 22, Year = 2023 }
            };

            _supplierServiceMock.Setup(x => x.GetById(supplier.Id)).Returns(supplier);
            _yearAmountsServiceMock.Setup(x => x.GetYearAmountsBySupplier(supplier)).Returns(externalInvoices.Select(e => new YearAmount(e.Year, e.TotalAmount)).AsQueryable());

            for (var i = 0; i < 4; i++)
            {
                _externalInvoiceServiceWrapperMock.Setup(x => x.GetInvoices(supplier.Id)).Throws(new TimeoutException($"Boom {i + 1}!"));
            }

            var spendService = new SpendService(_supplierServiceMock.Object, _yearAmountsServiceMock.Object);

            // Act
            var spendSummary = spendService.GetTotalSpend(supplier.Id);

            // Assert
            spendSummary.Should().BeEquivalentTo(new
            {
                Name = supplier.Name,
                Years = new[]
                {
                    new
                    {
                        Year = 2023,
                        TotalSpend = 27
                    },
                    new
                    {
                        Year = 2024,
                        TotalSpend = 6
                    }
                }
            });
        }

        [Fact]
        public void SpendService_GetTotalSpend_ShouldReturnInvoicesFromFailoverInvoicesAfter3ErrorsOnContinuousRequests()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var failoverInvoiceCollection = new FailoverInvoiceCollection
            {
                Timestamp = DateTime.Today,
                Invoices = new[]
                {
                    new ExternalInvoice { TotalAmount = 5, Year = 2023 }
                }
            };

            _supplierServiceMock.Setup(x => x.GetById(supplier.Id)).Returns(supplier);
            _yearAmountsServiceMock.Setup(x => x.GetYearAmountsBySupplier(supplier)).Returns(new[] { new YearAmount(failoverInvoiceCollection.Invoices.First().Year, failoverInvoiceCollection.Invoices.First().TotalAmount) }.AsQueryable());

            _externalInvoiceServiceWrapperMock.SetupSequence(x => x.GetInvoices(supplier.Id))
                .Throws(new TimeoutException("Boom 1!"))
                .Throws(new TimeoutException("Boom 2!"))
                .Throws(new TimeoutException("Boom 3!"))
                .Returns(failoverInvoiceCollection.Invoices);

            var spendService = new SpendService(_supplierServiceMock.Object, _yearAmountsServiceMock.Object);

            // Act
            spendService.GetTotalSpend(supplier.Id);
            var spendSummary = spendService.GetTotalSpend(supplier.Id);

            // Assert
            spendSummary.Should().BeEquivalentTo(new
            {
                Name = supplier.Name,
                Years = new[]
                {
                    new
                    {
                        Year = failoverInvoiceCollection.Invoices.First().Year,
                        TotalSpend = failoverInvoiceCollection.Invoices.First().TotalAmount
                    }
                }
            });
        }


        [Fact]
        public async Task SpendService_GetTotalSpend_ShouldResumeToExternalServiceAfterAGivenPeriodOfTime()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var failoverInvoiceCollection = new FailoverInvoiceCollection
            {
                Timestamp = DateTime.Today,
                Invoices = Array.Empty<ExternalInvoice>()
            };

            _supplierServiceMock.Setup(x => x.GetById(supplier.Id)).Returns(supplier);
            _yearAmountsServiceMock.Setup(x => x.GetYearAmountsBySupplier(supplier)).Returns(Array.Empty<YearAmount>().AsQueryable());

            for (var i = 0; i < 4; i++)
            {
                _externalInvoiceServiceWrapperMock.Setup(x => x.GetInvoices(supplier.Id)).Throws(new TimeoutException($"Boom {i + 1}!"));
            }

            var circuitBreakDuration = TimeSpan.FromMilliseconds(500);
            var spendService = new SpendService(_supplierServiceMock.Object, _yearAmountsServiceMock.Object);

            // Act
            spendService.GetTotalSpend(supplier.Id);

            await Task.Delay(circuitBreakDuration);
            var spendSummary = spendService.GetTotalSpend(supplier.Id);

            // Assert
            spendSummary.Should().BeEquivalentTo(new
            {
                Name = supplier.Name,
                Years = new object[0]
            });
        }
    }
}
