import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { switchMap, map, catchError } from 'rxjs/operators';
import { NotificationService } from '../../core/services/notification.service';
import * as NotificationsActions from './notifications.actions';

@Injectable()
export class NotificationsEffects {
  loadNotifications$ = createEffect(() =>
    this.actions$.pipe(
      ofType(NotificationsActions.loadNotifications),
      switchMap(({ unreadOnly, page, pageSize }) =>
        this.notificationService
          .getNotifications(unreadOnly ?? false, page ?? 1, pageSize ?? 20)
          .pipe(
            map(response => NotificationsActions.loadNotificationsSuccess({ response })),
            catchError(err =>
              of(
                NotificationsActions.loadNotificationsFailure({
                  error: err.error?.message || 'Failed to load notifications'
                })
              )
            )
          )
      )
    )
  );

  markAsRead$ = createEffect(() =>
    this.actions$.pipe(
      ofType(NotificationsActions.markAsRead),
      switchMap(({ id }) =>
        this.notificationService.markAsRead(id).pipe(
          map(() => NotificationsActions.markAsReadSuccess({ id })),
          catchError(err =>
            of(
              NotificationsActions.markAsReadFailure({
                error: err.error?.message || 'Failed to mark notification as read'
              })
            )
          )
        )
      )
    )
  );

  constructor(
    private readonly actions$: Actions,
    private readonly notificationService: NotificationService
  ) {}
}
