using System.Security.Claims;
using AutoserviceVehicle.BLL.DTO;
using AutoserviceVehicle.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoserviceVehicle.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize(Policy = "VehicleOwner")]
public sealed class VehiclesController(IVehicleService service) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(UserId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await service.GetAsync(UserId, id, cancellationToken);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> Create(SaveVehicleDto request, CancellationToken cancellationToken)
    {
        var vehicle = await service.CreateAsync(UserId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VehicleDto>> Update(Guid id, SaveVehicleDto request, CancellationToken cancellationToken)
    {
        var vehicle = await service.UpdateAsync(UserId, id, request, cancellationToken);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await service.DeleteAsync(UserId, id, cancellationToken) ? NoContent() : NotFound();
}
