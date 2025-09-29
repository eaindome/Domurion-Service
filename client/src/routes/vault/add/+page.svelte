<script lang="ts">
	// eslint-disable-next-line svelte/no-navigation-without-resolve
	import { goto } from '$app/navigation';
	import { addVaultEntry } from '$lib/api/vault';
	import { authStore } from '$lib/stores/authStore';
	import type { VaultEntryErrors } from '$lib/types';
	import { validateVaultEntry } from '$lib/validation/validations';
	import { toast } from '$lib/stores/toast';
	import VaultEntryForm from '$lib/components/VaultEntryForm.svelte';

	// Form data
	let formData = {
		site: '',
		siteUrl: '',
		email: '',
		password: '',
		notes: ''
	};

	let isSubmitting = false;
	let errors: VaultEntryErrors = {};

	// Handle form submission
	async function handleSubmit() {
		console.log(`Submitting`);
		errors = validateVaultEntry(formData);
		if (Object.keys(errors).length > 0) return;
		isSubmitting = true;
		console.log(`Form data: ${JSON.stringify(formData)}`);
		console.log(`isSubmitting: ${isSubmitting}`);
		console.log('Getting user id');
		let userId: string | undefined;
		const unsubscribe = authStore.subscribe((state) => {
			userId = state.user?.id;
		});
		console.log(`userId: ${userId}`);
		unsubscribe();
		if (!userId) {
			toast.show('User not authenticated', 'error');
			isSubmitting = false;
			return;
		}
		console.log(`Adding vault entry for userId: ${userId}`);
		try {
			const { site, siteUrl, email, password, notes } = formData;
			const result = await addVaultEntry(site, email, password, notes, siteUrl);
			if (result.success) {
				toast.show('Entry saved successfully', 'success');
				goto('/dashboard');
			} else {
				toast.show(result.error || 'Failed to save entry', 'error');
				console.error('Failed to save entry', result.error);
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
			<h1 class="w-full text-center text-lg font-semibold text-gray-900">Add New Entry</h1>
		</div>
	</header>
	<!-- Main Content -->
	<main class="mx-auto max-w-2xl px-4 py-8 sm:px-6 lg:px-8">
		<div class="relative">
			<div
				class="animate-fadein translate-y-6 transform overflow-hidden rounded-xl border border-gray-200 bg-white via-gray-50 to-indigo-50 opacity-0 shadow-xl transition-all duration-500"
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
				<div
					class="absolute inset-0 z-10 flex items-center justify-center bg-white/60 backdrop-blur-sm"
				>
					<svg class="h-8 w-8 animate-spin text-indigo-500" fill="none" viewBox="0 0 24 24">
						<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"
						></circle>
						<path
							class="opacity-75"
							fill="currentColor"
							d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
						></path>
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
			animation: fadein-slide 0.6s cubic-bezier(0.4, 0, 0.2, 1) forwards;
		}
	</style>
</div>
