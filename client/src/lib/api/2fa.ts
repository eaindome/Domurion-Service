
// Enable 2FA: returns secret and QR code
export async function enable2FA(): Promise<{ secret: string; qrCode: string }> {
	const response = await fetch('/api/2fa/enable', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' }
	});
	if (!response.ok) {
		throw new Error('Failed to enable 2FA');
	}
	return response.json();
}

// Verify 2FA: pass TOTP code, returns success message
export async function verify2FA(code: string): Promise<string> {
	const response = await fetch('/api/2fa/verify', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(code)
	});
	if (!response.ok) {
		const data = await response.json();
		throw new Error(data?.message || data || 'Failed to verify 2FA');
	}
	return response.text();
}

// Disable 2FA: pass TOTP code, returns success message
export async function disable2FA(code: string): Promise<string> {
	const response = await fetch('/api/2fa/disable', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(code)
	});
	if (!response.ok) {
		const data = await response.json();
		throw new Error(data?.message || data || 'Failed to disable 2FA');
	}
	return response.text();
}

// Generate new recovery codes (returns array of codes)
export async function generateRecoveryCodes(): Promise<string[]> {
	const response = await fetch('/api/2fa/generate-recovery-codes', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' }
	});
	if (!response.ok) {
		throw new Error('Failed to generate recovery codes');
	}
	const data = await response.json();
	return data.recoveryCodes;
}

// Use a recovery code (returns success message)
export async function useRecoveryCode(code: string): Promise<string> {
	const response = await fetch('/api/2fa/use-recovery-code', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(code)
	});
	if (!response.ok) {
		const data = await response.json();
		throw new Error(data?.message || data || 'Failed to use recovery code');
	}
	return response.text();
}

// Get 2FA status (enabled: boolean)
export async function get2FAStatus(): Promise<{ enabled: boolean }> {
	const response = await fetch('/api/2fa/status', {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' }
	});
	if (!response.ok) {
		throw new Error('Failed to get 2FA status');
	}
	return response.json();
}

// Get current recovery codes (returns array of codes)
export async function getRecoveryCodes(): Promise<string[]> {
	const response = await fetch('/api/2fa/recovery-codes', {
		method: 'GET',
		headers: { 'Content-Type': 'application/json' }
	});
	if (!response.ok) {
		throw new Error('Failed to get recovery codes');
	}
	const data = await response.json();
	return data.recoveryCodes;
}

// Regenerate recovery codes (requires TOTP code, returns array of codes)
export async function regenerateRecoveryCodes(code: string): Promise<string[]> {
	const response = await fetch('/api/2fa/regenerate-recovery-codes', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(code)
	});
	if (!response.ok) {
		throw new Error('Failed to regenerate recovery codes');
	}
	const data = await response.json();
	return data.recoveryCodes;
}
