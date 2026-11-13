import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Room } from './models/room.model';
import { Booking, AvailableRoomsQuery } from './models/booking.model';
import { TimelineSlot } from './models/timeline.model';
import { RoomService } from './services/room.service';
import { BookingService } from './services/booking.service';
import { ToastService } from './services/toast.service';
import { BookingModalComponent } from './components/booking-modal.component';
import { CancelDialogComponent } from './components/cancel-dialog.component';
import { ToastContainerComponent } from './components/toast-container.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    BookingModalComponent,
    CancelDialogComponent,
    ToastContainerComponent
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App implements OnInit {
  private readonly roomService = inject(RoomService);
  private readonly bookingService = inject(BookingService);
  readonly toastService = inject(ToastService);

  // Rooms state
  rooms = signal<Room[]>([]);
  availableRooms = signal<Room[]>([]);
  filteredRooms = signal<Room[]>([]);
  selectedRoom = signal<Room | null>(null);
  isLoadingRooms = signal<boolean>(false);
  searchQuery = signal<string>('');

  // Selected schedule state
  selectedDate = signal<string>(new Date().toISOString().split('T')[0]);
  bookings = signal<Booking[]>([]);
  timelineSlots = signal<TimelineSlot[]>([]);
  isLoadingBookings = signal<boolean>(false);

  // Availability filter form state
  filterDate: string = new Date().toISOString().split('T')[0];
  filterStartTime: string = '09:00';
  filterEndTime: string = '11:00';
  filterMinCapacity: number | string | null = null;
  isFilterActive = signal<boolean>(false);
  isSearchingAvailable = signal<boolean>(false);

  // Modals state
  isBookingModalOpen = signal<boolean>(false);
  modalInitialDate: string = new Date().toISOString().split('T')[0];
  modalInitialStartTime: string = '09:00';
  modalInitialEndTime: string = '10:00';

  bookingToCancel = signal<Booking | null>(null);
  isCancelling = signal<boolean>(false);

  // 15-min options
  timeOptions: string[] = [];

  ngOnInit(): void {
    this.generateTimeOptions();
    this.initDefaultFilter();
    this.loadRooms();
  }

  initDefaultFilter(): void {
    const now = new Date();
    const currentHour = now.getHours();
    let initialDate = now.toISOString().split('T')[0];
    let initialStart = '09:00';
    let initialEnd = '11:00';

    if (currentHour >= 19) {
      // If evening, default availability search and timeline view to tomorrow morning 09:00 - 11:00
      const tomorrow = new Date(now);
      tomorrow.setDate(tomorrow.getDate() + 1);
      initialDate = tomorrow.toISOString().split('T')[0];
    } else if (currentHour >= 8 && currentHour < 19) {
      // If during working hours, default to next hour
      const nextH = currentHour + 1;
      const endH = Math.min(nextH + 1, 20);
      initialStart = `${nextH.toString().padStart(2, '0')}:00`;
      initialEnd = `${endH.toString().padStart(2, '0')}:00`;
    }

    this.selectedDate.set(initialDate);
    this.modalInitialDate = initialDate;
    this.filterDate = initialDate;
    this.filterStartTime = initialStart;
    this.filterEndTime = initialEnd;
  }

  generateTimeOptions(): void {
    const list: string[] = [];
    for (let h = 8; h <= 20; h++) {
      for (let m = 0; m < 60; m += 30) {
        if (h === 20 && m > 0) break;
        const hh = h.toString().padStart(2, '0');
        const mm = m.toString().padStart(2, '0');
        list.push(`${hh}:${mm}`);
      }
    }
    this.timeOptions = list;
  }

  loadRooms(): void {
    this.isLoadingRooms.set(true);
    this.roomService.getAllRooms().subscribe({
      next: (data) => {
        this.rooms.set(data);
        this.availableRooms.set(data);
        this.applyFilterAndSearch();
        this.isLoadingRooms.set(false);

        // Auto-select first room if none selected
        if (!this.selectedRoom() && data.length > 0) {
          this.selectRoom(data[0]);
        }
      },
      error: () => {
        this.isLoadingRooms.set(false);
        this.toastService.error('Failed to connect to backend API. Please ensure the server is running.', 'Connection Error');
      }
    });
  }

  selectRoom(room: Room): void {
    this.selectedRoom.set(room);
    this.loadRoomSchedule(room.id, this.selectedDate());
  }

  loadRoomSchedule(roomId: number, date: string): void {
    this.isLoadingBookings.set(true);
    this.bookingService.getBookingsForRoom(roomId, date).subscribe({
      next: (data) => {
        this.bookings.set(data);
        this.computeTimelineSlots(data, date);
        this.isLoadingBookings.set(false);
      },
      error: (err) => {
        this.isLoadingBookings.set(false);
        this.toastService.error(err.error?.message || 'Failed to load room schedule.', 'Schedule Error');
      }
    });
  }

  computeTimelineSlots(roomBookings: Booking[], dateStr: string): void {
    const slots: TimelineSlot[] = [];

    // Working hours: 08:00 to 20:00
    const [year, month, day] = dateStr.split('-').map(Number);
    let cursor = new Date(year, month - 1, day, 8, 0, 0);
    const dayEnd = new Date(year, month - 1, day, 20, 0, 0);

    // Bookings are guaranteed ordered by StartTime ascending from API
    for (const booking of roomBookings) {
      const bStart = new Date(booking.startTime);
      const bEnd = new Date(booking.endTime);

      // Gap before booking
      if (bStart.getTime() > cursor.getTime()) {
        const gapMins = Math.round((bStart.getTime() - cursor.getTime()) / 60000);
        if (gapMins >= 15) {
          slots.push({
            type: 'available',
            startFormatted: this.formatTime(cursor),
            endFormatted: this.formatTime(bStart),
            startDate: new Date(cursor),
            endDate: new Date(bStart),
            durationMinutes: gapMins
          });
        }
      }

      // Booked interval
      const bDurationMins = Math.round((bEnd.getTime() - bStart.getTime()) / 60000);
      slots.push({
        type: 'booked',
        startFormatted: this.formatTime(bStart),
        endFormatted: this.formatTime(bEnd),
        startDate: bStart,
        endDate: bEnd,
        durationMinutes: bDurationMins,
        booking
      });

      if (bEnd.getTime() > cursor.getTime()) {
        cursor = bEnd;
      }
    }

    // Gap after last booking until 20:00
    if (cursor.getTime() < dayEnd.getTime()) {
      const remainingMins = Math.round((dayEnd.getTime() - cursor.getTime()) / 60000);
      if (remainingMins >= 15) {
        slots.push({
          type: 'available',
          startFormatted: this.formatTime(cursor),
          endFormatted: this.formatTime(dayEnd),
          startDate: new Date(cursor),
          endDate: dayEnd,
          durationMinutes: remainingMins
        });
      }
    }

    this.timelineSlots.set(slots);
  }

  // Date Navigation
  onDateChange(newDate: string): void {
    this.selectedDate.set(newDate);
    this.modalInitialDate = newDate;
    this.filterDate = newDate;
    if (this.selectedRoom()) {
      this.loadRoomSchedule(this.selectedRoom()!.id, newDate);
    }
  }

  goToPreviousDay(): void {
    const current = new Date(this.selectedDate());
    current.setDate(current.getDate() - 1);
    this.onDateChange(current.toISOString().split('T')[0]);
  }

  goToNextDay(): void {
    const current = new Date(this.selectedDate());
    current.setDate(current.getDate() + 1);
    this.onDateChange(current.toISOString().split('T')[0]);
  }

  goToToday(): void {
    const today = new Date().toISOString().split('T')[0];
    this.onDateChange(today);
  }

  // Text search in rooms list
  onSearchQueryChange(query: string): void {
    this.searchQuery.set(query);
    this.applyFilterAndSearch();
  }

  applyFilterAndSearch(): void {
    const baseList = this.isFilterActive() ? this.availableRooms() : this.rooms();
    const query = this.searchQuery().trim().toLowerCase();

    if (!query) {
      this.filteredRooms.set(baseList);
      return;
    }

    const matched = baseList.filter(room =>
      room.name.toLowerCase().includes(query) ||
      `floor ${room.floor}`.toLowerCase().includes(query) ||
      `${room.capacity} seats`.toLowerCase().includes(query) ||
      `${room.capacity}` === query
    );

    this.filteredRooms.set(matched);
  }

  // Availability Search
  searchAvailableRooms(): void {
    const [startH, startM] = this.filterStartTime.split(':').map(Number);
    const [endH, endM] = this.filterEndTime.split(':').map(Number);
    const startMins = startH * 60 + startM;
    const endMins = endH * 60 + endM;

    if (endMins <= startMins) {
      this.toastService.error('End time must be strictly after start time.', 'Invalid Search Slot');
      return;
    }

    if (endMins - startMins > 240) {
      this.toastService.error('Search slot duration cannot exceed 4 hours.', 'Invalid Search Slot');
      return;
    }

    let safeMinCap: number | null = null;
    if (this.filterMinCapacity !== null && this.filterMinCapacity !== undefined && this.filterMinCapacity !== '') {
      const parsed = parseInt(String(this.filterMinCapacity).trim(), 10);
      if (!isNaN(parsed) && parsed > 0) {
        safeMinCap = parsed;
      }
    }

    this.isSearchingAvailable.set(true);
    const query: AvailableRoomsQuery = {
      date: this.filterDate,
      startTime: this.filterStartTime,
      endTime: this.filterEndTime,
      minCapacity: safeMinCap
    };

    this.roomService.getAvailableRooms(query).subscribe({
      next: (avail) => {
        this.isSearchingAvailable.set(false);
        this.availableRooms.set(avail);
        this.isFilterActive.set(true);
        this.applyFilterAndSearch();

        // 1. Synchronize timeline view date to the requested search date
        this.selectedDate.set(this.filterDate);
        this.modalInitialDate = this.filterDate;

        // 2. Load schedule for searched date
        if (avail.length > 0) {
          const current = this.selectedRoom();
          const targetRoom = (current && avail.some(r => r.id === current.id)) ? current : avail[0];
          this.selectedRoom.set(targetRoom);
          this.loadRoomSchedule(targetRoom.id, this.filterDate);
          this.toastService.success(`Found ${avail.length} free room(s) for the requested slot.`, 'Availability Found');
        } else {
          if (this.selectedRoom()) {
            this.loadRoomSchedule(this.selectedRoom()!.id, this.filterDate);
          }
          this.toastService.info('No rooms are completely free for that time slot and capacity.', 'No Rooms Available');
        }
      },
      error: (err) => {
        this.isSearchingAvailable.set(false);
        this.toastService.error(err.error?.message || 'Failed to search available rooms.', 'Search Failed');
      }
    });
  }

  clearFilter(): void {
    this.availableRooms.set(this.rooms());
    this.isFilterActive.set(false);
    this.filterMinCapacity = null;
    this.applyFilterAndSearch();
  }

  // Quick reservation modal
  openBookingModal(startTime?: string, endTime?: string, slotDate?: string): void {
    const targetDate = slotDate || this.selectedDate();
    this.modalInitialDate = targetDate;

    if (startTime && endTime) {
      this.modalInitialStartTime = startTime;
      this.modalInitialEndTime = endTime;
    } else if (this.isFilterActive()) {
      this.modalInitialStartTime = this.filterStartTime;
      this.modalInitialEndTime = this.filterEndTime;
    } else {
      const now = new Date();
      const todayStr = now.toISOString().split('T')[0];
      if (targetDate === todayStr) {
        const curH = now.getHours();
        if (curH >= 19) {
          this.modalInitialStartTime = '09:00';
          this.modalInitialEndTime = '10:00';
        } else {
          const nextH = Math.max(8, curH + 1);
          const endH = Math.min(nextH + 1, 20);
          this.modalInitialStartTime = `${nextH.toString().padStart(2, '0')}:00`;
          this.modalInitialEndTime = `${endH.toString().padStart(2, '0')}:00`;
        }
      } else {
        this.modalInitialStartTime = '09:00';
        this.modalInitialEndTime = '10:00';
      }
    }

    this.isBookingModalOpen.set(true);
  }

  onBookingCreated(): void {
    if (this.selectedRoom()) {
      this.loadRoomSchedule(this.selectedRoom()!.id, this.selectedDate());
    }
  }

  // Cancellation flow
  openCancelDialog(booking: Booking, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.bookingToCancel.set(booking);
  }

  confirmCancelBooking(booking: Booking): void {
    this.isCancelling.set(true);
    this.bookingService.deleteBooking(booking.id).subscribe({
      next: () => {
        this.isCancelling.set(false);
        this.bookingToCancel.set(null);
        this.toastService.success(`Reservation "${booking.title}" has been cancelled.`, 'Reservation Released');
        if (this.selectedRoom()) {
          this.loadRoomSchedule(this.selectedRoom()!.id, this.selectedDate());
        }
      },
      error: (err) => {
        this.isCancelling.set(false);
        this.toastService.error(err.error?.message || 'Failed to cancel reservation.', 'Cancellation Failed');
      }
    });
  }

  // Helpers
  formatTime(date: Date): string {
    const hh = date.getHours().toString().padStart(2, '0');
    const mm = date.getMinutes().toString().padStart(2, '0');
    return `${hh}:${mm}`;
  }

  formatDuration(mins: number): string {
    const hours = Math.floor(mins / 60);
    const m = mins % 60;
    if (hours > 0 && m > 0) return `${hours} hr ${m} min`;
    if (hours > 0) return `${hours} hr`;
    return `${m} min`;
  }
}
