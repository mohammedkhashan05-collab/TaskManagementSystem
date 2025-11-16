import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Task {
  id: number;
  title: string;
  description: string;
  status: string;
  assignedUserId: number;
  assignedUserName: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  status?: string;
  assignedUserId: number;
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  status?: string;
  assignedUserId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAllTasks(): Observable<Task[]> {
    return this.http.get<Task[]>(`${this.apiUrl}/api/tasks`);
  }

  getTaskById(id: number): Observable<Task> {
    return this.http.get<Task>(`${this.apiUrl}/api/tasks/${id}`);
  }

  createTask(task: CreateTaskRequest): Observable<Task> {
    return this.http.post<Task>(`${this.apiUrl}/api/tasks`, task);
  }

  updateTask(id: number, task: UpdateTaskRequest): Observable<Task> {
    return this.http.put<Task>(`${this.apiUrl}/api/tasks/${id}`, task);
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/tasks/${id}`);
  }
}

