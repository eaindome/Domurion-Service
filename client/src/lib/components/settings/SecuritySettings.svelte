<script lang="ts">
	import { fetchUserPreferences, updateUserPreferences } from '$lib/api/settings';
	import { get2FAStatus, enable2FA, disable2FA, generateRecoveryCodes } from '$lib/api/2fa';
	import { onMount } from 'svelte';
	import type { UserPreferences } from '$lib/types';

	export let isLoading = false;

	// Create event dispatcher
	import { createEventDispatcher } from 'svelte';
	const dispatch = createEventDispatcher<{
		success: { message: string };
		error: { message: string };
		loading: { isLoading: boolean };
	}>();

	// Security settings state
	let securitySettings = {
		twoFactorEnabled: false,
		sessionTimeout: 60,
		autoLock: true,
		loginNotifications: true
	};

	let userPreferences: UserPreferences | null = null;
	let showRecoveryCodes = false;
	let recoveryCodes: string[] = [];
	let isToggling2FA = false;

	// Load settings on component mount
	onMount(async () => {
		await loadSecuritySettings();
	});

	async function loadSecuritySettings() {
		dispatch('loading', { isLoading: true });
		try {
			// Load user preferences
			const prefs = await fetchUserPreferences();
			userPreferences = prefs;
			
			// Load 2FA status
			const twoFactorStatus = await get2FAStatus();
			
			// Map backend data to frontend state
			securitySettings = {
				twoFactorEnabled: twoFactorStatus.enabled,
				sessionTimeout: prefs.sessionTimeoutMinutes || 60,
				autoLock: prefs.autoLockEnabled ?? true,
				loginNotifications: prefs.loginNotificationsEnabled ?? true
			};
		} catch (error) {
			dispatch('error', { message: 'Failed to load security settings.' });
			console.error('Error loading security settings:', error);
		} finally {
			dispatch('loading', { isLoading: false });
		}
	}

	async function toggle2FA() {
		if (isToggling2FA) return;
		
		isToggling2FA = true;
		try {
			if (securitySettings.twoFactorEnabled) {
				// Disable 2FA - your backend doesn't require a code for disable
				await disable2FA(''); // Pass empty string since your backend expects it
				securitySettings.twoFactorEnabled = false;
				showRecoveryCodes = false;
				recoveryCodes = [];
				dispatch('success', { message: '2FA has been disabled.' });
			} else {
				// Enable 2FA
				await enable2FA();
				securitySettings.twoFactorEnabled = true;
				dispatch('success', { message: '2FA has been enabled! You will receive OTP codes via email.' });
			}
		} catch (error) {
			dispatch('error', { message: `Failed to ${securitySettings.twoFactorEnabled ? 'disable' : 'enable'} 2FA.` });
			console.error('Error toggling 2FA:', error);
		} finally {
			isToggling2FA = false;
		}
	}

	async function handleGenerateRecoveryCodes() {
		try {
			const codes = await generateRecoveryCodes();
			recoveryCodes = codes;
			showRecoveryCodes = true;
			dispatch('success', { message: 'Recovery codes generated successfully!' });
		} catch (error) {
			dispatch('error', { message: 'Failed to generate recovery codes.' });
			console.error('Error generating recovery codes:', error);
		}
	}

	async function updateSecuritySettings() {
		if (!userPreferences) return;
		
		dispatch('loading', { isLoading: true });

		try {
			await updateUserPreferences({
				...userPreferences,
				sessionTimeoutMinutes: securitySettings.sessionTimeout,
				autoLockEnabled: securitySettings.autoLock,
				loginNotificationsEnabled: securitySettings.loginNotifications
			});

			dispatch('success', { message: 'Security settings updated successfully!' });
		} catch (error) {
			dispatch('error', { message: 'Failed to update security settings.' });
			console.error('Error updating security settings:', error);
		} finally {
			dispatch('loading', { isLoading: false });
		}
	}

	function copyRecoveryCodes() {
		const codesText = recoveryCodes.join('\n');
		navigator.clipboard.writeText(codesText).then(() => {
			dispatch('success', { message: 'Recovery codes copied to clipboard!' });
		}).catch(() => {
			dispatch('error', { message: 'Failed to copy recovery codes to clipboard.' });
		});
	}
</script>

<div class="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
	<h2 class="mb-6 text-xl font-semibold text-gray-900">Security Settings</h2>

	<form on:submit|preventDefault={updateSecuritySettings} class="space-y-6">
		<!-- Two-Factor Authentication -->
		<div class="rounded-xl border border-gray-200 p-4">
			<div class="flex items-center justify-between mb-4">
				<div>
					<h3 class="text-sm font-medium text-gray-900">Two-Factor Authentication</h3>
					<p class="text-sm text-gray-500">
						Add an extra layer of security to your account via email OTP
					</p>
				</div>
				<label class="relative inline-flex cursor-pointer items-center">
					<input
						type="checkbox"
						bind:checked={securitySettings.twoFactorEnabled}
						on:change={toggle2FA}
						disabled={isToggling2FA}
						class="peer sr-only"
					/>
					<div
						class="peer h-6 w-11 rounded-full bg-gray-200 peer-checked:bg-indigo-600 peer-focus:ring-4 peer-focus:ring-indigo-300 peer-focus:outline-none peer-disabled:cursor-not-allowed peer-disabled:opacity-50 after:absolute after:top-[2px] after:left-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-gray-300 after:bg-white after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-white"
					></div>
				</label>
			</div>

			{#if securitySettings.twoFactorEnabled}
				<div class="border-t border-gray-100 pt-4 space-y-3">
					<p class="text-sm text-green-700 bg-green-50 p-3 rounded-lg">
						✅ Two-factor authentication is enabled. You'll receive OTP codes via email when logging in.
					</p>
					
					<button
						type="button"
						on:click={handleGenerateRecoveryCodes}
						class="inline-flex items-center px-3 py-2 border border-indigo-300 shadow-sm text-sm leading-4 font-medium rounded-md text-indigo-700 bg-indigo-50 hover:bg-indigo-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
					>
						Generate Recovery Codes
					</button>

					{#if showRecoveryCodes && recoveryCodes.length > 0}
						<div class="mt-4 p-4 bg-amber-50 border border-amber-200 rounded-lg">
							<div class="flex items-center justify-between mb-2">
								<h4 class="text-sm font-medium text-amber-800">Recovery Codes</h4>
								<button
									type="button"
									on:click={copyRecoveryCodes}
									class="text-sm text-amber-700 hover:text-amber-900 underline"
								>
									Copy All
								</button>
							</div>
							<p class="text-sm text-amber-700 mb-3">
								⚠️ Save these codes securely! Each can only be used once.
							</p>
							<div class="grid grid-cols-2 gap-2 text-sm font-mono">
								{#each recoveryCodes as code}
									<div class="bg-white p-2 rounded border text-gray-900">{code}</div>
								{/each}
							</div>
						</div>
					{/if}
				</div>
			{/if}
		</div>

		<!-- Session Timeout -->
		<div>
			<label for="sessionTimeout" class="mb-2 block text-sm font-medium text-gray-700">
				Session Timeout
			</label>
			<select
				id="sessionTimeout"
				bind:value={securitySettings.sessionTimeout}
				class="w-full rounded-xl border border-gray-200 px-4 py-3 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
			>
				<option value={15}>15 minutes</option>
				<option value={30}>30 minutes</option>
				<option value={60}>1 hour</option>
				<option value={120}>2 hours</option>
				<option value={240}>4 hours</option>
				<option value={0}>Never timeout</option>
			</select>
			<p class="mt-1 text-sm text-gray-500">
				Automatically log out after this period of inactivity
			</p>
		</div>

		<!-- Auto-lock Vault -->
		<div class="flex items-center justify-between rounded-xl bg-gray-50 p-4">
			<div>
				<h3 class="text-sm font-medium text-gray-900">Auto-lock Vault</h3>
				<p class="text-sm text-gray-500">Automatically lock vault when browser is idle</p>
			</div>
			<label class="relative inline-flex cursor-pointer items-center">
				<input
					type="checkbox"
					bind:checked={securitySettings.autoLock}
					class="peer sr-only"
				/>
				<div
					class="peer h-6 w-11 rounded-full bg-gray-200 peer-checked:bg-indigo-600 peer-focus:ring-4 peer-focus:ring-indigo-300 peer-focus:outline-none after:absolute after:top-[2px] after:left-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-gray-300 after:bg-white after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-white"
				></div>
			</label>
		</div>

		<!-- Login Notifications -->
		<div class="flex items-center justify-between rounded-xl bg-gray-50 p-4">
			<div>
				<h3 class="text-sm font-medium text-gray-900">Login Notifications</h3>
				<p class="text-sm text-gray-500">Get notified of new sign-ins to your account</p>
			</div>
			<label class="relative inline-flex cursor-pointer items-center">
				<input
					type="checkbox"
					bind:checked={securitySettings.loginNotifications}
					class="peer sr-only"
				/>
				<div
					class="peer h-6 w-11 rounded-full bg-gray-200 peer-checked:bg-indigo-600 peer-focus:ring-4 peer-focus:ring-indigo-300 peer-focus:outline-none after:absolute after:top-[2px] after:left-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-gray-300 after:bg-white after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-white"
				></div>
			</label>
		</div>

		<button
			type="submit"
			disabled={isLoading}
			class="w-full rounded-xl bg-indigo-600 px-6 py-3 text-white transition-colors hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50 md:w-auto"
		>
			{isLoading ? 'Updating...' : 'Update Security Settings'}
		</button>
	</form>
</div>