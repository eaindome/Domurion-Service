<script lang="ts">
	// eslint-disable-next-line svelte/no-navigation-without-resolve
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { toast } from '$lib/stores/toast';
	import navLogo from '$lib/assets/navLogo.png';
	import { authStore } from '$lib/stores/authStore';
	import { browser } from '$app/environment';

	import VaultItemRow from '$lib/components/VaultItemRow.svelte';
	import type { VaultItem } from '$lib/types';

	import { listVaultEntries, deleteVaultEntry } from '$lib/api/vault';

	// Vault items will be loaded from API
	let vaultItems: VaultItem[] = [];

	let searchQuery = '';
	let showDeleteModal = false;
	let itemToDelete: VaultItem | null = null;
	let isLoading = false;
	let isDeletingItem = false;

	// User info will be loaded from auth store
	let user = { email: '', name: '', id: '', username: '' };

	// User menu dropdown state
	let showUserMenu = false;
	let userMenuRef: HTMLDivElement | null = null;
	let userMenuButtonRef: HTMLButtonElement | null = null;

	function handleDocumentClick(event: MouseEvent) {
		if (
			userMenuRef &&
			!userMenuRef.contains(event.target as Node) &&
			userMenuButtonRef &&
			!userMenuButtonRef.contains(event.target as Node)
		) {
			showUserMenu = false;
		}
	}

	onMount(() => {
		document.addEventListener('click', handleDocumentClick);
		return () => {
			document.removeEventListener('click', handleDocumentClick);
		};
	});

	// Filter vault items based on search
	$: filteredItems = vaultItems.filter(
		(item) =>
			item.siteName.toLowerCase().includes(searchQuery.toLowerCase()) ||
			item.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
			item.siteUrl.toLowerCase().includes(searchQuery.toLowerCase())
	);

	$: isAuthenticated = $authStore.isAuthenticated;
	$: isLoadingAuth = $authStore.loading;

	$: if (!isLoadingAuth && isAuthenticated) {
	    loadVaultItems();
	}

	async function loadVaultItems() {
		isLoading = true;
		try {
			if (!$authStore.user?.id) {
				// console.log(`User data:`, $authStore.user);
				toast.show('User not authenticated', 'error');
				return;
			}
			// console.log(`Loading vault items for user ID: ${$authStore.user.id}`);
			const result = await listVaultEntries();
			// console.log(`Vault items loaded:`, result);
			if (result.success && result.entries) {
				vaultItems = result.entries.map((entry) => ({
					id: String(entry.id),
					siteName: String(entry.site || entry.siteName || ''),
					siteUrl: String(entry.siteUrl || ''),
					email: String(entry.email || ''),
					password: String(entry.password || ''),
					notes: String(entry.notes || ''),
					createdAt: String(entry.createdAt || ''),
					updatedAt: String(entry.updatedAt || '')
				}));
			} else {
				vaultItems = [];
				toast.show(result.error || 'Failed to load vault items', 'error');
			}
		} catch (error) {
			console.error('Failed to load vault items:', error);
			toast.show('Failed to load vault items', 'error');
		} finally {
			// Always inject dummy data for UI testing if vaultItems is empty
			// if (vaultItems.length === 0) {
			// 	vaultItems = mockVaultItems;
			// }
			isLoading = false;
		}
	}

	function copyToClipboard(text: string, type: string) {
		navigator.clipboard.writeText(text).then(() => {
			toast.show('Copied to clipboard', 'success');
		});
	}

	function confirmDelete(item: VaultItem) {
		itemToDelete = item;
		showDeleteModal = true;
	}

	async function deleteItem() {
		if (!itemToDelete || isDeletingItem) return;
		isDeletingItem = true;
		try {
			if (!$authStore.user?.id) {
				toast.show('User not authenticated', 'error');
				return;
			}
			const result = await deleteVaultEntry(String(itemToDelete.id));
			if (result.success) {
				if (itemToDelete && itemToDelete.id != null) {
					vaultItems = vaultItems.filter((item) => String(item.id) !== String(itemToDelete?.id));
				}
				toast.show('Entry deleted successfully', 'success');
			} else {
				toast.show(result.error || 'Failed to delete entry', 'error');
			}
			showDeleteModal = false;
			itemToDelete = null;
		} catch (error) {
			console.error('Failed to delete item:', error);
			toast.show('Failed to delete entry', 'error');
		} finally {
			isDeletingItem = false;
		}
	}

	async function logout() {
		authStore.clearUser();
		document.cookie = "access_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
		goto('/login');
	}

	function handleSettingsClick() {
		showUserMenu = false;
		setTimeout(() => goto('/settings'), 10);
	}

	function handleHelpClick() {
		showUserMenu = false;
		setTimeout(() => goto('/help'), 10);
	}
</script>

<svelte:head>
	<title>Dashboard - Vault</title>
</svelte:head>

<div class="min-h-screen bg-gray-50">
	{#if $authStore.loading}
		<!-- Blocked by layout spinner -->
	{:else if !$authStore.isAuthenticated}
		<!-- Redirection -->
	{:else}
		<!-- Navigation Header -->
		<nav class="border-b border-gray-100 bg-white shadow-sm">
			<div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
				<div class="flex h-16 items-center justify-between">
					<!-- Logo and Brand -->
					<div class="flex items-center">
						<div class="flex items-center justify-center h-16">
							<img src={navLogo} alt="Domurion Logo" class="max-h-32 max-w-32 rounded-lg" />
						</div>
					</div>

					<!-- User Menu -->
					<div class="flex items-center space-x-6">
						<!-- Welcome message with better typography -->
						<div class="hidden items-center sm:flex">
							<span class="text-sm font-medium text-gray-600">Welcome back,</span>
							<span class="ml-1 text-sm font-semibold text-gray-900">{$authStore.user?.username}</span>
						</div>

						<!-- User dropdown with enhanced design -->
								<div class="relative" bind:this={userMenuRef}>
									<button
										bind:this={userMenuButtonRef}
										class="group flex items-center space-x-2 rounded-xl p-1 transition-all duration-200 hover:bg-gray-50 focus:ring-2 focus:ring-indigo-500 focus:ring-offset-1 focus:outline-none"
										aria-haspopup="true"
										aria-expanded={showUserMenu}
										aria-label="User menu"
										on:click={() => (showUserMenu = !showUserMenu)}
									>
								<!-- Avatar with status indicator -->
								<div class="relative">
									<div
										class="flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-indigo-600 shadow-sm ring-2 ring-white overflow-hidden"
									>
										{#if $authStore.user?.profilePictureUrl}
											<img 
												src={$authStore.user.profilePictureUrl} 
												alt={$authStore.user.username || ''}
												class="h-full w-full object-cover"
												on:error={(e) => {
													const imgElement = e.target as HTMLImageElement;
													const fallbackSpan = imgElement.nextElementSibling as HTMLSpanElement;
													if (imgElement && fallbackSpan) {
														imgElement.style.display = 'none';
														fallbackSpan.style.display = 'flex';
													}
												}}
											/>
											<span class="text-sm font-semibold text-white hidden items-center justify-center h-full w-full">
												{$authStore.user?.username ? $authStore.user.username.charAt(0) : ''}
											</span>
										{:else}
											<span class="text-sm font-semibold text-white">
												{$authStore.user?.username ? $authStore.user.username.charAt(0) : ''}
											</span>
										{/if}
									</div>
									<!-- Online status indicator -->
									<div
										class="absolute -right-0.5 -bottom-0.5 h-3 w-3 rounded-full border-2 border-white bg-green-400"
									></div>
								</div>

								<!-- Chevron with smooth rotation -->
								<svg
									class="h-4 w-4 text-gray-400 transition-all duration-200 group-hover:text-gray-600 {showUserMenu
										? 'rotate-180'
										: 'rotate-0'}"
									fill="none"
									stroke="currentColor"
									viewBox="0 0 24 24"
								>
									<path
										stroke-linecap="round"
										stroke-linejoin="round"
										stroke-width="2"
										d="M19 9l-7 7-7-7"
									/>
								</svg>
							</button>

							<!-- Enhanced dropdown menu -->
							{#if showUserMenu}
								<div
									class="animate-menu-enter absolute right-0 z-50 mt-3 w-56 rounded-2xl border border-gray-200 bg-white py-2 shadow-xl"
								>
									<!-- User info section -->
									<div class="border-b border-gray-100 px-4 py-3">
										<div class="flex items-center space-x-3">
											<div
												class="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-indigo-600 overflow-hidden"
											>
												{#if $authStore.user?.profilePictureUrl}
													<img 
														src={$authStore.user.profilePictureUrl} 
														alt={$authStore.user.username || ''}
														class="h-full w-full object-cover"
														on:error={(e) => {
															const imgElement = e.target as HTMLImageElement;
															const fallbackSpan = imgElement.nextElementSibling as HTMLSpanElement;
															if (imgElement && fallbackSpan) {
																imgElement.style.display = 'none';
																fallbackSpan.style.display = 'flex';
															}
														}}
													/>
													<span class="font-semibold text-white hidden items-center justify-center h-full w-full">
														{$authStore.user?.username?.charAt(0) ?? ''}
													</span>
												{:else}
													<span class="font-semibold text-white">{$authStore.user?.username?.charAt(0) ?? ''}</span>
												{/if}
											</div>
											<div class="min-w-0 flex-1">
												<p class="truncate text-sm font-semibold text-gray-900">{$authStore.user?.username}</p>
												<p class="truncate text-xs text-gray-500">
													{$authStore.user?.email || 'user@example.com'}
												</p>
											</div>
										</div>
									</div>

									<!-- Menu items -->
									<div class="py-1">
										<button
											type="button"
											class="group flex w-full items-center px-4 py-3 text-sm text-gray-700 transition-all duration-150 hover:bg-indigo-50 hover:text-indigo-700"
											on:mousedown={handleSettingsClick}
										>
											<svg
												class="mr-3 h-4 w-4 text-indigo-500 group-hover:text-indigo-600"
												fill="none"
												stroke="currentColor"
												viewBox="0 0 24 24"
											>
												<path
													stroke-linecap="round"
													stroke-linejoin="round"
													stroke-width="2"
													d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"
												/>
												<path
													stroke-linecap="round"
													stroke-linejoin="round"
													stroke-width="2"
													d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
												/>
											</svg>
											<span>Account Settings</span>
										</button>

										<button
											type="button"
											class="group flex w-full items-center px-4 py-3 text-sm text-gray-700 transition-all duration-150 hover:bg-gray-50"
											on:mousedown={handleHelpClick}
										>
											<svg
												class="mr-3 h-4 w-4 text-gray-400 group-hover:text-gray-600"
												fill="none"
												stroke="currentColor"
												viewBox="0 0 24 24"
											>
												<path
													stroke-linecap="round"
													stroke-linejoin="round"
													stroke-width="2"
													d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
												/>
											</svg>
											<span>Help & Support</span>
										</button>
									</div>

									<!-- Separator -->
									<div class="my-1 border-t border-gray-100"></div>

									<!-- Sign out button -->
									<button
										on:click={() => {
											showUserMenu = false;
											logout();
										}}
										class="group flex w-full items-center px-4 py-3 text-sm text-gray-700 transition-all duration-150 hover:bg-red-50 hover:text-red-600"
										tabindex="0"
									>
										<svg
											class="mr-3 h-4 w-4 text-red-500 group-hover:text-red-600"
											fill="none"
											stroke="currentColor"
											viewBox="0 0 24 24"
										>
											<path
												stroke-linecap="round"
												stroke-linejoin="round"
												stroke-width="2"
												d="M17 16l4-4m0 0l-4-4m4 4H7m10 0v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h8a3 3 0 013 3v1"
											/>
										</svg>
										<span>Sign Out</span>
									</button>
								</div>
							{/if}
						</div>
					</div>
				</div>
			</div>
		</nav>

		<style>
			/* Enhanced menu animations */
			@keyframes menu-enter {
				from {
					opacity: 0;
					transform: translateY(-8px) scale(0.95);
				}
				to {
					opacity: 1;
					transform: translateY(0) scale(1);
				}
			}

			.animate-menu-enter {
				animation: menu-enter 0.2s cubic-bezier(0.4, 0, 0.2, 1) forwards;
				transform-origin: top right;
			}

			/* Smooth transitions for interactive elements */
			.group:hover .group-hover\:text-indigo-600 {
				transition-property: color;
				transition-duration: 150ms;
				transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
			}

			/* Custom scrollbar for long user names/emails */
			.truncate {
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
			}
		</style>

		<!-- Main Content -->
		<div class="mx-auto max-w-7xl py-6 sm:px-6 lg:px-8">
			<!-- Header Section -->
			<div class="px-4 sm:px-0">
				<div class="mb-8 flex items-center justify-between">
					<div>
						<h1 class="text-3xl font-semibold text-gray-900">Your Vault</h1>
						<p class="mt-2 text-gray-600">Manage your passwords and credentials securely</p>
					</div>
					<div class="flex flex-col sm:flex-row gap-2">
						<a
							href={'/vault/shared'}
							class="inline-flex items-center rounded-xl border border-indigo-200 bg-white px-4 py-2 text-sm font-medium text-indigo-700 transition-all duration-200 hover:bg-indigo-50 focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 focus:outline-none"
						>
							<svg class="mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 8a3 3 0 11-6 0 3 3 0 016 0zm6 8a6 6 0 00-12 0v1a3 3 0 003 3h6a3 3 0 003-3v-1z"/>
							</svg>
							Shared With Me
						</a>
						<a
							href={'/vault/add'}
							class="inline-flex items-center rounded-xl border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition-all duration-200 hover:bg-indigo-700 focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 focus:outline-none"
						>
							<svg class="mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M12 6v6m0 0v6m0-6h6m-6 0H6"
								/>
							</svg>
							Add New Entry
						</a>
					</div>
				</div>

				<!-- Search and Stats -->
				<div class="mb-6 rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
					{#if isLoading}
						<!-- Search and stats skeleton -->
						<div class="flex flex-col justify-between space-y-4 sm:flex-row sm:items-center sm:space-y-0">
							<!-- Search bar skeleton -->
							<div class="relative max-w-md flex-1">
								<div class="h-10 bg-gradient-to-r from-gray-200 to-gray-300 rounded-xl animate-pulse"></div>
							</div>
							
							<!-- Stats skeleton -->
							<div class="flex items-center space-x-6">
								<div class="flex items-center space-x-2">
									<div class="h-2 w-2 bg-gradient-to-r from-gray-200 to-gray-300 rounded-full animate-pulse"></div>
									<div class="h-4 w-20 bg-gradient-to-r from-gray-200 to-gray-300 rounded animate-pulse"></div>
								</div>
								<div class="flex items-center space-x-2">
									<div class="h-2 w-2 bg-gradient-to-r from-gray-200 to-gray-300 rounded-full animate-pulse"></div>
									<div class="h-4 w-16 bg-gradient-to-r from-gray-200 to-gray-300 rounded animate-pulse"></div>
								</div>
							</div>
						</div>
					{:else}
						<div
							class="flex flex-col justify-between space-y-4 sm:flex-row sm:items-center sm:space-y-0"
						>
							<!-- Search Bar -->
							<div class="relative max-w-md flex-1">
								<div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
									<svg
										class="h-5 w-5 text-gray-400"
										fill="none"
										stroke="currentColor"
										viewBox="0 0 24 24"
									>
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
										/>
									</svg>
								</div>
								<input
									type="text"
									bind:value={searchQuery}
									placeholder="Search your vault..."
									class="block w-full rounded-xl border border-gray-200 py-2.5 pr-3 pl-10 text-sm focus:border-transparent focus:ring-2 focus:ring-indigo-500"
								/>
							</div>

							<!-- Stats -->
							<div class="flex items-center space-x-6 text-sm text-gray-600">
								<div class="flex items-center">
									<div class="mr-2 h-2 w-2 rounded-full bg-indigo-500"></div>
									<span>{vaultItems.length} total entries</span>
								</div>
								<div class="flex items-center">
									<div class="mr-2 h-2 w-2 rounded-full bg-green-500"></div>
									<span>{filteredItems.length} shown</span>
								</div>
							</div>
						</div>
					{/if}
				</div>
			</div>

			<!-- Vault Items -->
			<div class="px-4 sm:px-0">
				{#if isLoading}
					<div class="overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm">
						<div class="divide-y divide-gray-100">
							<!-- Skeleton loader for vault items -->
							{#each Array(5) as _, i}
								<div class="p-6">
									<div class="flex items-center justify-between">
										<div class="flex items-center space-x-4 flex-1">
											<!-- Site favicon skeleton -->
											<div class="h-12 w-12 rounded-xl bg-gradient-to-r from-gray-200 to-gray-300 animate-pulse"></div>
											
											<div class="flex-1 space-y-2">
												<!-- Site name skeleton -->
												<div class="h-5 bg-gradient-to-r from-gray-200 to-gray-300 rounded-lg animate-pulse" style="width: {Math.random() * 40 + 20}%"></div>
												<!-- Site URL skeleton -->
												<div class="h-4 bg-gradient-to-r from-gray-200 to-gray-300 rounded-lg animate-pulse" style="width: {Math.random() * 60 + 30}%"></div>
											</div>
										</div>
										
										<div class="flex items-center space-x-3">
											<!-- Action buttons skeleton -->
											<div class="h-8 w-8 bg-gradient-to-r from-gray-200 to-gray-300 rounded-lg animate-pulse"></div>
											<div class="h-8 w-8 bg-gradient-to-r from-gray-200 to-gray-300 rounded-lg animate-pulse"></div>
											<div class="h-8 w-8 bg-gradient-to-r from-gray-200 to-gray-300 rounded-lg animate-pulse"></div>
											<div class="h-8 w-8 bg-gradient-to-r from-gray-200 to-gray-300 rounded-lg animate-pulse"></div>
										</div>
									</div>
								</div>
							{/each}
						</div>
					</div>
				{:else if filteredItems.length === 0}
					<div class="rounded-2xl border border-gray-100 bg-white p-12 shadow-sm">
						<div class="text-center">
							<svg
								class="mx-auto mb-4 h-16 w-16 text-gray-400"
								fill="none"
								stroke="currentColor"
								viewBox="0 0 24 24"
							>
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
								/>
							</svg>
							<h3 class="mb-2 text-lg font-medium text-gray-900">
								{searchQuery ? 'No matching entries' : 'Your vault is empty'}
							</h3>
							<p class="mb-6 text-gray-600">
								{searchQuery
									? 'Try adjusting your search terms'
									: 'Start by adding your first password entry'}
							</p>
							{#if !searchQuery}
								<a
									href={'/vault/add'}
									class="inline-flex items-center rounded-xl border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-indigo-700"
								>
									<svg class="mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M12 6v6m0 0v6m0-6h6m-6 0H6"
										/>
									</svg>
									Add Your First Entry
								</a>
							{/if}
						</div>
					</div>
				{:else}
					<div class="overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm">
						<div class="divide-y divide-gray-100">
							{#each filteredItems as item (item.id)}
								<VaultItemRow
									{item}
									on:copy={(e) => copyToClipboard(e.detail.value, e.detail.type)}
									on:delete={() => confirmDelete(item)}
								/>
							{/each}
						</div>
					</div>
				{/if}
			</div>
		</div>
	{/if}
</div>

<!-- Delete Confirmation Modal -->
{#if showDeleteModal}
	<div
		class="fixed inset-0 z-50 flex items-center justify-center p-4"
		role="dialog"
		aria-modal="true"
		tabindex="0"
		on:keydown={(e) => {
			if (e.key === 'Escape') {
				showDeleteModal = false;
				itemToDelete = null;
			}
		}}
	>
		<!-- Glassmorphic Backdrop -->
		<button
			type="button"
			class="absolute inset-0 bg-black/30 backdrop-blur-sm"
			aria-label="Close delete modal"
			on:click={() => {
				showDeleteModal = false;
				itemToDelete = null;
			}}
			tabindex="0"
			style="border: none; padding: 0; margin: 0; cursor: pointer;"
		></button>
		<div
			class="relative w-full max-w-md rounded-3xl border border-white/30 bg-white p-8 shadow-2xl backdrop-blur-xl"
			style="box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.18);"
		>
			<div
				class="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-red-100 shadow-md"
			>
				<svg class="h-6 w-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"
					/>
				</svg>
			</div>
			<h3 class="mb-2 text-center text-lg font-semibold text-gray-900">Delete Entry</h3>
			<p class="mb-6 text-center text-sm text-gray-700">
				Are you sure you want to delete the entry for <strong>{itemToDelete?.siteName}</strong>?
				This action cannot be undone.
			</p>
			<div class="flex space-x-3">
				<button
					on:click={() => {
						showDeleteModal = false;
						itemToDelete = null;
					}}
					disabled={isDeletingItem}
					class="flex-1 rounded-xl bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
				>
					Cancel
				</button>
				<button
					on:click={deleteItem}
					disabled={isDeletingItem}
					class="flex-1 rounded-xl bg-red-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-red-700 disabled:opacity-75 disabled:cursor-not-allowed flex items-center justify-center"
				>
					{#if isDeletingItem}
						<svg class="mr-2 h-4 w-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
						</svg>
						Deleting...
					{:else}
						Delete
					{/if}
				</button>
			</div>
		</div>
	</div>
{/if}

<style>
	/* Additional custom styles if needed */
	input:focus {
		box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
	}
</style>

