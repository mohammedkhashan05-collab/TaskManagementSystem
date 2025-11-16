import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface User {
  id: number;
  username: string;
  email: string;
  role: string;
  createdAt: string;
}

export interface CreateUserRequest {
  username: string;
  email: string;
  password: string;
  role?: string;
}

export interface UpdateUserRequest {
  username?: string;
  email?: string;
  role?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/api/users`);
  }

  getUserById(id: number): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/api/users/${id}`);
  }

  createUser(user: CreateUserRequest): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/api/users`, user);
  }

  updateUser(id: number, user: UpdateUserRequest): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/api/users/${id}`, user);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/users/${id}`);
  }
}

