<script lang="ts">
	// eslint-disable-next-line svelte/no-navigation-without-resolve
	import { createEventDispatcher } from 'svelte';
	import type { VaultItem } from '$lib/types';
	import { maskPassword, getSiteFavicon } from '$lib/utils/helpers';
	import { goto } from '$app/navigation';
	import { requestViewOtp, verifyViewOtp, get2FAStatus } from '$lib/api/2fa';
	import { toast } from '$lib/stores/toast';
	import { onMount } from 'svelte';

	export let item: VaultItem;

	const dispatch = createEventDispatcher();

	let showDetails = false;

	// Password strength indicator state
	let passwordStrength = { color: 'gray', text: 'Unknown' };


	// Show/hide password in modal (only after 2FA)
	let showPassword = false;

	// Share modal state
	let showShareModal = false;

	// Share credential form component (lazy loaded)
	let ShareCredentialForm: typeof import('./ShareCredentialForm.svelte').default | null = null;

	async function openShareModal() {
		showShareModal = true;
		if (!ShareCredentialForm) {
			const mod = await import('./ShareCredentialForm.svelte');
			ShareCredentialForm = mod.default;
		}
	}


	// 2FA modal state
	let show2FAModal = false;
	let twoFACodeDigits = ['', '', '', '', '', ''];
	let twoFAError = '';
	let verifying2FA = false;
	let requestingOtp = false;
	let otpRequested = false;
	let is2FAEnabled = false;
	let checking2FAStatus = false;
	let codeInputs: Array<HTMLInputElement | null> = [null, null, null, null, null, null];

	function assignCodeInput(idx: number) {
		return (el: HTMLInputElement | null) => {
			codeInputs[idx] = el;
		};
	}

	function handle2FADigitInput(e: Event, idx: number) {
		const input = e.target as HTMLInputElement;
		let val = input.value.replace(/\D/g, '');
		if (val.length > 1) val = val.slice(-1); // Only last digit
		twoFACodeDigits[idx] = val;
		if (val && idx < 5) {
			codeInputs[idx + 1]?.focus();
		}
		// If user pasted all at once
		if (val.length === 6 && idx === 0) {
			for (let i = 0; i < 6; i++) {
				twoFACodeDigits[i] = val[i] || '';
				if (val[i] && codeInputs[i]) codeInputs[i]!.value = val[i];
			}
			codeInputs[5]?.focus();
		}
	}

	function handle2FADigitKeydown(e: KeyboardEvent, idx: number) {
		const input = e.target as HTMLInputElement;
		if (e.key === 'Backspace') {
			if (!input.value && idx > 0) {
				codeInputs[idx - 1]?.focus();
			}
		} else if (e.key === 'ArrowLeft' && idx > 0) {
			codeInputs[idx - 1]?.focus();
		} else if (e.key === 'ArrowRight' && idx < 5) {
			codeInputs[idx + 1]?.focus();
		}
	}

	function getTwoFACode() {
		return twoFACodeDigits.join('');
	}

	// Track which field was copied
	let copiedField: string | null = null;


	// Compute password strength
	function getPasswordStrength(password: string) {
		if (!password) return { color: 'gray', text: 'Empty' };
		let score = 0;
		if (password.length >= 12) score++;
		if (/[A-Z]/.test(password)) score++;
		if (/[a-z]/.test(password)) score++;
		if (/[0-9]/.test(password)) score++;
		if (/[^A-Za-z0-9]/.test(password)) score++;
		if (score >= 5) return { color: 'green', text: 'Strong' };
		if (score >= 3) return { color: 'blue', text: 'Medium' };
		if (score >= 2) return { color: 'yellow', text: 'Weak' };
		return { color: 'red', text: 'Very Weak' };
	}

	// Mock 2FA verification (replace with real API call)
	async function verify2FACode(code: string): Promise<boolean> {
		try {
			const result = await verifyViewOtp(code);
			return result.verified;
		} catch (error) {
			console.error('Error verifying OTP:', error);
			twoFAError = error instanceof Error ? error.message : 'Failed to verify OTP';
			return false;
		}
	}

	// Request OTP for viewing credential
	async function requestOtpCode(): Promise<void> {
		if (requestingOtp) return;
		
		requestingOtp = true;
		twoFAError = '';
		
		try {
			await requestViewOtp(item.id);
			otpRequested = true;
			toast.show('OTP sent to your email', 'success');
		} catch (error) {
			console.error('Error requesting OTP:', error);
			twoFAError = error instanceof Error ? error.message : 'Failed to request OTP';
		} finally {
			requestingOtp = false;
		}
	}

	// Check 2FA status
	async function check2FAStatus(): Promise<void> {
		if (checking2FAStatus) return;
		
		checking2FAStatus = true;
		try {
			const status = await get2FAStatus();
			is2FAEnabled = status.enabled;
		} catch (error) {
			console.error('Error checking 2FA status:', error);
			is2FAEnabled = false;
		} finally {
			checking2FAStatus = false;
		}
	}

	// Handle showing password - check 2FA status first
	async function handleShowPassword(): Promise<void> {
		if (!showPassword) {
			await check2FAStatus();
			
			if (!is2FAEnabled) {
				show2FAModal = true;
				return;
			}
			
			// 2FA is enabled, proceed with OTP flow
			show2FAModal = true;
			twoFACodeDigits = ['', '', '', '', '', ''];
			twoFAError = '';
			otpRequested = false;
			// Automatically request OTP when modal opens
			setTimeout(() => requestOtpCode(), 100);
		} else {
			showPassword = false;
		}
	}

	// Navigate to settings with 2FA highlight
	function goToSettings(): void {
		show2FAModal = false;
		// Add a URL parameter to highlight the 2FA section
		goto('/settings?highlight=2fa');
	}

	$: passwordStrength = getPasswordStrength(item?.password);

	// Handle copy action
	function handleCopy(value: string, type: string) {
		dispatch('copy', { value, type });
		copiedField = type.toLowerCase();
		setTimeout(() => {
			if (copiedField === type.toLowerCase()) copiedField = null;
		}, 1200);
	}
	// Wrapper to avoid inline function in on:click
	function copyToClipboard(value: string, type: string) {
		handleCopy(value, type);
	}

	// Wrapper to avoid inline function in on:click
	function handleEdit() {
		console.log(`Editing item ${item.id}`);
		goto(`/vault/${item.id}/edit`);
	}

	function handleDelete() {
		dispatch('delete', { item });
	}
	function handleView() {
		showDetails = true;
	}
	function closeModal() {
		showDetails = false;
	}

	function formatDate(dateStr: string) {
        if (!dateStr) return '';
        const date = new Date(dateStr);
        return date.toLocaleString(undefined, {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
</script>

<div class="p-6 transition-colors hover:bg-gray-50">
	<div class="grid grid-cols-6 items-center gap-4">
		<!-- Site Icon -->
		<div class="flex justify-center">
			<img
				src={getSiteFavicon(item.siteUrl)}
				alt="{item.siteName} favicon"
				class="h-10 w-10 rounded-lg border border-gray-200"
				on:error={(e) => {
					const target = e.target as HTMLImageElement | null;
					if (target) {
						target.src =
							"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23e5e7eb'%3E%3Cpath d='M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z'/%3E%3C/svg%3E";
					}
				}}
			/>
		</div>

		<!-- Site Details -->
		<div class="min-w-0">
			<div class="flex items-center space-x-2">
				<h3 class="truncate text-sm font-medium text-gray-900">{item.siteName}</h3>
				<span class="text-xs text-gray-500">•</span>
				<span class="truncate text-xs text-gray-500">{item.siteUrl}</span>
			</div>
			{#if item.notes}
				<p class="mt-1 truncate text-xs text-gray-500">{item.notes}</p>
			{/if}
		</div>

		<!-- Email -->
		<div class="flex items-center space-x-2">
			<span class="text-xs text-gray-500">Email:</span>
			<span class="truncate text-sm text-gray-900">{item.email}</span>
			<button
				on:click={() => handleCopy(item.email, 'email')}
				class="text-gray-400 transition-colors hover:text-gray-600"
				title="Copy email"
				aria-label="Copy email"
			>
				<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
					/>
				</svg>
			</button>
		</div>

		<!-- Password -->
		<div class="flex items-center space-x-2">
			<span class="text-xs text-gray-500">Password:</span>
			<span class="truncate font-mono text-sm text-gray-900">{maskPassword(item.password)}</span>
			<button
				on:click={() => handleCopy(showPassword ? item.password : maskPassword(item.password), 'Password')}
				class="text-gray-400 transition-colors hover:text-gray-600"
				title={showPassword ? "Copy password" : "Copy masked password (verify to copy actual password)"}
				aria-label={showPassword ? "Copy password" : "Copy masked password"}
			>
				<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
					/>
				</svg>
			</button>
		</div>

		<!-- Notes (hidden on small screens, already shown above for details) -->
		<div class="hidden md:block">
			{#if item.notes}
				<span class="truncate text-xs text-gray-500">{item.notes}</span>
			{/if}
		</div>

		<!-- Actions -->
		<div class="flex items-center justify-end space-x-2">
			<!-- View (eye) icon -->
			<button
				on:click={handleView}
				class="rounded-lg p-2 text-gray-400 transition-colors hover:bg-blue-50 hover:text-blue-600"
				title="View details"
				aria-label="View details"
			>
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
						d="M2.458 12C3.732 7.943 7.523 5 12 5c4.477 0 8.268 2.943 9.542 7-1.274 4.057-5.065 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
					/>
				</svg>
			</button>
			<!-- Share button -->
			<!-- <button
				on:click={() => showShareModal = true}
				class="rounded-lg p-2 text-gray-400 transition-colors hover:bg-indigo-50 hover:text-indigo-600"
				title="Share entry"
				aria-label="Share entry"
			>
				<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 8a3 3 0 11-6 0 3 3 0 016 0zm6 8a6 6 0 00-12 0v1a3 3 0 003 3h6a3 3 0 003-3v-1z"/>
				</svg>
			</button> -->
			<!-- eslint-disable-next-line svelte/no-navigation-without-resolve -->
			<a
				href={`/vault/${item.id}/edit`}
				class="rounded-lg p-2 text-gray-400 transition-colors hover:bg-indigo-50 hover:text-indigo-600"
				title="Edit entry"
				aria-label="Edit entry"
			>
				<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
					/>
				</svg>
			</a>
			<button
				on:click={handleDelete}
				class="rounded-lg p-2 text-gray-400 transition-colors hover:bg-red-50 hover:text-red-600"
				title="Delete entry"
				aria-label="Delete entry"
			>
				<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
					/>
				</svg>
			</button>

					{#if showShareModal}
						<!-- Glassmorphic Backdrop -->
						<div class="fixed inset-0 z-50 flex items-center justify-center p-4">
							<div class="absolute inset-0 bg-black/30 backdrop-blur-sm"></div>
							<!-- Modal -->
							<div class="relative w-full max-w-sm rounded-2xl border border-white/30 bg-white/90 p-8 shadow-2xl backdrop-blur-xl" style="box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.18);">
								<button class="absolute top-3 right-3 rounded-full p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-600 focus:ring-2 focus:ring-indigo-500 focus:outline-none" on:click={() => showShareModal = false} aria-label="Close share modal">
									<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
									</svg>
								</button>
								<h2 class="mb-4 text-lg font-semibold text-gray-900">Share Credential</h2>
								{#if ShareCredentialForm}
									<svelte:component this={ShareCredentialForm} {item} closeShareModal={() => showShareModal = false} />
								{:else}
									<div class="flex flex-col items-center justify-center py-8">
										<div class="mb-3 h-8 w-8 animate-spin rounded-full border-2 border-indigo-500 border-t-transparent"></div>
										<div class="text-gray-500 text-sm">Loading share form...</div>
									</div>
								{/if}
							</div>
						</div>
					{/if}
		</div>

		<!-- Details Modal -->
		{#if showDetails}
			<div class="fixed inset-0 z-50 flex items-center justify-center p-4">
				<!-- Backdrop -->
				<div
					class="animate-fade-in absolute inset-0 bg-black/50 backdrop-blur-sm"
					role="button"
					tabindex="0"
					aria-label="Close details modal"
					on:click={closeModal}
					on:keydown={(e) => e.key === 'Escape' && closeModal()}
				></div>

				<!-- Modal Content -->
				<div
					class="animate-modal-enter relative w-full max-w-md rounded-3xl border border-gray-200 bg-white shadow-2xl overflow-hidden max-h-[90vh] flex flex-col"
				>
					<!-- Compact Header -->
					<div class="relative bg-gradient-to-br from-indigo-50 to-blue-50 border-b border-gray-100 px-6 pt-6 pb-4">
						<button
							class="absolute top-3 right-3 rounded-full p-1.5 text-gray-400 transition-all duration-200 hover:bg-white/50 hover:text-gray-600 focus:ring-2 focus:ring-indigo-500 focus:outline-none backdrop-blur-sm"
							on:click={closeModal}
							aria-label="Close details"
						>
							<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M6 18L18 6M6 6l12 12"
								/>
							</svg>
						</button>

						<div class="flex items-center space-x-3">
							<div class="relative">
								<img
									src={getSiteFavicon(item.siteUrl)}
									alt="{item.siteName} favicon"
									class="h-12 w-12 rounded-xl border-2 border-white shadow-md"
									on:error={(e) => {
										const target = e.target as HTMLImageElement | null;
										if (target) {
											target.src = `data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='48' height='48' viewBox='0 0 24 24' fill='none' stroke='%236366f1' stroke-width='2'%3E%3Cpath d='M21 16V8a2 2 0 0 0-1-1.73L12 2 4 6.27A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73L12 22l8-4.27A2 2 0 0 0 21 16z'/%3E%3Cpath d='M12 12l8-4.27L12 2 4 7.73z'/%3E%3C/svg%3E`;
										}
									}}
								/>
								<!-- Smaller trust indicator -->
								<div
									class="absolute -right-0.5 -bottom-0.5 flex h-4 w-4 items-center justify-center rounded-full border border-white bg-green-500 shadow-sm"
								>
									<svg class="h-2 w-2 text-white" fill="currentColor" viewBox="0 0 20 20">
										<path
											fill-rule="evenodd"
											d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
											clip-rule="evenodd"
										/>
									</svg>
								</div>
							</div>
							<div class="flex-1 text-left">
								<h2 class="text-lg font-bold text-gray-900 truncate">{item.siteName}</h2>
								<!-- eslint-disable-next-line svelte/no-navigation-without-resolve -->
								<a
									href={item.siteUrl}
									target="_blank"
									rel="noopener noreferrer"
									class="inline-flex items-center text-xs text-indigo-600 transition-colors hover:text-indigo-700 truncate"
								>
									{item.siteUrl}
									<svg class="ml-1 h-2.5 w-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
										/>
									</svg>
								</a>
							</div>
						</div>
					</div>

					<!-- Scrollable Content -->
					<div class="flex-1 overflow-y-auto px-6 py-4 space-y-4">
						<!-- Email Field -->
						<div class="group">
							<div class="mb-2 flex items-center justify-between">
								<label for="username-field" class="flex items-center text-sm font-medium text-gray-700">
									<svg class="mr-1.5 h-3.5 w-3.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 12a4 4 0 10-8 0 4 4 0 008 0zm0 0v1.5a2.5 2.5 0 005 0V12a9 9 0 10-9 9m4.5-1.206a8.959 8.959 0 01-4.5 1.207"/>
									</svg>
									Email
								</label>
								<button
									on:click={() => copyToClipboard(item.email, 'email')}
									class="flex items-center rounded-md px-2 py-1 text-xs text-gray-500 transition-all duration-200 hover:bg-indigo-50 hover:text-indigo-600"
								>
									{#if copiedField === 'email'}
										<svg class="mr-1 h-3 w-3 text-green-600" fill="currentColor" viewBox="0 0 20 20">
											<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
										</svg>
										Copied!
									{:else}
										<svg class="mr-1 h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
										</svg>
										Copy
									{/if}
								</button>
							</div>
							<input
								id="email-field"
								type="text"
								value={item.email}
								readonly
								class="w-full cursor-pointer rounded-lg border border-gray-200 bg-gray-50/50 px-3 py-2.5 text-sm text-gray-900 transition-all duration-200 hover:bg-gray-50 hover:border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:outline-none"
								on:click={() => copyToClipboard(item.email, 'email')}
							/>
						</div>

						<!-- Password Field -->
						<div class="group">
							<div class="mb-2 flex items-center justify-between">
								<label for="password-field" class="flex items-center text-sm font-medium text-gray-700">
									<svg class="mr-1.5 h-3.5 w-3.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z"/>
									</svg>
									Password
								</label>
								<div class="flex items-center space-x-2">
									<!-- Compact Password Strength -->
									<div class="flex items-center px-2 py-0.5 rounded-full bg-gray-100">
										<div class="mr-1 h-1.5 w-1.5 rounded-full bg-{passwordStrength.color}-500"></div>
										<span class="text-xs text-{passwordStrength.color}-600 font-medium">{passwordStrength.text}</span>
									</div>
									<button
										on:click={() => copyToClipboard(showPassword ? item.password : maskPassword(item.password), 'password')}
										class="flex items-center rounded-md px-2 py-1 text-xs text-gray-500 transition-all duration-200 hover:bg-indigo-50 hover:text-indigo-600"
									>
										{#if copiedField === 'password'}
											<svg class="mr-1 h-3 w-3 text-green-600" fill="currentColor" viewBox="0 0 20 20">
												<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
											</svg>
											Copied!
										{:else}
											<svg class="mr-1 h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
												<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
											</svg>
											Copy
										{/if}
									</button>
								</div>
							</div>
							<div class="relative">
								<input
									id="password-field"
									type={showPassword ? 'text' : 'password'}
									value={item.password}
									readonly
									class="w-full cursor-pointer rounded-lg border border-gray-200 bg-gray-50/50 px-3 py-2.5 pr-10 font-mono text-sm text-gray-900 transition-all duration-200 hover:bg-gray-50 hover:border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:outline-none"
									on:click={() => copyToClipboard(showPassword ? item.password : maskPassword(item.password), 'password')}
								/>
								<button
									on:click={handleShowPassword}
									class="absolute top-1/2 right-2.5 -translate-y-1/2 transform p-1 text-gray-400 transition-all duration-200 hover:text-gray-600 hover:bg-gray-100 rounded-md"
									aria-label={showPassword ? 'Hide password' : 'Show password'}
								>
									{#if showPassword}
										<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path
												stroke-linecap="round"
												stroke-linejoin="round"
												stroke-width="2"
												d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21"
											/>
										</svg>
									{:else}
										<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
								{#if show2FAModal}
									<div class="fixed inset-0 z-50 flex items-center justify-center p-4">
										<!-- Backdrop -->
										<div
											class="animate-fade-in absolute inset-0 bg-black/50 backdrop-blur-sm"
											role="button"
											tabindex="0"
											aria-label="Close 2FA modal"
											on:click={() => { 
												show2FAModal = false; 
												twoFACodeDigits = ['', '', '', '', '', '']; 
												twoFAError = '';
												otpRequested = false;
												requestingOtp = false;
											}}
											on:keydown={(e) => e.key === 'Escape' && (show2FAModal = false)}
										></div>
										<!-- Modal Content -->
										<form
											class="animate-modal-enter relative w-full max-w-xs rounded-2xl border border-gray-100 bg-white shadow-2xl p-8"
											on:submit|preventDefault={async () => {
												if (!is2FAEnabled) {
													// Redirect to settings
													goToSettings();
													return;
												}
												
												if (!otpRequested) {
													await requestOtpCode();
													return;
												}
												
												verifying2FA = true;
												twoFAError = '';
												const code = getTwoFACode();
												const ok = await verify2FACode(code);
												verifying2FA = false;
												if (ok) {
													show2FAModal = false;
													showPassword = true;
													twoFACodeDigits = ['', '', '', '', '', ''];
													otpRequested = false;
												}
											}}
										>
											<h3 class="mb-4 text-lg font-semibold text-gray-900 text-center">
												{#if !is2FAEnabled}
													2FA Required
												{:else if !otpRequested}
													Request View Access
												{:else}
													Enter Email OTP
												{/if}
											</h3>
											
											{#if !is2FAEnabled}
												<div class="text-center mb-4">
													<div class="mb-4 p-3 rounded-lg bg-amber-50 border border-amber-200">
														<svg class="mx-auto mb-2 h-8 w-8 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
															<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"/>
														</svg>
														<h4 class="text-sm font-medium text-amber-800 mb-1">2FA Not Enabled</h4>
														<p class="text-sm text-amber-700">
															Two-factor authentication is required to view passwords. Please enable 2FA in your security settings first.
														</p>
													</div>
													<button
														type="submit"
														class="w-full rounded-lg bg-indigo-600 px-4 py-2 text-white font-semibold transition-colors hover:bg-indigo-700"
													>
														Go to Security Settings
													</button>
												</div>
											{:else if !otpRequested}
												<div class="text-center mb-4">
													<p class="text-sm text-gray-600 mb-4">
														An OTP will be sent to your email to verify your identity before viewing this password.
													</p>
													<button
														type="submit"
														class="w-full rounded-lg bg-indigo-600 px-4 py-2 text-white font-semibold transition-colors hover:bg-indigo-700 disabled:opacity-60"
														disabled={requestingOtp}
													>
														{requestingOtp ? 'Sending OTP...' : 'Send OTP to Email'}
													</button>
												</div>
											{:else}
												<div class="text-center mb-4">
													<p class="text-sm text-gray-600 mb-4">
														Check your email for the 6-digit OTP code and enter it below.
													</p>
												</div>
												<div class="mb-2 flex justify-center space-x-2">
													{#each [0,1,2,3,4,5] as idx}
														<input
															type="text"
															inputmode="numeric"
															maxlength="1"
															class="w-10 h-12 rounded-lg border border-gray-200 text-center text-xl font-mono focus:ring-2 focus:ring-indigo-500 focus:outline-none"
															bind:this={codeInputs[idx]}
															bind:value={twoFACodeDigits[idx]}
															on:input={(e) => handle2FADigitInput(e, idx)}
															on:keydown={(e) => handle2FADigitKeydown(e, idx)}
															autocomplete={idx === 0 ? 'one-time-code' : 'off'}
														/>
													{/each}
												</div>
												<button
													type="submit"
													class="mt-2 w-full rounded-lg bg-indigo-600 px-4 py-2 text-white font-semibold transition-colors hover:bg-indigo-700 disabled:opacity-60"
													disabled={verifying2FA || getTwoFACode().length !== 6 || twoFACodeDigits.some(d => d === '')}
												>
													{verifying2FA ? 'Verifying...' : 'Verify & Show Password'}
												</button>
												
												<div class="mt-3 text-center">
													<button
														type="button"
														class="text-sm text-indigo-600 hover:text-indigo-700 underline"
														on:click={requestOtpCode}
														disabled={requestingOtp}
													>
														{requestingOtp ? 'Resending...' : 'Resend OTP'}
													</button>
												</div>
											{/if}
											
											{#if twoFAError}
												<div class="mb-2 text-sm text-red-600 text-center">{twoFAError}</div>
											{/if}
											
											<button
												type="button"
												class="mt-3 w-full rounded-lg bg-gray-100 px-4 py-2 text-gray-700 font-medium hover:bg-gray-200"
												on:click={() => { 
													show2FAModal = false; 
													twoFACodeDigits = ['', '', '', '', '', '']; 
													twoFAError = '';
													otpRequested = false;
													requestingOtp = false;
													is2FAEnabled = false;
												}}
											>
												Cancel
											</button>
										</form>
									</div>
								{/if}
							</div>
						</div>

						<!-- Notes Field (if exists) -->
						{#if item.notes}
							<div class="group">
								<div class="mb-2 flex items-center justify-between">
									<label for="notes-field" class="flex items-center text-sm font-medium text-gray-700">
										<svg class="mr-1.5 h-3.5 w-3.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
										</svg>
										Notes
									</label>
									<button
										on:click={() => copyToClipboard(item.notes ?? '', 'notes')}
										class="flex items-center rounded-md px-2 py-1 text-xs text-gray-500 transition-all duration-200 hover:bg-indigo-50 hover:text-indigo-600"
									>
										{#if copiedField === 'notes'}
											<svg class="mr-1 h-3 w-3 text-green-600" fill="currentColor" viewBox="0 0 20 20">
												<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
											</svg>
											Copied!
										{:else}
											<svg class="mr-1 h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
												<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
											</svg>
											Copy
										{/if}
									</button>
								</div>
								<textarea
									id="notes-field"
									value={item.notes}
									readonly
									class="w-full cursor-pointer resize-none rounded-lg border border-gray-200 bg-gray-50/50 px-3 py-2.5 text-sm text-gray-900 transition-all duration-200 hover:bg-gray-50 hover:border-gray-300 focus:ring-2 focus:ring-indigo-500 focus:outline-none"
									rows="2"
									on:click={() => copyToClipboard(item.notes ?? '', 'notes')}
								></textarea>
							</div>
						{/if}

						<!-- Compact Metadata -->
						<div class="grid grid-cols-2 gap-3 border-t border-gray-100 pt-3">
							<div class="text-center p-2 rounded-md bg-gray-50/50">
								<div class="flex items-center justify-center mb-1">
									<svg class="h-3 w-3 text-gray-400 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"/>
									</svg>
									<div class="text-xs font-medium text-gray-500">Created</div>
								</div>
								<div class="text-xs text-gray-900">{formatDate(item.createdAt)}</div>
							</div>
							<div class="text-center p-2 rounded-md bg-gray-50/50">
								<div class="flex items-center justify-center mb-1">
									<svg class="h-3 w-3 text-gray-400 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
									</svg>
									<div class="text-xs font-medium text-gray-500">Updated</div>
								</div>
								<div class="text-xs text-gray-900">{formatDate(item.updatedAt)}</div>
							</div>
						</div>
					</div>

					<!-- Compact Footer -->
					<div class="border-t border-gray-100 bg-gray-50/50 px-6 py-3">
						<div class="flex items-center justify-between space-x-3">
							<button
								on:click={handleEdit}
								class="flex items-center rounded-lg bg-indigo-50 px-3 py-2 text-sm text-indigo-600 transition-all duration-200 hover:bg-indigo-100"
							>
								<svg class="mr-1.5 h-3.5 w-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
								</svg>
								Edit
							</button>

							<button
								on:click={handleDelete}
								class="flex items-center rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 transition-all duration-200 hover:bg-red-100"
							>
								<svg class="mr-1.5 h-3.5 w-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
								</svg>
								Delete
							</button>
						</div>
					</div>
				</div>
			</div>
		{/if}
	</div>
</div>

<style>
	@keyframes fade-in {
		from {
			opacity: 0;
		}
		to {
			opacity: 1;
		}
	}

	@keyframes modal-enter {
		from {
			opacity: 0;
			transform: scale(0.95) translateY(-20px);
		}
		to {
			opacity: 1;
			transform: scale(1) translateY(0);
		}
	}

	.animate-fade-in {
		animation: fade-in 0.2s ease-out;
	}

	.animate-modal-enter {
		animation: modal-enter 0.3s cubic-bezier(0.4, 0, 0.2, 1);
	}

	/* Password strength colors */
	.bg-red-500 {
		background-color: #ef4444;
	}
	.bg-yellow-500 {
		background-color: #eab308;
	}
	.bg-blue-500 {
		background-color: #3b82f6;
	}
	.bg-green-500 {
		background-color: #22c55e;
	}
	.bg-gray-500 {
		background-color: #6b7280;
	}

	.text-red-600 {
		color: #dc2626;
	}
	.text-yellow-600 {
		color: #d97706;
	}
	.text-blue-600 {
		color: #2563eb;
	}
	.text-green-600 {
		color: #16a34a;
	}
	.text-gray-600 {
		color: #4b5563;
	}
</style>
