import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';

interface Book {
  bookId: number;
  title: string;
  author: string;
  description: string;
  genre: string;
  coverImageUrl: string;
  availableCopies: number;
  totalCopies: number;
  isBorrowed?: boolean; // UI only
  borrowedByMe?: boolean; // UI only
}

@Component({
  selector: 'app-book-details',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './book-details.component.html',
  styleUrl: './book-details.component.css'
})
export class BookDetailsComponent {
  book = signal<Book | null>(null);
  loading = signal(false);
  error = signal('');
  borrowLoading = signal(false);
  borrowError = signal('');
  borrowSuccess = signal('');

  constructor(private route: ActivatedRoute, private http: HttpClient) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.fetchBook(id);
    }
  }

  fetchBook(id: string) {
    this.loading.set(true);
    this.error.set('');
    const token = localStorage.getItem('jwt');
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;

    this.http.get<Book>(`https://localhost:7297/api/Books/${id}`, { headers }).subscribe({
      next: (data) => {
        // Use backend info (no random logic now)
        const isBorrowed = data.availableCopies === 0;
        this.book.set({ ...data, isBorrowed, borrowedByMe: false }); // Adjust `borrowedByMe` if API provides it
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load book details.');
        this.loading.set(false);
      }
    });
  }

 borrowBook() {
  const token = localStorage.getItem('jwt');
  const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;

  this.borrowLoading.set(true);
  this.borrowError.set('');
  this.borrowSuccess.set('');

  const b = this.book();
  if (!b) return;

  this.http.post(`https://localhost:7297/api/Books/${b.bookId}/borrow`, {}, { headers }).subscribe({
    next: () => {
      this.borrowSuccess.set('Book borrowed successfully!');
      this.book.update(b => b ? { ...b, isBorrowed: true, borrowedByMe: true } : b);
      this.borrowLoading.set(false);
    },
    error: (err) => {
      console.error('Borrow error:', err); // helpful for debugging
      this.borrowError.set('Failed to borrow the book.');
      this.borrowLoading.set(false);
    }
  });
}


  returnBook() {
    const b = this.book();
    if (!b) return;

    this.borrowLoading.set(true);
    this.borrowError.set('');
    this.borrowSuccess.set('');
    const token = localStorage.getItem('jwt');
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;

    this.http.post(`https://localhost:7297/api/Books/${b.bookId}/return`, {}, { headers }).subscribe({
      next: () => {
        this.borrowLoading.set(false);
        this.borrowSuccess.set('Book returned successfully!');
        this.book.update(book => book ? {
          ...book,
          isBorrowed: false,
          borrowedByMe: false,
          availableCopies: book.availableCopies + 1
        } : book);
      },
      error: () => {
        this.borrowLoading.set(false);
        this.borrowError.set('Failed to return the book.');
      }
    });
  }
}
