import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject, takeUntil } from 'rxjs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { AppState } from '../../../store/app.state';
import { loadTasks, deleteTask, clearSelectedTask } from '../../../store/tasks/tasks.actions';
import {
  selectTasks,
  selectTasksLoading,
  selectTasksError
} from '../../../store/tasks/tasks.selectors';
import { PagedResult } from '../../../core/models/paged-result.model';
import { TaskItem } from '../../../core/models/task-item.model';
import { TaskFormComponent, TaskFormData } from '../task-form/task-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    RouterModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatChipsModule,
    MatTooltipModule,
    MatPaginatorModule,
    MatButtonToggleModule
  ],
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.scss']
})
export class TaskListComponent implements OnInit, OnDestroy {
  tasks$: Observable<PagedResult<TaskItem> | null>;
  isLoading$: Observable<boolean>;
  error$: Observable<string | null>;

  projectId!: string;
  currentPage = 1;
  pageSize = 20;
  selectedStatus = '';

  displayedColumns = ['title', 'status', 'priority', 'dueDate', 'assignees', 'actions'];

  readonly statusOptions = [
    { value: '', label: 'All' },
    { value: 'Todo', label: 'To Do' },
    { value: 'InProgress', label: 'In Progress' },
    { value: 'Review', label: 'Review' },
    { value: 'Done', label: 'Done' }
  ];

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly store: Store<AppState>,
    private readonly dialog: MatDialog
  ) {
    this.tasks$ = this.store.select(selectTasks);
    this.isLoading$ = this.store.select(selectTasksLoading);
    this.error$ = this.store.select(selectTasksError);
  }

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('projectId') ?? '';
    this.loadTasks();
  }

  ngOnDestroy(): void {
    this.store.dispatch(clearSelectedTask());
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadTasks(): void {
    this.store.dispatch(loadTasks({
      projectId: this.projectId,
      page: this.currentPage,
      pageSize: this.pageSize,
      status: this.selectedStatus || undefined
    }));
  }

  onStatusFilter(status: string): void {
    this.selectedStatus = status;
    this.currentPage = 1;
    this.loadTasks();
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadTasks();
  }

  switchToBoard(): void {
    this.router.navigate(['/projects', this.projectId, 'board']);
  }

  openCreateTaskDialog(): void {
    const data: TaskFormData = { projectId: this.projectId };
    this.dialog.open(TaskFormComponent, { width: '560px', data });
  }

  onDeleteTask(task: TaskItem): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '360px',
      data: {
        title: 'Delete Task',
        message: `Are you sure you want to delete "${task.title}"? This action cannot be undone.`,
        confirmLabel: 'Delete',
        cancelLabel: 'Cancel'
      }
    });

    dialogRef.afterClosed().pipe(takeUntil(this.destroy$)).subscribe(confirmed => {
      if (confirmed) {
        this.store.dispatch(deleteTask({ projectId: this.projectId, taskId: task.id }));
      }
    });
  }

  getStatusLabel(): string {
    return this.statusOptions.find(o => o.value === this.selectedStatus)?.label ?? 'All';
  }

  getStatusClass(status: string): string {
    return 'status-' + status.toLowerCase().replace(/\s+/g, '');
  }

  getPriorityClass(priority: string | null): string {
    return priority ? 'priority-' + priority.toLowerCase() : '';
  }
}
