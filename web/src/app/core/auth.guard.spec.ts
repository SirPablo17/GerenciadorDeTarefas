import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('authGuard', () => {
  let httpMock: HttpTestingController;

  const runGuard = (url: string) =>
    TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    );

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

  it('denies activation and redirects to /login with returnUrl when there is no session', () => {
    const router = TestBed.inject(Router);

    const result = runGuard('/tasks');

    expect(router.serializeUrl(result as UrlTree)).toBe('/login?returnUrl=%2Ftasks');
  });

  it('allows activation when a session is active', () => {
    localStorage.setItem('auth.token', 'stored-token');
    localStorage.setItem('auth.expiresAt', '2026-01-01T00:00:00Z');
    TestBed.inject(AuthService);

    const result = runGuard('/tasks');

    expect(result).toBe(true);
  });
});
