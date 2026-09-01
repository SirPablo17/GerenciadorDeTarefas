import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { TaskItemStatus } from '../../core/models';
import { TaskForm } from './task-form';

describe('TaskForm', () => {
  let fixture: ComponentFixture<TaskForm>;
  let component: TaskForm;
  let httpMock: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskForm],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(TaskForm);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    httpMock.verify();
    history.replaceState({}, '', location.href);
  });

  it('announces a move to the Completed tab when a task is created already marked Completed', () => {
    fixture.detectChanges();
    const navigateSpy = vi.spyOn(router, 'navigate');

    component.form.setValue({ title: 'Task', description: '', status: TaskItemStatus.Completed });
    component.submit();

    httpMock
      .expectOne('/tasks')
      .flush({ id: 't1', number: 1, title: 'Task', description: '', status: TaskItemStatus.Completed });

    expect(navigateSpy).toHaveBeenCalledWith(
      ['/tasks'],
      expect.objectContaining({
        state: expect.objectContaining({ statusChangeAnnouncement: expect.stringContaining('Concluídas') }),
      }),
    );
  });

  it('does not announce a tab move when a task is created with a non-Completed status', () => {
    fixture.detectChanges();
    const navigateSpy = vi.spyOn(router, 'navigate');

    component.form.setValue({ title: 'Task', description: '', status: TaskItemStatus.Pending });
    component.submit();

    httpMock
      .expectOne('/tasks')
      .flush({ id: 't1', number: 1, title: 'Task', description: '', status: TaskItemStatus.Pending });

    expect(navigateSpy).toHaveBeenCalledWith(['/tasks'], expect.objectContaining({ state: undefined }));
  });

  it('returns to the tab it was opened from on cancel', () => {
    history.pushState({ fromTab: 'completed' }, '');
    fixture.detectChanges();
    const navigateSpy = vi.spyOn(router, 'navigate');

    component.cancel();

    expect(navigateSpy).toHaveBeenCalledWith(['/tasks'], { queryParams: { tab: 'completed' } });
  });

  it('returns to /tasks with no tab query param when opened without a known origin tab', () => {
    fixture.detectChanges();
    const navigateSpy = vi.spyOn(router, 'navigate');

    component.cancel();

    expect(navigateSpy).toHaveBeenCalledWith(['/tasks'], { queryParams: undefined });
  });
});
