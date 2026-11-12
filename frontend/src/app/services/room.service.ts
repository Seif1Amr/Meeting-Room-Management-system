import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Room } from '../models/room.model';
import { AvailableRoomsQuery } from '../models/booking.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RoomService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/rooms`;

  getAllRooms(): Observable<Room[]> {
    return this.http.get<Room[]>(this.baseUrl);
  }

  getRoomById(id: number): Observable<Room> {
    return this.http.get<Room>(`${this.baseUrl}/${id}`);
  }

  getAvailableRooms(query: AvailableRoomsQuery): Observable<Room[]> {
    let params = new HttpParams()
      .set('date', query.date)
      .set('startTime', query.startTime)
      .set('endTime', query.endTime);

    if (query.minCapacity && query.minCapacity > 0) {
      params = params.set('minCapacity', query.minCapacity.toString());
    }

    return this.http.get<Room[]>(`${this.baseUrl}/available`, { params });
  }
}
