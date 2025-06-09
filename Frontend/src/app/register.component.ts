import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  email = '';
  fullName = '';
  password = '';
  error = '';
  message = '';

  emailError = '';
  fullNameError = '';
  passwordError = '';

  // Inject HttpClient here
  constructor(private http: HttpClient) {}

  onInput(field: 'email' | 'fullName' | 'password', event: Event) {
    const input = event.target as HTMLInputElement | null;
    if (input) this[field] = input.value;
  }

  validateEmail() {
    if (!this.email) {
      this.emailError = 'Email is required.';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email)) {
      this.emailError = 'Invalid email format.';
    } else {
      this.emailError = '';
    }
  }

  validateFullName() {
    if (!this.fullName) {
      this.fullNameError = 'Full name is required.';
    } else if (this.fullName.length < 3) {
      this.fullNameError = 'Full name must be at least 3 characters.';
    } else {
      this.fullNameError = '';
    }
  }

  validatePassword() {
    if (!this.password) {
      this.passwordError = 'Password is required.';
    } else if (this.password.length < 3) {
      this.passwordError = 'Password must be at least 3 characters.';
    } else {
      this.passwordError = '';
    }
  }

  isFormValid() {
    return (
      this.email &&
      !this.emailError &&
      this.fullName &&
      !this.fullNameError &&
      this.password &&
      !this.passwordError
    );
  }

  register() {
    this.validateEmail();
    this.validateFullName();
    this.validatePassword();

    if (!this.isFormValid()) {
      this.error = 'Please fix validation errors.';
      return;
    }

    this.error = '';
    this.http.post('https://localhost:7297/api/Auth/register', {
      email: this.email,
      fullName: this.fullName,
      password: this.password
    }).subscribe({
      next: () => {
        this.message = 'Registration successful. Check your email for confirmation link.';
        this.error = '';
      },
      error: (err: HttpErrorResponse) => {
        this.error = err.error?.message || err.error || 'Registration failed.';
        this.message = '';
      }
    });
  }
}
