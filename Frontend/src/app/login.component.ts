import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule, HttpClient, HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email = '';
  password = '';
  role = 'user'; // default role
  error = '';

  constructor(private http: HttpClient) {}

  onInput(field: 'email' | 'password' | 'role', event: Event) {
    const input = event.target as HTMLInputElement | HTMLSelectElement | null;
    if (input) this[field] = input.value;
  }

  login() {
    if (!this.email || !this.password) {
      this.error = 'Email and password are required.';
      return;
    }

    this.error = '';

    this.http.post<{ token: string }>('https://localhost:7297/api/Auth/login', {
      email: this.email,
      password: this.password,
      role: this.role
    }).subscribe({
      next: (res) => {
        localStorage.setItem('jwt', res.token);
        window.location.href = '/';
      },
      error: (err: HttpErrorResponse) => {
        // Extract and show only meaningful error message
        if (err.error && typeof err.error === 'object' && 'message' in err.error) {
          this.error = err.error.message;
        } else if (typeof err.error === 'string') {
          this.error = err.error;
        } else {
          this.error = 'Invalid email or password.';
        }
      }
    });
  }
}
