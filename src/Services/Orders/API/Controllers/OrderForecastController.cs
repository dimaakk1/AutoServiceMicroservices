using System.Security.Claims;
using AutoserviceOrders.BLL.Services;
using AutoserviceOrders.DAL.Repositories.Interfaces;
using AutoserviceOrders.DAL.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoserviceOrders.API.Controllers;

[ApiController]
[Authorize]
[Route("api/Orders/Order/{id:int}")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class OrderForecastController(IUnitOfWork unitOfWork, RepairPredictionService predictions) : ControllerBase
{
    [HttpGet("estimated-duration")]
    [ProducesResponseType<EstimatedDurationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEstimatedDuration(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var order = await unitOfWork.GetRepository<IOrderRepository>().GetByIdAsync(id);
        if (order is null || !string.Equals(order.UserId, userId, StringComparison.Ordinal))
            return NotFound();

        var forecast = await predictions.TryPredictAsync(order, cancellationToken);
        return forecast is null ? NoContent() : Ok(new EstimatedDurationResponse(forecast.EstimatedDurationMinutes));
    }

    [HttpGet("delay-risk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<DelayRiskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDelayRisk(int id, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.GetRepository<IOrderRepository>().GetByIdAsync(id);
        if (order is null)
            return NotFound();
        if (order.PlannedCompletionAt is null)
            return NoContent();

        var forecast = await predictions.TryPredictAsync(order, cancellationToken);
        return forecast?.DelayProbability is { } probability ? Ok(new DelayRiskResponse(probability)) : NoContent();
    }
}

public sealed record EstimatedDurationResponse(double EstimatedDurationMinutes);
public sealed record DelayRiskResponse(double DelayProbability);
