import { Injectable, WritableSignal, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ENVIRONMENT, Environment } from '../config/environment.token';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly http = inject(HttpClient);
    private readonly env = inject(ENVIRONMENT);
    private readonly token: WritableSignal<string | null> = signal(this.readToken());

    readonly isAuthenticated = computed(() => Boolean(this.token()));

    getToken(): string | null {
        return this.token();
    }

    async login(email: string, password: string): Promise<void> {
        // Replace with real API call
        const mockToken = btoa(`${email}:${password}`);
        this.persistToken(mockToken);
    }

    logout(): void {
        this.persistToken(null);
    }

    private persistToken(value: string | null): void {
        this.token.set(value);
        if (value) {
            localStorage.setItem('auth_token', value);
        } else {
            localStorage.removeItem('auth_token');
        }
    }

    private readToken(): string | null {
        return localStorage.getItem('auth_token');
    }
}
