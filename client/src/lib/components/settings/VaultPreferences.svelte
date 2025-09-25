<script lang="ts">
	import { fetchUserPreferences, updateUserPreferences } from '$lib/api/settings';
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

	// Add initial loading state
	let isInitialLoading = true;

	// Vault preferences state
	let vaultPreferences = {
		autoSaveEntries: true,
		showPasswordStrength: true,
		passwordLength: 16,
		useUppercase: true,
		useLowercase: true,
		useNumbers: true,
		useSymbols: true
	};

	let userPreferences: UserPreferences | null = null;

	// Load preferences on component mount
	onMount(async () => {
		await loadVaultPreferences();
	});

	async function loadVaultPreferences() {
		dispatch('loading', { isLoading: true });
		try {
			const prefs = await fetchUserPreferences();
			userPreferences = prefs;
			
			// Map backend data to frontend state
			vaultPreferences = {
				autoSaveEntries: prefs.autoSaveEntries ?? true,
				showPasswordStrength: prefs.showPasswordStrength ?? true,
				passwordLength: prefs.passwordLength ?? 16,
				useUppercase: prefs.useUppercase ?? true,
				useLowercase: prefs.useLowercase ?? true,
				useNumbers: prefs.useNumbers ?? true,
				useSymbols: prefs.useSymbols ?? true
			};
		} catch (error) {
			dispatch('error', { message: 'Failed to load vault preferences.' });
			console.error('Error loading vault preferences:', error);
		} finally {
			isInitialLoading = false;
			dispatch('loading', { isLoading: false });
		}
	}

	async function updateVaultPreferences() {
		if (!userPreferences) return;

		dispatch('loading', { isLoading: true });

		try {
			await updateUserPreferences({
				...userPreferences,
				autoSaveEntries: vaultPreferences.autoSaveEntries,
				showPasswordStrength: vaultPreferences.showPasswordStrength,
				passwordLength: vaultPreferences.passwordLength,
				useUppercase: vaultPreferences.useUppercase,
				useLowercase: vaultPreferences.useLowercase,
				useNumbers: vaultPreferences.useNumbers,
				useSymbols: vaultPreferences.useSymbols
			});
			
			dispatch('success', { message: 'Vault preferences updated successfully!' });
		} catch (error) {
			dispatch('error', { message: 'Failed to update vault preferences.' });
			console.error('Error updating vault preferences:', error);
		} finally {
			dispatch('loading', { isLoading: false });
		}
	}
</script>

<div class="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
	<h2 class="mb-6 text-xl font-semibold text-gray-900">Vault Preferences</h2>

	{#if isInitialLoading}
		<!-- Loading skeleton while fetching preferences -->
		<div class="space-y-6 animate-pulse">
			<div class="flex items-center justify-between rounded-xl bg-gray-100 p-4">
				<div class="space-y-2">
					<div class="h-4 bg-gray-200 rounded w-32"></div>
					<div class="h-3 bg-gray-200 rounded w-48"></div>
				</div>
				<div class="h-6 w-11 bg-gray-200 rounded-full"></div>
			</div>
			
			<div class="flex items-center justify-between rounded-xl bg-gray-100 p-4">
				<div class="space-y-2">
					<div class="h-4 bg-gray-200 rounded w-40"></div>
					<div class="h-3 bg-gray-200 rounded w-56"></div>
				</div>
				<div class="h-6 w-11 bg-gray-200 rounded-full"></div>
			</div>
			
			<div class="space-y-4">
				<div class="h-5 bg-gray-200 rounded w-48"></div>
				<div class="h-16 bg-gray-100 rounded-xl"></div>
			</div>
		</div>
	{:else}
		<!-- Actual content once data is loaded -->

	<form on:submit|preventDefault={updateVaultPreferences} class="space-y-6">
		<!-- Auto-save -->
		<div class="flex items-center justify-between rounded-xl bg-gray-50 p-4">
			<div>
				<h3 class="text-sm font-medium text-gray-900">Auto-save Entries</h3>
				<p class="text-sm text-gray-500">Automatically save vault entries as you type</p>
			</div>
			<label class="relative inline-flex cursor-pointer items-center">
				<input
					type="checkbox"
					bind:checked={vaultPreferences.autoSaveEntries}
					class="peer sr-only"
				/>
				<div
					class="peer h-6 w-11 rounded-full bg-gray-200 peer-checked:bg-indigo-600 peer-focus:ring-4 peer-focus:ring-indigo-300 peer-focus:outline-none after:absolute after:top-[2px] after:left-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-gray-300 after:bg-white after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-white"
				></div>
			</label>
		</div>

		<!-- Show Password Strength -->
		<div class="flex items-center justify-between rounded-xl bg-gray-50 p-4">
			<div>
				<h3 class="text-sm font-medium text-gray-900">Show Password Strength</h3>
				<p class="text-sm text-gray-500">Display password strength indicators when creating passwords</p>
			</div>
			<label class="relative inline-flex cursor-pointer items-center">
				<input
					type="checkbox"
					bind:checked={vaultPreferences.showPasswordStrength}
					class="peer sr-only"
				/>
				<div
					class="peer h-6 w-11 rounded-full bg-gray-200 peer-checked:bg-indigo-600 peer-focus:ring-4 peer-focus:ring-indigo-300 peer-focus:outline-none after:absolute after:top-[2px] after:left-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-gray-300 after:bg-white after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-white"
				></div>
			</label>
		</div>

		<!-- Password Generation Settings -->
		<div class="space-y-4">
			<h3 class="text-lg font-medium text-gray-900 border-b border-gray-200 pb-2">Default Password Generation</h3>
			
			<!-- Password Length -->
			<div>
				<label for="passwordLength" class="mb-2 block text-sm font-medium text-gray-700">
					Password Length: {vaultPreferences.passwordLength} characters
				</label>
				<input
					type="range"
					id="passwordLength"
					bind:value={vaultPreferences.passwordLength}
					min="8"
					max="32"
					step="1"
					class="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer slider"
				/>
				<div class="flex justify-between text-xs text-gray-500 mt-1">
					<span>8</span>
					<span>32</span>
				</div>
			</div>

			<!-- Character Type Options -->
			<div class="grid grid-cols-2 gap-4">
				<label class="flex items-center space-x-3">
					<input
						type="checkbox"
						bind:checked={vaultPreferences.useUppercase}
						class="h-4 w-4 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
					/>
					<span class="text-sm text-gray-700">Uppercase (A-Z)</span>
				</label>

				<label class="flex items-center space-x-3">
					<input
						type="checkbox"
						bind:checked={vaultPreferences.useLowercase}
						class="h-4 w-4 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
					/>
					<span class="text-sm text-gray-700">Lowercase (a-z)</span>
				</label>

				<label class="flex items-center space-x-3">
					<input
						type="checkbox"
						bind:checked={vaultPreferences.useNumbers}
						class="h-4 w-4 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
					/>
					<span class="text-sm text-gray-700">Numbers (0-9)</span>
				</label>

				<label class="flex items-center space-x-3">
					<input
						type="checkbox"
						bind:checked={vaultPreferences.useSymbols}
						class="h-4 w-4 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
					/>
					<span class="text-sm text-gray-700">Symbols (!@#$)</span>
				</label>
			</div>

			<!-- Password Preview -->
			<div class="mt-4 p-3 bg-gray-50 rounded-lg">
				<span class="text-sm font-medium text-gray-700">Preview:</span>
				<div class="mt-1 font-mono text-sm text-gray-900">
					{'*'.repeat(vaultPreferences.passwordLength)}
				</div>
				<p class="mt-1 text-xs text-gray-500">
					Generated passwords will use these settings by default
				</p>
			</div>
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
				Update peferences
			{/if}
		</button>
	</form>
	{/if}
</div>