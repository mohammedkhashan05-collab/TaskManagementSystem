import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService, User, CreateUserRequest, UpdateUserRequest } from '../../services/user.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  showForm: boolean = false;
  editingUser: User | null = null;
  userForm: FormGroup;

  constructor(
    private userService: UserService,
    private fb: FormBuilder
  ) {
    this.userForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['User', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load users';
        console.error('Error loading users:', error);
      }
    });
  }

  openCreateForm(): void {
    this.editingUser = null;
    this.userForm.reset({
      username: '',
      email: '',
      password: '',
      role: 'User'
    });
    this.showForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditForm(user: User): void {
    this.editingUser = user;
    this.userForm.patchValue({
      username: user.username,
      email: user.email,
      password: '', // Don't populate password for edit
      role: user.role
    });
    this.userForm.get('password')?.clearValidators();
    this.userForm.get('password')?.updateValueAndValidity();
    this.showForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingUser = null;
    this.userForm.reset();
    this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
    this.userForm.get('password')?.updateValueAndValidity();
  }

  onSubmit(): void {
    if (this.userForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      if (this.editingUser) {
        // Update user
        const updateData: UpdateUserRequest = {
          username: this.userForm.value.username,
          email: this.userForm.value.email,
          role: this.userForm.value.role
        };

        this.userService.updateUser(this.editingUser.id, updateData).subscribe({
          next: () => {
            this.isLoading = false;
            this.successMessage = 'User updated successfully';
            this.loadUsers();
            this.cancelForm();
          },
          error: (error) => {
            this.isLoading = false;
            this.errorMessage = error.error?.message || 'Failed to update user';
          }
        });
      } else {
        // Create user
        const createData: CreateUserRequest = {
          username: this.userForm.value.username,
          email: this.userForm.value.email,
          password: this.userForm.value.password,
          role: this.userForm.value.role
        };

        this.userService.createUser(createData).subscribe({
          next: () => {
            this.isLoading = false;
            this.successMessage = 'User created successfully';
            this.loadUsers();
            this.cancelForm();
          },
          error: (error) => {
            this.isLoading = false;
            this.errorMessage = error.error?.message || 'Failed to create user';
          }
        });
      }
    }
  }

  deleteUser(id: number): void {
    if (confirm('Are you sure you want to delete this user?')) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      this.userService.deleteUser(id).subscribe({
        next: () => {
          this.isLoading = false;
          this.successMessage = 'User deleted successfully';
          this.loadUsers();
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Failed to delete user';
        }
      });
    }
  }

  get username() {
    return this.userForm.get('username');
  }

  get email() {
    return this.userForm.get('email');
  }

  get password() {
    return this.userForm.get('password');
  }

  get role() {
    return this.userForm.get('role');
  }
}

