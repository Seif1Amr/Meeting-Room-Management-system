namespace MeetingRoomManagement.Api.DTOs;

public record RoomDto(
    int Id,
    string Name,
    int Capacity,
    int Floor
);
