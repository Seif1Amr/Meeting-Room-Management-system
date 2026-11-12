import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { App } from './app';
import { RoomService } from './services/room.service';
import { BookingService } from './services/booking.service';
import { Room } from './models/room.model';
import { Booking } from './models/booking.model';

describe('App', () => {
  const mockRooms: Room[] = [
    { id: 1, name: 'Cairo Executive Suite', capacity: 16, floor: 3 },
    { id: 2, name: 'Nile Boardroom', capacity: 12, floor: 2 }
  ];

  const mockBookings: Booking[] = [
    {
      id: 101,
      roomId: 1,
      roomName: 'Cairo Executive Suite',
      title: 'Strategy Sync',
      bookedBy: 'Sarah Jenkins',
      startTime: '2026-11-20T10:00:00',
      endTime: '2026-11-20T12:00:00',
      createdAt: '2026-11-20T08:00:00'
    }
  ];

  let roomServiceMock: any;
  let bookingServiceMock: any;

  beforeEach(async () => {
    roomServiceMock = {
      getAllRooms: vi.fn().mockReturnValue(of(mockRooms)),
      getAvailableRooms: vi.fn().mockReturnValue(of([]))
    };

    bookingServiceMock = {
      getBookingsForRoom: vi.fn().mockReturnValue(of(mockBookings)),
      createBooking: vi.fn().mockReturnValue(of(mockBookings[0])),
      deleteBooking: vi.fn().mockReturnValue(of(undefined))
    };

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RoomService, useValue: roomServiceMock },
        { provide: BookingService, useValue: bookingServiceMock }
      ]
    }).compileComponents();
  });

  it('should create the application', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should load rooms on init and auto-select the first room', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    fixture.detectChanges();

    expect(roomServiceMock.getAllRooms).toHaveBeenCalled();
    expect(app.rooms().length).toBe(2);
    expect(app.selectedRoom()?.id).toBe(1);
    expect(app.selectedRoom()?.name).toBe('Cairo Executive Suite');
  });

  it('should compute timeline slots highlighting available gaps between 08:00 and 20:00', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    // Test computeTimelineSlots directly with date '2026-11-20' and 10:00-12:00 booking
    app.computeTimelineSlots(mockBookings, '2026-11-20');
    const slots = app.timelineSlots();

    // 1) Gap 08:00 to 10:00 (available)
    // 2) Booked 10:00 to 12:00 (booked)
    // 3) Gap 12:00 to 20:00 (available)
    expect(slots.length).toBe(3);
    expect(slots[0].type).toBe('available');
    expect(slots[0].startFormatted).toBe('08:00');
    expect(slots[0].endFormatted).toBe('10:00');

    expect(slots[1].type).toBe('booked');
    expect(slots[1].startFormatted).toBe('10:00');
    expect(slots[1].endFormatted).toBe('12:00');
    expect(slots[1].booking?.title).toBe('Strategy Sync');

    expect(slots[2].type).toBe('available');
    expect(slots[2].startFormatted).toBe('12:00');
    expect(slots[2].endFormatted).toBe('20:00');
  });

  it('should update date when navigating to next or previous day', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    app.selectedDate.set('2026-11-20');

    app.goToNextDay();
    expect(app.selectedDate()).toBe('2026-11-21');

    app.goToPreviousDay();
    expect(app.selectedDate()).toBe('2026-11-20');
  });
});
