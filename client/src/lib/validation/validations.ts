import type { VaultEntryForm, VaultEntryErrors } from '$lib/types';

export function validateVaultEntry(formData: VaultEntryForm): VaultEntryErrors {
	const errors: VaultEntryErrors = {};
	if (!formData.site || !formData.site.trim()) {
		errors.site = 'Site name is required';
	}
	if (!formData.email || !formData.email.trim()) {
		errors.email = 'Email is required';
	}
	if (!formData.password || !formData.password.trim()) {
		errors.password = 'Password is required';
	}
	return errors;
}
