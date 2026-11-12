namespace MeetingRoomManagement.Api.Domain.Exceptions;

public class BookingConflictException : Exception
{
    public string Code { get; }

    public BookingConflictException(string message, string code = "BOOKING_CONFLICT")
        : base(message)
    {
        Code = code;
    }
}
