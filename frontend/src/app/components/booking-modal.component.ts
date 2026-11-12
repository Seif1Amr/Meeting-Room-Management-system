import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Room } from '../models/room.model';
import { BookingService } from '../services/booking.service';
import { ToastService } from '../services/toast.service';
import { ApiErrorResponse } from '../models/api-error.model';

@Component({
  selector: 'app-booking-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './booking-modal.component.html',
  styleUrls: ['./booking-modal.component.css']
})
export class BookingModalComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly bookingService = inject(BookingService);
  private readonly toastService = inject(ToastService);

  @Input() rooms: Room[] = [];
  @Input() selectedRoomId: number | null = null;
  @Input() initialDate: string = '';
  @Input() initialStartTime: string = '09:00';
  @Input() initialEndTime: string = '10:00';

  @Output() closed = new EventEmitter<void>();
  @Output() bookingCreated = new EventEmitter<void>();

  bookingForm!: FormGroup;
  isSubmitting = false;
  apiErrorMessage: string | null = null;
  apiErrorCode: string | null = null;

  // 15-minute slot intervals between 08:00 and 20:00
  timeOptions: string[] = [];

  ngOnInit(): void {
    this.generateTimeOptions();

    const todayStr = this.initialDate || new Date().toISOString().split('T')[0];

    this.bookingForm = this.fb.group({
      roomId: [this.selectedRoomId || (this.rooms.length > 0 ? this.rooms[0].id : null), [Validators.required]],
      title: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      bookedBy: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      date: [todayStr, [Validators.required]],
      startTime: [this.initialStartTime || '09:00', [Validators.required]],
      endTime: [this.initialEndTime || '10:00', [Validators.required]]
    }, {
      validators: [this.validateTimeRules]
    });
  }

  generateTimeOptions(): void {
    const options: string[] = [];
    for (let h = 8; h <= 20; h++) {
      for (let m = 0; m < 60; m += 15) {
        if (h === 20 && m > 0) break; // Ends at 20:00
        const hh = h.toString().padStart(2, '0');
        const mm = m.toString().padStart(2, '0');
        options.push(`${hh}:${mm}`);
      }
    }
    this.timeOptions = options;
  }

  // Dual-layer client-side validation
  validateTimeRules(group: FormGroup): Record<string, boolean> | null {
    const dateStr = group.get('date')?.value;
    const startStr = group.get('startTime')?.value;
    const endStr = group.get('endTime')?.value;

    if (!dateStr || !startStr || !endStr) {
      return null;
    }

    const [startH, startM] = startStr.split(':').map(Number);
    const [endH, endM] = endStr.split(':').map(Number);

    const startTotal = startH * 60 + startM;
    const endTotal = endH * 60 + endM;

    // Rule: End must be strictly after start
    if (endTotal <= startTotal) {
      return { endBeforeStart: true };
    }

    // Rule: Within working hours (08:00 to 20:00)
    if (startTotal < 8 * 60 || endTotal > 20 * 60) {
      return { outsideWorkingHours: true };
    }

    // Rule: Max duration 4 hours (240 minutes)
    if (endTotal - startTotal > 240) {
      return { exceedsMaxDuration: true };
    }

    // Rule: Cannot book in past
    const selectedStart = new Date(`${dateStr}T${startStr}:00`);
    const now = new Date();
    if (selectedStart.getTime() < now.getTime() - 60000) { // 1 min buffer
      return { pastDate: true };
    }

    return null;
  }

  get calculatedDurationFormatted(): string {
    const startStr = this.bookingForm?.get('startTime')?.value;
    const endStr = this.bookingForm?.get('endTime')?.value;
    if (!startStr || !endStr) return '';

    const [startH, startM] = startStr.split(':').map(Number);
    const [endH, endM] = endStr.split(':').map(Number);
    const diff = (endH * 60 + endM) - (startH * 60 + startM);
    if (diff <= 0) return '';

    const hours = Math.floor(diff / 60);
    const mins = diff % 60;
    if (hours > 0 && mins > 0) return `${hours} hr ${mins} min`;
    if (hours > 0) return `${hours} hr`;
    return `${mins} min`;
  }

  submitBooking(): void {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    this.apiErrorMessage = null;
    this.apiErrorCode = null;
    this.isSubmitting = true;

    const val = this.bookingForm.value;
    const startDateTime = `${val.date}T${val.startTime}:00`;
    const endDateTime = `${val.date}T${val.endTime}:00`;

    this.bookingService.createBooking({
      roomId: Number(val.roomId),
      title: val.title.trim(),
      bookedBy: val.bookedBy.trim(),
      startTime: startDateTime,
      endTime: endDateTime
    }).subscribe({
      next: (created) => {
        this.isSubmitting = false;
        this.toastService.success(
          `Meeting "${created.title}" successfully booked in ${created.roomName || 'selected room'}.`,
          'Booking Confirmed'
        );
        this.bookingCreated.emit();
        this.closed.emit();
      },
      error: (err) => {
        this.isSubmitting = false;
        const errPayload: ApiErrorResponse = err.error || {};
        this.apiErrorMessage = errPayload.message || 'Unable to complete booking. Please check details.';
        this.apiErrorCode = errPayload.code || 'REQUEST_FAILED';

        if (err.status === 409) {
          this.toastService.error('Time slot is already occupied. Please pick another time or room.', 'Conflict Detected');
        } else {
          this.toastService.error(this.apiErrorMessage, 'Booking Error');
        }
      }
    });
  }

  close(): void {
    this.closed.emit();
  }
}
