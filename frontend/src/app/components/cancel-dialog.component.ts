import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Booking } from '../models/booking.model';

@Component({
  selector: 'app-cancel-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="modal-backdrop animate-fade-in" (click)="dismiss()">
      <div class="dialog-card" (click)="$event.stopPropagation()">
        <div class="dialog-icon-wrapper">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="3 6 5 6 21 6"></polyline>
            <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
            <line x1="10" y1="11" x2="10" y2="17"></line>
            <line x1="14" y1="11" x2="14" y2="17"></line>
          </svg>
        </div>

        <div class="dialog-body">
          <h3 class="dialog-title">Cancel Meeting Reservation?</h3>
          <p class="dialog-desc">
            Are you sure you want to cancel <strong>"{{ booking?.title }}"</strong>?
            This will immediately release the room slot for other team members.
          </p>

          <div class="booking-meta-card" *ngIf="booking">
            <div class="meta-row">
              <span class="meta-label">Room:</span>
              <span class="meta-value">{{ booking.roomName || 'Meeting Room' }}</span>
            </div>
            <div class="meta-row">
              <span class="meta-label">Time:</span>
              <span class="meta-value tabular">{{ formatTime(booking.startTime) }} – {{ formatTime(booking.endTime) }}</span>
            </div>
            <div class="meta-row">
              <span class="meta-label">Booked By:</span>
              <span class="meta-value">{{ booking.bookedBy }}</span>
            </div>
          </div>
        </div>

        <div class="dialog-actions">
          <button type="button" class="btn-cancel" (click)="dismiss()" [disabled]="isDeleting">
            Keep Reservation
          </button>
          <button type="button" class="btn-confirm-delete" (click)="confirm()" [disabled]="isDeleting">
            <span class="btn-spinner" *ngIf="isDeleting"></span>
            <span>{{ isDeleting ? 'Releasing Room...' : 'Yes, Cancel Meeting' }}</span>
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-backdrop {
      position: fixed;
      inset: 0;
      background-color: rgba(15, 23, 42, 0.45);
      backdrop-filter: blur(4px);
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 16px;
    }

    .dialog-card {
      background: #FFFFFF;
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-xl);
      border: 1px solid var(--border-subtle);
      width: 100%;
      max-width: 440px;
      padding: 24px;
    }

    .dialog-icon-wrapper {
      width: 48px;
      height: 48px;
      border-radius: 50%;
      background: var(--accent-rose-subtle);
      color: var(--accent-rose);
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 16px;
    }

    .dialog-title {
      font-family: var(--font-display);
      font-size: 18px;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 8px;
    }

    .dialog-desc {
      font-size: 13.5px;
      color: var(--text-secondary);
      line-height: 1.5;
      margin-bottom: 16px;
    }

    .booking-meta-card {
      background: var(--bg-card-subtle);
      border-radius: var(--radius-md);
      padding: 12px 14px;
      margin-bottom: 20px;
      display: flex;
      flex-direction: column;
      gap: 6px;
      border: 1px solid var(--border-subtle);
    }

    .meta-row {
      display: flex;
      justify-content: space-between;
      font-size: 13px;
    }

    .meta-label {
      color: var(--text-muted);
      font-weight: 500;
    }

    .meta-value {
      color: var(--text-primary);
      font-weight: 600;
    }

    .dialog-actions {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 12px;
    }

    .btn-cancel {
      height: 38px;
      padding: 0 16px;
      border-radius: var(--radius-md);
      border: 1px solid var(--border-subtle);
      background: #FFFFFF;
      color: var(--text-secondary);
      font-size: 13px;
      font-weight: 600;
      transition: all var(--transition-fast);
    }

    .btn-cancel:hover:not(:disabled) {
      background: var(--bg-card-subtle);
      color: var(--text-primary);
    }

    .btn-confirm-delete {
      height: 38px;
      padding: 0 16px;
      border-radius: var(--radius-md);
      background: var(--accent-rose);
      color: #FFFFFF;
      font-size: 13px;
      font-weight: 600;
      display: inline-flex;
      align-items: center;
      gap: 8px;
      box-shadow: var(--shadow-sm);
      transition: all var(--transition-fast);
    }

    .btn-confirm-delete:hover:not(:disabled) {
      background: #BE123C;
    }

    .btn-confirm-delete:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .btn-spinner {
      width: 14px;
      height: 14px;
      border: 2px solid rgba(255, 255, 255, 0.4);
      border-top-color: #FFFFFF;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    @media (max-width: 640px) {
      .modal-backdrop {
        padding: 8px;
        align-items: flex-end;
      }
      .dialog-card {
        padding: 20px 16px;
        border-bottom-left-radius: 0;
        border-bottom-right-radius: 0;
        width: 100%;
      }
      .dialog-actions {
        flex-direction: column-reverse;
        gap: 10px;
        width: 100%;
      }
      .btn-cancel, .btn-confirm-delete {
        width: 100%;
        height: 46px;
        justify-content: center;
        font-size: 14px;
        touch-action: manipulation;
      }
    }
  `]
})
export class CancelDialogComponent {
  @Input() booking: Booking | null = null;
  @Input() isDeleting = false;
  @Output() confirmed = new EventEmitter<Booking>();
  @Output() cancelled = new EventEmitter<void>();

  formatTime(isoString: string): string {
    if (!isoString) return '';
    const date = new Date(isoString);
    const hh = date.getHours().toString().padStart(2, '0');
    const mm = date.getMinutes().toString().padStart(2, '0');
    return `${hh}:${mm}`;
  }

  confirm(): void {
    if (this.booking) {
      this.confirmed.emit(this.booking);
    }
  }

  dismiss(): void {
    this.cancelled.emit();
  }
}
