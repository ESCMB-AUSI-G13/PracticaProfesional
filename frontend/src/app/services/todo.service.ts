import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TodoItem {
  id: number;
  title: string;
  completed: boolean;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class TodoService {
  private readonly apiUrl = 'http://localhost:5000/api/todo';

  constructor(private http: HttpClient) {}

  getAll(): Observable<TodoItem[]> {
    return this.http.get<TodoItem[]>(this.apiUrl);
  }

  create(title: string): Observable<TodoItem> {
    return this.http.post<TodoItem>(this.apiUrl, { title });
  }

  toggle(id: number): Observable<TodoItem> {
    return this.http.patch<TodoItem>(`${this.apiUrl}/${id}/toggle`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
