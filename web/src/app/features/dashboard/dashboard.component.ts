import { Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-dashboard',
    template: `
    <div class="layout">
      <h2 class="section-title">Welcome back</h2>
      <div class="card-grid">
        <div class="card">
          <h3 class="section-title">Resume</h3>
          <p class="text-muted">Upload, parse, and polish your CV.</p>
        </div>
        <div class="card">
          <h3 class="section-title">Job intake</h3>
          <p class="text-muted">Capture job descriptions with OCR support.</p>
        </div>
        <div class="card">
          <h3 class="section-title">Cover letters</h3>
          <p class="text-muted">Generate tailored letters in seconds.</p>
        </div>
      </div>
    </div>
  `
})
export class DashboardComponent { }
