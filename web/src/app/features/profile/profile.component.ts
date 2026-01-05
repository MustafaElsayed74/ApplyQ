import { Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-profile',
    template: `
    <div class="layout">
      <h2 class="section-title">Profile</h2>
      <div class="card">
        <p class="text-muted">Manage account details, plan, and API usage.</p>
      </div>
    </div>
  `
})
export class ProfileComponent { }
