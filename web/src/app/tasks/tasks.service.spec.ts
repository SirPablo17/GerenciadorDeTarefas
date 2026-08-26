import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TaskItemStatus, TaskDto } from '../core/models';
import { TasksService } from './tasks.service';

describe('TasksService', () => {
  let service: TasksService;
  let httpMock: HttpTestingController;

  const task: TaskDto = {
    id: 'task-1',
    title: 'Title',
    description: 'Description',
    status: TaskItemStatus.Pending,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(TasksService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('creates a task via POST /tasks and propagates the created task', () => {
    const request = { title: 'New', description: '', status: TaskItemStatus.Pending };
    let result: TaskDto | undefined;

    service.create(request).subscribe((r) => (result = r));

    const req = httpMock.expectOne('/tasks');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(task);

    expect(result).toEqual(task);
  });

  it('propagates a validation error (400) on create', () => {
    const request = { title: '', description: '', status: TaskItemStatus.Pending };
    let errorStatus: number | undefined;

    service.create(request).subscribe({ error: (err) => (errorStatus = err.status) });

    const req = httpMock.expectOne('/tasks');
    req.flush({ title: 'validation failed' }, { status: 400, statusText: 'Bad Request' });

    expect(errorStatus).toBe(400);
  });

  it('updates a task via PUT /tasks/{id} sending the full payload', () => {
    const request = { title: 'Edited', description: 'New desc', status: TaskItemStatus.Completed };
    let result: TaskDto | undefined;

    service.update('task-1', request).subscribe((r) => (result = r));

    const req = httpMock.expectOne('/tasks/task-1');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(request);
    req.flush({ ...task, ...request });

    expect(result?.status).toBe(TaskItemStatus.Completed);
  });

  it('propagates a validation error (400) on update', () => {
    const request = { title: '', description: '', status: TaskItemStatus.Pending };
    let errorStatus: number | undefined;

    service.update('task-1', request).subscribe({ error: (err) => (errorStatus = err.status) });

    const req = httpMock.expectOne('/tasks/task-1');
    req.flush({ title: 'validation failed' }, { status: 400, statusText: 'Bad Request' });

    expect(errorStatus).toBe(400);
  });

  it('deletes a task via DELETE /tasks/{id}', () => {
    let completed = false;

    service.remove('task-1').subscribe({ complete: () => (completed = true) });

    const req = httpMock.expectOne('/tasks/task-1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null, { status: 204, statusText: 'No Content' });

    expect(completed).toBe(true);
  });

  it('propagates a not-found error (404) on delete', () => {
    let errorStatus: number | undefined;

    service.remove('missing').subscribe({ error: (err) => (errorStatus = err.status) });

    const req = httpMock.expectOne('/tasks/missing');
    req.flush({ title: 'not found' }, { status: 404, statusText: 'Not Found' });

    expect(errorStatus).toBe(404);
  });
});
