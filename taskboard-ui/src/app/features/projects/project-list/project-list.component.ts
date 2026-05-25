import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject, takeUntil } from 'rxjs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ProjectService } from '../../../core/services/project.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AppState } from '../../../store/app.state';
import { loadProjects, createProject } from '../../../store/projects/projects.actions';
import {
  selectProjects,
  selectProjectsLoading,
  selectProjectsError
} from '../../../store/projects/projects.selectors';
import { Project } from '../../../core/models/project.model';
import { CreateProjectDialogComponent } from './create-project-dialog.component';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatTooltipModule,
    MatChipsModule,
    MatSlideToggleModule
  ],
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.scss']
})
export class ProjectListComponent implements OnInit, OnDestroy {
  projects$: Observable<Project[]>;
  isLoading$: Observable<boolean>;
  error$: Observable<string | null>;

  displayedColumns = ['name', 'ownerName', 'memberCount', 'taskCount', 'status', 'actions'];
  showArchived = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly store: Store<AppState>,
    private readonly dialog: MatDialog,
    private readonly projectService: ProjectService
  ) {
    this.projects$ = this.store.select(selectProjects);
    this.isLoading$ = this.store.select(selectProjectsLoading);
    this.error$ = this.store.select(selectProjectsError);
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.store.dispatch(loadProjects({ includeArchived: this.showArchived }));
  }

  onToggleArchived(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onArchiveProject(project: Project, event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    const isArchived = project.isArchived;
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '360px',
      data: {
        title: isArchived ? 'Unarchive Project' : 'Archive Project',
        message: isArchived
          ? `Restore "${project.name}" to the active project list?`
          : `Archive "${project.name}"? It will be hidden from the default list.`,
        confirmLabel: isArchived ? 'Unarchive' : 'Archive',
        cancelLabel: 'Cancel'
      }
    });

    dialogRef.afterClosed().pipe(takeUntil(this.destroy$)).subscribe(confirmed => {
      if (!confirmed) return;
      const action$ = isArchived
        ? this.projectService.unarchiveProject(project.id)
        : this.projectService.archiveProject(project.id);
      action$.pipe(takeUntil(this.destroy$)).subscribe(() => this.load());
    });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreateProjectDialogComponent, {
      width: '480px'
    });

    dialogRef.afterClosed().pipe(takeUntil(this.destroy$)).subscribe(result => {
      if (result) {
        this.store.dispatch(createProject({ name: result.name, description: result.description }));
      }
    });
  }
}
