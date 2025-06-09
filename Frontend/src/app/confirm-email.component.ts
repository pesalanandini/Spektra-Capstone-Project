import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './confirm-email.component.html',
  styleUrl: './confirm-email.component.css'
})
export class ConfirmEmailComponent {
  loading = true;
  successMessage = '';
  errorMessage = '';
  manualToken = '';

  constructor(private route: ActivatedRoute, private http: HttpClient, private router: Router) {
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      if (token) {
        this.confirmEmail(token);
      } else {
        this.loading = false;
      }
    });
  }
confirmEmail(token: string) {
  this.loading = true;
  this.errorMessage = '';
  this.successMessage = '';
  this.http.get(`https://localhost:7297/api/Auth/confirm-email?token=${encodeURIComponent(token)}`)
    .subscribe({
      next: () => {
        this.loading = false;
        this.successMessage = 'Your email has been confirmed!';
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Email confirmation failed.';
      }
    });
}


  onManualConfirm() {
    if (this.manualToken) {
      this.confirmEmail(this.manualToken);
    }
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}