# MeetingHub &bull; Enterprise Meeting Room Management System

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-16%2B%20%2F%2021-DD0031.svg)](https://angular.dev/)
[![EF Core SQLite](https://img.shields.io/badge/EF%20Core-SQLite-003B57.svg)](https://learn.microsoft.com/ef/core/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED.svg)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/xUnit%20Tests-34%20Passed-brightgreen.svg)]()
[![Frontend Tests](https://img.shields.io/badge/Vitest-4%20Passed-brightgreen.svg)]()

> A full-stack, enterprise-grade Meeting Room Booking application built for the **DigitalHub Technical Assessment**.
> Features clean architecture, collision-free scheduling validation, modern minimalist UI (Linear/Stripe aesthetic), zero-setup auto-seeded database, and one-click execution.

---

## ⚡ Quick Start for Reviewers (Zero Setup Required)

Choose whichever method fits your local machine best:

### 🚀 Option 1: Docker Compose (Recommended - Zero Installation Needed)
If you have Docker Desktop running, spin up both the backend and frontend with one command:

```bash
docker compose up --build
```

- **Frontend Web UI**: [http://localhost:4200](http://localhost:4200) (also [http://localhost](http://localhost))
- **Swagger / OpenAPI Documentation**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

*(To stop the containers later: `docker compose down`)*

---

### 💻 Option 2: One-Click Local Script
If you prefer running locally with .NET and Node already installed:

- **Windows (PowerShell)**:
  ```powershell
  .\start.ps1
  ```
- **macOS / Linux**:
  ```bash
  chmod +x start.sh && ./start.sh
  ```

This automatically starts the backend API on port `5000` and the Angular frontend on port `4200`.

---

### 🛠️ Option 3: Standard Terminal Commands

1. **Start Backend (.NET 8)**:
   ```bash
   cd backend
   dotnet run --urls http://localhost:5000
   ```
   *Swagger available at: [http://localhost:5000/swagger](http://localhost:5000/swagger)*

2. **Start Frontend (Angular)**:
   ```bash
   cd frontend
   npm install --legacy-peer-deps
   npm start
   ```
   *Application available at: [http://localhost:4200](http://localhost:4200)*

---

## 🧪 Running Automated Tests

Run the full automated test suite (34 backend tests + 4 frontend tests) in one command:

- **Windows**:
  ```powershell
  .\test.ps1
  ```
- **macOS / Linux**:
  ```bash
  chmod +x test.sh && ./test.sh
  ```
- **Or via npm**:
  ```bash
  npm test
  ```

### Individual Test Runs:
- **Backend Tests (xUnit)**:
  ```bash
  dotnet test backend.Tests/MeetingRoomManagement.Tests.csproj
  ```
- **Frontend Tests (Vitest)**:
  ```bash
  cd frontend && npm test -- --watch=false
  ```

---

## 🏢 Pre-Seeded Meeting Rooms & Sample Data

On startup, the SQLite database (`rooms.db`) is automatically created and seeded. No manual migrations or SQL scripts required:

| Room Name | Floor | Capacity | Features / Character |
|---|:---:|:---:|---|
| **Cairo Executive Suite** | 3 | 16 seats | Boardroom display, conferencing suite |
| **Nile Boardroom** | 2 | 12 seats | Dual screen, executive conference setup |
| **Giza Innovation Pod** | 1 | 6 seats | Collaborative brainstorming pod |
| **Alexandria Focus Room** | 2 | 4 seats | 1-on-1s, interviews, and deep work |
| **Heliopolis Studio** | 1 | 8 seats | Design workshops & project team syncs |

*Sample bookings are also pre-seeded for today so the schedule timeline is immediately populated with interactive cards and open gaps.*

---

## 📋 Strict Booking Business Rules

Enforced centrally in `BookingService` and mirrored client-side in the Angular Reactive Form:

| Rule | Description | Status Code | Error Code |
|---|---|:---:|:---:|
| **1. Overlap Prevention** | Booking must not overlap existing meetings ($\text{newStart} < \text{existingEnd} \land \text{newEnd} > \text{existingStart}$).<br/>*Back-to-back bookings (e.g. 10:00 end / 10:00 start) ARE allowed.* | `409 Conflict` | `BOOKING_CONFLICT` |
| **2. Chronology & Future** | `EndTime` must be strictly after `StartTime`, and meetings cannot be scheduled in the past. | `400 Bad Request` | `INVALID_TIME_RANGE`<br/>`PAST_BOOKING_NOT_ALLOWED` |
| **3. Working Hours & Same Day** | Must start and end on the same calendar day, strictly between `08:00` and `20:00`. | `400 Bad Request` | `OUTSIDE_WORKING_HOURS`<br/>`CROSS_DAY_NOT_ALLOWED` |
| **4. Max Duration** | Meeting duration cannot exceed `4 hours`. | `400 Bad Request` | `EXCEEDS_MAX_DURATION` |

---

## 📡 REST API Endpoints Specification

| Method | Endpoint | Description | Return Code |
|---|---|---|:---:|
| `GET` | `/api/rooms` | List all meeting rooms ordered by floor and name | `200 OK` |
| `GET` | `/api/rooms/{id}` | Get single room details by ID | `200 OK` / `404` |
| `GET` | `/api/rooms/{id}/bookings?date=YYYY-MM-DD` | Get room bookings for date, strictly ordered by `StartTime` ascending | `200 OK` / `404` |
| `POST` | `/api/bookings` | Validate business rules and create new reservation | `201 Created` / `400` / `409` |
| `DELETE` | `/api/bookings/{id}` | Cancel/delete reservation | `204 No Content` / `404` |
| `GET` | `/api/rooms/available?date=...&startTime=...&endTime=...&minCapacity=...` | **Bonus Endpoint**: Filter collision-free rooms for requested slot | `200 OK` / `400` |

---

## 🎨 UI/UX Highlights (Linear / Stripe Aesthetic)

- **Geometric Typography & Tabular Numbers**: Utilizes `Inter` and `Plus Jakarta Sans` with `font-variant-numeric: tabular-nums` to eliminate visual jitter on clock values and counts.
- **Quick Availability Finder**: Header search bar lets users enter a desired slot and capacity to filter all rooms in real time.
- **Visual Schedule & Open Gap Highlighting**: Segments the 08:00–20:00 day into booked slots and emerald **"Available Slot"** gaps with a one-click **"Quick Reserve"** button.
- **Dual-Layer Validation**: Instant feedback in Angular forms plus an error banner that displays server 409 conflict reasons.
- **Cancellation Confirmation**: Clean confirmation modal with optimistic UI removal and toast alert.

---

## 🏛️ Architectural Highlights & Assumptions

1. **Thin Controllers & Rich Services**:
   Controllers only handle routing and HTTP status codes. Domain assertions, conflict math, and validations reside in `BookingService` and `RoomService`.

2. **Strict Separation of Domain Entities & DTOs**:
   EF Core entities (`Room`, `Booking`) are decoupled from external API contracts. DTOs prevent accidental over-posting and protect internal database structures.

3. **Deterministic Time Testing with `IDateTimeProvider`**:
   Time checks inject `IDateTimeProvider` (backed by `SystemDateTimeProvider`), allowing unit tests to freeze time deterministically without clock drift.

4. **Robust Reverse Proxy in Production**:
   Nginx handles client-side Angular routing (`try_files $uri $uri/ /index.html`), reverse-proxies `/api/` calls to the backend, and enables gzip compression.

---

## 🚀 Production Roadmap & Future Enhancements

1. **Authentication & Role-Based Access Control (RBAC)**: Azure AD / Auth0 SSO integration with admin-only room deletion permissions.
2. **Real-Time SignalR WebSockets**: Instant schedule sync across all open browser tabs when any booking is created or cancelled.
3. **Recurring Bookings**: Support for daily, weekly, or monthly repeating reservations with conflict reporting.
4. **Calendar Integration**: Bidirectional sync with Google Calendar and Microsoft 365 Outlook via Microsoft Graph.
5. **IoT Room Telemetry**: Live occupancy indicators connected to hardware motion sensors and meeting display tablets.
