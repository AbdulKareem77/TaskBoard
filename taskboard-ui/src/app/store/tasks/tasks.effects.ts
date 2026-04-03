import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { switchMap, map, catchError, mergeMap } from 'rxjs/operators';
import { TaskService } from '../../core/services/task.service';
import * as TasksActions from './tasks.actions';

@Injectable()
export class TasksEffects {
  loadTasks$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TasksActions.loadTasks),
      switchMap(({ projectId, page, pageSize, status }) =>
        this.taskService.getTasks(projectId, page ?? 1, pageSize ?? 20, status).pipe(
          map(pagedResult => TasksActions.loadTasksSuccess({ pagedResult })),
          catchError(err =>
            of(
              TasksActions.loadTasksFailure({
                error: err.error?.message || 'Failed to load tasks'
              })
            )
          )
        )
      )
    )
  );

  loadTask$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TasksActions.loadTask),
      switchMap(({ projectId, taskId }) =>
        this.taskService.getTask(projectId, taskId).pipe(
          map(task => TasksActions.loadTaskSuccess({ task })),
          catchError(err =>
            of(
              TasksActions.loadTaskFailure({
                error: err.error?.message || 'Failed to load task'
              })
            )
          )
        )
      )
    )
  );

  createTask$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TasksActions.createTask),
      switchMap(({ projectId, title, description, status, dueDate }) =>
        this.taskService.createTask(projectId, { title, description, status, dueDate }).pipe(
          map(task => TasksActions.createTaskSuccess({ task })),
          catchError(err =>
            of(
              TasksActions.createTaskFailure({
                error: err.error?.message || 'Failed to create task'
              })
            )
          )
        )
      )
    )
  );

  updateTask$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TasksActions.updateTask),
      switchMap(({ projectId, taskId, title, description, status, dueDate, rowVersion }) =>
        this.taskService
          .updateTask(projectId, taskId, { title, description, status, dueDate, rowVersion })
          .pipe(
            map(task => TasksActions.updateTaskSuccess({ task })),
            catchError(err => {
              if (err.status === 409) {
                return of(
                  TasksActions.updateTaskConflict({
                    error:
                      'This task was modified by another user. Please refresh and try again.'
                  })
                );
              }
              return of(
                TasksActions.updateTaskFailure({
                  error: err.error?.message || 'Failed to update task'
                })
              );
            })
          )
      )
    )
  );

  deleteTask$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TasksActions.deleteTask),
      switchMap(({ projectId, taskId }) =>
        this.taskService.deleteTask(projectId, taskId).pipe(
          map(() => TasksActions.deleteTaskSuccess({ taskId })),
          catchError(err =>
            of(
              TasksActions.deleteTaskFailure({
                error: err.error?.message || 'Failed to delete task'
              })
            )
          )
        )
      )
    )
  );

  assignTask$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TasksActions.assignTask),
      switchMap(({ projectId, taskId, userId }) =>
        this.taskService.assignTask(projectId, taskId, userId).pipe(
          mergeMap(() => [
            TasksActions.assignTaskSuccess(),
            TasksActions.loadTask({ projectId, taskId })
          ]),
          catchError(err =>
            of(
              TasksActions.assignTaskFailure({
                error: err.error?.message || 'Failed to assign task'
              })
            )
          )
        )
      )
    )
  );

  unassignTask$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TasksActions.unassignTask),
      switchMap(({ projectId, taskId, userId }) =>
        this.taskService.unassignTask(projectId, taskId, userId).pipe(
          mergeMap(() => [
            TasksActions.unassignTaskSuccess(),
            TasksActions.loadTask({ projectId, taskId })
          ]),
          catchError(err =>
            of(
              TasksActions.unassignTaskFailure({
                error: err.error?.message || 'Failed to unassign task'
              })
            )
          )
        )
      )
    )
  );

  constructor(
    private readonly actions$: Actions,
    private readonly taskService: TaskService
  ) {}
}
