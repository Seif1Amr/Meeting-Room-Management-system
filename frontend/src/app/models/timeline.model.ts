import { Booking } from './booking.model';

export interface TimelineSlot {
  type: 'booked' | 'available';
  startFormatted: string; // e.g., "09:00"
  endFormatted: string;   // e.g., "11:00"
  startDate: Date;
  endDate: Date;
  durationMinutes: number;
  booking?: Booking;
}
