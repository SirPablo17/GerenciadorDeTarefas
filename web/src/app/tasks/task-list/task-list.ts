import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { TaskDto, TaskItemStatus } from '../../core/models';
import { ConfirmDeleteDialog } from '../confirm-delete-dialog/confirm-delete-dialog';
import { TasksService } from '../tasks.service';

const STATUS_OPTIONS = [
  { value: TaskItemStatus.Pending, label: 'Pendente' },
  { value: TaskItemStatus.InProgress, label: 'Em andamento' },
  { value: TaskItemStatus.Completed, label: 'Concluída' },
];

@Component({
  selector: 'app-task-list',
  imports: [ConfirmDeleteDialog],
  templateUrl: './task-list.html',
})
export class TaskList implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly tasksService = inject(TasksService);

  readonly statusOptions = STATUS_OPTIONS;
  readonly deleteTarget = signal<TaskDto | null>(null);

  ngOnInit(): void {
    this.tasksService.load();
  }

  retry(): void {
    this.tasksService.load();
  }

  logout(): void {
    this.authService.logout();
  }

  newTask(): void {
    this.router.navigateByUrl('/tasks/new');
  }

  editTask(id: string): void {
    this.router.navigateByUrl(`/tasks/${id}/edit`);
  }

  changeStatus(task: TaskDto, status: TaskItemStatus): void {
    if (status === task.status) {
      return;
    }
    this.tasksService
      .update(task.id, { title: task.title, description: task.description, status })
      .subscribe(() => this.tasksService.load());
  }

  deleteTask(task: TaskDto): void {
    this.deleteTarget.set(task);
  }

  confirmDelete(): void {
    const task = this.deleteTarget();
    if (!task) {
      return;
    }
    this.tasksService.remove(task.id).subscribe(() => this.tasksService.load());
    this.deleteTarget.set(null);
  }

  cancelDelete(): void {
    this.deleteTarget.set(null);
  }
}
