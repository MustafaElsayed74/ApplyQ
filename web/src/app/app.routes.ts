import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const appRoutes: Routes = [
    { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
    {
        path: 'auth',
        loadComponent: () => import('./features/auth/login.component').then((m) => m.LoginComponent)
    },
    {
        path: 'dashboard',
        canActivate: [authGuard],
        loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
    },
    {
        path: 'resume',
        canActivate: [authGuard],
        loadComponent: () => import('./features/resume/resume.component').then((m) => m.ResumeComponent)
    },
    {
        path: 'job',
        canActivate: [authGuard],
        loadComponent: () => import('./features/job/job.component').then((m) => m.JobComponent)
    },
    {
        path: 'cover-letter',
        canActivate: [authGuard],
        loadComponent: () => import('./features/cover-letter/cover-letter.component').then((m) => m.CoverLetterComponent)
    },
    {
        path: 'profile',
        canActivate: [authGuard],
        loadComponent: () => import('./features/profile/profile.component').then((m) => m.ProfileComponent)
    },
    {
        path: '**',
        loadComponent: () => import('./features/error/not-found.component').then((m) => m.NotFoundComponent)
    }
];
