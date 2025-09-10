<script lang=ts>
	import { goto } from '$app/navigation';
	import { createVaultEntry } from '$lib/api/vault';
	import type { VaultEntryErrors } from '$lib/types';
	import { validateVaultEntry } from '$lib/validation/validations';
	import { toast } from '$lib/stores/toast';
	import VaultEntryForm from '$lib/components/VaultEntryForm.svelte';

	// Form data
	let formData = {
		siteName: '',
		url: '',
		username: '',
		password: '',
		notes: ''
	};

	let isSubmitting = false;
	let errors: VaultEntryErrors = {};

	// Handle form submission
	async function handleSubmit() {
		errors = validateVaultEntry(formData);
		if (Object.keys(errors).length > 0) return;
		isSubmitting = true;
		try {
			const success = await createVaultEntry(formData);
			if (success) {
				toast.show('Entry saved successfully', 'success');
				goto('/dashboard');
			} else {
				toast.show('Failed to save entry', 'error');
				console.error('Failed to save entry');
			}
		} catch (error) {
			toast.show('An error occurred while saving the entry', 'error');
			console.error('Error saving entry:', error);
		} finally {
			isSubmitting = false;
		}
	}

	// Handle cancel
	function handleCancel() {
		goto('/dashboard');
	}
</script>

<svelte:head>
	<title>Add New Entry - Vault</title>
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
				<h1 class="text-lg font-semibold text-gray-900">Add New Entry</h1>
				<div class="w-24"></div>
			</div>
		</div>
	</header>
	<!-- Main Content -->
	<main class="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
		<div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
			<VaultEntryForm
				{formData}
				{errors}
				{isSubmitting}
				mode="add"
				onSubmit={handleSubmit}
				onCancel={handleCancel}
				on:toast={(e) => toast.show(e.detail.message, e.detail.type)}
			/>
		</div>
	</main>
</div>				
