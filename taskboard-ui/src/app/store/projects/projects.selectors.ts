import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ProjectsState } from './projects.reducer';

export const selectProjectsState = createFeatureSelector<ProjectsState>('projects');

export const selectProjects = createSelector(
  selectProjectsState,
  state => state.projects
);

export const selectSelectedProject = createSelector(
  selectProjectsState,
  state => state.selectedProject
);

export const selectProjectsLoading = createSelector(
  selectProjectsState,
  state => state.isLoading
);

export const selectProjectsError = createSelector(
  selectProjectsState,
  state => state.error
);
