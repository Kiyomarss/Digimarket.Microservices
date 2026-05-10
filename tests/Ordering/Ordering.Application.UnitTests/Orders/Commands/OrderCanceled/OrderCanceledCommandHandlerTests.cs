using BuildingBlocks.Exceptions.Application;
using BuildingBlocks.UnitOfWork;
using FluentAssertions;
using MediatR;
using Moq;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.Enum;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Application.Orders.Commands.OrderCancelled;
using Ordering.Application.RepositoryContracts;
using Ordering.TestingInfrastructure.Builders;
using ProductGrpc;
using Shared;

namespace Ordering.Application.UnitTests.Orders.Commands.OrderCanceled;

public class OrderCanceledCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly OrderCanceledHandler _handler;

    public OrderCanceledCommandHandlerTests()
    {
        _handler = new OrderCanceledHandler(
                                            _unitOfWorkMock.Object,
                                            _orderRepoMock.Object);
    }

    private OrderCanceledCommand CreateValidCommand()
    {
        return new OrderCanceledCommand
        {
            Id = TestGuids.Guid3
        };
    }

    [Fact]
    public async Task Handle_OrderExists_Should_Cancel_Order_And_Save()
    {
        // Arrange
        var command = CreateValidCommand();
        var order =  new OrderBuilder().Build();

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        order.State.Should().Be(OrderState.Canceled);

        _orderRepoMock.Verify(r => r.GetByIdAsync(command.Id), Times.Once);

        _unitOfWorkMock.Verify(
                               u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                               Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_Should_Throw_NotFoundException()
    {
        // Arrange
        var command = CreateValidCommand();

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync((Order?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();

        _unitOfWorkMock.Verify(
                               u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                               Times.Never);
    }
}