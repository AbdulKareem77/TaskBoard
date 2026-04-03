import { createAction, props } from '@ngrx/store';
import { Project, ProjectDetail } from '../../core/models/project.model';

export const loadProjects = createAction('[Projects] Load Projects');

export const loadProjectsSuccess = createAction(
  '[Projects] Load Projects Success',
  props<{ projects: Project[] }>()
);

export const loadProjectsFailure = createAction(
  '[Projects] Load Projects Failure',
  props<{ error: string }>()
);

export const loadProject = createAction(
  '[Projects] Load Project',
  props<{ projectId: string }>()
);

export const loadProjectSuccess = createAction(
  '[Projects] Load Project Success',
  props<{ project: ProjectDetail }>()
);

export const loadProjectFailure = createAction(
  '[Projects] Load Project Failure',
  props<{ error: string }>()
);

export const createProject = createAction(
  '[Projects] Create Project',
  props<{ name: string; description?: string | null }>()
);

export const createProjectSuccess = createAction(
  '[Projects] Create Project Success',
  props<{ project: Project }>()
);

export const createProjectFailure = createAction(
  '[Projects] Create Project Failure',
  props<{ error: string }>()
);

export const clearSelectedProject = createAction('[Projects] Clear Selected Project');
