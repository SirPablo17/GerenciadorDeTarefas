import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, switchMap, tap } from 'rxjs';
import { TasksService } from '../tasks/tasks.service';
import { AuthResponse, LoginRequest, RegisterRequest } from './models';

const TOKEN_KEY = 'auth.token';
const EXPIRES_AT_KEY = 'auth.expiresAt';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tasksService = inject(TasksService);

  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly _expiresAt = signal<string | null>(localStorage.getItem(EXPIRES_AT_KEY));

  readonly token = this._token.asReadonly();
  readonly isAuthenticated = computed(() => this._token() !== null);

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/auth/login', request).pipe(
      tap((response) => {
        localStorage.setItem(TOKEN_KEY, response.token);
        localStorage.setItem(EXPIRES_AT_KEY, response.expiresAt);
        this._token.set(response.token);
        this._expiresAt.set(response.expiresAt);
      }),
    );
  }

  /** Creates the account, then logs in with the same credentials — the API's
   * register endpoint returns no token, so this avoids asking the user to
   * type their password a second time (see design.md - Decision 2). */
  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<void>('/auth/register', request)
      .pipe(switchMap(() => this.login(request)));
  }

  /** User-initiated logout: ends the session and returns to login. */
  logout(): void {
    this.clearSession();
    this.router.navigate(['/login'], { replaceUrl: true });
  }

  /** Session invalidated by the server (e.g. expired/invalid token): ends the
   * session and sends the user to login, preserving where they were headed. */
  forceLogout(returnUrl: string): void {
    this.clearSession();
    this.router.navigate(['/login'], { queryParams: { returnUrl }, replaceUrl: true });
  }

  private clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXPIRES_AT_KEY);
    this._token.set(null);
    this._expiresAt.set(null);
    this.tasksService.reset();
  }
}
