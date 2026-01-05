import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    standalone: true,
    selector: 'app-login',
    imports: [FormsModule],
    template: `
    <div class="app-shell">
      <div class="card" style="max-width: 420px; margin: 40px auto;">
        <h2 class="section-title">Sign in</h2>
        <form (ngSubmit)="submit()" class="layout" style="gap: 12px;">
          <label class="layout" style="gap: 6px;">
            <span class="text-muted">Email</span>
            <input type="email" [(ngModel)]="email" name="email" required />
          </label>
          <label class="layout" style="gap: 6px;">
            <span class="text-muted">Password</span>
            <input type="password" [(ngModel)]="password" name="password" required />
          </label>
          <button class="button" type="submit">Continue</button>
        </form>
      </div>
    </div>
  `
})
export class LoginComponent {
    private readonly auth = inject(AuthService);
    private readonly router = inject(Router);

    email = '';
    password = '';

    async submit(): Promise<void> {
        await this.auth.login(this.email, this.password);
        this.router.navigate(['/dashboard']);
    }
}
