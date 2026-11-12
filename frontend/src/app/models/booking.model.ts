export interface Booking {
  id: number;
  roomId: number;
  roomName: string;
  title: string;
  bookedBy: string;
  startTime: string; // ISO 8601 string
  endTime: string;   // ISO 8601 string
  createdAt: string;
}

export interface CreateBookingRequest {
  roomId: number;
  title: string;
  bookedBy: string;
  startTime: string; // ISO 8601 string
  endTime: string;   // ISO 8601 string
}

export interface AvailableRoomsQuery {
  date: string;       // YYYY-MM-DD
  startTime: string;  // HH:mm
  endTime: string;    // HH:mm
  minCapacity?: number | null;
}
