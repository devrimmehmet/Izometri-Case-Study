using ExpenseService.Api.Controllers;
using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using ExpenseService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseService.Tests;

public sealed class ExpensesControllerTests
{
    [Fact]
    public async Task Create_returns_created_at_action()
    {
        var request = new CreateExpenseRequest(ExpenseCategory.Travel, ExpenseCurrency.TRY, 3500, null, "Valid business trip description");
        var response = ExpenseResponse();
        var service = new Mock<IExpenseAppService>();
        service.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new ExpensesController(service.Object);

        var result = await controller.Create(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(ExpensesController.GetById), created.ActionName);
        Assert.Same(response, created.Value);
    }

    [Fact]
    public async Task GetList_returns_paged_expenses()
    {
        var query = new ExpenseQuery(null, null, null, null, 1, 20);
        var response = new PagedResponse<ExpenseResponse>(new[] { ExpenseResponse() }, 1, 20, 1);
        var service = new Mock<IExpenseAppService>();
        service.Setup(x => x.GetListAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new ExpensesController(service.Object);

        var result = await controller.GetList(query, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task GetById_returns_not_found_when_missing()
    {
        var service = new Mock<IExpenseAppService>();
        service.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Expense not found."));
        var controller = new ExpensesController(service.Object);

        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Submit_returns_no_content()
    {
        var expenseId = Guid.NewGuid();
        var service = new Mock<IExpenseAppService>();
        var controller = new ExpensesController(service.Object);

        var result = await controller.Submit(expenseId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        service.Verify(x => x.SubmitAsync(expenseId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Approve_returns_ok()
    {
        var response = ExpenseResponse(status: ExpenseStatus.Approved);
        var service = new Mock<IExpenseAppService>();
        service.Setup(x => x.ApproveAsync(response.Id, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new ExpensesController(service.Object);

        var result = await controller.Approve(response.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task Reject_returns_bad_request_for_invalid_state()
    {
        var request = new RejectExpenseRequest("valid reason");
        var service = new Mock<IExpenseAppService>();
        service.Setup(x => x.RejectAsync(It.IsAny<Guid>(), request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Only pending expenses can be rejected."));
        var controller = new ExpensesController(service.Object);

        var result = await controller.Reject(Guid.NewGuid(), request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_returns_forbidden_when_user_is_not_allowed()
    {
        var service = new Mock<IExpenseAppService>();
        service.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Forbidden."));
        var controller = new ExpensesController(service.Object);

        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbidden.StatusCode);
    }

    private static ExpenseResponse ExpenseResponse(ExpenseStatus status = ExpenseStatus.Draft)
    {
        return new ExpenseResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ExpenseCategory.Travel,
            ExpenseCurrency.TRY,
            3500,
            1.0m,
            3500.0m,
            "Valid business trip description",
            status,
            status == ExpenseStatus.Approved,
            false,
            null,
            DateTime.UtcNow,
            status == ExpenseStatus.Approved ? DateTime.UtcNow : null,
            null,
            DateTime.UtcNow);
    }
}
