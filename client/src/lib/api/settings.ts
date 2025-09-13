import type { UserSettings } from '$lib/types';

export async function fetchUserSettings(userId: string): Promise<UserSettings> {
	const response = await fetch(`/api/settings/${userId}`);
	if (!response.ok) {
		throw new Error('Failed to fetch user settings');
	}
	return response.json();
}