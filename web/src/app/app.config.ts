import { ApplicationConfig, ErrorHandler, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { appRoutes } from './app.routes';
import { jwtInterceptor } from './core/auth/jwt.interceptor';
import { GlobalErrorHandler } from './core/error/global-error.handler';
import { ENVIRONMENT } from './core/config/environment.token';
import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
    providers: [
        provideZoneChangeDetection({ eventCoalescing: true }),
        provideRouter(appRoutes),
        provideHttpClient(withInterceptors([jwtInterceptor])),
        { provide: ErrorHandler, useClass: GlobalErrorHandler },
        { provide: ENVIRONMENT, useValue: environment }
    ]
};
