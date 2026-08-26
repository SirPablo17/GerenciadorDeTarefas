import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { AuthResponse } from './models';

describe('AuthService', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('restores authenticated state from a token already in localStorage', () => {
    localStorage.setItem('auth.token', 'stored-token');
    localStorage.setItem('auth.expiresAt', '2026-01-01T00:00:00Z');

    const restored = TestBed.inject(AuthService);

    expect(restored.isAuthenticated()).toBe(true);
  });

  it('starts unauthenticated when there is no stored token', () => {
    const authService = TestBed.inject(AuthService);

    expect(authService.isAuthenticated()).toBe(false);
  });

  it('updates session state and persists it on successful login', () => {
    const authService = TestBed.inject(AuthService);
    const response: AuthResponse = { token: 'new-token', expiresAt: '2026-01-01T00:00:00Z' };

    authService.login({ email: 'user@example.com', password: 'secret' }).subscribe();

    const req = httpMock.expectOne('/auth/login');
    expect(req.request.method).toBe('POST');
    req.flush(response);

    expect(authService.isAuthenticated()).toBe(true);
    expect(authService.token()).toBe('new-token');
    expect(localStorage.getItem('auth.token')).toBe('new-token');
  });

  it('logs in with the same credentials after a successful registration', () => {
    const authService = TestBed.inject(AuthService);
    const response: AuthResponse = { token: 'new-token', expiresAt: '2026-01-01T00:00:00Z' };
    const credentials = { email: 'new-user@example.com', password: 'Secret123' };

    authService.register(credentials).subscribe();

    const registerReq = httpMock.expectOne('/auth/register');
    expect(registerReq.request.method).toBe('POST');
    expect(registerReq.request.body).toEqual(credentials);
    registerReq.flush(null, { status: 201, statusText: 'Created' });

    const loginReq = httpMock.expectOne('/auth/login');
    expect(loginReq.request.method).toBe('POST');
    expect(loginReq.request.body).toEqual(credentials);
    loginReq.flush(response);

    expect(authService.isAuthenticated()).toBe(true);
  });

  it('clears session state and storage, and navigates to /login, on logout', () => {
    localStorage.setItem('auth.token', 'stored-token');
    localStorage.setItem('auth.expiresAt', '2026-01-01T00:00:00Z');
    const loggedIn = TestBed.inject(AuthService);
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate');

    loggedIn.logout();

    expect(loggedIn.isAuthenticated()).toBe(false);
    expect(localStorage.getItem('auth.token')).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith(['/login'], { replaceUrl: true });
  });
});
