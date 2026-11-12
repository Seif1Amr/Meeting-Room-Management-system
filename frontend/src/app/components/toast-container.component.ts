import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-wrapper" *ngIf="toastService.toasts().length > 0">
      <div
        *ngFor="let toast of toastService.toasts()"
        class="toast-item animate-fade-in"
        [class.toast-success]="toast.type === 'success'"
        [class.toast-error]="toast.type === 'error'"
        [class.toast-info]="toast.type === 'info'"
      >
        <div class="toast-icon">
          <!-- Success Checkmark -->
          <svg *ngIf="toast.type === 'success'" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="20 6 9 17 4 12"></polyline>
          </svg>
          <!-- Error Alert Triangle -->
          <svg *ngIf="toast.type === 'error'" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="8" x2="12" y2="12"></line>
            <line x1="12" y1="16" x2="12.01" y2="16"></line>
          </svg>
          <!-- Info Circle -->
          <svg *ngIf="toast.type === 'info'" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="16" x2="12" y2="12"></line>
            <line x1="12" y1="8" x2="12.01" y2="8"></line>
          </svg>
        </div>

        <div class="toast-content">
          <div class="toast-title" *ngIf="toast.title">{{ toast.title }}</div>
          <div class="toast-message">{{ toast.message }}</div>
        </div>

        <button class="toast-close" (click)="toastService.remove(toast.id)" aria-label="Close notification">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>
      </div>
    </div>
  `,
  styles: [`
    .toast-wrapper {
      position: fixed;
      bottom: 24px;
      right: 24px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 420px;
      width: calc(100vw - 48px);
      pointer-events: none;
    }

    .toast-item {
      pointer-events: auto;
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 14px 16px;
      border-radius: var(--radius-md);
      background: #FFFFFF;
      box-shadow: var(--shadow-xl);
      border: 1px solid var(--border-subtle);
      transition: all var(--transition-fast);
    }

    .toast-icon {
      flex-shrink: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-top: 2px;
    }

    .toast-content {
      flex: 1;
      min-width: 0;
    }

    .toast-title {
      font-weight: 600;
      font-size: 13.5px;
      line-height: 1.3;
      margin-bottom: 2px;
    }

    .toast-message {
      font-size: 13px;
      color: var(--text-secondary);
      line-height: 1.4;
      word-break: break-word;
    }

    .toast-close {
      flex-shrink: 0;
      color: var(--text-muted);
      padding: 2px;
      border-radius: 4px;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: color var(--transition-fast);
    }

    .toast-close:hover {
      color: var(--text-primary);
    }

    .toast-success {
      border-left: 4px solid var(--accent-emerald);
    }
    .toast-success .toast-icon {
      color: var(--accent-emerald);
    }

    .toast-error {
      border-left: 4px solid var(--accent-rose);
    }
    .toast-error .toast-icon {
      color: var(--accent-rose);
    }

    .toast-info {
      border-left: 4px solid var(--accent-primary);
    }
    .toast-info .toast-icon {
      color: var(--accent-primary);
    }
  `]
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);
}
