import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, DatePipe, Location } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject, combineLatest, takeUntil } from 'rxjs';
import { map, filter } from 'rxjs/operators';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AppState } from '../../../store/app.state';
import { loadTask, clearSelectedTask, deleteTask, assignTask, unassignTask } from '../../../store/tasks/tasks.actions';
import {
  selectSelectedTask,
  selectTasksLoading,
  selectTasksError
} from '../../../store/tasks/tasks.selectors';
import { loadProject } from '../../../store/projects/projects.actions';
import { selectSelectedProject } from '../../../store/projects/projects.selectors';
import { TaskItemDetail } from '../../../core/models/task-item.model';
import { ProjectMember } from '../../../core/models/project.model';
import { TaskFormComponent, TaskFormData } from '../task-form/task-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { TimeAgoPipe } from '../../../shared/pipes/time-ago.pipe';
import { Router } from '@angular/router';
import { TaskService } from '../../../core/services/task.service';
import { TaskComment } from '../../../core/models/comment.model';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatChipsModule,
    MatDividerModule,
    MatListModule,
    MatTooltipModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    TimeAgoPipe
  ],
  templateUrl: './task-detail.component.html',
  styleUrls: ['./task-detail.component.scss']
})
export class TaskDetailComponent implements OnInit, OnDestroy {
  task$: Observable<TaskItemDetail | null>;
  isLoading$: Observable<boolean>;
  error$: Observable<string | null>;
  availableMembers$!: Observable<ProjectMember[]>;

  projectId!: string;
  taskId!: string;
  showAddAssignee = false;
  comments: TaskComment[] = [];
  commentsLoading = false;
  commentSubmitting = false;
  commentForm!: FormGroup;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly location: Location,
    private readonly store: Store<AppState>,
    private readonly dialog: MatDialog,
    private readonly fb: FormBuilder,
    private readonly taskService: TaskService
  ) {
    this.task$ = this.store.select(selectSelectedTask);
    this.isLoading$ = this.store.select(selectTasksLoading);
    this.error$ = this.store.select(selectTasksError);
  }

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('projectId') ?? '';
    this.taskId = this.route.snapshot.paramMap.get('taskId') ?? '';
    this.commentForm = this.fb.group({
      content: ['', [Validators.required, Validators.maxLength(4000)]]
    });
    this.store.dispatch(loadTask({ projectId: this.projectId, taskId: this.taskId }));
    this.store.dispatch(loadProject({ projectId: this.projectId }));
    this.loadComments();

    this.availableMembers$ = combineLatest([
      this.task$.pipe(filter((t): t is TaskItemDetail => t !== null)),
      this.store.select(selectSelectedProject)
    ]).pipe(
      map(([task, project]) => {
        if (!project) return [];
        const assignedIds = new Set(task.assignees.map(a => a.userId));
        return project.members.filter(m => !assignedIds.has(m.userId));
      })
    );
  }

  ngOnDestroy(): void {
    this.store.dispatch(clearSelectedTask());
    this.destroy$.next();
    this.destroy$.complete();
  }

  goBack(): void {
    this.location.back();
  }

  scrollToComments(): void {
    document.getElementById('task-comments')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  onAssignUser(userId: string): void {
    this.showAddAssignee = false;
    this.store.dispatch(assignTask({ projectId: this.projectId, taskId: this.taskId, userId }));
  }

  onUnassignUser(userId: string): void {
    this.store.dispatch(unassignTask({ projectId: this.projectId, taskId: this.taskId, userId }));
  }

  openEditDialog(task: TaskItemDetail): void {
    const data: TaskFormData = { projectId: this.projectId, task };
    this.dialog.open(TaskFormComponent, { width: '560px', data });
  }

  onDeleteTask(task: TaskItemDetail): void {
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
        this.router.navigate(['/projects', this.projectId, 'tasks']);
      }
    });
  }

  getStatusClass(status: string): string {
    return 'status-' + status.toLowerCase().replace(/\s+/g, '');
  }

  getPriorityClass(priority: string | null): string {
    return priority ? 'priority-' + priority.toLowerCase() : '';
  }

  loadComments(): void {
    this.commentsLoading = true;
    this.taskService
      .getTaskComments(this.projectId, this.taskId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: comments => {
          this.comments = comments;
          this.commentsLoading = false;
        },
        error: () => {
          this.commentsLoading = false;
        }
      });
  }

  onPostComment(): void {
    if (this.commentForm.invalid || this.commentSubmitting) {
      this.commentForm.markAllAsTouched();
      return;
    }

    const content = this.commentForm.value.content?.trim();
    if (!content) return;

    this.commentSubmitting = true;
    this.taskService
      .createTaskComment(this.projectId, this.taskId, content)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: comment => {
          this.comments = [comment, ...this.comments];
          this.commentForm.reset();
          this.commentSubmitting = false;
        },
        error: () => {
          this.commentSubmitting = false;
        }
      });
  }
}
