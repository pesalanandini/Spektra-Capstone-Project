import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent {
  token = '';
  password = '';
  confirmPassword = '';
  error = '';
  message = '';

  constructor(private http: HttpClient) {}

  onInput(field: 'token' | 'password' | 'confirmPassword', event: Event) {
    const input = event.target as HTMLInputElement | null;
    if (input) this[field] = input.value;
  }

  reset() {
    if (!this.token || !this.password || !this.confirmPassword) {
      this.error = 'All fields are required.';
      return;
    }
    if (this.password !== this.confirmPassword) {
      this.error = 'Passwords do not match.';
      return;
    }

    this.error = '';
    this.http.post('/api/auth/reset-password', {
      token: this.token,
      newPassword: this.password
    }).subscribe({
      next: () => {
        this.message = 'Password reset successful!';
        this.error = '';
      },
      error: (err: HttpErrorResponse) => {
        this.error = err.error?.message || err.error || 'Reset failed.';
        this.message = '';
      }
    });
  }
}
