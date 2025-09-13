
import type { VaultEntry } from '$lib/types';

// Add vault entry
export async function addVaultEntry(userId: string, site: string, username: string, password: string): Promise<{ success: boolean; entry?: VaultEntry; error?: string }> {
	const params = new URLSearchParams({ userId, site, username, password });
	const response = await fetch(`/api/vault/add?${params.toString()}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' }
	});
	if (response.ok) {
		const data = await response.json();
		return { success: true, entry: data };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error adding entry: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// List vault entries for user
export async function listVaultEntries(userId: string): Promise<{ success: boolean; entries?: VaultEntry[]; error?: string }> {
	const response = await fetch(`/api/vault/list?userId=${encodeURIComponent(userId)}`);
	if (response.ok) {
		const data = await response.json();
		return { success: true, entries: data };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error listing entries: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// Retrieve password for a vault entry
export async function retrieveVaultPassword(credentialId: string, userId: string): Promise<{ success: boolean; password?: string; error?: string }> {
	const response = await fetch(`/api/vault/retrieve?credentialId=${encodeURIComponent(credentialId)}&userId=${encodeURIComponent(userId)}`);
	if (response.ok) {
		const data = await response.json();
		return { success: true, password: data.Password };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error retrieving password: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// Update vault entry
export async function updateVaultEntry(credentialId: string, userId: string, site?: string, username?: string, password?: string): Promise<{ success: boolean; entry?: VaultEntry; error?: string }> {
	const params = new URLSearchParams({ credentialId, userId });
	if (site) params.append('site', site);
	if (username) params.append('username', username);
	if (password) params.append('password', password);
	const response = await fetch(`/api/vault/update?${params.toString()}`, {
		method: 'PUT',
		headers: { 'Content-Type': 'application/json' }
	});
	if (response.ok) {
		const data = await response.json();
		return { success: true, entry: data };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error updating entry: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// Delete vault entry
export async function deleteVaultEntry(credentialId: string, userId: string): Promise<{ success: boolean; error?: string }> {
	const response = await fetch(`/api/vault/delete?credentialId=${encodeURIComponent(credentialId)}&userId=${encodeURIComponent(userId)}`, {
		method: 'DELETE'
	});
	if (response.status === 204) {
		return { success: true };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error deleting entry: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// Share vault entry
export async function shareVaultEntry(credentialId: string, fromUserId: string, toUsername: string): Promise<{ success: boolean; entry?: VaultEntry; error?: string }> {
	const params = new URLSearchParams({ credentialId, fromUserId, toUsername });
	const response = await fetch(`/api/vault/share?${params.toString()}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' }
	});
	if (response.ok) {
		const data = await response.json();
		return { success: true, entry: data };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error sharing entry: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// Export all vault entries (with passwords)
export async function exportVault(userId: string): Promise<{ success: boolean; entries?: VaultEntry[]; error?: string }> {
	const response = await fetch(`/api/vault/export?userId=${encodeURIComponent(userId)}`);
	if (response.ok) {
		const data = await response.json();
		return { success: true, entries: data };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error exporting vault: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// Import vault entries
export async function importVault(userId: string, credentials: { site: string; username: string; password: string }[]): Promise<{ success: boolean; results?: VaultEntry[]; error?: string }> {
	const response = await fetch(`/api/vault/import?userId=${encodeURIComponent(userId)}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(credentials)
	});
	if (response.ok) {
		const data = await response.json();
		return { success: true, results: data };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error importing vault: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}
