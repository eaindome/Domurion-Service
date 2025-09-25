<script lang="ts">
	// eslint-disable-next-line svelte/no-navigation-without-resolve
    import { onMount } from 'svelte';
    import { toast } from '$lib/stores/toast';
    import { authStore } from '$lib/stores/authStore';
    import type { VaultItem } from '$lib/types';
    import navLogo from '$lib/assets/navLogo.png';
    import { goto } from '$app/navigation';
    import { listSharedVaultEntries } from '$lib/api/vault';

    let sharedItems: VaultItem[] = [];
    let isLoading = false;
    $: user = {
		email: $authStore.user?.email || '',
		username: $authStore.user?.username || '',
		id: $authStore.user?.id || '',
		profilePictureUrl: $authStore.user?.profilePictureUrl || ''
	};

    // User menu dropdown state
    let showUserMenu = false;
    function setTimeoutMenuClose() {
        setTimeout(() => (showUserMenu = false), 120);
    }

    $: if (user.id) {
		loadSharedItems();
	}

    async function loadSharedItems() {
        isLoading = true;
        try {
            const result = await listSharedVaultEntries(user.id);
			if (result.success && result.entries) {
				sharedItems = result.entries.map((entry) => ({
					id: String(entry.id),
					siteName: String(entry.siteName),
					email: String(entry.email),
					username: String(entry.username),
					password: String(entry.password),
					siteUrl: String(entry.siteUrl ?? ''),
					createdAt: String(entry.createdAt ?? ''),
					updatedAt: String(entry.updatedAt ?? '')
				}));
			} else {
				sharedItems = [];
				toast.show(result.error || 'Failed to load shared items', 'error');
			}
        } catch (error) {
            console.error('Failed to load shared items:', error);
            toast.show('Failed to load shared items', 'error');
        } finally {
            isLoading = false;
        }
    }

    function handleHelpClick() {
		showUserMenu = false;
		console.log('Navigating to help');
		setTimeout(() => goto('/help'), 10);
	}

	function handleDashboardClick() {
		showUserMenu = false;
		console.log('Navigating to dashboard');
		setTimeout(() => goto('/dashboard'), 10);
	}

	function logout() {
		authStore.logout();
		document.cookie = "access_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
		goto('/login');
	}
</script>

<svelte:head>
    <title>Shared With Me - Vault</title>
</svelte:head>

<div class="min-h-screen bg-gray-50">
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
					<!-- User dropdown -->
					<div class="relative" on:focusout={setTimeoutMenuClose}>
						<button
							class="group flex items-center space-x-2 rounded-xl p-1 transition-all duration-200 hover:bg-gray-50 focus:ring-2 focus:ring-indigo-500 focus:ring-offset-1 focus:outline-none"
							aria-haspopup="true"
							aria-expanded={showUserMenu}
							aria-label="User menu"
							on:click={() => (showUserMenu = !showUserMenu)}
						>
							<div class="relative">
								<div class="flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-indigo-600 shadow-sm ring-2 ring-white overflow-hidden">
									{#if user.profilePictureUrl}
										<img 
											src={user.profilePictureUrl} 
											alt={user.username}
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
											{user.username.charAt(0)}
										</span>
									{:else}
										<span class="text-sm font-semibold text-white">{user.username.charAt(0)}</span>
									{/if}
								</div>
								<div class="absolute -right-0.5 -bottom-0.5 h-3 w-3 rounded-full border-2 border-white bg-green-400"></div>
							</div>
							<svg
								class="h-4 w-4 text-gray-400 transition-all duration-200 group-hover:text-gray-600 {showUserMenu ? 'rotate-180' : 'rotate-0'}"
								fill="none"
								stroke="currentColor"
								viewBox="0 0 24 24"
							>
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
							</svg>
						</button>
						{#if showUserMenu}
							<div class="animate-menu-enter absolute right-0 z-50 mt-3 w-56 rounded-2xl border border-gray-200 bg-white py-2 shadow-xl">
								<div class="border-b border-gray-100 px-4 py-3">
									<div class="flex items-center space-x-3">
										<div class="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-indigo-600 overflow-hidden">
											{#if user.profilePictureUrl}
												<img 
													src={user.profilePictureUrl} 
													alt={user.username}
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
													{user.username.charAt(0)}
												</span>
											{:else}
												<span class="font-semibold text-white">{user.username.charAt(0)}</span>
											{/if}
										</div>
										<div class="min-w-0 flex-1">
											<p class="truncate text-sm font-semibold text-gray-900">{user.username}</p>
											<p class="truncate text-xs text-gray-500">{user.email || 'user@example.com'}</p>
										</div>
									</div>
								</div>
								<div class="py-1">
									<button
										type="button"
										class="group flex w-full items-center px-4 py-3 text-sm text-gray-700 transition-all duration-150 hover:bg-indigo-50 hover:text-indigo-700"
										on:mousedown={handleDashboardClick}
									>
										<svg class="mr-3 h-4 w-4 text-indigo-500 group-hover:text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<rect x="3" y="3" width="7" height="7" rx="2" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
											<rect x="14" y="3" width="7" height="7" rx="2" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
											<rect x="14" y="14" width="7" height="7" rx="2" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
											<rect x="3" y="14" width="7" height="7" rx="2" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
										</svg>
										Dashboard
									</button>
									<button
										type="button"
										class="group flex w-full items-center px-4 py-3 text-sm text-gray-700 transition-all duration-150 hover:bg-gray-50"
										on:mousedown={handleHelpClick}
									>
										<svg class="mr-3 h-4 w-4 text-gray-400 group-hover:text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
										</svg>
										Help & Support
									</button>
								</div>
								<div class="my-1 border-t border-gray-100"></div>
								<button
									on:click={() => {
										showUserMenu = false;
										logout();
									}}
									class="group flex w-full items-center px-4 py-3 text-sm text-gray-700 transition-all duration-150 hover:bg-red-50 hover:text-red-600"
									tabindex="0"
								>
									<svg class="mr-3 h-4 w-4 text-red-500 group-hover:text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m10 0v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h8a3 3 0 013 3v1" />
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
    <div class="mx-auto max-w-3xl py-10 px-4 sm:px-6 lg:px-8">
        <h1 class="text-3xl font-semibold text-gray-900 mb-2">Shared With Me</h1>
        <p class="mb-8 text-gray-600">Credentials that have been shared with your account will appear here.</p>
        {#if isLoading}
            <div class="rounded-2xl border border-gray-100 bg-white p-12 shadow-sm text-center">
                <div class="mx-auto h-8 w-8 animate-spin rounded-full border-b-2 border-indigo-600"></div>
                <p class="mt-4 text-gray-600">Loading shared credentials...</p>
            </div>
        {:else if sharedItems.length === 0}
            <div class="rounded-2xl border border-gray-100 bg-white p-12 shadow-sm text-center">
                <svg class="mx-auto mb-4 h-16 w-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 8a3 3 0 11-6 0 3 3 0 016 0zm6 8a6 6 0 00-12 0v1a3 3 0 003 3h6a3 3 0 003-3v-1z"/>
                </svg>
                <h3 class="mb-2 text-lg font-medium text-gray-900">No shared credentials</h3>
                <p class="mb-6 text-gray-600">You don't have any credentials shared with you yet.</p>
            </div>
        {:else}
            <div class="overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm">
                <div class="divide-y divide-gray-100">
                    {#each sharedItems as item (item.id)}
                        <!-- You can use your VaultItemRow or a similar component here -->
                        <div class="p-6">
                            <div class="flex items-center justify-between">
                                <div>
                                    <div class="font-semibold text-gray-900">{item.siteName}</div>
                                    <!-- <div class="text-xs text-gray-500">{item.name}</div> -->
                                </div>
                                <!-- <div class="text-xs text-gray-400">Shared by: {item.sharedBy || 'Unknown'}</div> -->
                            </div>
                        </div>
                    {/each}
                </div>
            </div>
        {/if}
    </div>
</div>
