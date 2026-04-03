import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { switchMap, map, catchError, tap } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { SessionService } from '../../core/services/session.service';
import * as AuthActions from './auth.actions';

@Injectable()
export class AuthEffects {
  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.login),
      switchMap(({ email, password }) =>
        this.authService.login(email, password).pipe(
          map(({ oneTimeCode }) => AuthActions.loginSuccess({ oneTimeCode })),
          catchError(err =>
            of(AuthActions.loginFailure({ error: err.error?.error || 'Login failed' }))
          )
        )
      )
    )
  );

  loginSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loginSuccess),
      switchMap(({ oneTimeCode }) =>
        this.authService.exchangeToken(oneTimeCode).pipe(
          map(({ accessToken, user }) =>
            AuthActions.exchangeTokenSuccess({ accessToken, user })
          ),
          catchError(err =>
            of(
              AuthActions.exchangeTokenFailure({
                error: err.error?.error || 'Token exchange failed'
              })
            )
          )
        )
      )
    )
  );

  exchangeTokenSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.exchangeTokenSuccess),
        tap(({ accessToken, user }) => {
          this.sessionService.setToken(accessToken);
          this.sessionService.setUser(user);
          this.router.navigate(['/dashboard']);
        })
      ),
    { dispatch: false }
  );

  logout$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logout),
        tap(() => {
          this.sessionService.clearSession();
          this.router.navigate(['/login']);
        })
      ),
    { dispatch: false }
  );

  constructor(
    private readonly actions$: Actions,
    private readonly authService: AuthService,
    private readonly sessionService: SessionService,
    private readonly router: Router
  ) {}
}
