import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, inject } from '@angular/core';
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
export class BookingModalComponent implements OnInit, OnChanges {
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
  formSubmitted = false;
  apiErrorMessage: string | null = null;
  apiErrorCode: string | null = null;

  // 15-minute slot intervals between 08:00 and 20:00
  timeOptions: string[] = [];

  ngOnInit(): void {
    this.generateTimeOptions();
    this.initForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.bookingForm) {
      if (changes['selectedRoomId'] && this.selectedRoomId !== null && this.selectedRoomId !== undefined) {
        this.bookingForm.patchValue({ roomId: Number(this.selectedRoomId) });
      }
      if (changes['initialDate'] && this.initialDate) {
        this.bookingForm.patchValue({ date: this.initialDate });
      }
      if (changes['initialStartTime'] && this.initialStartTime) {
        this.bookingForm.patchValue({ startTime: this.initialStartTime });
      }
      if (changes['initialEndTime'] && this.initialEndTime) {
        this.bookingForm.patchValue({ endTime: this.initialEndTime });
      }
    }
  }

  private initForm(): void {
    const now = new Date();
    const todayStr = now.toISOString().split('T')[0];
    let defaultDate = this.initialDate || todayStr;
    let defaultStart = this.initialStartTime || '09:00';
    let defaultEnd = this.initialEndTime || '10:00';

    // If defaultDate is today, ensure default start time is not already in the past
    if (defaultDate === todayStr) {
      const [sh, sm] = defaultStart.split(':').map(Number);
      const startMinutes = sh * 60 + sm;
      const currentMinutes = now.getHours() * 60 + now.getMinutes();

      if (startMinutes <= currentMinutes) {
        if (now.getHours() >= 19) {
          const tomorrow = new Date(now);
          tomorrow.setDate(tomorrow.getDate() + 1);
          defaultDate = tomorrow.toISOString().split('T')[0];
          defaultStart = '09:00';
          defaultEnd = '10:00';
        } else {
          const nextH = Math.max(8, now.getHours() + 1);
          const endH = Math.min(nextH + 1, 20);
          defaultStart = `${nextH.toString().padStart(2, '0')}:00`;
          defaultEnd = `${endH.toString().padStart(2, '0')}:00`;
        }
      }
    }

    const defaultRoomId = this.selectedRoomId ?? (this.rooms.length > 0 ? this.rooms[0].id : null);

    this.bookingForm = this.fb.group({
      roomId: [defaultRoomId, [Validators.required]],
      title: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      bookedBy: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      date: [defaultDate, [Validators.required]],
      startTime: [defaultStart, [Validators.required]],
      endTime: [defaultEnd, [Validators.required]]
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

    // Rule: Cannot book in past (with 2 min buffer)
    const selectedStart = new Date(`${dateStr}T${startStr}:00`);
    const now = new Date();
    if (selectedStart.getTime() < now.getTime() - 120000) {
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
    this.formSubmitted = true;

    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();

      const errors = this.bookingForm.errors;
      if (errors?.['pastDate']) {
        this.apiErrorMessage = 'Cannot schedule meetings in the past. Please select a future date or time.';
        this.toastService.warning('Cannot schedule meetings in the past.', 'Validation Warning');
      } else if (errors?.['endBeforeStart']) {
        this.apiErrorMessage = 'End time must be strictly after start time.';
        this.toastService.warning('End time must be strictly after start time.', 'Validation Warning');
      } else if (errors?.['outsideWorkingHours']) {
        this.apiErrorMessage = 'Bookings must be scheduled between 08:00 and 20:00.';
        this.toastService.warning('Bookings must be between 08:00 and 20:00.', 'Validation Warning');
      } else if (errors?.['exceedsMaxDuration']) {
        this.apiErrorMessage = 'Duration cannot exceed 4 hours.';
        this.toastService.warning('Duration cannot exceed 4 hours.', 'Validation Warning');
      } else {
        this.apiErrorMessage = 'Please check the required fields (Title, Organizer, Room).';
        this.toastService.warning('Please complete all required fields.', 'Validation Warning');
      }
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
