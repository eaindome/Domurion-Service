// API for shared credentials (invitations, accept/reject, list)
import type { VaultEntry, ShareInvitation } from '$lib/types';
import { fetchWithAuth } from '$lib/utils/fetchWithAuth';

// List credentials shared with the user (pending and accepted)
export async function listSharedCredentials(): Promise<{ success: boolean; shared?: VaultEntry[]; error?: string }> {
	const response = await fetchWithAuth('/api/vault/shared', {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (response.ok) {
		const data = await response.json();
		return { success: true, shared: data };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error listing shared credentials: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}



export async function createShareInvitation(credentialId: string, toIdentifier: string): Promise<{ success: boolean; invitation?: ShareInvitation; error?: string }> {
	const params = new URLSearchParams({ credentialId, toIdentifier });
	const response = await fetchWithAuth(`/api/vault/share?${params.toString()}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (response.ok) {
		const data = await response.json();
		return { success: true, invitation: data };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error creating share invitation: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// Accept a share invitation
export async function acceptShareInvitation(invitationId: string): Promise<{ success: boolean; credential?: VaultEntry; error?: string }> {
	const params = new URLSearchParams({ invitationId });
	const response = await fetchWithAuth(`/api/vault/share/accept?${params.toString()}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (response.ok) {
		const data = await response.json();
		return { success: true, credential: data };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error accepting share invitation: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}

// Reject a share invitation
export async function rejectShareInvitation(invitationId: string): Promise<{ success: boolean; error?: string }> {
	const params = new URLSearchParams({ invitationId });
	const response = await fetchWithAuth(`/api/vault/share/reject?${params.toString()}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (response.ok) {
		return { success: true };
	} else {
		let errorMsg = 'Unknown error';
		try {
			const data = await response.json();
			errorMsg = data.error || data.message || errorMsg;
		} catch (err) {
			console.log(`Error rejecting share invitation: ${err}`);
		}
		return { success: false, error: errorMsg };
	}
}
