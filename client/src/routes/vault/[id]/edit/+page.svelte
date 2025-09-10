<script lang=ts>
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { onMount } from 'svelte';

	// Get entry ID from URL params
	$: entryId = $page.params.id;

	// Form data
	let formData = {
		siteName: '',
		url: '',
		username: '',
		password: '',
		notes: ''
	};

	let originalData = {};
	let showPassword = false;
	let isSubmitting = false;
	let isLoading = true;
	let isDeleting = false;
	let errors: { siteName?: string; username?: string; password?: string } = {};
	let showDeleteConfirm = false;

	// Load existing entry data
	onMount(async () => {
		try {
			const response = await fetch(`/api/vault/${entryId}`);
			
			if (response.ok) {
				const entry = await response.json();
				formData = { ...entry };
				originalData = { ...entry };
			} else if (response.status === 404) {
				// Entry not found, redirect to dashboard
				goto('/dashboard');
			} else {
				console.error('Failed to load entry');
				goto('/dashboard');
			}
		} catch (error) {
			console.error('Error loading entry:', error);
			goto('/dashboard');
		} finally {
			isLoading = false;
		}
	});

	// Check if form has changes
	$: hasChanges = JSON.stringify(formData) !== JSON.stringify(originalData);

	// Generate password functionality
	function generatePassword() {
		const length = 16;
		const charset = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*';
		let password = '';
		for (let i = 0; i < length; i++) {
			password += charset.charAt(Math.floor(Math.random() * charset.length));
		}
		formData.password = password;
	}

	// Form validation
	function validateForm() {
		errors = {};
		
		if (!formData.siteName.trim()) {
			errors.siteName = 'Site name is required';
		}
		
		if (!formData.username.trim()) {
			errors.username = 'Username is required';
		}
		
		if (!formData.password.trim()) {
			errors.password = 'Password is required';
		}

		return Object.keys(errors).length === 0;
	}

	// Handle form submission
	async function handleSubmit() {
		if (!validateForm()) return;
		
		isSubmitting = true;
		
		try {
			const response = await fetch(`/api/vault/${entryId}`, {
				method: 'PUT',
				headers: {
					'Content-Type': 'application/json',
				},
				body: JSON.stringify(formData)
			});

			if (response.ok) {
				// Success - redirect to dashboard
				goto('/dashboard');
			} else {
				// Handle error
				const error = await response.json();
				console.error('Failed to update entry:', error);
			}
		} catch (error) {
			console.error('Error updating entry:', error);
		} finally {
			isSubmitting = false;
		}
	}

	// Handle delete
	async function handleDelete() {
		isDeleting = true;
		
		try {
			const response = await fetch(`/api/vault/${entryId}`, {
				method: 'DELETE'
			});

			if (response.ok) {
				// Success - redirect to dashboard
				goto('/dashboard');
			} else {
				console.error('Failed to delete entry');
			}
		} catch (error) {
			console.error('Error deleting entry:', error);
		} finally {
			isDeleting = false;
			showDeleteConfirm = false;
		}
	}

	// Handle cancel with unsaved changes warning
	function handleCancel() {
		if (hasChanges) {
			if (confirm('You have unsaved changes. Are you sure you want to leave?')) {
				goto('/dashboard');
			}
		} else {
			goto('/dashboard');
		}
	}

	// Copy password to clipboard
	async function copyPassword() {
		try {
			await navigator.clipboard.writeText(formData.password);
			// You could add a toast notification here
		} catch (err) {
			console.error('Failed to copy password:', err);
		}
	}
</script>

<svelte:head>
	<title>Edit Entry - {formData.siteName || 'Vault'}</title>
</svelte:head>

<div class="min-h-screen bg-gray-50">
	<!-- Navigation Header -->
	<header class="bg-white border-b border-gray-200">
		<div class="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8">
			<div class="flex items-center justify-between h-16">
				<button
					on:click={handleCancel}
					class="flex items-center text-indigo-600 hover:text-indigo-700 transition-colors"
				>
					<svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
					</svg>
					Back to Vault
				</button>
				
				<h1 class="text-lg font-semibold text-gray-900">
					{isLoading ? 'Loading...' : 'Edit Entry'}
				</h1>
				
				<div class="w-24"></div> <!-- Spacer for centering -->
			</div>
		</div>
	</header>

	<!-- Main Content -->
	<main class="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
		{#if isLoading}
			<!-- Loading State -->
			<div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
				<div class="animate-pulse">
					<div class="space-y-6">
						<div class="h-4 bg-gray-200 rounded w-1/4"></div>
						<div class="h-12 bg-gray-200 rounded"></div>
						<div class="h-4 bg-gray-200 rounded w-1/4"></div>
						<div class="h-12 bg-gray-200 rounded"></div>
						<div class="h-4 bg-gray-200 rounded w-1/4"></div>
						<div class="h-12 bg-gray-200 rounded"></div>
					</div>
				</div>
			</div>
		{:else}
			<div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
				<form on:submit|preventDefault={handleSubmit} class="p-6 space-y-6">
					<!-- Site Name -->
					<div>
						<label for="siteName" class="block text-sm font-medium text-gray-700 mb-2">
							Site Name *
						</label>
						<input
							type="text"
							id="siteName"
							bind:value={formData.siteName}
							placeholder="e.g., Google, GitHub, Facebook"
							class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors bg-white"
							class:border-red-300={errors.siteName}
							class:focus:ring-red-500={errors.siteName}
							class:focus:border-red-500={errors.siteName}
						/>
						{#if errors.siteName}
							<p class="mt-1 text-sm text-red-600">{errors.siteName}</p>
						{/if}
					</div>

					<!-- Website URL -->
					<div>
						<label for="url" class="block text-sm font-medium text-gray-700 mb-2">
							Website URL
							<span class="text-gray-500 font-normal">(optional)</span>
						</label>
						<input
							type="url"
							id="url"
							bind:value={formData.url}
							placeholder="https://example.com"
							class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors bg-white"
						/>
					</div>

					<!-- Username/Email -->
					<div>
						<label for="username" class="block text-sm font-medium text-gray-700 mb-2">
							Username/Email *
						</label>
						<input
							type="text"
							id="username"
							bind:value={formData.username}
							placeholder="your-username or email@example.com"
							class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors bg-white"
							class:border-red-300={errors.username}
							class:focus:ring-red-500={errors.username}
							class:focus:border-red-500={errors.username}
						/>
						{#if errors.username}
							<p class="mt-1 text-sm text-red-600">{errors.username}</p>
						{/if}
					</div>

					<!-- Password -->
					<div>
						<label for="password" class="block text-sm font-medium text-gray-700 mb-2">
							Password *
						</label>
						<div class="relative">
							<input
								type={showPassword ? 'text' : 'password'}
								id="password"
								bind:value={formData.password}
								placeholder="Enter or generate a secure password"
								class="w-full px-4 py-3 pr-20 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors bg-white"
								class:border-red-300={errors.password}
								class:focus:ring-red-500={errors.password}
								class:focus:border-red-500={errors.password}
							/>
							<div class="absolute inset-y-0 right-0 flex items-center space-x-1 pr-3">
								<button
									type="button"
									on:click={() => showPassword = !showPassword}
									class="p-1 text-gray-400 hover:text-gray-600 transition-colors"
									title={showPassword ? 'Hide password' : 'Show password'}
								>
									{#if showPassword}
										<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21" />
										</svg>
									{:else}
										<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
										</svg>
									{/if}
								</button>
								
								{#if formData.password}
									<button
										type="button"
										on:click={copyPassword}
										class="p-1 text-gray-400 hover:text-gray-600 transition-colors"
										title="Copy password"
										aria-label="Copy password"
									>
										<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
										</svg>
									</button>
								{/if}
							</div>
						</div>
						
						{#if errors.password}
							<p class="mt-1 text-sm text-red-600">{errors.password}</p>
						{/if}
						
						<button
							type="button"
							on:click={generatePassword}
							class="mt-2 text-sm text-indigo-600 hover:text-indigo-700 font-medium transition-colors"
						>
							Generate new password
						</button>
					</div>

					<!-- Notes -->
					<div>
						<label for="notes" class="block text-sm font-medium text-gray-700 mb-2">
							Notes
							<span class="text-gray-500 font-normal">(optional)</span>
						</label>
						<textarea
							id="notes"
							bind:value={formData.notes}
							placeholder="Additional information or security questions..."
							rows="3"
							class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors bg-white resize-none"
						></textarea>
					</div>

					<!-- Action Buttons -->
					<div class="flex flex-col sm:flex-row sm:justify-between items-center space-y-3 sm:space-y-0 pt-4 border-t border-gray-200">
						<!-- Delete Button (Left side) -->
						<button
							type="button"
							on:click={() => showDeleteConfirm = true}
							class="w-full sm:w-auto px-4 py-2 text-sm font-medium text-red-600 bg-white border border-red-300 rounded-lg hover:bg-red-50 focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
						>
							Delete Entry
						</button>

						<!-- Save/Cancel Buttons (Right side) -->
						<div class="flex flex-col-reverse sm:flex-row w-full sm:w-auto sm:space-x-3 space-y-3 space-y-reverse sm:space-y-0">
							<button
								type="button"
								on:click={handleCancel}
								class="w-full sm:w-auto px-6 py-3 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors"
							>
								Cancel
							</button>
							
							<button
								type="submit"
								disabled={isSubmitting || !hasChanges}
								class="w-full sm:w-auto px-6 py-3 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
							>
								{#if isSubmitting}
									<span class="flex items-center justify-center">
										<svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
											<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
											<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
										</svg>
										Saving...
									</span>
								{:else}
									{hasChanges ? 'Save Changes' : 'No Changes'}
								{/if}
							</button>
						</div>
					</div>

					<!-- Unsaved Changes Indicator -->
					{#if hasChanges}
						<div class="flex items-center justify-center space-x-2 text-sm text-amber-600 bg-amber-50 rounded-lg py-2 px-4 border border-amber-200">
							<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z" />
							</svg>
							<span>You have unsaved changes</span>
						</div>
					{/if}
				</form>
			</div>
		{/if}
	</main>
</div>

<!-- Delete Confirmation Modal -->
{#if showDeleteConfirm}
	<div class="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center p-4 z-50">
		<div class="bg-white rounded-xl shadow-xl max-w-md w-full p-6">
			<div class="flex items-center space-x-3 mb-4">
				<div class="flex-shrink-0">
					<svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z" />
					</svg>
				</div>
				<h3 class="text-lg font-medium text-gray-900">Delete Entry</h3>
			</div>
			
			<p class="text-sm text-gray-500 mb-6">
				Are you sure you want to delete "<strong>{formData.siteName}</strong>"? This action cannot be undone.
			</p>
			
			<div class="flex flex-col-reverse sm:flex-row sm:space-x-3 space-y-3 space-y-reverse sm:space-y-0">
				<button
					on:click={() => showDeleteConfirm = false}
					class="w-full sm:w-auto px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors"
				>
					Cancel
				</button>
				
				<button
					on:click={handleDelete}
					disabled={isDeleting}
					class="w-full sm:w-auto px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
				>
					{#if isDeleting}
						<span class="flex items-center justify-center">
							<svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
								<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
								<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
							</svg>
							Deleting...
						</span>
					{:else}
						Delete Entry
					{/if}
				</button>
			</div>
		</div>
	</div>
{/if}

<style>
	/* Custom focus styles for better accessibility */
	input:focus, textarea:focus {
		outline: none;
	}
	
	/* Smooth transitions for all interactive elements */
	input, textarea, button {
		transition: all 0.2s ease-in-out;
	}
	
	/* iOS-style input styling */
	input, textarea {
		-webkit-appearance: none;
		appearance: none;
	}
</style>