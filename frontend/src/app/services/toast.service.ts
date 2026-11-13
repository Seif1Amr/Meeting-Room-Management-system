import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  title?: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private readonly _toasts = signal<ToastMessage[]>([]);
  public readonly toasts = this._toasts.asReadonly();

  success(message: string, title: string = 'Success'): void {
    this.addToast('success', message, title);
  }

  error(message: string, title: string = 'Error'): void {
    this.addToast('error', message, title);
  }

  warning(message: string, title: string = 'Warning'): void {
    this.addToast('warning', message, title);
  }

  info(message: string, title: string = 'Notice'): void {
    this.addToast('info', message, title);
  }

  remove(id: string): void {
    this._toasts.update(current => current.filter(t => t.id !== id));
  }

  private addToast(type: 'success' | 'error' | 'info' | 'warning', message: string, title: string): void {
    const id = Math.random().toString(36).substring(2, 9);
    const newToast: ToastMessage = { id, type, title, message };
    this._toasts.update(current => [...current, newToast]);

    setTimeout(() => {
      this.remove(id);
    }, 4500);
  }
}
