import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Booking, CreateBookingRequest } from '../models/booking.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/bookings`;
  private readonly roomsUrl = `${environment.apiUrl}/rooms`;

  getBookingsForRoom(roomId: number, date: string): Observable<Booking[]> {
    const params = new HttpParams().set('date', date);
    return this.http.get<Booking[]>(`${this.roomsUrl}/${roomId}/bookings`, { params });
  }

  createBooking(request: CreateBookingRequest): Observable<Booking> {
    return this.http.post<Booking>(this.baseUrl, request);
  }

  deleteBooking(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
