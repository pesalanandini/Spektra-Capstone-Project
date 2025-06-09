import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

interface Book {
  bookId: number;
  title: string;
  author: string;
  description: string;
  genre: string;
  coverImageUrl: string;
}

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './books.component.html',
  styleUrl: './books.component.css'
})
export class BooksComponent implements OnInit {
  books = signal<Book[]>([]);
  loading = signal(false);
  error = signal('');
  search = '';
  genre = '';
  findAll = '';

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    // Load all books when the component is initialized
    this.findAll = 'allBooks';
    this.fetchBooks();
  }

  fetchBooks() {
    this.loading.set(true);
    this.error.set('');

    const params: any = {};
    if (this.search) params.search = this.search;
    if (this.genre) params.genre = this.genre;
    if (this.findAll) params.findAll = this.findAll;

    const token = localStorage.getItem('jwt');
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;

    this.http.get<Book[]>('https://localhost:7297/api/Books', { params, headers }).subscribe({
      next: (data) => {
        this.books.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load books.');
        this.loading.set(false);
      }
    });
  }

  onSearchInput(event: Event) {
    const value = (event.target as HTMLInputElement)?.value || '';
    this.search = value;
    this.findAll = ''; // Disable findAll when searching
    this.fetchBooks();
  }

  onGenreInput(event: Event) {
    const value = (event.target as HTMLInputElement)?.value || '';
    this.genre = value;
    this.findAll = ''; // Disable findAll when filtering by genre
    this.fetchBooks();
  }

  viewBook(book: Book) {
    this.router.navigate(['/books', book.bookId]);
  }
}
