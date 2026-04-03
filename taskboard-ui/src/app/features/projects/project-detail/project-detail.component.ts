import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject, takeUntil } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { AppState } from '../../../store/app.state';
import { loadProject, clearSelectedProject } from '../../../store/projects/projects.actions';
import {
  selectSelectedProject,
  selectProjectsLoading,
  selectProjectsError
} from '../../../store/projects/projects.selectors';
import { ProjectDetail } from '../../../core/models/project.model';
import { MemberListComponent } from './components/member-list.component';
import { AddMemberDialogComponent } from './components/add-member-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ProjectService } from '../../../core/services/project.service';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatDividerModule,
    MemberListComponent
  ],
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.scss']
})
export class ProjectDetailComponent implements OnInit, OnDestroy {
  project$: Observable<ProjectDetail | null>;
  isLoading$: Observable<boolean>;
  error$: Observable<string | null>;

  private projectId!: string;
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly store: Store<AppState>,
    private readonly dialog: MatDialog,
    private readonly projectService: ProjectService
  ) {
    this.project$ = this.store.select(selectSelectedProject);
    this.isLoading$ = this.store.select(selectProjectsLoading);
    this.error$ = this.store.select(selectProjectsError);
  }

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('projectId') ?? '';
    this.store.dispatch(loadProject({ projectId: this.projectId }));
  }

  ngOnDestroy(): void {
    this.store.dispatch(clearSelectedProject());
    this.destroy$.next();
    this.destroy$.complete();
  }

  openAddMemberDialog(): void {
    const dialogRef = this.dialog.open(AddMemberDialogComponent, { width: '400px' });

    dialogRef.afterClosed().pipe(takeUntil(this.destroy$)).subscribe(result => {
      if (result) {
        this.projectService
          .addMember(this.projectId, result)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.store.dispatch(loadProject({ projectId: this.projectId }));
          });
      }
    });
  }

  onRemoveMember(userId: string): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '360px',
      data: {
        title: 'Remove Member',
        message: 'Are you sure you want to remove this member from the project?',
        confirmLabel: 'Remove',
        cancelLabel: 'Cancel'
      }
    });

    dialogRef.afterClosed().pipe(takeUntil(this.destroy$)).subscribe(confirmed => {
      if (confirmed) {
        this.projectService
          .removeMember(this.projectId, userId)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.store.dispatch(loadProject({ projectId: this.projectId }));
          });
      }
    });
  }
}
