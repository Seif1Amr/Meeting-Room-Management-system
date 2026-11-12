using Microsoft.AspNetCore.Mvc;
using MeetingRoomManagement.Api.DTOs;
using MeetingRoomManagement.Api.Services;

namespace MeetingRoomManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Create a new booking after validating overlap, working hours, chronology, and duration rules.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken ct)
    {
        var created = await _bookingService.CreateBookingAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>
    /// Cancel/delete a booking.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBooking(int id, CancellationToken ct)
    {
        var deleted = await _bookingService.DeleteBookingAsync(id, ct);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse($"Booking with ID {id} was not found.", "BOOKING_NOT_FOUND"));
        }

        return NoContent();
    }
}
