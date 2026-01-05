import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [RouterOutlet, RouterLink],
    template: `
    <div class="app-shell">
      <header class="navbar">
        <div>
          <strong>AI Resume & Cover Letter</strong>
        </div>
        <nav class="nav-links" *ngIf="isAuthed()">
          <a routerLink="/dashboard">Dashboard</a>
          <a routerLink="/resume">Resumes</a>
          <a routerLink="/job">Jobs</a>
          <a routerLink="/cover-letter">Cover Letters</a>
          <a routerLink="/profile">Profile</a>
        </nav>
        <button class="button secondary" *ngIf="isAuthed()" (click)="logout()">Logout</button>
      </header>

      <main class="layout">
        <router-outlet />
      </main>
    </div>
  `
})
export class AppComponent {
    private readonly auth = inject(AuthService);
    private readonly router = inject(Router);

    readonly isAuthed = computed(() => this.auth.isAuthenticated());

    logout(): void {
        this.auth.logout();
        this.router.navigate(['/auth']);
    }
}
