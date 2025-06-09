import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

interface Book {
  bookId: number;
  title: string;
  author: string;
  description: string;
  genre: string;
  coverImageUrl: string;
  pdfUrl: string;
  availableCopies: number;
  totalCopies: number;
}

@Component({
  selector: 'app-admin-books',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-books.component.html',
  styleUrls: ['./admin-books.component.css']
})
export class AdminBooksComponent {
  books: Book[] = [];
  showAdd = false;
  showEdit = false;
  editIndex = -1;
  form: any = {
    bookId: 0,
    title: '',
    author: '',
    description: '',
    genre: '',
    coverImageUrl: '',
    pdfUrl: '',
    totalCopies: 1
  };
  error = '';

  constructor(private http: HttpClient, private router: Router) {
    const jwt = localStorage.getItem('jwt');
    const role = localStorage.getItem('role');
    if (!jwt || role !== 'Admin') {
      this.router.navigate(['/login']);
      return;
    }
    this.fetchBooks();
  }

  getAuthHeaders() {
    const token = localStorage.getItem('jwt');
    return token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;
  }

  fetchBooks() {
    const params: any = {
      search: 'test',
      genre: 'test',
      findAll: 'allBooks'
    };
    this.http.get<Book[]>('https://localhost:7297/api/Books', {
      params,
      headers: this.getAuthHeaders()
    }).subscribe({
      next: (data) => this.books = data,
      error: (err: HttpErrorResponse) => {
        this.error = err.error?.message || err.error || 'Failed to load books.';
      }
    });
  }

  openAdd() {
    this.form = {
      bookId: 0,
      title: '',
      author: '',
      description: '',
      genre: '',
      coverImageUrl: '',
      pdfUrl: '',
      totalCopies: 1
    };
    this.showAdd = true;
    this.showEdit = false;
    this.error = '';
  }

  openEdit(i: number) {
    this.form = { ...this.books[i] };
    this.editIndex = i;
    this.showEdit = true;
    this.showAdd = false;
    this.error = '';
  }

  saveAdd() {
    const payload = {
      title: this.form.title,
      author: this.form.author,
      description: this.form.description,
      genre: this.form.genre,
      coverImageUrl: this.form.coverImageUrl,
      pdfUrl: this.form.pdfUrl,
      totalCopies: this.form.totalCopies || 1
    };
    this.http.post('https://localhost:7297/api/Books', payload, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: () => {
        this.showAdd = false;
        this.fetchBooks();
      },
      error: (err: HttpErrorResponse) => {
        this.error = err.error?.message || err.error || 'Failed to add book.';
      }
    });
  }

  saveEdit() {
    const idx = this.editIndex;
    if (idx >= 0) {
      const book = this.books[idx];
      const payload = {
        title: this.form.title,
        author: this.form.author,
        description: this.form.description,
        genre: this.form.genre,
        coverImageUrl: this.form.coverImageUrl,
        pdfUrl: this.form.pdfUrl,
        totalCopies: this.form.totalCopies || 1
      };
      this.http.put(`https://localhost:7297/api/Books/${book.bookId}`, payload, {
        headers: this.getAuthHeaders()
      }).subscribe({
        next: () => {
          this.showEdit = false;
          this.fetchBooks();
        },
        error: (err: HttpErrorResponse) => {
          this.error = err.error?.message || err.error || 'Failed to update book.';
        }
      });
    }
  }

  deleteBook(i: number) {
    const book = this.books[i];
    if (confirm(`Delete book "${book.title}"?`)) {
      this.http.delete(`https://localhost:7297/api/Books/${book.bookId}`, {
        headers: this.getAuthHeaders()
      }).subscribe({
        next: () => this.fetchBooks(),
        error: (err: HttpErrorResponse) => {
          this.error = err.error?.message || err.error || 'Failed to delete book.';
        }
      });
    }
  }

  cancel() {
    this.showAdd = false;
    this.showEdit = false;
    this.error = '';
  }

  onInput(field: keyof Book | 'pdfUrl' | 'totalCopies', event: Event) {
    const input = event.target as HTMLInputElement | null;
    if (input) {
      if (field === 'totalCopies') {
        this.form[field] = +input.value;
      } else {
        this.form[field] = input.value;
      }
    }
  }
}
