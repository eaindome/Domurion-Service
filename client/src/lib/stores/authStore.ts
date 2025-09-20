import { writable } from 'svelte/store';

export interface User {
	id: string;
	email: string;
	name?: string;
	username?: string;
	authProvider?: string;
	googleId?: string;
	twoFactorEnabled?: boolean;
}

interface AuthState {
	user: User | null;
	isAuthenticated: boolean;
	loading: boolean;
	error: string | null;
}

const initialState: AuthState = {
	user: null,
	isAuthenticated: false,
	loading: false,
	error: null
};

import { fetchCurrentUser } from '$lib/api/users';

function createAuthStore() {
	const { subscribe, set, update } = writable<AuthState>(initialState);

	async function hydrateAuth() {
		update((state) => ({ ...state, loading: true }));
		try {
			const res = await fetchCurrentUser();
			console.log('Hydrating auth store with user:', res);
			if (res.success && res.user) {
				set({ 
					user: { 
						id: res.user.id, 
						email: res.user.email ?? '', 
						username: res.user.username ?? '',
						googleId: res.user.googleId,
						name: res.user.name ?? '',
						authProvider: res.user.authProvider ?? '',
						twoFactorEnabled: res.user.twoFactorEnabled ?? false
					}, 
					isAuthenticated: true, 
					loading: false, 
					error: null 
				});
			} else {
				set(initialState);
			}
		} catch (err) {
			const error = err as Error;
			if (error.message !== 'Unauthorized access') {
				console.log(`Error hydrating auth: ${error}`);
			}
			set(initialState);
		}
	}

	return {
		subscribe,
		setUser: (user: User) =>
			update((state) => ({ ...state, user, isAuthenticated: true, loading: false, error: null })),
		clearUser: () => set(initialState),
		setLoading: (loading: boolean) => update((state) => ({ ...state, loading })),
		setError: (error: string | null) => update((state) => ({ ...state, error, loading: false })),
		logout: () => set(initialState),
		hydrateAuth
	};
}

export const authStore = createAuthStore();

// so how are we going to be using authStore, we you go through the codebase, where are we suppose to use authStore
