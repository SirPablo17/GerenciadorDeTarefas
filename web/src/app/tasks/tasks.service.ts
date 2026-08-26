import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateTaskRequest, TaskDto, UpdateTaskRequest } from '../core/models';

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient);

  private readonly _tasks = signal<TaskDto[] | null>(null);
  private readonly _loading = signal(false);
  private readonly _error = signal(false);

  readonly tasks = this._tasks.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  load(): void {
    this._loading.set(true);
    this._error.set(false);

    this.http.get<TaskDto[]>('/tasks').subscribe({
      next: (tasks) => {
        this._tasks.set(tasks);
        this._loading.set(false);
      },
      error: () => {
        this._error.set(true);
        this._loading.set(false);
      },
    });
  }

  getById(id: string): Observable<TaskDto> {
    return this.http.get<TaskDto>(`/tasks/${id}`);
  }

  create(request: CreateTaskRequest): Observable<TaskDto> {
    return this.http.post<TaskDto>('/tasks', request);
  }

  update(id: string, request: UpdateTaskRequest): Observable<TaskDto> {
    return this.http.put<TaskDto>(`/tasks/${id}`, request);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`/tasks/${id}`);
  }

  reset(): void {
    this._tasks.set(null);
    this._loading.set(false);
    this._error.set(false);
  }
}
