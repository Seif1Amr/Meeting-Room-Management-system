namespace MeetingRoomManagement.Api.DTOs;

public record ApiErrorResponse(
    string Message,
    string Code,
    IDictionary<string, string[]>? Errors = null
);
