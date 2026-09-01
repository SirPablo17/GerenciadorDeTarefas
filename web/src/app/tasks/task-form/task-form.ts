import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TaskItemStatus } from '../../core/models';
import { TasksService } from '../tasks.service';

const GENERIC_ERROR_MESSAGE = 'Não foi possível salvar a tarefa. Tente novamente.';

const STATUS_OPTIONS: { value: TaskItemStatus; label: string }[] = [
  { value: TaskItemStatus.Pending, label: 'Pendente' },
  { value: TaskItemStatus.InProgress, label: 'Em andamento' },
  { value: TaskItemStatus.Completed, label: 'Concluída' },
];

@Component({
  selector: 'app-task-form',
  imports: [ReactiveFormsModule],
  templateUrl: './task-form.html',
})
export class TaskForm implements OnInit {
  private readonly tasksService = inject(TasksService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  private taskId: string | null = null;
  private loadedStatus: TaskItemStatus | null = TaskItemStatus.Pending;
  private returnTab: 'active' | 'completed' | null = null;

  readonly statusOptions = STATUS_OPTIONS;

  readonly form = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(2000)] }),
    status: new FormControl<TaskItemStatus>(TaskItemStatus.Pending, { nonNullable: true }),
  });

  readonly isEditMode = signal(false);
  readonly loading = signal(false);
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    const navigationState = history.state as { fromTab?: 'active' | 'completed' } | null;
    this.returnTab = navigationState?.fromTab ?? null;

    this.taskId = this.route.snapshot.paramMap.get('id');

    if (this.taskId) {
      this.isEditMode.set(true);
      this.loading.set(true);
      this.tasksService.getById(this.taskId).subscribe({
        next: (task) => {
          this.form.setValue({
            title: task.title,
            description: task.description,
            status: task.status,
          });
          this.loadedStatus = task.status;
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Não foi possível carregar a tarefa.');
        },
      });
    }
  }

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const request = this.form.getRawValue();
    const save$ = this.taskId
      ? this.tasksService.update(this.taskId, request)
      : this.tasksService.create(request);

    save$.subscribe({
      next: () => {
        const willBeCompleted = request.status === TaskItemStatus.Completed;
        const crossedCompletedBoundary =
          this.loadedStatus !== null && willBeCompleted !== (this.loadedStatus === TaskItemStatus.Completed);

        this.router.navigate(['/tasks'], {
          queryParams: this.returnTab ? { tab: this.returnTab } : undefined,
          state: crossedCompletedBoundary
            ? {
                statusChangeAnnouncement: willBeCompleted
                  ? 'Tarefa concluída. Veja na aba Concluídas.'
                  : 'Tarefa reaberta. Veja na aba Ativas.',
              }
            : undefined,
        });
      },
      error: (error: unknown) => {
        this.submitting.set(false);
        this.errorMessage.set(this.extractMessage(error));
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/tasks'], { queryParams: this.returnTab ? { tab: this.returnTab } : undefined });
  }

  private extractMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && typeof error.error?.title === 'string') {
      return error.error.title;
    }
    return GENERIC_ERROR_MESSAGE;
  }
}
