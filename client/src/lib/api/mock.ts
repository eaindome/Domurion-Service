/* eslint-disable @typescript-eslint/no-unused-vars */
// Mock API for UI testing
import type { VaultItem } from '$lib/types';

// Dummy vault items (same as dashboard dummy data)
export const mockVaultItems: VaultItem[] = [
	{
		id: 101,
		siteName: 'GitHub',
		siteUrl: 'https://github.com',
		username: 'dummyuser',
		password: 'dummyPass123!',
		notes: 'Personal GitHub account',
		createdAt: '2025-09-01',
		updatedAt: '2025-09-10',
	},
	{
		id: 102,
		siteName: 'Google',
		siteUrl: 'https://accounts.google.com',
		username: 'dummy@gmail.com',
		password: 'dummyGooglePass!',
		notes: 'Google account for testing',
		createdAt: '2025-08-15',
		updatedAt: '2025-09-12',
	},
	{
		id: 103,
		siteName: 'Twitter',
		siteUrl: 'https://twitter.com',
		username: 'dummy_twitter',
		password: 'dummyTwitterPass!',
		notes: 'Test Twitter account',
		createdAt: '2025-07-20',
		updatedAt: '2025-09-13',
	},
];

// Simulate network delay
function delay(ms: number) {
	return new Promise((resolve) => setTimeout(resolve, ms));
}

// Mock: List vault entries
export async function listVaultEntries(_userId: string) {
	await delay(300);
	return {
		success: true,
		entries: mockVaultItems,
	};
}

// Mock: Delete vault entry
export async function deleteVaultEntry(_entryId: string, _userId: string) {
	await delay(200);
	return {
		success: true,
	};
}

// Mock: Create share invitation
export async function createShareInvitation(entryId: string, _recipient: string) {
	await delay(400);
	// Always succeed for UI testing
	return {
		success: true,
		invitationId: 'mock-invite-' + entryId,
	};
}

// Mock: List shared with me
export async function listSharedWithMe(_userId: string) {
	await delay(300);
	// Return a subset for demo
	return {
		success: true,
		entries: [mockVaultItems[0]],
	};
}
