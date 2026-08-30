export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
}

export enum TaskItemStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2,
}

export interface TaskDto {
  id: string;
  number: number;
  title: string;
  description: string;
  status: TaskItemStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  status: TaskItemStatus;
}

export interface UpdateTaskRequest {
  title: string;
  description: string;
  status: TaskItemStatus;
}
