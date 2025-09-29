<script lang="ts">
	// eslint-disable-next-line svelte/no-navigation-without-resolve
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { onMount, onDestroy } from 'svelte';
	import { toast, type ToastType } from '$lib/stores/toast';
	import { deleteVaultEntry, updateVaultEntry, getVaultEntry } from '$lib/api/vault';
	import { validateVaultEntry } from '$lib/validation/validations';
	import VaultEntryForm from '$lib/components/VaultEntryForm.svelte';
	import { authStore } from '../../../../lib/stores/authStore';

	$: userId = $authStore.user?.id ?? '';

	// Get entry ID from URL params
	$: entryId = $page.params.id ?? '';

	// Form data
	let formData: {
		site: string;
		siteUrl?: string;
		email: string;
		password: string;
		notes?: string;
	} = { site: '', siteUrl: '', email: '', password: '', notes: '' };

	let originalData = {};
	let isSubmitting = false;
	let isLoading = true;
	let isDeleting = false;
	let errors: { siteName?: string; username?: string; password?: string } = {};
	let showDeleteConfirm = false;
	let showUnsavedConfirm = false;

	// Load existing entry data
	onMount(async () => {
		try {
			const result = await getVaultEntry(entryId);
			console.log(`Load entry result: ${JSON.stringify(result)}`);
			if (result.success && result.entry) {
				formData = { ...result.entry };
				console.log(`Form data loaded: ${JSON.stringify(formData)}`);
				originalData = { ...result.entry };
				console.log(`Original data set: ${JSON.stringify(originalData)}`);
			} else {
				// Entry not found or error
				toast.show('Entry not found', 'error');
				// setTimeout(() => goto('/dashboard'), 2000);
			}
		} catch (error) {
			console.error('Error loading entry:', error);
			// goto('/dashboard');
		} finally {
			isLoading = false;
		}
	});

	// Check if form has changes
	$: hasChanges = JSON.stringify(formData) !== JSON.stringify(originalData);

	// Handle form submission
	async function handleSubmit() {
		errors = validateVaultEntry(formData);
		if (Object.keys(errors).length > 0) return;
		isSubmitting = true;
		try {
			const result = await updateVaultEntry(
				entryId,
				formData.site,
				formData.siteUrl,
				formData.email,
				formData.password,
				formData.notes
			);
			if (result.success) {
				toast.show('Entry updated successfully', 'success');
				goto('/dashboard');
			} else {
				toast.show('Failed to update entry', 'error');
				console.error('Failed to update entry:', result.error);
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
			const result = await deleteVaultEntry(entryId);
			if (result.success) {
				toast.show('Entry deleted successfully', 'success');
				goto('/dashboard');
			} else {
				toast.show('Error deleting entry', 'error');
				console.error('Failed to delete entry:', result.error);
			}
		} catch (error) {
			toast.show('An error occurred while deleting the entry', 'error');
			console.error('Error deleting entry:', error);
		} finally {
			isDeleting = false;
			showDeleteConfirm = false;
		}
	}

	// Handle cancel with unsaved changes warning
	function handleCancel() {
		if (hasChanges) {
			showUnsavedConfirm = true;
		} else {
			goto('/dashboard');
		}
	}

	// Toast event handler from VaultEntryForm
	function handleToast(event: CustomEvent<{ message: string; type: ToastType }>) {
		toast.show(event.detail.message, event.detail.type);
	}
</script>

<svelte:head>
	<title>Edit Entry - {formData.site || 'Vault'}</title>
</svelte:head>

<div class="min-h-screen bg-gray-50">
	<!-- Navigation Header -->
	<header
		class="sticky top-0 z-30 border-b border-gray-200 bg-white/80 shadow-lg shadow-gray-200/60 backdrop-blur-md"
	>
		<div
			class="relative mx-auto flex h-16 max-w-2xl items-center justify-center px-4 sm:px-6 lg:px-8"
		>
			<button
				on:click={handleCancel}
				class="absolute top-1/2 left-0 flex -translate-y-1/2 items-center rounded-lg px-3 py-2 text-indigo-600 transition-colors hover:bg-indigo-50 hover:text-indigo-700 focus:ring-2 focus:ring-indigo-400 focus:outline-none"
				aria-label="Back to Vault"
			>
				<svg class="mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M15 19l-7-7 7-7"
					/>
				</svg>
				<span class="hidden sm:inline">Back to Vault</span>
			</button>
			<h1 class="w-full text-center text-lg font-semibold text-gray-900">
				{isLoading ? 'Loading...' : 'Edit Entry'}
			</h1>
		</div>
	</header>

	<!-- Main Content -->
	<main class="mx-auto max-w-2xl px-4 py-8 sm:px-6 lg:px-8">
		<div class="relative">
			{#if isLoading}
				<!-- Loading State -->
				<div class="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
					<div class="animate-pulse">
						<div class="space-y-6">
							<div class="h-4 w-1/4 rounded bg-gray-200"></div>
							<div class="h-12 rounded bg-gray-200"></div>
							<div class="h-4 w-1/4 rounded bg-gray-200"></div>
							<div class="h-12 rounded bg-gray-200"></div>
							<div class="h-4 w-1/4 rounded bg-gray-200"></div>
							<div class="h-12 rounded bg-gray-200"></div>
						</div>
					</div>
				</div>
			{:else}
				<div
					class="animate-fadein translate-y-6 transform overflow-hidden rounded-xl border border-gray-200 bg-white via-gray-50 to-indigo-50 opacity-0 shadow-xl transition-all duration-500"
					style="animation: fadein-slide 0.6s cubic-bezier(0.4,0,0.2,1) forwards;"
				>
					<div class="space-y-6 p-6">
						<VaultEntryForm
							{formData}
							{errors}
							{isSubmitting}
							mode="edit"
							onSubmit={handleSubmit}
							onCancel={handleCancel}
							on:toast={handleToast}
						/>

						<!-- Unsaved Changes Indicator -->
						{#if hasChanges}
							<div
								class="flex items-center justify-center space-x-2 rounded-lg border border-amber-200 bg-amber-50 px-4 py-2 text-sm text-amber-600"
							>
								<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path
										stroke-linecap="round"
										stroke-linejoin="round"
										stroke-width="2"
										d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z"
									/>
								</svg>
								<span>You have unsaved changes</span>
							</div>
						{/if}

						<!-- Delete Button -->
						<div class="border-t border-gray-200 pt-4">
							<button
								type="button"
								on:click={() => (showDeleteConfirm = true)}
								class="w-full rounded-lg border border-red-600 bg-red-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:border-red-700 hover:bg-red-700 focus:ring-2 focus:ring-red-500 focus:ring-offset-2 sm:w-auto"
							>
								Delete Entry
							</button>
						</div>
					</div>
				</div>
				{#if isSubmitting}
					<div
						class="absolute inset-0 z-10 flex items-center justify-center bg-white/60 backdrop-blur-sm"
					>
						<svg class="h-8 w-8 animate-spin text-indigo-500" fill="none" viewBox="0 0 24 24">
							<circle
								class="opacity-25"
								cx="12"
								cy="12"
								r="10"
								stroke="currentColor"
								stroke-width="4"
							></circle>
							<path
								class="opacity-75"
								fill="currentColor"
								d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
							></path>
						</svg>
					</div>
				{/if}
			{/if}
		</div>
	</main>
	<style>
		@keyframes fadein-slide {
			from {
				opacity: 0;
				transform: translateY(24px);
			}
			to {
				opacity: 1;
				transform: translateY(0);
			}
		}
		.animate-fadein {
			animation: fadein-slide 0.6s cubic-bezier(0.4, 0, 0.2, 1) forwards;
		}
	</style>
</div>

<!-- Unsaved Changes Confirmation Modal -->
{#if showUnsavedConfirm}
	<div class="fixed inset-0 z-50 flex items-center justify-center">
		<div class="absolute inset-0 bg-gray-200/60 backdrop-blur-md transition-all"></div>
		<div class="relative mx-auto w-full max-w-md p-0">
			<div
				class="animate-fadein-modal flex flex-col items-center rounded-2xl border border-gray-100 bg-gradient-to-br from-white/90 via-gray-50/90 to-indigo-50/80 px-10 py-9 shadow-2xl backdrop-blur-xl"
			>
				<div class="mb-5 flex items-center space-x-3">
					<div class="flex-shrink-0">
						<svg
							class="h-9 w-9 text-amber-500"
							fill="none"
							viewBox="0 0 24 24"
							stroke="currentColor"
						>
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M12 9v3m0 4h.01M10.29 3.86l-7.09 12.42A2 2 0 005.18 19h13.64a2 2 0 001.98-2.72l-7.09-12.42a2 2 0 00-3.42 0z"
							/>
						</svg>
					</div>
					<h3 class="text-xl font-bold tracking-tight text-gray-900">Unsaved Changes</h3>
				</div>
				<p class="mb-7 text-center text-base text-gray-700">
					You have unsaved changes. Are you sure you want to leave this page?
				</p>
				<div
					class="mt-2 flex w-full flex-col-reverse space-y-3 space-y-reverse sm:flex-row sm:justify-end sm:space-y-0 sm:space-x-3"
				>
					<button
						on:click={() => (showUnsavedConfirm = false)}
						class="w-full rounded-lg border border-gray-200 bg-white/90 px-5 py-2.5 text-base font-medium text-gray-700 shadow-sm transition-colors hover:bg-gray-50 focus:ring-2 focus:ring-indigo-400 focus:ring-offset-2 sm:w-auto"
					>
						Stay on Page
					</button>
					<button
						on:click={() => {
							showUnsavedConfirm = false;
							// eslint-disable-next-line svelte/no-navigation-without-resolve
							goto('/dashboard');
						}}
						class="w-full rounded-lg bg-amber-500 px-5 py-2.5 text-base font-semibold text-white shadow-md transition-colors hover:bg-amber-600 focus:ring-2 focus:ring-amber-400 focus:ring-offset-2 sm:w-auto"
					>
						Leave Anyway
					</button>
				</div>
			</div>
		</div>
	</div>
{/if}

<!-- Delete Confirmation Modal -->
{#if showDeleteConfirm}
	<div class="fixed inset-0 z-50 flex items-center justify-center">
		<!-- Glassmorphism/blurred overlay -->
		<div class="absolute inset-0 bg-gray-200/60 backdrop-blur-md transition-all"></div>
		<div class="relative mx-auto w-full max-w-md p-0">
			<div
				class="animate-fadein-modal flex flex-col items-center rounded-2xl border border-gray-100 bg-gradient-to-br from-white/90 via-gray-50/90 to-indigo-50/80 px-10 py-9 shadow-2xl backdrop-blur-xl"
			>
				<div class="mb-5 flex items-center space-x-3">
					<div class="flex-shrink-0">
						<svg
							class="h-10 w-10 text-red-500"
							fill="none"
							stroke="currentColor"
							viewBox="0 0 24 24"
						>
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z"
							/>
						</svg>
					</div>
					<h3 class="text-xl font-bold tracking-tight text-gray-900">Delete Entry</h3>
				</div>
				<p class="mb-7 text-center text-base text-gray-700">
					Are you sure you want to delete <span class="font-semibold text-gray-900"
						>"{formData.site}"</span
					>?<br />
					<span class="font-semibold text-red-600">This action cannot be undone.</span>
				</p>
				<div
					class="mt-2 flex w-full flex-col-reverse space-y-3 space-y-reverse sm:flex-row sm:justify-end sm:space-y-0 sm:space-x-3"
				>
					<button
						on:click={() => (showDeleteConfirm = false)}
						class="w-full rounded-lg border border-gray-200 bg-white/90 px-5 py-2.5 text-base font-medium text-gray-700 shadow-sm transition-colors hover:bg-gray-50 focus:ring-2 focus:ring-indigo-400 focus:ring-offset-2 sm:w-auto"
					>
						Cancel
					</button>
					<button
						on:click={handleDelete}
						disabled={isDeleting}
						class="w-full rounded-lg bg-red-600 px-5 py-2.5 text-base font-semibold text-white shadow-md transition-colors hover:bg-red-700 focus:ring-2 focus:ring-red-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 sm:w-auto"
					>
						{#if isDeleting}
							<span class="flex items-center justify-center">
								<svg
									class="mr-2 -ml-1 h-5 w-5 animate-spin text-white"
									fill="none"
									viewBox="0 0 24 24"
								>
									<circle
										class="opacity-25"
										cx="12"
										cy="12"
										r="10"
										stroke="currentColor"
										stroke-width="4"
									></circle>
									<path
										class="opacity-75"
										fill="currentColor"
										d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
									></path>
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
		<style>
			@keyframes fadein-modal {
				from {
					opacity: 0;
					transform: translateY(24px) scale(0.98);
				}
				to {
					opacity: 1;
					transform: translateY(0) scale(1);
				}
			}
			.animate-fadein-modal {
				animation: fadein-modal 0.4s cubic-bezier(0.4, 0, 0.2, 1) forwards;
			}
		</style>
	</div>
{/if}
