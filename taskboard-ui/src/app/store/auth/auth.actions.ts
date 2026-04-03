import { createAction, props } from '@ngrx/store';
import { User } from '../../core/models/user.model';

export const login = createAction(
  '[Auth] Login',
  props<{ email: string; password: string }>()
);

export const loginSuccess = createAction(
  '[Auth] Login Success',
  props<{ oneTimeCode: string }>()
);

export const loginFailure = createAction(
  '[Auth] Login Failure',
  props<{ error: string }>()
);

export const exchangeToken = createAction(
  '[Auth] Exchange Token',
  props<{ code: string }>()
);

export const exchangeTokenSuccess = createAction(
  '[Auth] Exchange Token Success',
  props<{ accessToken: string; user: User }>()
);

export const exchangeTokenFailure = createAction(
  '[Auth] Exchange Token Failure',
  props<{ error: string }>()
);

export const logout = createAction('[Auth] Logout');

export const restoreSession = createAction('[Auth] Restore Session');

export const restoreSessionSuccess = createAction(
  '[Auth] Restore Session Success',
  props<{ user: User; token: string }>()
);
