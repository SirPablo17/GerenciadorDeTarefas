import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, provideRouter } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('authGuard', () => {
  const runGuard = (url: string) =>
    TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    );

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
  });

  it('denies activation and redirects to /login with returnUrl when there is no session', () => {
    const result = runGuard('/tasks');

    expect(result).not.toBe(true);
  });

  it('allows activation when a session is active', () => {
    localStorage.setItem('auth.token', 'stored-token');
    localStorage.setItem('auth.expiresAt', '2026-01-01T00:00:00Z');
    TestBed.inject(AuthService);

    const result = runGuard('/tasks');

    expect(result).toBe(true);
  });
});
