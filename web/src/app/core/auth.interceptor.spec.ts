import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  let httpMock: HttpTestingController;
  let http: HttpClient;
  let authService: AuthService;
  let router: Router;

  const configure = (token: string | null) => {
    TestBed.resetTestingModule();
    localStorage.clear();
    if (token) {
      localStorage.setItem('auth.token', token);
      localStorage.setItem('auth.expiresAt', '2026-01-01T00:00:00Z');
    }

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });

    httpMock = TestBed.inject(HttpTestingController);
    http = TestBed.inject(HttpClient);
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  };

  beforeEach(() => configure('stored-token'));

  afterEach(() => {
    httpMock.verify();
  });

  it('attaches the bearer token to outgoing requests when a session exists', () => {
    http.get('/tasks').subscribe();

    const req = httpMock.expectOne('/tasks');
    expect(req.request.headers.get('Authorization')).toBe('Bearer stored-token');
    req.flush([]);
  });

  it('ends the session and navigates to /login, preserving the current URL, on a 401', () => {
    const navigateSpy = vi.spyOn(router, 'navigate');

    http.get('/tasks').subscribe({ error: () => {} });

    const req = httpMock.expectOne('/tasks');
    req.flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(authService.isAuthenticated()).toBe(false);
    expect(navigateSpy).toHaveBeenCalledWith(['/login'], {
      queryParams: { returnUrl: router.url },
      replaceUrl: true,
    });
  });

  it('does not attach an Authorization header when there is no session', () => {
    configure(null);

    http.get('/tasks').subscribe({ error: () => {} });

    const req = httpMock.expectOne('/tasks');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush(null, { status: 401, statusText: 'Unauthorized' });
  });

  it('does not force a logout on a 401 when there was no session to begin with', () => {
    configure(null);
    const navigateSpy = vi.spyOn(router, 'navigate');

    http.get('/tasks').subscribe({ error: () => {} });

    const req = httpMock.expectOne('/tasks');
    req.flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(navigateSpy).not.toHaveBeenCalled();
  });
});
