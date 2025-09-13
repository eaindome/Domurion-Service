import type { UserSettings } from '$lib/types';

// Fetch current user's settings (preferences)
export async function fetchUserSettings(): Promise<UserSettings> {
	const response = await fetch('/api/user/preferences', {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' }
	});
	if (!response.ok) {
		throw new Error('Failed to fetch user settings');
	}
	return response.json();
}

// Update current user's settings (preferences)
export async function updateUserSettings(settings: UserSettings): Promise<UserSettings> {
	const response = await fetch('/api/user/preferences', {
		method: 'PUT',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(settings)
	});
	if (!response.ok) {
		throw new Error('Failed to update user settings');
	}
	return response.json();
}

// Generate password with custom options (all options optional)
export async function generatePassword(options?: {
	length?: number;
	useUppercase?: boolean;
	useLowercase?: boolean;
	useNumbers?: boolean;
	useSymbols?: boolean;
}): Promise<string> {
	const params = new URLSearchParams();
	if (options) {
		if (options.length !== undefined) params.append('length', options.length.toString());
		if (options.useUppercase !== undefined) params.append('useUppercase', String(options.useUppercase));
		if (options.useLowercase !== undefined) params.append('useLowercase', String(options.useLowercase));
		if (options.useNumbers !== undefined) params.append('useNumbers', String(options.useNumbers));
		if (options.useSymbols !== undefined) params.append('useSymbols', String(options.useSymbols));
	}
	const url = '/api/user/preferences/generate-password' + (params.toString() ? `?${params.toString()}` : '');
	const response = await fetch(url, {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' }
	});
	if (!response.ok) {
		throw new Error('Failed to generate password');
	}
	const data = await response.json();
	return data.password;
}