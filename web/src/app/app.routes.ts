import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { Login } from './login/login';
import { Register } from './register/register';
import { TaskForm } from './tasks/task-form/task-form';
import { TaskList } from './tasks/task-list/task-list';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'tasks', component: TaskList, canActivate: [authGuard] },
  { path: 'tasks/new', component: TaskForm, canActivate: [authGuard] },
  { path: 'tasks/:id/edit', component: TaskForm, canActivate: [authGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  { path: '**', redirectTo: 'tasks' },
];
