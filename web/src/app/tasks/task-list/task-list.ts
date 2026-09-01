import { NgTemplateOutlet } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
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
  private readonly route = inject(ActivatedRoute);
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

  private toastTimeoutId?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.tasksService.load();

    if (this.route.snapshot.queryParamMap.get('tab') === 'completed') {
      this.selectedTab.set('completed');
    }

    const navigationState = history.state as { statusChangeAnnouncement?: string } | null;
    if (navigationState?.statusChangeAnnouncement) {
      this.announce(navigationState.statusChangeAnnouncement);
      history.replaceState({}, '', location.href);
    }
  }

  selectTab(tab: TaskTab): void {
    this.selectedTab.set(tab);
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  private announce(message: string): void {
    this.statusChangeAnnouncement.set(message);
    clearTimeout(this.toastTimeoutId);
    this.toastTimeoutId = setTimeout(() => this.statusChangeAnnouncement.set(''), 4000);
  }

  retry(): void {
    this.tasksService.load();
  }

  logout(): void {
    this.authService.logout();
  }

  newTask(): void {
    this.router.navigate(['/tasks/new'], { state: { fromTab: this.selectedTab() } });
  }

  editTask(id: string): void {
    this.router.navigate(['/tasks', id, 'edit'], { state: { fromTab: this.selectedTab() } });
  }

  changeStatus(task: TaskDto, status: TaskItemStatus): void {
    if (status === task.status) {
      return;
    }

    const willBeCompleted = status === TaskItemStatus.Completed;
    const crossesTabBoundary = willBeCompleted !== (this.selectedTab() === 'completed');
    if (crossesTabBoundary) {
      // The task is about to leave the tab being viewed — move focus to the
      // (still-visible) tab button before the card disappears, since nothing
      // else on screen does once the update succeeds.
      this.focusTabButton(this.selectedTab());
    }

    this.tasksService.update(task.id, { title: task.title, description: task.description, status }).subscribe({
      next: () => {
        if (crossesTabBoundary) {
          this.announce(
            willBeCompleted ? 'Tarefa concluída. Veja na aba Concluídas.' : 'Tarefa reaberta. Veja na aba Ativas.',
          );
        }
        this.tasksService.load();
      },
      error: () => this.announce('Não foi possível atualizar o status. Tente novamente.'),
    });
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
    this.deleteTarget.set(null);
    this.tasksService.remove(task.id).subscribe({
      next: () => this.tasksService.load(),
      error: () => this.announce('Não foi possível excluir a tarefa. Tente novamente.'),
    });
  }

  cancelDelete(): void {
    this.deleteTarget.set(null);
  }
}
