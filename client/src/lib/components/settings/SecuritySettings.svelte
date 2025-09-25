<script lang="ts">
	import { fetchUserPreferences, updateUserPreferences } from '$lib/api/settings';
	import { get2FAStatus, enable2FA, disable2FA, generateRecoveryCodes } from '$lib/api/2fa';
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import type { UserPreferences } from '$lib/types';

	export let isLoading = false;

	// Create event dispatcher
	import { createEventDispatcher } from 'svelte';
	const dispatch = createEventDispatcher<{
		success: { message: string };
		error: { message: string };
		loading: { isLoading: boolean };
	}>();

	// Add initial loading state
	let isInitialLoading = true;

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
	let highlight2FA = false;

	// Check if we should highlight the 2FA section
	$: if ($page.url.searchParams.get('highlight') === '2fa') {
		highlight2FA = true;
		// Remove highlight after 5 seconds
		setTimeout(() => {
			highlight2FA = false;
		}, 5000);
	}

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
			isInitialLoading = false;
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

	{#if isInitialLoading}
		<!-- Loading skeleton while fetching settings -->
		<div class="space-y-6 animate-pulse">
			<div class="rounded-xl border border-gray-200 p-4">
				<div class="flex items-center justify-between mb-4">
					<div class="space-y-2">
						<div class="h-4 bg-gray-200 rounded w-44"></div>
						<div class="h-3 bg-gray-200 rounded w-64"></div>
					</div>
					<div class="h-6 w-11 bg-gray-200 rounded-full"></div>
				</div>
			</div>
			
			<div class="space-y-2">
				<div class="h-4 bg-gray-200 rounded w-32"></div>
				<div class="h-10 bg-gray-100 rounded-xl w-full"></div>
			</div>
			
			<div class="flex items-center justify-between rounded-xl bg-gray-100 p-4">
				<div class="space-y-2">
					<div class="h-4 bg-gray-200 rounded w-28"></div>
					<div class="h-3 bg-gray-200 rounded w-48"></div>
				</div>
				<div class="h-6 w-11 bg-gray-200 rounded-full"></div>
			</div>
		</div>
	{:else}
		<!-- Actual content once data is loaded -->

	<form on:submit|preventDefault={updateSecuritySettings} class="space-y-6">
		<!-- Two-Factor Authentication -->
		<div class="rounded-xl border border-gray-200 p-4 {highlight2FA ? 'ring-2 ring-indigo-500 bg-indigo-50/50 border-indigo-200' : ''}">
			{#if highlight2FA}
				<div class="mb-4 p-3 rounded-lg bg-indigo-100 border border-indigo-200">
					<div class="flex items-start">
						<svg class="flex-shrink-0 h-5 w-5 text-indigo-600 mt-0.5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
						</svg>
						<div>
							<h4 class="text-sm font-medium text-indigo-800">Enable 2FA Required</h4>
							<p class="text-sm text-indigo-700 mt-1">
								To view passwords in your vault, you need to enable two-factor authentication. Toggle the switch below to get started.
							</p>
						</div>
					</div>
				</div>
			{/if}
			
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
			class="w-full rounded-xl bg-indigo-600 px-6 py-3 text-white transition-all duration-200 hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-indigo-600 md:w-auto flex items-center justify-center"
		>
			{#if isLoading}
				<svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
					<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
					<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
				</svg>
				Updating...
			{:else}
				Update security
			{/if}
		</button>
	</form>
	{/if}
</div>