import type { SupportRequest } from '$lib/types';
import { fetchWithAuth } from '$lib/utils/fetchWithAuth';
// Request 2FA reset (user)
export async function request2FAReset(request: { username?: string; email?: string; reason?: string }): Promise<string> {
	const response = await fetchWithAuth('/api/support/request-2fa-reset', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(request),
		credentials: 'include'
	});
	if (!response.ok) {
		const data = await response.json();
		throw new Error(data?.message || data || 'Failed to submit 2FA reset request');
	}
	return response.text();
}

// List unresolved support requests (admin)
export async function getSupportRequests(): Promise<SupportRequest[]> {
	const response = await fetchWithAuth('/api/support/requests', {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (!response.ok) {
		throw new Error('Failed to fetch support requests');
	}
	return response.json();
}

// Resolve a support request and reset 2FA (admin)
export async function resolve2FAReset(requestId: string, resolutionNote: string): Promise<string> {
	const response = await fetchWithAuth(`/api/support/resolve-2fa-reset/${encodeURIComponent(requestId)}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(resolutionNote),
		credentials: 'include'
	});
	if (!response.ok) {
		const data = await response.json();
		throw new Error(data?.message || data || 'Failed to resolve 2FA reset request');
	}
	return response.text();
}
