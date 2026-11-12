# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MeetingRoomManagement.Api.csproj", "./"]
RUN dotnet restore "MeetingRoomManagement.Api.csproj"

COPY . .
RUN dotnet publish "MeetingRoomManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "MeetingRoomManagement.Api.dll"]
