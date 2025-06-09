import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';


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
  borrowedAt: string;  // ISO date string
  dueDate: string;     // ISO date string
  returnedAt?: string | null;  // ISO date string or null
}

@Component({
  selector: 'app-borrow-records',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './borrow-records.component.html',
  styleUrls: ['./borrow-records.component.css']
})
export class BorrowRecordsComponent {
  records = signal<BorrowRecord[]>([]);
  loading = signal(false);
  error = signal('');
  today = new Date();

  constructor(private http: HttpClient) {
    this.fetchRecords();
  }
fetchRecords() {
  this.loading.set(true);
  this.error.set('');
  
  const token = localStorage.getItem('jwt');
  const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;

  this.http.get<BorrowRecord[]>('https://localhost:7297/api/Books/borrow-records', { headers })
    .subscribe({
      next: (data) => {
        this.records.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading borrow records:', err);
        this.error.set('Failed to load borrow records.');
        this.loading.set(false);
      }
    });
}

  isOverdue(record: BorrowRecord): boolean {
    if (record.returnedAt) return false;
    const due = new Date(record.dueDate);
    return due < this.today;
  }

  isReturned(record: BorrowRecord): boolean {
    return !!record.returnedAt;
  }
}
