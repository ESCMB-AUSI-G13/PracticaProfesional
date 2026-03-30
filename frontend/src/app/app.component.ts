import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TodoService, TodoItem } from './services/todo.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  todos: TodoItem[] = [];
  newTitle = '';
  loading = true;
  error = '';

  constructor(private todoService: TodoService) {}

  ngOnInit(): void {
    this.loadTodos();
  }

  loadTodos(): void {
    this.loading = true;
    this.error = '';
    this.todoService.getAll().subscribe({
      next: (data) => { this.todos = data; this.loading = false; },
      error: () => { this.error = 'No se pudo conectar con el backend.'; this.loading = false; }
    });
  }

  addTodo(): void {
    const title = this.newTitle.trim();
    if (!title) return;
    this.todoService.create(title).subscribe({
      next: (item) => { this.todos.push(item); this.newTitle = ''; }
    });
  }

  toggleTodo(todo: TodoItem): void {
    this.todoService.toggle(todo.id).subscribe({
      next: (updated) => {
        const idx = this.todos.findIndex(t => t.id === updated.id);
        if (idx !== -1) this.todos[idx] = updated;
      }
    });
  }

  deleteTodo(id: number): void {
    this.todoService.delete(id).subscribe({
      next: () => { this.todos = this.todos.filter(t => t.id !== id); }
    });
  }

  get pending(): number { return this.todos.filter(t => !t.completed).length; }
}
