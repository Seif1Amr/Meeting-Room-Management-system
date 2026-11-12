using System.ComponentModel.DataAnnotations;

namespace MeetingRoomManagement.Api.DTOs;

public class AvailableRoomsQuery
{
    [Required(ErrorMessage = "Date is required (format: YYYY-MM-DD).")]
    public DateOnly Date { get; set; }

    [Required(ErrorMessage = "StartTime is required (format: HH:mm).")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "EndTime is required (format: HH:mm).")]
    public TimeOnly EndTime { get; set; }

    [Range(1, 500, ErrorMessage = "MinCapacity must be at least 1.")]
    public int? MinCapacity { get; set; }
}
