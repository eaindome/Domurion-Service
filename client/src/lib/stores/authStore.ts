import { writable } from 'svelte/store';

export interface User {
	id: string;
	email: string;
	name?: string;
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

function createAuthStore() {
	const { subscribe, set, update } = writable<AuthState>(initialState);

	return {
		subscribe,
		setUser: (user: User) =>
			update((state) => ({ ...state, user, isAuthenticated: true, loading: false, error: null })),
		clearUser: () => set(initialState),
		setLoading: (loading: boolean) => update((state) => ({ ...state, loading })),
		setError: (error: string | null) => update((state) => ({ ...state, error, loading: false })),
		logout: () => set(initialState)
	};
}

export const authStore = createAuthStore();

// so how are we going to be using authStore, we you go through the codebase, where are we suppose to use authStore
