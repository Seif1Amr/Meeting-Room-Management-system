using Microsoft.AspNetCore.Mvc;
using MeetingRoomManagement.Api.DTOs;
using MeetingRoomManagement.Api.Services;

namespace MeetingRoomManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;

    public RoomsController(IRoomService roomService, IBookingService bookingService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
    }

    /// <summary>
    /// List all meeting rooms.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAllRooms(CancellationToken ct)
    {
        var rooms = await _roomService.GetAllRoomsAsync(ct);
        return Ok(rooms);
    }

    /// <summary>
    /// Find available rooms for a given date, time slot, and optional minimum capacity.
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAvailableRooms(
        [FromQuery] AvailableRoomsQuery query,
        CancellationToken ct)
    {
        var availableRooms = await _roomService.GetAvailableRoomsAsync(
            query.Date,
            query.StartTime,
            query.EndTime,
            query.MinCapacity,
            ct
        );

        return Ok(availableRooms);
    }

    /// <summary>
    /// Get details of a single meeting room.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomDto>> GetRoomById(int id, CancellationToken ct)
    {
        var room = await _roomService.GetRoomByIdAsync(id, ct);
        if (room == null)
        {
            return NotFound(new ApiErrorResponse($"Room with ID {id} was not found.", "ROOM_NOT_FOUND"));
        }

        return Ok(room);
    }

    /// <summary>
    /// Return bookings for a room on that date, strictly ordered by StartTime ascending.
    /// </summary>
    [HttpGet("{id:int}/bookings")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetRoomBookings(
        int id,
        [FromQuery] DateOnly? date,
        CancellationToken ct)
    {
        var queryDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var bookings = await _bookingService.GetBookingsByRoomAndDateAsync(id, queryDate, ct);
        return Ok(bookings);
    }
}
