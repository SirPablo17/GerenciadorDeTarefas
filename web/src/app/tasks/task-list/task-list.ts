import { NgTemplateOutlet } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
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

type TaskTab = 'active' | 'completed';

@Component({
  selector: 'app-task-list',
  imports: [ConfirmDeleteDialog, NgTemplateOutlet],
  templateUrl: './task-list.html',
})
export class TaskList implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly tasksService = inject(TasksService);

  readonly statusOptions = STATUS_OPTIONS;
  readonly deleteTarget = signal<TaskDto | null>(null);

  readonly selectedTab = signal<TaskTab>('active');
  readonly statusChangeAnnouncement = signal('');

  @ViewChild('activeTabButton') private readonly activeTabButtonRef?: ElementRef<HTMLButtonElement>;
  @ViewChild('completedTabButton') private readonly completedTabButtonRef?: ElementRef<HTMLButtonElement>;

  readonly activeTasks = computed<TaskDto[] | null>(() => {
    const tasks = this.tasksService.tasks();
    return tasks === null ? null : tasks.filter((task) => task.status !== TaskItemStatus.Completed);
  });

  readonly completedTasks = computed<TaskDto[] | null>(() => {
    const tasks = this.tasksService.tasks();
    return tasks === null ? null : tasks.filter((task) => task.status === TaskItemStatus.Completed);
  });

  ngOnInit(): void {
    this.tasksService.load();

    const navigationState = history.state as { statusChangeAnnouncement?: string } | null;
    if (navigationState?.statusChangeAnnouncement) {
      this.statusChangeAnnouncement.set(navigationState.statusChangeAnnouncement);
      history.replaceState({}, '', location.href);
    }
  }

  selectTab(tab: TaskTab): void {
    this.selectedTab.set(tab);
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

    const willBeCompleted = status === TaskItemStatus.Completed;
    const crossesTabBoundary = willBeCompleted !== (this.selectedTab() === 'completed');
    if (crossesTabBoundary) {
      // The task is about to leave the tab being viewed — move focus to the
      // (still-visible) tab button before the card disappears, and announce
      // the move for screen-reader users, since nothing else on screen does.
      this.focusTabButton(this.selectedTab());
      this.statusChangeAnnouncement.set(
        willBeCompleted ? 'Tarefa concluída. Veja na aba Concluídas.' : 'Tarefa reaberta. Veja na aba Ativas.',
      );
    }

    this.tasksService
      .update(task.id, { title: task.title, description: task.description, status })
      .subscribe(() => this.tasksService.load());
  }

  private focusTabButton(tab: TaskTab): void {
    const ref = tab === 'active' ? this.activeTabButtonRef : this.completedTabButtonRef;
    ref?.nativeElement.focus();
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
