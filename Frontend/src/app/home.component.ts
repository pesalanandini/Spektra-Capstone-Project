import { Component, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';

interface Book {
  bookId: number;
  title: string;
  author: string;
  description: string;
  genre: string;
  coverImageUrl: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  books = signal<Book[]>([]);
  loading = signal(true);
  error = signal('');

  constructor(private http: HttpClient) {
    this.fetchRecommendations();
  }

  fetchRecommendations() {
    this.loading.set(true);
    this.error.set('');
    this.http.get<Book[]>('/api/books').subscribe({
      next: (data) => {
        // For demo, pick first 4 as recommendations
        this.books.set(data.slice(0, 4));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load recommendations.');
        this.loading.set(false);
      }
    });
  }
}
