using ETicketing.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETicketing.Tests
{
    public class PaymentHandlerTests
    {
        [Fact]
        public async Task CreditCardHandler_ReturnsSucceeded()
        {
            // Arrange
            var handler = new CreditCardHandler();

            // Act
            var result = await handler.ProcessAsync(CancellationToken.None);

            // Assert
            Assert.Equal(PaymentStatus.Succeeded, result);
        }

        [Fact]
        public async Task QRHandler_ReturnsPending()
        {
            // Arrange
            var handler = new QRHandler();

            // Act
            var result = await handler.ProcessAsync(CancellationToken.None);

            // Assert
            Assert.Equal(PaymentStatus.Pending, result);
        }

        [Fact]
        public void CreditCardHandler_UsesCorrectPaymentMethod()
        {
            var handler = new CreditCardHandler();

            Assert.Equal(PaymentMethod.CreditCard, handler.Method);
        }

        [Fact]
        public void QRHandler_UsesCorrectPaymentMethod()
        {
            var handler = new QRHandler();

            Assert.Equal(PaymentMethod.QR, handler.Method);
        }

        [Fact]
        public async Task CreditCardHandler_ThrowsWhenCancellationRequested()
        {
            // Arrange
            var handler = new CreditCardHandler();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            // Act and Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.ProcessAsync(cancellation.Token));
        }

        [Fact]
        public async Task QRHandler_ThrowsWhenCancellationRequested()
        {
            // Arrange
            var handler = new QRHandler();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            // Act and Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.ProcessAsync(cancellation.Token));
        }
    }
}
