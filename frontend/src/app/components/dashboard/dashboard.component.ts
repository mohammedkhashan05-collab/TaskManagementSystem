import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { TaskService, Task } from '../../services/task.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  currentUser: any;
  tasks: Task[] = [];
  isLoading: boolean = false;
  errorMessage: string = '';

  constructor(
    private authService: AuthService,
    private taskService: TaskService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.loadTasks();
  }

  loadTasks(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.taskService.getAllTasks().subscribe({
      next: (tasks) => {
        this.tasks = tasks;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load tasks';
        console.error('Error loading tasks:', error);
      }
    });
  }

  getStatusClass(status: string): string {
    const statusLower = status.toLowerCase();
    if (statusLower === 'completed') return 'badge-completed';
    if (statusLower === 'inprogress') return 'badge-inprogress';
    return 'badge-pending';
  }

  isAdmin(): boolean {
    return this.authService.isAdmin();
  }
}

