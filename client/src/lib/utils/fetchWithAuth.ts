/* eslint-disable svelte/no-navigation-without-resolve */
import { authStore } from "$lib/stores/authStore";
import { goto } from "$app/navigation";

export async function fetchWithAuth(input: RequestInfo, init?: RequestInit) {
    const response = await fetch(input, init);
    if (response.status === 401 || response.status === 403) {
        authStore.clearUser();
        await goto('/login');
        throw new Error('Unauthorized access');
    }
    return response;
}