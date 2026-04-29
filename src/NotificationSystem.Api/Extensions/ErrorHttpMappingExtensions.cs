using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Api.Extensions;

public static class ErrorHttpMappingExtensions
{
    public static IActionResult ToErrorResponse(this ControllerBase controller, Error error)
    {
        var envelope = Envelope<object>.Failure(error);

        return error.Type switch
        {
            "validation" => controller.BadRequest(envelope),
            "not_found" => controller.NotFound(envelope),
            "conflict" => controller.Conflict(envelope),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError, envelope)
        };
    }
}
