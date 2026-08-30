import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TaskDto, TaskItemStatus } from '../../core/models';
import { TaskList } from './task-list';

describe('TaskList', () => {
  let fixture: ComponentFixture<TaskList>;
  let component: TaskList;
  let httpMock: HttpTestingController;

  const makeTask = (overrides: Partial<TaskDto> & { id: string }): TaskDto => ({
    number: 1,
    title: 'Task',
    description: '',
    status: TaskItemStatus.Pending,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
    ...overrides,
  });

  const flushTasks = (tasks: TaskDto[]) => {
    const req = httpMock.expectOne('/tasks');
    req.flush(tasks);
  };

  const textContent = (): string => (fixture.nativeElement as HTMLElement).textContent ?? '';

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskList],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(TaskList);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    document.body.appendChild(fixture.nativeElement);
  });

  afterEach(() => {
    httpMock.verify();
    fixture.nativeElement.remove();
    history.replaceState({}, '', location.href);
  });

  it('defaults to the Active tab, showing only Pending/InProgress tasks', () => {
    fixture.detectChanges();
    flushTasks([
      makeTask({ id: 'p', status: TaskItemStatus.Pending }),
      makeTask({ id: 'i', status: TaskItemStatus.InProgress }),
      makeTask({ id: 'c', status: TaskItemStatus.Completed }),
    ]);
    fixture.detectChanges();

    expect(component.selectedTab()).toBe('active');
    expect(component.activeTasks()?.map((t) => t.id)).toEqual(['p', 'i']);
  });

  it('shows only Completed tasks when the Completed tab is selected', () => {
    fixture.detectChanges();
    flushTasks([
      makeTask({ id: 'p', status: TaskItemStatus.Pending }),
      makeTask({ id: 'c', status: TaskItemStatus.Completed }),
    ]);
    fixture.detectChanges();

    component.selectTab('completed');
    fixture.detectChanges();

    expect(component.completedTasks()?.map((t) => t.id)).toEqual(['c']);
  });

  it("moves a task to the Completed tab when its status changes to Completed", () => {
    fixture.detectChanges();
    flushTasks([makeTask({ id: 't1', status: TaskItemStatus.Pending })]);
    fixture.detectChanges();
    expect(component.activeTasks()?.map((t) => t.id)).toEqual(['t1']);

    component.changeStatus(component.activeTasks()![0], TaskItemStatus.Completed);
    httpMock.expectOne('/tasks/t1').flush(makeTask({ id: 't1', status: TaskItemStatus.Completed }));
    flushTasks([makeTask({ id: 't1', status: TaskItemStatus.Completed })]);
    fixture.detectChanges();

    expect(component.activeTasks()?.length).toBe(0);
    expect(component.completedTasks()?.map((t) => t.id)).toEqual(['t1']);
  });

  it('moves a task back to the Active tab when its status changes away from Completed', () => {
    fixture.detectChanges();
    flushTasks([makeTask({ id: 't1', status: TaskItemStatus.Completed })]);
    fixture.detectChanges();
    expect(component.completedTasks()?.map((t) => t.id)).toEqual(['t1']);

    component.changeStatus(component.completedTasks()![0], TaskItemStatus.Pending);
    httpMock.expectOne('/tasks/t1').flush(makeTask({ id: 't1', status: TaskItemStatus.Pending }));
    flushTasks([makeTask({ id: 't1', status: TaskItemStatus.Pending })]);
    fixture.detectChanges();

    expect(component.completedTasks()?.length).toBe(0);
    expect(component.activeTasks()?.map((t) => t.id)).toEqual(['t1']);
  });

  it('shows the onboarding empty state on the Active tab when the user has no tasks at all', () => {
    fixture.detectChanges();
    flushTasks([]);
    fixture.detectChanges();

    expect(textContent()).toContain('Você ainda não tem tarefas.');
  });

  it('shows the "no active tasks" empty state on the Active tab when only completed tasks exist', () => {
    fixture.detectChanges();
    flushTasks([makeTask({ id: 'c', status: TaskItemStatus.Completed })]);
    fixture.detectChanges();

    expect(textContent()).toContain('Nenhuma tarefa ativa.');
  });

  it('shows the "nothing completed yet" empty state on the Completed tab when active tasks exist', () => {
    fixture.detectChanges();
    flushTasks([makeTask({ id: 'a', status: TaskItemStatus.Pending })]);
    fixture.detectChanges();

    component.selectTab('completed');
    fixture.detectChanges();

    expect(textContent()).toContain('Nenhuma tarefa concluída ainda.');
  });

  it('shows the "nothing completed yet" empty state on the Completed tab even with zero tasks total', () => {
    fixture.detectChanges();
    flushTasks([]);
    fixture.detectChanges();

    component.selectTab('completed');
    fixture.detectChanges();

    expect(textContent()).toContain('Nenhuma tarefa concluída ainda.');
  });

  it('moves focus to the Active tab button and announces the move when a task is completed', () => {
    fixture.detectChanges();
    flushTasks([makeTask({ id: 't1', status: TaskItemStatus.Pending })]);
    fixture.detectChanges();

    const select: HTMLSelectElement = fixture.nativeElement.querySelector('select');
    select.focus();
    expect(document.activeElement).toBe(select);

    component.changeStatus(component.activeTasks()![0], TaskItemStatus.Completed);

    const activeTabButton: HTMLButtonElement = fixture.nativeElement.querySelector('#active-tab-button');
    expect(document.activeElement).toBe(activeTabButton);
    expect(component.statusChangeAnnouncement()).toContain('Concluídas');

    httpMock.expectOne('/tasks/t1').flush(makeTask({ id: 't1', status: TaskItemStatus.Completed }));
    flushTasks([makeTask({ id: 't1', status: TaskItemStatus.Completed })]);
  });

  it('does not move focus or announce when the status change stays within the current tab', () => {
    fixture.detectChanges();
    flushTasks([makeTask({ id: 't1', status: TaskItemStatus.Pending })]);
    fixture.detectChanges();

    component.changeStatus(component.activeTasks()![0], TaskItemStatus.InProgress);

    expect(component.statusChangeAnnouncement()).toBe('');
    httpMock.expectOne('/tasks/t1').flush(makeTask({ id: 't1', status: TaskItemStatus.InProgress }));
    flushTasks([makeTask({ id: 't1', status: TaskItemStatus.InProgress })]);
  });

  it('picks up a status-change announcement left in router navigation state', () => {
    history.pushState({ statusChangeAnnouncement: 'Tarefa concluída. Veja na aba Concluídas.' }, '');

    fixture.detectChanges();
    flushTasks([]);

    expect(component.statusChangeAnnouncement()).toBe('Tarefa concluída. Veja na aba Concluídas.');
  });

  it("renders each task's number on its card in both tabs", () => {
    fixture.detectChanges();
    flushTasks([
      makeTask({ id: 'a', number: 7, status: TaskItemStatus.Pending }),
      makeTask({ id: 'c', number: 12, status: TaskItemStatus.Completed }),
    ]);
    fixture.detectChanges();

    expect(textContent()).toContain('#7');

    component.selectTab('completed');
    fixture.detectChanges();

    expect(textContent()).toContain('#12');
  });
});
