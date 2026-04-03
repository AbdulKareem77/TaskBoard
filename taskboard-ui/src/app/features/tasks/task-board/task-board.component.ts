import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject, map } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { AppState } from '../../../store/app.state';
import { loadTasks } from '../../../store/tasks/tasks.actions';
import { selectTaskItems, selectTasksLoading, selectTasksError } from '../../../store/tasks/tasks.selectors';
import { TaskItem } from '../../../core/models/task-item.model';
import { TaskFormComponent, TaskFormData } from '../task-form/task-form.component';

@Component({
  selector: 'app-task-board',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatChipsModule,
    MatTooltipModule,
    MatButtonToggleModule
  ],
  templateUrl: './task-board.component.html',
  styleUrls: ['./task-board.component.scss']
})
export class TaskBoardComponent implements OnInit, OnDestroy {
  isLoading$: Observable<boolean>;
  error$: Observable<string | null>;

  todoTasks$: Observable<TaskItem[]>;
  inProgressTasks$: Observable<TaskItem[]>;
  reviewTasks$: Observable<TaskItem[]>;
  doneTasks$: Observable<TaskItem[]>;

  projectId!: string;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly store: Store<AppState>,
    private readonly dialog: MatDialog
  ) {
    this.isLoading$ = this.store.select(selectTasksLoading);
    this.error$ = this.store.select(selectTasksError);

    const allTasks$ = this.store.select(selectTaskItems);

    this.todoTasks$ = allTasks$.pipe(
      map(tasks => tasks.filter(t => t.status === 'Todo'))
    );
    this.inProgressTasks$ = allTasks$.pipe(
      map(tasks => tasks.filter(t => t.status === 'InProgress'))
    );
    this.reviewTasks$ = allTasks$.pipe(
      map(tasks => tasks.filter(t => t.status === 'Review'))
    );
    this.doneTasks$ = allTasks$.pipe(
      map(tasks => tasks.filter(t => t.status === 'Done'))
    );
  }

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('projectId') ?? '';
    this.store.dispatch(loadTasks({ projectId: this.projectId, page: 1, pageSize: 100 }));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  switchToList(): void {
    this.router.navigate(['/projects', this.projectId, 'tasks']);
  }

  openCreateTaskDialog(): void {
    const data: TaskFormData = { projectId: this.projectId };
    this.dialog.open(TaskFormComponent, { width: '560px', data });
  }

  getStatusClass(status: string): string {
    return 'status-' + status.toLowerCase().replace(/\s+/g, '');
  }

  getPriorityClass(priority: string | null): string {
    return priority ? 'priority-' + priority.toLowerCase() : '';
  }
}
