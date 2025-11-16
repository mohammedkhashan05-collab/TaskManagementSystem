import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TaskService, Task, CreateTaskRequest, UpdateTaskRequest } from '../../services/task.service';
import { UserService, User } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-task-list',
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css']
})
export class TaskListComponent implements OnInit {
  tasks: Task[] = [];
  users: User[] = [];
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  showForm: boolean = false;
  editingTask: Task | null = null;
  taskForm: FormGroup;
  isAdmin: boolean = false;
  currentUserId: number | null = null;

  constructor(
    private taskService: TaskService,
    private userService: UserService,
    private authService: AuthService,
    private fb: FormBuilder
  ) {
    this.isAdmin = this.authService.isAdmin();
    const currentUser = this.authService.getCurrentUser();
    this.currentUserId = currentUser?.id || null;

    this.taskForm = this.fb.group({
      title: ['', [Validators.required]],
      description: [''],
      status: ['Pending', [Validators.required]],
      assignedUserId: [null, [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadTasks();
    if (this.isAdmin) {
      this.loadUsers();
    } else {
      // For regular users, set their own ID as assigned user
      if (this.currentUserId) {
        this.taskForm.patchValue({ assignedUserId: this.currentUserId });
      }
    }
  }

  loadTasks(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

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

  loadUsers(): void {
    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.users = users;
      },
      error: (error) => {
        console.error('Error loading users:', error);
      }
    });
  }

  openCreateForm(): void {
    if (!this.isAdmin) {
      this.errorMessage = 'Only admins can create tasks';
      return;
    }

    this.editingTask = null;
    this.taskForm.reset({
      title: '',
      description: '',
      status: 'Pending',
      assignedUserId: null
    });
    this.showForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditForm(task: Task): void {
    // Check if user can edit this task
    if (!this.isAdmin && task.assignedUserId !== this.currentUserId) {
      this.errorMessage = 'You can only edit your own tasks';
      return;
    }

    this.editingTask = task;
    this.taskForm.patchValue({
      title: task.title,
      description: task.description,
      status: task.status,
      assignedUserId: task.assignedUserId
    });

    // If not admin, disable all fields except status
    if (!this.isAdmin) {
      this.taskForm.get('title')?.disable();
      this.taskForm.get('description')?.disable();
      this.taskForm.get('assignedUserId')?.disable();
    }

    this.showForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openStatusUpdateForm(task: Task): void {
    // For regular users, only allow status update
    if (!this.isAdmin && task.assignedUserId !== this.currentUserId) {
      this.errorMessage = 'You can only update the status of your own tasks';
      return;
    }

    this.editingTask = task;
    this.taskForm.patchValue({
      title: task.title,
      description: task.description,
      status: task.status,
      assignedUserId: task.assignedUserId
    });

    // Disable all fields except status
    this.taskForm.get('title')?.disable();
    this.taskForm.get('description')?.disable();
    this.taskForm.get('assignedUserId')?.disable();

    this.showForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingTask = null;
    this.taskForm.reset();
    this.taskForm.get('title')?.enable();
    this.taskForm.get('description')?.enable();
    this.taskForm.get('assignedUserId')?.enable();
  }

  onSubmit(): void {
    if (this.taskForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      if (this.editingTask) {
        // Update task
        const updateData: UpdateTaskRequest = {
          title: this.taskForm.get('title')?.value,
          description: this.taskForm.get('description')?.value,
          status: this.taskForm.get('status')?.value,
          assignedUserId: this.taskForm.get('assignedUserId')?.value
        };

        this.taskService.updateTask(this.editingTask.id, updateData).subscribe({
          next: () => {
            this.isLoading = false;
            this.successMessage = 'Task updated successfully';
            this.loadTasks();
            this.cancelForm();
          },
          error: (error) => {
            this.isLoading = false;
            this.errorMessage = error.error?.message || 'Failed to update task';
          }
        });
      } else {
        // Create task
        const createData: CreateTaskRequest = {
          title: this.taskForm.value.title,
          description: this.taskForm.value.description,
          status: this.taskForm.value.status,
          assignedUserId: this.taskForm.value.assignedUserId
        };

        this.taskService.createTask(createData).subscribe({
          next: () => {
            this.isLoading = false;
            this.successMessage = 'Task created successfully';
            this.loadTasks();
            this.cancelForm();
          },
          error: (error) => {
            this.isLoading = false;
            this.errorMessage = error.error?.message || 'Failed to create task';
          }
        });
      }
    }
  }

  deleteTask(id: number): void {
    if (confirm('Are you sure you want to delete this task?')) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      this.taskService.deleteTask(id).subscribe({
        next: () => {
          this.isLoading = false;
          this.successMessage = 'Task deleted successfully';
          this.loadTasks();
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Failed to delete task';
        }
      });
    }
  }

  getStatusClass(status: string): string {
    const statusLower = status.toLowerCase();
    if (statusLower === 'completed') return 'badge-completed';
    if (statusLower === 'inprogress') return 'badge-inprogress';
    return 'badge-pending';
  }

  canEditTask(task: Task): boolean {
    return this.isAdmin || task.assignedUserId === this.currentUserId;
  }

  canDeleteTask(): boolean {
    return this.isAdmin;
  }

  get title() {
    return this.taskForm.get('title');
  }

  get status() {
    return this.taskForm.get('status');
  }

  get assignedUserId() {
    return this.taskForm.get('assignedUserId');
  }
}

