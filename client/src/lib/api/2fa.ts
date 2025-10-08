import { API_BASE } from "./config";
import { fetchWithAuth } from '$lib/utils/fetchWithAuth';
// Enable 2FA: returns secret and QR code
export async function enable2FA(): Promise<{ secret: string; qrCode: string }> {
	const response = await fetchWithAuth(`${API_BASE}/api/2fa/enable`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	const text = await response.text();
	console.debug('enable2FA response text:', text);
	if (!response.ok) {
		let errorMessage = 'Failed to enable 2FA';
		try {
			const data = JSON.parse(text);
			errorMessage = data?.message || data?.error || text || errorMessage;
		} catch {
			errorMessage = text || errorMessage;
		}
		throw new Error(errorMessage);
	}
	if (!text) return { secret: '', qrCode: '' };
	try {
		const parsed = JSON.parse(text);
		return { secret: parsed?.secret || '', qrCode: parsed?.qrCode || '' };
	} catch {
		// If response isn't JSON, return raw text in qrCode for debugging
		return { secret: '', qrCode: text };
	}
}

// Verify 2FA: pass TOTP code, returns success message
export async function verify2FA(code: string): Promise<string> {
	const response = await fetchWithAuth(`${API_BASE}/api/2fa/verify`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(code),
		credentials: 'include'
	});
	const text = await response.text();
	console.debug('verify2FA response text:', text);
	if (!response.ok) {
		let errorMessage = 'Failed to verify 2FA';
		try {
			const data = JSON.parse(text);
			errorMessage = data?.message || data?.error || text || errorMessage;
		} catch {
			errorMessage = text || errorMessage;
		}
		throw new Error(errorMessage);
	}
	if (!text) return '';
	try {
		const parsed = JSON.parse(text);
		if (typeof parsed === 'string') return parsed;
		return parsed?.message || JSON.stringify(parsed);
	} catch {
		return text;
	}
}

// Disable 2FA: pass TOTP code, returns success message
export async function disable2FA(code: string): Promise<string> {
	const response = await fetchWithAuth(`${API_BASE}/api/2fa/disable`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(code),
		credentials: 'include'
	});
	const text = await response.text();
	console.debug('disable2FA response text:', text);
	if (!response.ok) {
		let errorMessage = 'Failed to disable 2FA';
		try {
			const data = JSON.parse(text);
			errorMessage = data?.message || data?.error || text || errorMessage;
		} catch {
			errorMessage = text || errorMessage;
		}
		throw new Error(errorMessage);
	}
	if (!text) return '';
	try {
		const parsed = JSON.parse(text);
		if (typeof parsed === 'string') return parsed;
		return parsed?.message || JSON.stringify(parsed);
	} catch {
		return text;
	}
}

// Generate new recovery codes (returns array of codes)
export async function generateRecoveryCodes(): Promise<string[]> {
	const response = await fetchWithAuth(`${API_BASE}/api/2fa/generate-recovery-codes`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (!response.ok) {
		throw new Error('Failed to generate recovery codes');
	}
	const data = await response.json();
	return data.recoveryCodes;
}

// Use a recovery code (returns success message)
export async function useRecoveryCode(code: string): Promise<string> {
	const response = await fetchWithAuth(`${API_BASE}/api/2fa/use-recovery-code`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(code),
		credentials: 'include'
	});
	if (!response.ok) {
		const data = await response.json();
		throw new Error(data?.message || data || 'Failed to use recovery code');
	}
	return response.text();
}

// Get 2FA status (enabled: boolean)
export async function get2FAStatus(): Promise<{ enabled: boolean }> {
	const response = await fetchWithAuth(`${API_BASE}/api/2fa/status`, {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (!response.ok) {
		throw new Error('Failed to get 2FA status');
	}
	return response.json();
}

// Get current recovery codes (returns array of codes)
export async function getRecoveryCodes(): Promise<string[]> {
	const response = await fetchWithAuth(`${API_BASE}/api/2fa/recovery-codes`, {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (!response.ok) {
		throw new Error('Failed to get recovery codes');
	}
	const data = await response.json();
	return data.recoveryCodes;
}

// Regenerate recovery codes (requires TOTP code, returns array of codes)
export async function regenerateRecoveryCodes(code: string): Promise<string[]> {
	const response = await fetchWithAuth(`${API_BASE}/api/2fa/regenerate-recovery-codes`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(code),
		credentials: 'include'
	});
	if (!response.ok) {
		throw new Error('Failed to regenerate recovery codes');
	}
	const data = await response.json();
	return data.recoveryCodes;
}

// Request OTP for viewing credential
export async function requestViewOtp(credentialId: string): Promise<{ message: string }> {
	const response = await fetchWithAuth(`${API_BASE}/api/users/request-view-otp?credentialId=${credentialId}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include'
	});
	if (!response.ok) {
		const text = await response.text();
		let errorMessage = 'Failed to request view OTP';
		try {
			const data = JSON.parse(text);
			errorMessage = data?.message || data?.error || errorMessage;
		} catch {
			// If parsing fails, use the raw text as error message
			errorMessage = text || errorMessage;
		}
		throw new Error(errorMessage);
	}
	
	// Handle empty response
	const text = await response.text();
	if (!text) {
		return { message: 'OTP request successful' };
	}
	
	try {
		return JSON.parse(text);
	} catch {
		return { message: text };
	}
}

// Verify OTP for viewing credential
export async function verifyViewOtp(otp: string): Promise<{ verified: boolean }> {
	const response = await fetchWithAuth(`${API_BASE}/api/users/verify-view-otp`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ otp }),
		credentials: 'include'
	});
	if (!response.ok) {
		const text = await response.text();
		let errorMessage = 'Failed to verify OTP';
		try {
			const data = JSON.parse(text);
			errorMessage = data?.message || data?.error || errorMessage;
		} catch {
			// If parsing fails, use the raw text as error message
			errorMessage = text || errorMessage;
		}
		throw new Error(errorMessage);
	}
	
	// Handle empty response
	const text = await response.text();
	if (!text) {
		return { verified: true };
	}
	
	try {
		return JSON.parse(text);
	} catch {
		// If we can't parse the response, assume success if status was OK
		return { verified: true };
	}
}
