using MeetingRoomManagement.Api.Services;

namespace MeetingRoomManagement.Tests.TestHelpers;

public class TestDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; }
    public DateTime Now { get; set; }

    public TestDateTimeProvider(DateTime fixedNow)
    {
        Now = fixedNow;
        UtcNow = fixedNow.ToUniversalTime();
    }
}
