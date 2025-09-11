import { writable } from 'svelte/store';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

interface Toast {
	message: string;
	type?: ToastType;
	duration?: number; // ms
	description?: string;
}

const defaultDuration = 2500;

function createToastStore() {
	const { subscribe, set } = writable<Toast | null>(null);

	let timeout: ReturnType<typeof setTimeout>;

	function show(message: string, type: ToastType = 'info', duration = defaultDuration) {
		set({ message, type, duration });
		clearTimeout(timeout);
		timeout = setTimeout(() => set(null), duration);
	}

	function clear() {
		set(null);
		clearTimeout(timeout);
	}

	return {
		subscribe,
		show,
		clear
	};
}

export const toast = createToastStore();
