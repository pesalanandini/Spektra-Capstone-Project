import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
  email = '';
  message = '';
  error = '';

  constructor(private http: HttpClient) {}

  onInput(event: Event) {
    const input = event.target as HTMLInputElement | null;
    if (input) this.email = input.value;
  }

  sendReset() {
    if (!this.email.trim()) {
      this.error = 'Email is required.';
      this.message = '';
      return;
    }

    this.error = '';
    this.message = '';

    this.http.post('https://localhost:7297/api/Auth/forgot-password', { email: this.email }).subscribe({
      next: () => {
        this.message = 'If the email exists, a password reset token has been sent to your inbox.';
      },
      error: (err: HttpErrorResponse) => {
        this.error = err.error?.message || err.error || 'Request failed.';
      }
    });
  }
}
