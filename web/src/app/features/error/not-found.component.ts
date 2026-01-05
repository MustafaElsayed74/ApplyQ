import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    standalone: true,
    selector: 'app-not-found',
    imports: [RouterLink],
    template: `
    <div class="app-shell">
      <div class="card" style="max-width: 420px; margin: 40px auto; text-align: center;">
        <h2 class="section-title">Page not found</h2>
        <p class="text-muted">The page you are looking for does not exist.</p>
        <a class="button" routerLink="/dashboard">Go home</a>
      </div>
    </div>
  `
})
export class NotFoundComponent { }
