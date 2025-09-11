import { writable } from 'svelte/store';

export const settings = writable({
	autoLock: true
	// ...add other settings as needed
});
