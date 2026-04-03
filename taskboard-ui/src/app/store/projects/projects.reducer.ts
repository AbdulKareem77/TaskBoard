import { createReducer, on } from '@ngrx/store';
import { Project, ProjectDetail } from '../../core/models/project.model';
import * as ProjectsActions from './projects.actions';

export interface ProjectsState {
  projects: Project[];
  selectedProject: ProjectDetail | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: ProjectsState = {
  projects: [],
  selectedProject: null,
  isLoading: false,
  error: null
};

export const projectsReducer = createReducer(
  initialState,
  on(ProjectsActions.loadProjects, state => ({ ...state, isLoading: true, error: null })),
  on(ProjectsActions.loadProjectsSuccess, (state, { projects }) => ({
    ...state,
    projects,
    isLoading: false,
    error: null
  })),
  on(ProjectsActions.loadProjectsFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(ProjectsActions.loadProject, state => ({ ...state, isLoading: true, error: null })),
  on(ProjectsActions.loadProjectSuccess, (state, { project }) => ({
    ...state,
    selectedProject: project,
    isLoading: false,
    error: null
  })),
  on(ProjectsActions.loadProjectFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(ProjectsActions.createProject, state => ({ ...state, isLoading: true, error: null })),
  on(ProjectsActions.createProjectSuccess, (state, { project }) => ({
    ...state,
    projects: [...state.projects, project],
    isLoading: false,
    error: null
  })),
  on(ProjectsActions.createProjectFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(ProjectsActions.clearSelectedProject, state => ({
    ...state,
    selectedProject: null
  }))
);
