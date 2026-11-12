namespace MeetingRoomManagement.Api.Domain.Exceptions;

public class BookingValidationException : Exception
{
    public string Code { get; }

    public BookingValidationException(string message, string code = "VALIDATION_ERROR")
        : base(message)
    {
        Code = code;
    }
}
