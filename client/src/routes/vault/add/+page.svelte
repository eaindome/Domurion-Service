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
				<h1 class="text-lg font-semibold text-gray-900 text-center w-full">Add New Entry</h1>
			</div>
		</header>
	<!-- Main Content -->
	<main class="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
		<div class="relative">
			<div
				class="bg-white  via-gray-50 to-indigo-50 rounded-xl shadow-xl border border-gray-200 overflow-hidden transform transition-all duration-500 opacity-0 translate-y-6 animate-fadein"
				style="animation: fadein-slide 0.6s cubic-bezier(0.4,0,0.2,1) forwards;"
			>
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
			{#if isSubmitting}
				<div class="absolute inset-0 flex items-center justify-center bg-white/60 backdrop-blur-sm z-10">
					<svg class="animate-spin h-8 w-8 text-indigo-500" fill="none" viewBox="0 0 24 24">
						<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
						<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
					</svg>
				</div>
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
