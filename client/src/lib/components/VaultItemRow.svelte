<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import type { VaultItem } from '$lib/types';
	import { maskPassword, getSiteFavicon } from '$lib/utils/helpers';

	export let item: VaultItem;

	const dispatch = createEventDispatcher();

	let showDetails = false;

	// Password strength indicator state
	let passwordStrength = { color: 'gray', text: 'Unknown' };

	// Show/hide password in modal
	let showPassword = false;

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

	$: passwordStrength = getPasswordStrength(item?.password);

	function handleCopy(value: string, type: string) {
		dispatch('copy', { value, type });
		copiedField = type.toLowerCase();
		setTimeout(() => {
			if (copiedField === type.toLowerCase()) copiedField = null;
		}, 1200);
	}

	function copyToClipboard(value: string, type: string) {
		handleCopy(value, type);
	}

	import { goto } from '$app/navigation';
	function handleEdit() {
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

		<!-- Username -->
		<div class="flex items-center space-x-2">
			<span class="text-xs text-gray-500">Username:</span>
			<span class="truncate text-sm text-gray-900">{item.username}</span>
			<button
				on:click={() => handleCopy(item.username, 'Username')}
				class="text-gray-400 transition-colors hover:text-gray-600"
				title="Copy username"
				aria-label="Copy username"
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
				on:click={() => handleCopy(item.password, 'Password')}
				class="text-gray-400 transition-colors hover:text-gray-600"
				title="Copy password"
				aria-label="Copy password"
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
					class="animate-modal-enter relative w-full max-w-lg rounded-3xl border border-gray-100 bg-white shadow-2xl"
				>
					<!-- Header -->
					<div class="relative border-b border-gray-100 px-8 pt-8 pb-6">
						<button
							class="absolute top-4 right-4 rounded-full p-2 text-gray-400 transition-all duration-200 hover:bg-gray-100 hover:text-gray-600 focus:ring-2 focus:ring-indigo-500 focus:outline-none"
							on:click={closeModal}
							aria-label="Close details"
						>
							<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M6 18L18 6M6 6l12 12"
								/>
							</svg>
						</button>

						<div class="flex flex-col items-center text-center">
							<div class="relative mb-4">
								<img
									src={getSiteFavicon(item.siteUrl)}
									alt="{item.siteName} favicon"
									class="h-16 w-16 rounded-2xl border border-gray-200 shadow-md"
									on:error={(e) => {
										const target = e.target as HTMLImageElement | null;
										if (target) {
											target.src = `data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='64' height='64' viewBox='0 0 24 24' fill='none' stroke='%236366f1' stroke-width='2'%3E%3Cpath d='M21 16V8a2 2 0 0 0-1-1.73L12 2 4 6.27A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73L12 22l8-4.27A2 2 0 0 0 21 16z'/%3E%3Cpath d='M12 12l8-4.27L12 2 4 7.73z'/%3E%3C/svg%3E`;
										}
									}}
								/>
								<!-- Trust indicator -->
								<div
									class="absolute -right-1 -bottom-1 flex h-6 w-6 items-center justify-center rounded-full border-2 border-white bg-green-500"
								>
									<svg class="h-3 w-3 text-white" fill="currentColor" viewBox="0 0 20 20">
										<path
											fill-rule="evenodd"
											d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
											clip-rule="evenodd"
										/>
									</svg>
								</div>
							</div>

							<h2 class="mb-2 text-2xl font-bold text-gray-900">{item.siteName}</h2>
							<a
								href={item.siteUrl}
								target="_blank"
								rel="noopener noreferrer"
								class="inline-flex items-center text-sm text-indigo-600 transition-colors hover:text-indigo-700"
							>
								{item.siteUrl}
								<svg class="ml-1 h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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

					<!-- Content -->
					<div class="space-y-6 px-8 py-6">
						<!-- Username Field -->
						<div class="group">
							<div class="mb-2 flex items-center justify-between">
								<label for="username-field" class="text-sm font-medium text-gray-700"
									>Username</label
								>
								<button
									on:click={() => copyToClipboard(item.username, 'username')}
									class="flex items-center rounded-md px-2 py-1 text-xs text-gray-500 transition-all duration-200 hover:bg-indigo-50 hover:text-indigo-600"
								>
									{#if copiedField === 'username'}
										<svg
											class="mr-1 h-3 w-3 text-green-600"
											fill="currentColor"
											viewBox="0 0 20 20"
										>
											<path
												fill-rule="evenodd"
												d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
												clip-rule="evenodd"
											/>
										</svg>
										Copied!
									{:else}
										<svg class="mr-1 h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path
												stroke-linecap="round"
												stroke-linejoin="round"
												stroke-width="2"
												d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
											/>
										</svg>
										Copy
									{/if}
								</button>
							</div>
							<div class="relative">
								<input
									id="username-field"
									type="text"
									value={item.username}
									readonly
									class="w-full cursor-pointer rounded-xl border border-gray-200 bg-gray-50 px-4 py-3 text-gray-900 focus:ring-2 focus:ring-indigo-500 focus:outline-none"
									on:click={() => copyToClipboard(item.username, 'username')}
								/>
							</div>
						</div>

						<!-- Password Field -->
						<div class="group">
							<div class="mb-2 flex items-center justify-between">
								<label for="password-field" class="text-sm font-medium text-gray-700"
									>Password</label
								>
								<div class="flex items-center space-x-2">
									<!-- Password Strength Indicator -->
									<div class="flex items-center">
										<div class="mr-1 h-2 w-2 rounded-full bg-{passwordStrength.color}-500"></div>
										<span class="text-xs text-{passwordStrength.color}-600 font-medium"
											>{passwordStrength.text}</span
										>
									</div>
									<button
										on:click={() => copyToClipboard(item.password, 'password')}
										class="flex items-center rounded-md px-2 py-1 text-xs text-gray-500 transition-all duration-200 hover:bg-indigo-50 hover:text-indigo-600"
									>
										{#if copiedField === 'password'}
											<svg
												class="mr-1 h-3 w-3 text-green-600"
												fill="currentColor"
												viewBox="0 0 20 20"
											>
												<path
													fill-rule="evenodd"
													d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
													clip-rule="evenodd"
												/>
											</svg>
											Copied!
										{:else}
											<svg
												class="mr-1 h-3 w-3"
												fill="none"
												stroke="currentColor"
												viewBox="0 0 24 24"
											>
												<path
													stroke-linecap="round"
													stroke-linejoin="round"
													stroke-width="2"
													d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
												/>
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
									class="w-full cursor-pointer rounded-xl border border-gray-200 bg-gray-50 px-4 py-3 pr-12 font-mono text-gray-900 focus:ring-2 focus:ring-indigo-500 focus:outline-none"
									on:click={() => copyToClipboard(item.password, 'password')}
								/>
								<button
									on:click={() => (showPassword = !showPassword)}
									class="absolute top-1/2 right-3 -translate-y-1/2 transform p-1 text-gray-400 transition-colors hover:text-gray-600"
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
							</div>
						</div>

						<!-- Notes Field (if exists) -->
						{#if item.notes}
							<div class="group">
								<div class="mb-2 flex items-center justify-between">
									<label for="notes-field" class="text-sm font-medium text-gray-700">Notes</label>
									<button
										on:click={() => copyToClipboard(item.notes ?? '', 'notes')}
										class="flex items-center rounded-md px-2 py-1 text-xs text-gray-500 transition-all duration-200 hover:bg-indigo-50 hover:text-indigo-600"
									>
										{#if copiedField === 'notes'}
											<svg
												class="mr-1 h-3 w-3 text-green-600"
												fill="currentColor"
												viewBox="0 0 20 20"
											>
												<path
													fill-rule="evenodd"
													d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
													clip-rule="evenodd"
												/>
											</svg>
											Copied!
										{:else}
											<svg
												class="mr-1 h-3 w-3"
												fill="none"
												stroke="currentColor"
												viewBox="0 0 24 24"
											>
												<path
													stroke-linecap="round"
													stroke-linejoin="round"
													stroke-width="2"
													d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
												/>
											</svg>
											Copy
										{/if}
									</button>
								</div>
								<textarea
									id="notes-field"
									value={item.notes}
									readonly
									class="w-full cursor-pointer resize-none rounded-xl border border-gray-200 bg-gray-50 px-4 py-3 text-gray-900 focus:ring-2 focus:ring-indigo-500 focus:outline-none"
									rows="3"
									on:click={() => copyToClipboard(item.notes ?? '', 'notes')}
								></textarea>
							</div>
						{/if}

						<!-- Metadata -->
						<div class="grid grid-cols-2 gap-4 border-t border-gray-100 pt-4">
							<div class="text-center">
								<div class="mb-1 text-xs font-medium text-gray-500">Created</div>
								<div class="text-sm text-gray-900">{item.createdAt}</div>
							</div>
							<div class="text-center">
								<div class="mb-1 text-xs font-medium text-gray-500">Last Updated</div>
								<div class="text-sm text-gray-900">{item.updatedAt}</div>
							</div>
						</div>
					</div>

					<!-- Actions Footer -->
					<div class="rounded-b-3xl border-t border-gray-100 bg-gray-50 px-8 py-6">
						<div class="flex items-center justify-between">
							<button
								on:click={handleEdit}
								class="flex items-center rounded-lg bg-indigo-50 px-4 py-2 text-sm text-indigo-600 transition-colors hover:bg-indigo-100"
							>
								<svg class="mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path
										stroke-linecap="round"
										stroke-linejoin="round"
										stroke-width="2"
										d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
									/>
								</svg>
								Edit Entry
							</button>

							<button
								on:click={handleDelete}
								class="flex items-center rounded-lg bg-red-50 px-4 py-2 text-sm text-red-600 transition-colors hover:bg-red-100"
							>
								<svg class="mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path
										stroke-linecap="round"
										stroke-linejoin="round"
										stroke-width="2"
										d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
									/>
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
