import { Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-resume',
    template: `
    <div class="layout">
      <h2 class="section-title">Resumes</h2>
      <div class="card">
        <p class="text-muted">Upload your CV, track parsing status, and edit structured data.</p>
        <button class="button" type="button">Upload Resume</button>
      </div>
    </div>
  `
})
export class ResumeComponent { }
