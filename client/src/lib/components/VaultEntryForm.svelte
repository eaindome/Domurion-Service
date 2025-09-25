<script lang="ts">
	import { createEventDispatcher, onMount, onDestroy } from 'svelte';
	import { generatePassword, copyToClipboard } from '../../utils/helpers';
	import { createAutoSave, getStatusText, getStatusColor, type SaveStatus } from '$lib/utils/autoSave';
	import { fetchUserPreferences } from '$lib/api/settings';
	import type { VaultEntryErrors } from '$lib/types';

	export let formData: {
		site: string;
		siteUrl?: string;
		email: string;
		password: string;
		notes?: string;
	};
	export let errors: VaultEntryErrors = {};
	export let isSubmitting: boolean = false;
	export let mode: 'add' | 'edit' = 'add';

	// Handlers passed from parent
	export let onSubmit: () => void;
	export let onCancel: () => void;

	const dispatch = createEventDispatcher();

	let showPassword = false;
	let passwordStrength = 0;
	let isCopied = false;
	let isGenerating = false;

	// Auto-save functionality
	let autoSaveEnabled = false;
	let saveStatus: SaveStatus = 'saved';
	let autoSaveInstance: ReturnType<typeof createAutoSave> | null = null;

	// Load user preferences and setup auto-save
	onMount(async () => {
		try {
			const preferences = await fetchUserPreferences();
			autoSaveEnabled = preferences.autoSaveEntries ?? false;

			if (autoSaveEnabled) {
				// Create unique storage key based on mode and current data
				const storageKey = mode === 'add' 
					? 'vault-entry-draft-new'
					: `vault-entry-draft-${formData.site || 'editing'}`;

				autoSaveInstance = createAutoSave({
					storageKey,
					delay: 2000, // 2 second delay
					enabled: autoSaveEnabled,
					saveFunction: async () => {
						// For now, this just saves to localStorage
						// In the future, this could make API calls
						autoSaveInstance?.saveDraft(formData);
					},
					onStatusChange: (status) => {
						saveStatus = status;
					}
				});

				// Try to load draft if we're adding a new entry
				if (mode === 'add') {
					const draft = autoSaveInstance.loadDraft();
					if (draft) {
						// Ask user if they want to restore the draft
						const restore = confirm(
							'Found a saved draft from a previous session. Would you like to restore it?'
						);
						if (restore) {
							formData = { ...formData, ...draft };
							dispatch('toast', { 
								message: 'Draft restored successfully!', 
								type: 'success' 
							});
						} else {
							// Clear the draft if user doesn't want it
							autoSaveInstance.clearDraft();
						}
					}
				}
			}
		} catch (error) {
			console.warn('Failed to load user preferences for auto-save:', error);
		}
	});

	// Clean up auto-save on component destroy
	onDestroy(() => {
		autoSaveInstance?.destroy();
	});

	// Trigger auto-save when form data changes
	$: if (autoSaveInstance && autoSaveEnabled) {
		// Save draft immediately to localStorage
		autoSaveInstance.saveDraft(formData);
		// Trigger debounced save
		autoSaveInstance.trigger();
	}

	// Password strength calculation
	$: {
		passwordStrength = calculatePasswordStrength(formData.password);
	}

	function calculatePasswordStrength(password: string): number {
		if (!password) return 0;

		let score = 0;
		if (password.length >= 8) score += 25;
		if (password.length >= 12) score += 25;
		if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score += 20;
		if (/\d/.test(password)) score += 15;
		if (/[^a-zA-Z\d]/.test(password)) score += 15;

		return Math.min(score, 100);
	}

	function getPasswordStrengthText(strength: number): string {
		if (strength < 40) return 'Weak';
		if (strength < 70) return 'Medium';
		return 'Strong';
	}

	async function handleGeneratePassword() {
		isGenerating = true;
		// Add a slight delay for better UX feedback
		await new Promise((resolve) => setTimeout(resolve, 300));
		formData.password = generatePassword();
		isGenerating = false;
		dispatch('toast', { message: 'New password generated!', type: 'success' });
	}

	async function handleCopyPassword() {
		if (formData.password) {
			await copyToClipboard(formData.password);
			isCopied = true;
			dispatch('toast', { message: 'Password copied to clipboard!', type: 'success' });

			// Reset copied state after 2 seconds
			setTimeout(() => {
				isCopied = false;
			}, 2000);
		}
	}

	function handleUrlInput(event: Event) {
		const target = event.target as HTMLInputElement;
		let value = target.value;

		// Auto-add https:// if user enters a domain without protocol
		if (value && !value.includes('://') && value.includes('.')) {
			formData.siteUrl = `https://${value}`;
		}
	}

	// Auto-focus next field on Enter (except for textarea)
	function handleKeyDown(event: KeyboardEvent, nextFieldId?: string) {
		if (event.key === 'Enter' && nextFieldId) {
			event.preventDefault();
			const nextField = document.getElementById(nextFieldId);
			nextField?.focus();
		}
	}

	// Handle form submission - clear draft on successful save
	function handleSubmit() {
		onSubmit();
		// Clear draft after successful submission
		if (autoSaveInstance) {
			autoSaveInstance.clearDraft();
		}
	}
</script>

<form on:submit|preventDefault={handleSubmit} class="space-y-6 p-6" autocomplete="off">
	<!-- Auto-save Status Indicator -->
	{#if autoSaveEnabled && autoSaveInstance}
		<div class="flex items-center justify-between rounded-lg bg-gray-50 px-3 py-2 text-sm">
			<div class="flex items-center space-x-2">
				<svg class="h-4 w-4 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3M3 17V7a2 2 0 012-2h6l2 2h6a2 2 0 012 2v10a2 2 0 01-2 2H5a2 2 0 01-2-2z"></path>
				</svg>
				<span class="text-gray-600">Auto-save</span>
			</div>
			<div class="flex items-center space-x-2">
				{#if saveStatus === 'saving'}
					<svg class="h-3 w-3 animate-spin {getStatusColor(saveStatus)}" fill="none" viewBox="0 0 24 24">
						<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
						<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
					</svg>
				{:else if saveStatus === 'saved'}
					<svg class="h-3 w-3 {getStatusColor(saveStatus)}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
					</svg>
				{:else if saveStatus === 'error'}
					<svg class="h-3 w-3 {getStatusColor(saveStatus)}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
					</svg>
				{/if}
				<span class="{getStatusColor(saveStatus)} font-medium">
					{getStatusText(saveStatus)}
				</span>
			</div>
		</div>
	{/if}
	<!-- Site Name -->
	<div class="space-y-2">
		<label for="siteName" class="mb-1 block text-sm font-semibold text-gray-800">
			Site Name *
		</label>
		<input
			type="text"
			id="siteName"
			bind:value={formData.site}
			on:keydown={(e) => handleKeyDown(e, 'url')}
			placeholder="e.g., Google, GitHub, Facebook"
			class="w-full rounded-xl border-2 border-gray-200 bg-white px-4 py-3.5 placeholder-gray-400 transition-colors duration-150 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500"
			class:border-red-300={errors.site}
			class:focus:ring-red-500={errors.site}
			class:focus:border-red-500={errors.site}
			class:border-indigo-200={formData.site && !errors.site}
		/>
		{#if errors.site}
			<p class="flex items-center space-x-1 text-sm text-red-600">
				<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
					></path>
				</svg>
				<span>{errors.site}</span>
			</p>
		{/if}
	</div>

	<!-- Website URL -->
	<div class="space-y-2">
		<label for="url" class="mb-1 block text-sm font-semibold text-gray-800">
			Website URL
			<span class="text-xs font-normal text-gray-500">(optional)</span>
		</label>
		<input
			type="url"
			id="url"
			bind:value={formData.siteUrl}
			on:blur={handleUrlInput}
			on:keydown={(e) => handleKeyDown(e, 'username')}
			placeholder="https://example.com"
			class="w-full rounded-xl border-2 border-gray-200 bg-white px-4 py-3.5 placeholder-gray-400 transition-colors duration-150 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500"
			class:border-indigo-200={formData.siteUrl}
		/>
	</div>

	<!-- Username/Email -->
	<div class="space-y-2">
		<label for="email" class="mb-1 block text-sm font-semibold text-gray-800">
			Email *
		</label>
		<input
			type="text"
			id="email"
			bind:value={formData.email}
			on:keydown={(e) => handleKeyDown(e, 'password')}
			placeholder="your-username or email@example.com"
			class="w-full rounded-xl border-2 border-gray-200 bg-white px-4 py-3.5 placeholder-gray-400 transition-colors duration-150 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500"
			class:border-red-300={errors.email}
			class:focus:ring-red-500={errors.email}
			class:focus:border-red-500={errors.email}
			class:border-indigo-200={formData.email && !errors.email}
		/>
		{#if errors.email}
			<p class="flex items-center space-x-1 text-sm text-red-600">
				<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
					></path>
				</svg>
				<span>{errors.email}</span>
			</p>
		{/if}
	</div>

	<!-- Password -->
	<div class="space-y-2">
		<label for="password" class="mb-1 block text-sm font-semibold text-gray-800">
			Password *
		</label>
		<div class="relative">
			<input
				type={showPassword ? 'text' : 'password'}
				id="password"
				bind:value={formData.password}
				on:keydown={(e) => handleKeyDown(e, 'notes')}
				placeholder="Enter or generate a secure password"
				class="w-full rounded-xl border-2 border-gray-200 bg-white px-4 py-3.5 pr-24 placeholder-gray-400 transition-colors duration-150 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500"
				class:border-red-300={errors.password}
				class:focus:ring-red-500={errors.password}
				class:focus:border-red-500={errors.password}
				class:border-indigo-200={formData.password && !errors.password}
			/>
			<div class="absolute inset-y-0 right-0 flex items-center space-x-1 pr-3">
				<button
					type="button"
					on:click={() => (showPassword = !showPassword)}
					class="rounded-lg p-1.5 text-gray-400 transition-colors duration-150 hover:bg-gray-100 hover:text-gray-600"
					title={showPassword ? 'Hide password' : 'Show password'}
				>
					{#if showPassword}
						<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21"
							/>
						</svg>
					{:else}
						<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
							/>
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
							/>
						</svg>
					{/if}
				</button>
				{#if formData.password}
					<button
						type="button"
						on:click={handleCopyPassword}
						class="relative rounded-lg p-1.5 text-gray-400 transition-colors duration-150 hover:bg-gray-100 hover:text-gray-600"
						class:text-green-600={isCopied}
						class:hover:text-green-700={isCopied}
						title="Copy password"
						aria-label="Copy password"
					>
						{#if isCopied}
							<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M5 13l4 4L19 7"
								></path>
							</svg>
						{:else}
							<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
								/>
							</svg>
						{/if}
					</button>
				{/if}
			</div>
		</div>

		<!-- Password Strength Indicator -->
		{#if formData.password}
			<div class="space-y-2">
				<div class="flex items-center justify-between text-xs">
					<span class="text-gray-600">Password strength:</span>
					<span
						class="font-medium"
						class:text-red-600={passwordStrength < 40}
						class:text-yellow-600={passwordStrength >= 40 && passwordStrength < 70}
						class:text-green-600={passwordStrength >= 70}
					>
						{getPasswordStrengthText(passwordStrength)}
					</span>
				</div>
				<div class="h-2 w-full overflow-hidden rounded-full bg-gray-200">
					<div
						class="h-full rounded-full transition-all duration-300"
						class:bg-red-500={passwordStrength < 40}
						class:bg-yellow-500={passwordStrength >= 40 && passwordStrength < 70}
						class:bg-green-500={passwordStrength >= 70}
						style="width: {passwordStrength}%"
					></div>
				</div>
			</div>
		{/if}

		{#if errors.password}
			<p class="flex items-center space-x-1 text-sm text-red-600">
				<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
					></path>
				</svg>
				<span>{errors.password}</span>
			</p>
		{/if}

		<div class="flex items-center space-x-4">
			<button
				type="button"
				on:click={handleGeneratePassword}
				disabled={isGenerating}
				class="inline-flex items-center rounded-lg bg-indigo-50 px-3 py-1.5 text-sm font-medium text-indigo-600 transition-colors hover:bg-indigo-100 hover:text-indigo-700 disabled:opacity-50"
			>
				{#if isGenerating}
					<svg class="mr-2 -ml-1 h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
						<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"
						></circle>
						<path
							class="opacity-75"
							fill="currentColor"
							d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
						></path>
					</svg>
					Generating...
				{:else}
					<svg class="mr-1.5 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
						></path>
					</svg>
					Generate {mode === 'add' ? 'secure' : 'new'} password
				{/if}
			</button>
		</div>
	</div>

	<!-- Notes -->
	<div class="space-y-2">
		<label for="notes" class="mb-1 block text-sm font-semibold text-gray-800">
			Notes
			<span class="text-xs font-normal text-gray-500">(optional)</span>
		</label>
		<textarea
			id="notes"
			bind:value={formData.notes}
			placeholder="Additional information, security questions, or recovery codes..."
			rows="3"
			class="w-full resize-none rounded-xl border-2 border-gray-200 bg-white px-4 py-3.5 placeholder-gray-400 transition-colors duration-150 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500"
			class:border-indigo-200={formData.notes}
		></textarea>
	</div>

	<!-- Action Buttons -->
	<div
		class="flex flex-col-reverse space-y-3 space-y-reverse border-t-2 border-gray-100 pt-6 sm:flex-row sm:justify-end sm:space-y-0 sm:space-x-3"
	>
		<button
			type="button"
			on:click={onCancel}
			class="w-full rounded-xl border-2 border-gray-300 bg-white px-6 py-3.5 text-sm font-semibold text-gray-700 transition-all duration-200 hover:border-gray-400 hover:bg-gray-50 focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 sm:w-auto"
		>
			Cancel
		</button>
		<button
			type="submit"
			disabled={isSubmitting}
			class="flex w-full items-center justify-center rounded-xl bg-indigo-600 px-8 py-3.5 text-sm font-semibold text-white shadow-lg transition-all duration-200 hover:bg-indigo-700 hover:shadow-xl focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-indigo-600 sm:w-auto"
		>
			{#if isSubmitting}
				<svg class="mr-3 -ml-1 h-5 w-5 animate-spin" fill="none" viewBox="0 0 24 24">
					<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
					<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
				</svg>
				{mode === 'add' ? 'Saving Entry...' : 'Saving Changes...'}
			{:else}
				<svg class="mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
				</svg>
				{mode === 'add' ? 'Save Entry' : 'Save Changes'}
			{/if}
		</button>
	</div>
</form>

<style>
	input:focus,
	textarea:focus {
		outline: none;
	}
	input,
	textarea,
	button {
		transition:
			color 0.15s,
			border-color 0.15s,
			background 0.15s;
	}
	input,
	textarea {
		-webkit-appearance: none;
		appearance: none;
	}
	/* Custom scrollbar for textarea */
	textarea::-webkit-scrollbar {
		width: 6px;
	}
	textarea::-webkit-scrollbar-track {
		background: #f1f5f9;
		border-radius: 3px;
	}
	textarea::-webkit-scrollbar-thumb {
		background: #cbd5e1;
		border-radius: 3px;
	}
	textarea::-webkit-scrollbar-thumb:hover {
		background: #94a3b8;
	}
</style>
