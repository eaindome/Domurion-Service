import { API_BASE } from "./config";
import type { VaultEntry } from '$lib/types';
import { fetchWithAuth } from '$lib/utils/fetchWithAuth';

// Add vault entry
export async function addVaultEntry(site: string, email: string, password: string, notes?: string, siteUrl?: string): Promise<{ success: boolean; entry?: VaultEntry; error?: string }> {
	const params = new URLSearchParams({ site, email, password, notes: notes || '', siteUrl: siteUrl || '' });
	const response = await fetchWithAuth(`${API_BASE}/api/vault/add?${params.toString()}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
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

// Fetch a specific vault entry by credentialId
export async function getVaultEntry(credentialId: string): Promise<{ success: boolean; entry?: VaultEntry; error?: string }> {
	const response = await fetchWithAuth(`${API_BASE}/api/vault/retrieve?credentialId=${encodeURIComponent(credentialId)}`, {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
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
			console.log(`Error fetching entry: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// List vault entries for user
export async function listVaultEntries(): Promise<{ success: boolean; entries?: VaultEntry[]; error?: string }> {
	const response = await fetchWithAuth(`${API_BASE}/api/vault/list`, {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	console.log(`Response: ${response}`);
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
export async function retrieveVaultPassword(credentialId: string): Promise<{ success: boolean; password?: string; error?: string }> {
	const response = await fetchWithAuth(`${API_BASE}/api/vault/retrieve?credentialId=${encodeURIComponent(credentialId)}`, {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
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
export async function updateVaultEntry(credentialId: string, site?: string, username?: string, password?: string, notes?: string, siteUrl?: string): Promise<{ success: boolean; entry?: VaultEntry; error?: string }> {
	const params = new URLSearchParams({ credentialId });
	if (site) params.append('site', site);
	if (siteUrl) params.append('siteUrl', siteUrl);
	if (username) params.append('username', username);
	if (password) params.append('password', password);
	if (notes) params.append('notes', notes);
	const response = await fetchWithAuth(`${API_BASE}/api/vault/update?${params.toString()}`, {
		method: 'PUT',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
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
export async function deleteVaultEntry(credentialId: string): Promise<{ success: boolean; error?: string }> {
	const response = await fetchWithAuth(`${API_BASE}/api/vault/delete?credentialId=${encodeURIComponent(credentialId)}`, {
		method: 'DELETE',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	console.log(`Delete response status: ${response.status}`);
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
	const response = await fetchWithAuth(`${API_BASE}/api/vault/share?${params.toString()}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
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

// Export all vault entries
export async function exportVaultData(): Promise<{ success: boolean; data?: VaultEntry[]; error?: string }> {
	const response = await fetchWithAuth(`${API_BASE}/api/vault/export`, {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (response.ok) {
		const data: VaultEntry[] = await response.json();
		return { success: true, data };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error exporting vault data: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// Import vault data
export async function importVaultData(importData: VaultEntry[]): Promise<{ success: boolean; message?: string; error?: string }> {
	const response = await fetchWithAuth(`${API_BASE}/api/vault/import`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(importData),
		credentials: 'include'
	});
	if (response.ok) {
		const data = await response.json();
		return { success: true, message: data.message };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error importing vault data: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// Delete all vault data
export async function deleteAllVaultData(): Promise<{ success: boolean; message?: string; error?: string }> {
	const response = await fetchWithAuth(`${API_BASE}/api/vault/delete-all`, {
		method: 'DELETE',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (response.ok) {
		const data = await response.json();
		return { success: true, message: data.message };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error deleting all vault data: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

export async function listSharedVaultEntries(): Promise<{ success: boolean; entries?: VaultEntry[]; error?: string }> {
	const response = await fetchWithAuth(`${API_BASE}/api/vault/shared`, {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (response.ok) {
		const data = await response.json();
		return { success: true, entries: data };
	}
	else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error listing shared entries: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}
