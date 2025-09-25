/* eslint-disable svelte/no-navigation-without-resolve */
import { authStore } from "$lib/stores/authStore";
import { goto } from "$app/navigation";
import { browser } from "$app/environment";

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
    
    if (response.status === 401 || response.status === 403) {
        const url = typeof input === 'string' ? input : input.url;
        
        // Don't clear auth or redirect for public endpoints
        if (isPublicEndpoint(url)) {
            return response;
        }
        
        // Only clear auth and redirect for protected endpoints
        authStore.clearUser();
        
        // Only redirect if we're in the browser and not already on a public page
        if (browser) {
            const currentPath = window.location.pathname;
            const publicRoutes = ['/login', '/register', '/verify', '/message', '/otp', '/'];
            const isOnPublicRoute = publicRoutes.some(route => currentPath.startsWith(route));
            
            if (!isOnPublicRoute) {
                await goto('/login');
            }
        }
        
        throw new Error('Unauthorized access');
    }
    
    return response;
}