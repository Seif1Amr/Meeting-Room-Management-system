# MeetingHub &bull; Space Management System

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-16%2B%20%2F%2021-DD0031.svg)](https://angular.dev/)
[![EF Core SQLite](https://img.shields.io/badge/EF%20Core-SQLite-003B57.svg)](https://learn.microsoft.com/ef/core/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED.svg)](https://www.docker.com/)
[![Backend Tests](https://img.shields.io/badge/xUnit%20Tests-34%2F34%20Passed-brightgreen.svg)]()
[![Frontend Tests](https://img.shields.io/badge/Vitest-4%2F4%20Passed-brightgreen.svg)]()

Full-stack, enterprise-grade Meeting Room Booking application built for the **DigitalHub Technical Assessment**.
Features a .NET 8 ASP.NET Core Web API, Angular 21 Standalone Components, collision-free schedule validation, and an auto-seeded SQLite database.

---

## ⚡ 1-Click Quick Start (Zero Setup Needed)

Everything is pre-configured to run out of the box with zero manual migrations or database setup.

### 🖱️ Windows (1-Click Run)
Double-click **`run.bat`** (or `start.bat`) in the root folder.
> This automatically starts the backend and frontend, and immediately opens your default browser to [http://localhost:4200](http://localhost:4200).

---

### 🐳 Docker Compose (Universal - Mac / Windows / Linux)
If you have Docker Desktop running:
```bash
docker compose up --build
```
- **Web App**: [http://localhost:4200](http://localhost:4200) (or [http://localhost](http://localhost))
- **Swagger API**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

*(To stop later: `docker compose down`)*

---

### 💻 macOS / Linux (Terminal)
```bash
chmod +x start.sh && ./start.sh
```

---

## 🌐 Application URLs

| Service | URL | Notes |
|---|---|---|
| **Frontend Web App** | [http://localhost:4200](http://localhost:4200) | Responsive on Desktop & Mobile |
| **Backend Swagger API** | [http://localhost:5000/swagger](http://localhost:5000/swagger) | Interactive OpenAPI Explorer |

---

## 🧪 1-Click Automated Tests

The test suite validates all 4 strict business rules, overlap edge cases, and client-side behavior:

- **Windows**: Double-click **`test.bat`** (or run `.\test.ps1`)
- **macOS / Linux**: `./test.sh`
- **Or via npm**: `npm test`

```bash
Backend xUnit:   34/34 Passed (Overlap, Chronology, Working Hours, Max Duration, Seed Data)
Frontend Vitest:  4/4 Passed (Components, Form Validations, Services)
```

---

## 📋 Strict Booking Business Rules Enforced

Enforced centrally in `BookingService` and mirrored client-side:

| Rule | Requirement | HTTP Status | Error Code |
|---|---|:---:|:---:|
| **1. No Overlap** | A booking must NOT overlap existing bookings in the same room.<br/>*Back-to-back bookings (e.g. 10:00 end / 10:00 start) ARE allowed.* | `409 Conflict` | `BOOKING_CONFLICT` |
| **2. Chronology & Future** | `EndTime` must be after `StartTime`, and bookings cannot be made in the past. | `400 Bad Request` | `INVALID_TIME_RANGE`<br/>`PAST_BOOKING_NOT_ALLOWED` |
| **3. Working Hours & Same Day** | Must start and end on the same calendar day, strictly between `08:00` and `20:00`. | `400 Bad Request` | `OUTSIDE_WORKING_HOURS`<br/>`CROSS_DAY_NOT_ALLOWED` |
| **4. Max Duration** | Single meeting duration cannot exceed `4 hours`. | `400 Bad Request` | `EXCEEDS_MAX_DURATION` |

---

## 🏢 Auto-Seeded Meeting Rooms

The SQLite database (`rooms.db`) is automatically initialized and seeded on startup:

- **Cairo Executive Suite** (Floor 3 &bull; 16 seats)
- **Nile Boardroom** (Floor 2 &bull; 12 seats)
- **Giza Innovation Pod** (Floor 1 &bull; 6 seats)
- **Alexandria Focus Room** (Floor 2 &bull; 4 seats)
- **Heliopolis Studio** (Floor 1 &bull; 8 seats)

---

## 🏛️ Architecture Highlights

1. **Thin Controllers & Dedicated Services**: Controllers only handle HTTP status codes. Business logic and conflict math are strictly encapsulated inside `BookingService` and `RoomService`.
2. **Strict Entity/DTO Separation**: EF Core entities (`Room`, `Booking`) are internal. All requests and responses use strongly-typed DTOs (`RoomDto`, `BookingDto`, `CreateBookingRequest`, `ApiErrorResponse`).
3. **Bonus Endpoint**: `GET /api/rooms/available?date=...&startTime=...&endTime=...&minCapacity=...` powers the real-time **Quick Availability Finder**.
4. **Mobile & Desktop Responsive**: Includes segmented navigation tabs, horizontal room selector chips, touch-friendly 44px tap targets, and bottom-sheet booking modal.
