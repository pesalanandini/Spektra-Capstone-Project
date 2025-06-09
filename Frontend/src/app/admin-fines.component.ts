import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';

interface User {
  userId: number;
  fullName: string;
  email: string;
}

interface Book {
  bookId: number;
  title: string;
  author: string;
}

interface BorrowRecord {
  borrowRecordId: number;
  user: User;
  book: Book;
  borrowedAt: string;
  dueDate: string;
  returnedAt?: string | null;
}

interface Fine {
  fineId: number;
  borrowRecordId: number;
  borrowRecord: BorrowRecord;
  fineAmount: number;
  isPaid: boolean;
  paymentDate?: string | null;
}

@Component({
  selector: 'app-admin-fines',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './admin-fines.component.html',
  styleUrls: ['./admin-fines.component.css']
})
export class AdminFinesComponent {
  fines = signal<Fine[]>([]);
  loading = signal(false);
  error = signal('');
  today = new Date();

  constructor(private http: HttpClient) {
    this.fetchFines();
  }

  fetchFines() {
    this.loading.set(true);
    this.error.set('');

    const token = localStorage.getItem('jwt');
    if (!token) {
      this.error.set('Unauthorized: No token found.');
      this.loading.set(false);
      return;
    }

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    this.http.get<Fine[]>('https://localhost:7297/api/Books/fines', { headers }).subscribe({
      next: (data) => {
        this.fines.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading fines:', err);
        if (err.status === 401) {
          this.error.set('Unauthorized: Admin access required.');
        } else {
          this.error.set('Failed to load fines.');
        }
        this.loading.set(false);
      }
    });
  }

  isOverdue(fine: Fine): boolean {
    if (fine.borrowRecord.returnedAt) return false;
    const due = new Date(fine.borrowRecord.dueDate);
    return due < this.today;
  }

  isPaid(fine: Fine): boolean {
    return fine.isPaid;
  }
}
