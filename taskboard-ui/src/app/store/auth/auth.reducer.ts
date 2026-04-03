import { createReducer, on } from '@ngrx/store';
import { User } from '../../core/models/user.model';
import * as AuthActions from './auth.actions';

export interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

const initialState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: false,
  error: null
};

export const authReducer = createReducer(
  initialState,
  on(AuthActions.login, state => ({ ...state, isLoading: true, error: null })),
  on(AuthActions.loginSuccess, state => ({ ...state, isLoading: false })),
  on(AuthActions.loginFailure, (state, { error }) => ({ ...state, isLoading: false, error })),
  on(AuthActions.exchangeTokenSuccess, (state, { accessToken, user }) => ({
    ...state,
    user,
    token: accessToken,
    isAuthenticated: true,
    isLoading: false,
    error: null
  })),
  on(AuthActions.exchangeTokenFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(AuthActions.logout, () => initialState),
  on(AuthActions.restoreSessionSuccess, (state, { user, token }) => ({
    ...state,
    user,
    token,
    isAuthenticated: true
  }))
);
