using System.ComponentModel.DataAnnotations;
using AutoserviceVehicle.BLL.Services;
using AutoserviceVehicle.DAL.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoserviceVehicle.API;

public sealed class VehicleExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            DuplicateVehicleException => StatusCodes.Status409Conflict,
            VinNotFoundException => StatusCodes.Status404NotFound,
            VinProviderUnavailableException => StatusCodes.Status503ServiceUnavailable,
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = status == 500 ? "Не вдалося виконати операцію з автомобілем."
                    : exception is DbUpdateConcurrencyException ? "Авто вже змінено або видалено. Оновіть список."
                    : exception.Message
            }
        });
    }
}
