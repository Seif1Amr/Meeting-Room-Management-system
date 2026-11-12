# MeetingHub &bull; Enterprise Meeting Room Management System

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-16%2B%20%2F%2021-DD0031.svg)](https://angular.dev/)
[![EF Core SQLite](https://img.shields.io/badge/EF%20Core-SQLite-003B57.svg)](https://learn.microsoft.com/ef/core/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED.svg)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/xUnit%20Tests-34%20Passed-brightgreen.svg)]()

> A full-stack, enterprise-grade Meeting Room Booking application built for the **DigitalHub Technical Assessment**. Designed with clean architecture, strict collision-free scheduling validation, modern minimalist UI (Linear/Stripe aesthetic), and zero-setup local & containerized execution.

---

## Table of Contents
- [Architecture & Monorepo Layout](#architecture--monorepo-layout)
- [Quick Start Guide](#quick-start-guide)
  - [Option A: Docker Compose (One-Command Setup)](#option-a-docker-compose-one-command-setup)
  - [Option B: Local Development Run](#option-b-local-development-run)
- [API Specification & Endpoints](#api-specification--endpoints)
- [Strict Booking Business Rules](#strict-booking-business-rules)
- [Testing Suite](#testing-suite)
- [Key Architectural Decisions & Assumptions](#key-architectural-decisions--assumptions)
- [Future Enhancements & Roadmap](#future-enhancements--roadmap)

---

## Architecture & Monorepo Layout

```
meetings-room-managment/
├── backend/                             # .NET 8 ASP.NET Core Web API
│   ├── Controllers/                     # Thin REST controllers (RoomsController, BookingsController)
│   ├── Data/                            # AppDbContext & DbInitializer (SQLite auto-seed)
│   ├── Domain/
│   │   ├── Entities/                    # Room, Booking domain entities
│   │   └── Exceptions/                  # Domain exceptions (BookingConflictException, etc.)
│   ├── DTOs/                            # Clean Request/Response contracts & API error models
│   ├── Middleware/                      # Global ExceptionHandlingMiddleware (400, 404, 409, 500)
│   ├── Services/                        # Business logic & validation (BookingService, RoomService)
│   ├── appsettings.json
│   ├── Dockerfile                       # Multi-stage .NET 8 production Dockerfile
│   └── Program.cs                       # DI, CORS, Swagger, and DB lifecycle
│
├── backend.Tests/                       # xUnit Test Suite (34 automated unit tests)
│   ├── TestHelpers/                     # In-memory DbContext helper & TestDateTimeProvider
│   └── UnitTests/                       # Exhaustive overlap, validation rule & availability tests
│
├── frontend/                            # Angular Standalone Architecture (Angular 16+)
│   ├── src/app/
│   │   ├── components/                  # BookingModalComponent, CancelDialogComponent, ToastContainer
│   │   ├── models/                      # Strongly-typed TypeScript interfaces
│   │   ├── services/                    # RoomService, BookingService, ToastService
│   │   ├── app.ts / app.html / app.css  # Unified dashboard, timeline & room schedule view
│   ├── nginx.conf                       # Production Nginx reverse-proxy & SPA fallback
│   └── Dockerfile                       # Multi-stage Node.js build + Nginx runtime
│
├── docker-compose.yml                   # Root orchestration for Backend & Frontend
└── README.md                            # Comprehensive engineering documentation
```

---

## Quick Start Guide

### Option A: Docker Compose (One-Command Setup)

Ensure Docker Desktop or Docker engine is running:

```bash
docker compose up --build
```

- **Frontend Application**: [http://localhost:4200](http://localhost:4200) (also available on port 80: [http://localhost](http://localhost))
- **Backend API & Swagger UI**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

> **Zero Manual Setup**: The SQLite database (`rooms.db`) is automatically generated and seeded on startup with 5 realistic enterprise rooms and sample schedule bookings.

---

### Option B: Local Development Run

#### Prerequisites:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+ or v20+)](https://nodejs.org/) & npm

#### 1. Start the Backend API (.NET 8):
```bash
cd backend
dotnet run --urls http://localhost:5000
```
- Swagger documentation opens at: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- The SQLite database `rooms.db` is automatically created and populated.

#### 2. Start the Frontend (Angular):
```bash
cd frontend
npm install --legacy-peer-deps
npm start
```
- Open [http://localhost:4200](http://localhost:4200) in your browser.

---

## API Specification & Endpoints

All endpoints follow strict REST conventions, returning standard JSON representations. Errors return a uniform contract:
```json
{
  "message": "The requested time slot overlaps with an existing booking in this room.",
  "code": "BOOKING_CONFLICT"
}
```

| Method | Endpoint | Description | Status Codes |
|---|---|---|---|
| `GET` | `/api/rooms` | Retrieve all 5 seeded meeting rooms | `200 OK` |
| `GET` | `/api/rooms/{id}` | Get room details by ID | `200 OK`, `404 Not Found` |
| `GET` | `/api/rooms/{id}/bookings?date=YYYY-MM-DD` | List bookings for a room on given date, ordered by StartTime ascending | `200 OK`, `404 Not Found` |
| `POST` | `/api/bookings` | Validate business rules and create booking | `201 Created`, `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| `DELETE` | `/api/bookings/{id}` | Cancel/delete an existing reservation | `204 No Content`, `404 Not Found` |
| `GET` | `/api/rooms/available?date=YYYY-MM-DD&startTime=HH:mm&endTime=HH:mm&minCapacity=N` | **Bonus Endpoint**: Discover collision-free rooms matching capacity for slot | `200 OK`, `400 Bad Request` |

### Sample `POST /api/bookings` Payload:
```json
{
  "roomId": 1,
  "title": "Executive Board Sync",
  "bookedBy": "Layla Omar (layla@company.com)",
  "startTime": "2026-11-20T10:00:00",
  "endTime": "2026-11-20T11:30:00"
}
```

---

## Strict Booking Business Rules

All validation rules are enforced centrally inside `BookingService` and mirrored client-side in the Angular Reactive Form for instant feedback:

1. **Rule 1 (Overlap Prevention)**:
   - A booking must not overlap with any existing booking in the same room:
     $$\text{newStart} < \text{existingEnd} \land \text{newEnd} > \text{existingStart}$$
   - **Back-to-back bookings ARE strictly permitted** (e.g., Meeting A ending at `10:00` and Meeting B starting at `10:00` do not collide).
   - Collision triggers HTTP `409 Conflict` with code `"BOOKING_CONFLICT"`.

2. **Rule 2 (Chronology & Future-Only)**:
   - `EndTime` must be strictly after `StartTime`.
   - Bookings cannot be made in the past.
   - Triggers HTTP `400 Bad Request` (`"INVALID_TIME_RANGE"` / `"PAST_BOOKING_NOT_ALLOWED"`).

3. **Rule 3 (Working Hours & Same Day)**:
   - Bookings must start and end on the same calendar day.
   - Must fit entirely within working hours `08:00` to `20:00` (`StartTime >= 08:00` and `EndTime <= 20:00`).
   - Triggers HTTP `400 Bad Request` (`"OUTSIDE_WORKING_HOURS"` / `"CROSS_DAY_NOT_ALLOWED"`).

4. **Rule 4 (Maximum Duration)**:
   - A booking cannot exceed `4 hours` in duration.
   - Triggers HTTP `400 Bad Request` (`"EXCEEDS_MAX_DURATION"`).

---

## Testing Suite

### Backend Unit Tests (`/backend.Tests` using xUnit)
The backend test suite contains **34 comprehensive unit tests** testing all edge cases:

```bash
dotnet test backend.Tests/MeetingRoomManagement.Tests.csproj
```

- **Overlap Scenarios Tested**:
  - Exact identical overlap (10:00–11:00 vs 10:00–11:00) $\to$ `409 Conflict`
  - Partial overlap starting inside existing booking $\to$ `409 Conflict`
  - Partial overlap ending inside existing booking $\to$ `409 Conflict`
  - Enveloping overlap (new spans outside existing) $\to$ `409 Conflict`
  - Enveloped overlap (existing spans outside new) $\to$ `409 Conflict`
  - Back-to-back boundary immediately before (09:00–10:00 / 10:00–11:00) $\to$ Success
  - Back-to-back boundary immediately after (10:00–11:00 / 11:00–12:00) $\to$ Success
  - Cross-room isolation (same time slot in different room) $\to$ Success

- **Validation Rules Tested**:
  - Past dates & past times earlier today $\to$ `400 Bad Request`
  - End time earlier than or equal to start time $\to$ `400 Bad Request`
  - Multi-day spanning bookings $\to$ `400 Bad Request`
  - Booking before 08:00 or ending after 20:00 $\to$ `400 Bad Request`
  - Exact 08:00 start and exact 20:00 end boundaries $\to$ Success
  - Duration > 4 hours (e.g. 4 hrs 1 min) $\to$ `400 Bad Request`
  - Exact 4 hours duration $\to$ Success
  - Missing or blank title/organizer $\to$ `400 Bad Request`
  - Non-existent room ID $\to$ `404 Not Found`

- **Availability Finder Tests**:
  - Excludes booked rooms, includes free rooms, applies capacity filtering.

### Frontend Unit Tests (Vitest)
```bash
cd frontend
npm test -- --watch=false
```
- Tests component rendering, initial room selection, chronological timeline generation with open gap calculation, and date navigation.

---

## Key Architectural Decisions & Assumptions

1. **Thin Controllers & Rich Services**:
   Controllers only handle HTTP serialization and status mapping. All domain assertions, conflict checks, and queries live in `BookingService` and `RoomService`.

2. **Strict Separation of Domain & DTOs**:
   EF Core entities (`Room`, `Booking`) are never returned directly to API callers. Strongly-typed DTOs decouple persistence schemas from public API contracts.

3. **Deterministic Time Simulation via `IDateTimeProvider`**:
   Rather than hardcoding `DateTime.Now`, backend services inject `IDateTimeProvider`. This allows unit tests to freeze or advance time deterministically without clock drift.

4. **Zero-Setup Database Initialization**:
   `DbInitializer.InitializeAsync` runs on startup, automatically creating tables and seeding the 5 specified enterprise rooms:
   - Cairo Executive Suite (Floor 3, Capacity 16)
   - Nile Boardroom (Floor 2, Capacity 12)
   - Giza Innovation Pod (Floor 1, Capacity 6)
   - Alexandria Focus Room (Floor 2, Capacity 4)
   - Heliopolis Studio (Floor 1, Capacity 8)

5. **Linear/Stripe Minimalist UI Aesthetic**:
   - Tabular typography (`font-variant-numeric: tabular-nums`) prevents visual jitter in time displays and capacities.
   - Visual schedule alternates booked cards with emerald available gaps featuring a **"Quick Reserve"** action that pre-fills the booking modal.
   - Dual-layer validation prevents unnecessary roundtrips while gracefully rendering server 409 conflict messages if a race condition occurs.

---

## Future Enhancements & Roadmap

If extended for production deployment, the following features would be added:
1. **Authentication & RBAC**: Integration with OAuth2 / OpenID Connect (Azure AD / Auth0) for Single Sign-On and admin-only room deletion permissions.
2. **Real-Time Live Updates via SignalR**: Push notifications when another user reserves or cancels a room so all clients update instantaneously without polling.
3. **Recurring Reservations**: Support for weekly/bi-weekly recurring meetings with conflict exception reporting.
4. **Calendar Integration**: Two-way sync with Google Calendar and Microsoft 365 Exchange via Microsoft Graph.
5. **Room Hardware Telemetry**: Display live IoT occupancy sensors, video conferencing hardware (e.g. Teams Rooms, Zoom Rooms), and digital whiteboard equipment.
