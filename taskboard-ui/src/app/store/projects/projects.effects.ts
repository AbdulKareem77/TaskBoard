import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { switchMap, map, catchError } from 'rxjs/operators';
import { ProjectService } from '../../core/services/project.service';
import * as ProjectsActions from './projects.actions';

@Injectable()
export class ProjectsEffects {
  loadProjects$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProjectsActions.loadProjects),
      switchMap(() =>
        this.projectService.getProjects().pipe(
          map(projects => ProjectsActions.loadProjectsSuccess({ projects })),
          catchError(err =>
            of(
              ProjectsActions.loadProjectsFailure({
                error: err.error?.message || 'Failed to load projects'
              })
            )
          )
        )
      )
    )
  );

  loadProject$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProjectsActions.loadProject),
      switchMap(({ projectId }) =>
        this.projectService.getProject(projectId).pipe(
          map(project => ProjectsActions.loadProjectSuccess({ project })),
          catchError(err =>
            of(
              ProjectsActions.loadProjectFailure({
                error: err.error?.message || 'Failed to load project'
              })
            )
          )
        )
      )
    )
  );

  createProject$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ProjectsActions.createProject),
      switchMap(({ name, description }) =>
        this.projectService.createProject({ name, description }).pipe(
          map(project => ProjectsActions.createProjectSuccess({ project })),
          catchError(err =>
            of(
              ProjectsActions.createProjectFailure({
                error: err.error?.message || 'Failed to create project'
              })
            )
          )
        )
      )
    )
  );

  constructor(
    private readonly actions$: Actions,
    private readonly projectService: ProjectService
  ) {}
}
