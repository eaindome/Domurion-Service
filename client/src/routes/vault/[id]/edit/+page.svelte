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

	// Load existing entry data
	onMount(async () => {
		try {
			const entry = await getVaultEntry(entryId);
			if (entry) {
				formData = { ...entry };
				originalData = { ...entry };
			} else {
				// Entry not found or error
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
			if (confirm('You have unsaved changes. Are you sure you want to leave?')) {
				goto('/dashboard');
			}
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
				<div class="w-24"></div>
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
							class="w-full sm:w-auto px-4 py-2 text-sm font-medium text-red-600 bg-white border border-red-300 rounded-lg hover:bg-red-50 focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
						>
							Delete Entry
						</button>
					</div>
				</div>
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