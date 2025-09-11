import type { VaultEntryForm, VaultEntryErrors } from '$lib/types';

export function validateVaultEntry(formData: VaultEntryForm): VaultEntryErrors {
	const errors: VaultEntryErrors = {};
	if (!formData.siteName || !formData.siteName.trim()) {
		errors.siteName = 'Site name is required';
	}
	if (!formData.username || !formData.username.trim()) {
		errors.username = 'Username is required';
	}
	if (!formData.password || !formData.password.trim()) {
		errors.password = 'Password is required';
	}
	return errors;
}
