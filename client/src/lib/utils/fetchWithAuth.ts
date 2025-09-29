/* eslint-disable svelte/no-navigation-without-resolve */
import { authStore } from "$lib/stores/authStore";
import { goto } from "$app/navigation";

// Public endpoints that should not trigger auth redirects
const PUBLIC_ENDPOINTS = [
    '/api/users/login',
    '/api/users/register',
    '/api/users/verify-email',
    '/api/users/verify-otp',
    '/api/users/resend-otp',
    '/api/auth/google-login'
];

function isPublicEndpoint(url: string): boolean {
    return PUBLIC_ENDPOINTS.some(endpoint => url.includes(endpoint));
}

export async function fetchWithAuth(input: RequestInfo, init?: RequestInit) {
    const response = await fetch(input, init);
    
    if (response.status === 401) {
        const url = typeof input === 'string' ? input : input.url;

        // Don't clear auth or redirect for public endpoints
        if (isPublicEndpoint(url)) {
            return response;
        }

        
        authStore.clearUser();
        await goto('/login');

        return response;       
    } else if (response.status === 403) {
        return response;
    }
    return response;
}