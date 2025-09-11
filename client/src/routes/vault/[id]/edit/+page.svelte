<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import { toast, type ToastType } from '$lib/stores/toast';
	import { deleteVaultEntry, updateVaultEntry, getVaultEntry } from '$lib/api/vault';
	import { validateVaultEntry } from '$lib/validation/validations';
	import VaultEntryForm from '$lib/components/VaultEntryForm.svelte';

	// Get entry ID from URL params
	$: entryId = $page.params.id ?? '';

	// Form data
	let formData: {
		siteName: string;
		url?: string;
		username: string;
		password: string;
		notes?: string;
	} = { siteName: '', url: '', username: '', password: '', notes: '' };

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
			const entry = await getVaultEntry(entryId);
			if (entry) {
				formData = { ...entry };
				originalData = { ...entry };
			} else {
				// Entry not found or error
				// goto('/dashboard');
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
			const result = await updateVaultEntry(entryId, formData);
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
	<title>Edit Entry - {formData.siteName || 'Vault'}</title>
</svelte:head>

<div class="min-h-screen bg-gray-50">
	<!-- Navigation Header -->
	<header class="bg-white/80 backdrop-blur-md border-b border-gray-200 shadow-lg shadow-gray-200/60 sticky top-0 z-30">
		<div class="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 relative h-16 flex items-center justify-center">
			<button
				on:click={handleCancel}
				class="absolute left-0 top-1/2 -translate-y-1/2 flex items-center px-3 py-2 text-indigo-600 hover:text-indigo-700 rounded-lg hover:bg-indigo-50 focus:outline-none focus:ring-2 focus:ring-indigo-400 transition-colors"
				aria-label="Back to Vault"
			>
				<svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
				</svg>
				<span class="hidden sm:inline">Back to Vault</span>
			</button>
			<h1 class="text-lg font-semibold text-gray-900 text-center w-full">
				{isLoading ? 'Loading...' : 'Edit Entry'}
			</h1>
		</div>
	</header>

	<!-- Main Content -->
		<main class="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
			<div class="relative">
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
					<div
						class="bg-white via-gray-50 to-indigo-50 rounded-xl shadow-xl border border-gray-200 overflow-hidden transform transition-all duration-500 opacity-0 translate-y-6 animate-fadein"
						style="animation: fadein-slide 0.6s cubic-bezier(0.4,0,0.2,1) forwards;"
					>
						<div class="p-6 space-y-6">
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
								<div class="flex items-center justify-center space-x-2 text-sm text-amber-600 bg-amber-50 rounded-lg py-2 px-4 border border-amber-200">
									<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z" />
									</svg>
									<span>You have unsaved changes</span>
								</div>
							{/if}

							<!-- Delete Button -->
							<div class="pt-4 border-t border-gray-200">
								<button
									type="button"
									on:click={() => showDeleteConfirm = true}
									class="w-full sm:w-auto px-4 py-2 text-sm font-medium text-white bg-red-600 border border-red-600 rounded-lg hover:bg-red-700 hover:border-red-700 focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
								>
									Delete Entry
								</button>
							</div>
						</div>
					</div>
					{#if isSubmitting}
						<div class="absolute inset-0 flex items-center justify-center bg-white/60 backdrop-blur-sm z-10">
							<svg class="animate-spin h-8 w-8 text-indigo-500" fill="none" viewBox="0 0 24 24">
								<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
								<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
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
	animation: fadein-slide 0.6s cubic-bezier(0.4,0,0.2,1) forwards;
}
</style>
</div>

<!-- Unsaved Changes Confirmation Modal -->
{#if showUnsavedConfirm}
	<div class="fixed inset-0 z-50 flex items-center justify-center">
		<div class="absolute inset-0 bg-gray-200/60 backdrop-blur-md transition-all"></div>
		<div class="relative max-w-md w-full mx-auto p-0">
			<div class="bg-gradient-to-br from-white/90 via-gray-50/90 to-indigo-50/80 rounded-2xl shadow-2xl border border-gray-100 backdrop-blur-xl px-10 py-9 flex flex-col items-center animate-fadein-modal">
				<div class="flex items-center space-x-3 mb-5">
					<div class="flex-shrink-0">
						<svg class="w-9 h-9 text-amber-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v3m0 4h.01M10.29 3.86l-7.09 12.42A2 2 0 005.18 19h13.64a2 2 0 001.98-2.72l-7.09-12.42a2 2 0 00-3.42 0z" />
						</svg>
					</div>
					<h3 class="text-xl font-bold text-gray-900 tracking-tight">Unsaved Changes</h3>
				</div>
				<p class="text-center text-base text-gray-700 mb-7">
					You have unsaved changes. Are you sure you want to leave this page?
				</p>
				<div class="flex flex-col-reverse sm:flex-row sm:justify-end sm:space-x-3 space-y-3 space-y-reverse sm:space-y-0 w-full mt-2">
					<button
						on:click={() => showUnsavedConfirm = false}
						class="w-full sm:w-auto px-5 py-2.5 text-base font-medium text-gray-700 bg-white/90 border border-gray-200 rounded-lg hover:bg-gray-50 focus:ring-2 focus:ring-offset-2 focus:ring-indigo-400 transition-colors shadow-sm"
					>
						Stay on Page
					</button>
					<button
						on:click={() => { showUnsavedConfirm = false; goto('/dashboard'); }}
						class="w-full sm:w-auto px-5 py-2.5 text-base font-semibold text-white bg-amber-500 rounded-lg hover:bg-amber-600 focus:ring-2 focus:ring-offset-2 focus:ring-amber-400 transition-colors shadow-md"
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
		<div class="relative max-w-md w-full mx-auto p-0">
			<div class="bg-gradient-to-br from-white/90 via-gray-50/90 to-indigo-50/80 rounded-2xl shadow-2xl border border-gray-100 backdrop-blur-xl px-10 py-9 flex flex-col items-center animate-fadein-modal">
				<div class="flex items-center space-x-3 mb-5">
					<div class="flex-shrink-0">
						<svg class="w-10 h-10 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z" />
						</svg>
					</div>
					<h3 class="text-xl font-bold text-gray-900 tracking-tight">Delete Entry</h3>
				</div>
				<p class="text-center text-base text-gray-700 mb-7">
					Are you sure you want to delete <span class="font-semibold text-gray-900">"{formData.siteName}"</span>?<br />
					<span class="text-red-600 font-semibold">This action cannot be undone.</span>
				</p>
				<div class="flex flex-col-reverse sm:flex-row sm:justify-end sm:space-x-3 space-y-3 space-y-reverse sm:space-y-0 w-full mt-2">
					<button
						on:click={() => showDeleteConfirm = false}
						class="w-full sm:w-auto px-5 py-2.5 text-base font-medium text-gray-700 bg-white/90 border border-gray-200 rounded-lg hover:bg-gray-50 focus:ring-2 focus:ring-offset-2 focus:ring-indigo-400 transition-colors shadow-sm"
					>
						Cancel
					</button>
					<button
						on:click={handleDelete}
						disabled={isDeleting}
						class="w-full sm:w-auto px-5 py-2.5 text-base font-semibold text-white bg-red-600 rounded-lg hover:bg-red-700 focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors shadow-md"
					>
						{#if isDeleting}
							<span class="flex items-center justify-center">
								<svg class="animate-spin -ml-1 mr-2 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
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
		<style>
			@keyframes fadein-modal {
				from { opacity: 0; transform: translateY(24px) scale(0.98); }
				to { opacity: 1; transform: translateY(0) scale(1); }
			}
			.animate-fadein-modal {
				animation: fadein-modal 0.4s cubic-bezier(0.4,0,0.2,1) forwards;
			}
		</style>
	</div>
{/if}