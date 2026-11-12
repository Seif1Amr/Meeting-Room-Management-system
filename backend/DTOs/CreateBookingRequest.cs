using System.ComponentModel.DataAnnotations;

namespace MeetingRoomManagement.Api.DTOs;

public class CreateBookingRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "RoomId must be a valid positive integer.")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "BookedBy is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "BookedBy must be between 2 and 100 characters.")]
    public string BookedBy { get; set; } = string.Empty;

    [Required(ErrorMessage = "StartTime is required.")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "EndTime is required.")]
    public DateTime EndTime { get; set; }
}
