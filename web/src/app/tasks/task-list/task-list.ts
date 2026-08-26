import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatToolbarModule } from '@angular/material/toolbar';
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
  imports: [
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatSelectModule,
    MatToolbarModule,
  ],
  templateUrl: './task-list.html',
  styleUrl: './task-list.css',
})
export class TaskList implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  protected readonly tasksService = inject(TasksService);

  readonly statusOptions = STATUS_OPTIONS;

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
    const dialogRef = this.dialog.open(ConfirmDeleteDialog, {
      data: { taskTitle: task.title },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.tasksService.remove(task.id).subscribe(() => this.tasksService.load());
      }
    });
  }
}
