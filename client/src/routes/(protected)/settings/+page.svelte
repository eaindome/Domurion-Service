<script lang="ts">
	import { fetchUserSettings } from '$lib/api/settings';
	import { fetchCurrentUser, deleteUser } from '$lib/api/users';
	import { get2FAStatus } from '$lib/api/2fa';
	import { goto } from '$app/navigation';
	import navLogo from '$lib/assets/navLogo.png';
	import { authStore } from '$lib/stores/authStore';

	// Import components
	import AccountSettings from '$lib/components/settings/AccountSettings.svelte';
	import SecuritySettings from '$lib/components/settings/SecuritySettings.svelte';
	import VaultPreferences from '$lib/components/settings/VaultPreferences.svelte';
	import DataExport from '$lib/components/settings/DataExport.svelte';
	import Toast from '$lib/components/Toast.svelte';
	import type ToastType from '$lib/components/Toast.svelte';
	let toast: ToastType;

	let user = {
	  id: '',
	  name: '',
	  email: '',
	  username: '',
	  profilePictureUrl: '',
	};

	let activeTab = 'account';
	let isLoading = false;
	let successMessage = '';
	let errorMessage = '';

	let accountData = {
		username: '',
		email: '',
		name: '',
		currentPassword: '',
		newPassword: '',
		confirmPassword: ''
	};

	let securitySettings = {
		twoFactorEnabled: false,
		sessionTimeout: 30,
		autoLock: true,
		loginNotifications: true
	};

	let showUserMenu = false;
	let showDeleteModal = false;
	function setTimeoutMenuClose() {
		setTimeout(() => (showUserMenu = false), 120);
	}

	$: isAuthenticated = $authStore.isAuthenticated;
	$: isLoadingAuth = $authStore.loading;

	let hasLoadedSettings = false;
	$: if (!isLoadingAuth && isAuthenticated && !hasLoadedSettings) {
	    loadSettings();
	}
	
	async function loadSettings() {
	  if (hasLoadedSettings) return; // Prevent multiple calls
	  
	  isLoading = true;
	  try {
	    // Fetch user info
	    const userRes = await fetchCurrentUser();
		console.log('Fetched user:', userRes);
	    if (userRes.success && userRes.user) {
	      user = {
	        id: userRes.user.id,
	        name: userRes.user.name ?? '',
	        email: userRes.user.email,
			username: userRes.user.username ?? '',
			profilePictureUrl: userRes.user.profilePictureUrl ?? '',
	      };
	      accountData.username = user.username;
	      accountData.email = user.email;
	      accountData.name = user.name;
	    }
	    
	    // Fetch user settings
	    const settingsRes = await fetchUserSettings();
		console.log(`Fetched settings:`, settingsRes);
		if (settingsRes) {
			// Map settings to components
			if ('autoLock' in settingsRes) securitySettings.autoLock = Boolean(settingsRes.autoLock);
			if ('sessionTimeout' in settingsRes) securitySettings.sessionTimeout = Number(settingsRes.sessionTimeout);
			if ('loginNotifications' in settingsRes) securitySettings.loginNotifications = Boolean(settingsRes.loginNotifications);
	    }
	    
	    // Fetch 2FA status
	    const twoFARes = await get2FAStatus();
	    if (twoFARes && 'enabled' in twoFARes) securitySettings.twoFactorEnabled = twoFARes.enabled;
	  } catch (error) {
	    errorMessage = 'Failed to load settings.';
	    console.error('Failed to load settings:', error);
	  } finally {
	    hasLoadedSettings = true;
	    isLoading = false;
	  }
	}

	// Event handlers for components
	function handleSuccess(event: CustomEvent<{ message: string }>) {
		successMessage = event.detail.message;
		if (toast && typeof toast.show === 'function') {
			toast.show(event.detail.message, 'success');
		}
		setTimeout(() => { successMessage = ''; }, 1000);
	}

	function handleError(event: CustomEvent<{ message: string }>) {
		errorMessage = event.detail.message;
		if (toast && typeof toast.show === 'function') {
			toast.show(event.detail.message, 'error');
		}
		setTimeout(() => { errorMessage = ''; }, 1000);
	}

	function handleLoading(event: CustomEvent<{ isLoading: boolean }>) {
		isLoading = event.detail.isLoading;
	}

	function openDeleteModal() {
		showDeleteModal = true;
	}

	async function confirmDeleteAccount() {
		try {
			const result = await deleteUser(user.id);
			if (result.success) {
				successMessage = 'Account deleted successfully.';
				goto('/login');
			} else {
				errorMessage = result.message || 'Failed to delete account.';
			}
		} catch (error) {
			errorMessage = 'Failed to delete account.';
			console.error('Error deleting account:', error);
		} finally {
			showDeleteModal = false;
		}
	}

	function handleHelpClick() {
		showUserMenu = false;
		setTimeout(() => goto('/help'), 10);
	}

	function handleDashboardClick() {
		showUserMenu = false;
		setTimeout(() => goto('/dashboard'), 10);
	}

	async function logout() {
		authStore.clearUser();
		document.cookie = "access_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
		goto('/login');
	}
</script>

<svelte:head>
	<title>Settings - Vault</title>
</svelte:head>

<div class="min-h-screen bg-gray-50">
	{#if isLoadingAuth}
		<!-- Blocked by layout spinner -->
	{:else if !isAuthenticated}
		<!-- Redirecting to login... -->
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

		<!-- Main Content -->
		<div class="mx-auto max-w-4xl px-4 py-6 sm:px-6 lg:px-8">
			<!-- Header -->
			<div class="mb-8">
				<h1 class="text-3xl font-semibold text-gray-900">Account Settings</h1>
				<p class="mt-2 text-gray-600">Manage your account, security, and vault preferences</p>
			</div>

			<!-- Status Messages -->
			{#if successMessage}
				<div
					class="mb-6 rounded-xl border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700"
				>
					{successMessage}
				</div>
			{/if}

			{#if errorMessage}
				<div class="mb-6 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
					{errorMessage}
				</div>
			{/if}

			<div class="flex flex-col gap-8 lg:flex-row">
				<!-- Sidebar Navigation -->
				<div class="flex-shrink-0 lg:w-64">
					<nav class="rounded-2xl border border-gray-100 bg-white p-2 shadow-sm">
						<div class="space-y-1">
							<button
								on:click={() => (activeTab = 'account')}
								class="w-full rounded-xl px-4 py-3 text-left text-sm font-medium transition-all duration-200 {activeTab ===
								'account'
									? 'bg-indigo-50 text-indigo-700'
									: 'text-gray-600 hover:bg-gray-50'}"
							>
								<div class="flex items-center">
									<svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
										/>
									</svg>
									Account
								</div>
							</button>

							<button
								on:click={() => (activeTab = 'security')}
								class="w-full rounded-xl px-4 py-3 text-left text-sm font-medium transition-all duration-200 {activeTab ===
								'security'
									? 'bg-indigo-50 text-indigo-700'
									: 'text-gray-600 hover:bg-gray-50'}"
							>
								<div class="flex items-center">
									<svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
										/>
									</svg>
									Security
								</div>
							</button>

							<button
								on:click={() => (activeTab = 'vault')}
								class="w-full rounded-xl px-4 py-3 text-left text-sm font-medium transition-all duration-200 {activeTab ===
								'vault'
									? 'bg-indigo-50 text-indigo-700'
									: 'text-gray-600 hover:bg-gray-50'}"
							>
								<div class="flex items-center">
									<svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
									Preferences
								</div>
							</button>

							<button
								on:click={() => (activeTab = 'data')}
								class="w-full rounded-xl px-4 py-3 text-left text-sm font-medium transition-all duration-200 {activeTab ===
								'data'
									? 'bg-indigo-50 text-indigo-700'
									: 'text-gray-600 hover:bg-gray-50'}"
							>
								<div class="flex items-center">
									<svg class="mr-3 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M9 19l3 3m0 0l3-3m-3 3V10"
										/>
									</svg>
									Data & Export
								</div>
							</button>
						</div>
					</nav>
				</div>

				<!-- Content Area -->
				<div class="flex-1">
					<!-- Account Tab -->
					{#if activeTab === 'account'}
						<AccountSettings
							user={user}
							{isLoading}
							on:success={handleSuccess}
							on:error={handleError}
							on:loading={handleLoading}
						/>
					{/if}

					<!-- Security Tab -->
					{#if activeTab === 'security'}
						<SecuritySettings
							{isLoading}
							on:success={handleSuccess}
							on:error={handleError}
							on:loading={handleLoading}
						/>
					{/if}

					<!-- Vault Preferences Tab -->
					{#if activeTab === 'vault'}
						<VaultPreferences
							{isLoading}
							on:success={handleSuccess}
							on:error={handleError}
							on:loading={handleLoading}
						/>
					{/if}

					<!-- Data & Export Tab -->
					{#if activeTab === 'data'}
						<DataExport
							{isLoading}
							on:success={handleSuccess}
							on:error={handleError}
							on:loading={handleLoading}
						/>
						<!-- Delete Account Section -->
						<div class="mt-6 rounded-2xl border border-red-300 bg-red-100 p-6 shadow-sm">
							<div class="flex items-start space-x-3">
								<div class="flex-shrink-0">
									<svg class="h-6 w-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
									</svg>
								</div>
								<div class="flex-1">
									<h3 class="mb-2 text-lg font-semibold text-red-800">Danger Zone</h3>
									<p class="mb-4 text-sm text-red-700">
										Once you delete your account, there is no going back. This will permanently delete your account, all vault entries, and cannot be undone.
									</p>
									<button
										type="button"
										on:click={openDeleteModal}
										class="inline-flex items-center rounded-xl bg-red-600 px-6 py-3 text-sm font-semibold text-white shadow-lg transition-all duration-200 hover:bg-red-700 hover:shadow-xl focus:ring-4 focus:ring-red-300 focus:outline-none"
									>
										<svg class="mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
										</svg>
										Delete Account
									</button>
								</div>
							</div>
						</div>
					{/if}
				</div>
			</div>
		</div>
	{/if}
</div>

<!-- Custom Delete Account Confirmation Modal -->
{#if showDeleteModal}
	<div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
		<div class="bg-white rounded-3xl shadow-2xl border border-gray-200 max-w-md w-full p-8 transform transition-all">
			<!-- Warning Icon -->
			<div class="flex items-center justify-center w-16 h-16 mx-auto mb-6 bg-red-100 rounded-full">
				<svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
				</svg>
			</div>
			
			<!-- Modal Content -->
			<div class="text-center mb-8">
				<h3 class="text-2xl font-bold text-gray-900 mb-3">Delete Account?</h3>
				<p class="text-gray-600 leading-relaxed">
					This will permanently delete <strong>ALL</strong> your passwords and data. Are you absolutely sure?
				</p>
			</div>
			
			<!-- Action Buttons -->
			<div class="flex flex-col-reverse sm:flex-row gap-3">
				<button
					on:click={() => showDeleteModal = false}
					class="flex-1 px-6 py-3 text-gray-700 bg-gray-100 rounded-xl font-medium transition-all duration-200 hover:bg-gray-200 focus:ring-4 focus:ring-gray-300 focus:outline-none"
				>
					Cancel
				</button>
				<button
					on:click={confirmDeleteAccount}
					class="flex-1 px-6 py-3 bg-red-600 text-white rounded-xl font-medium transition-all duration-200 hover:bg-red-700 focus:ring-4 focus:ring-red-300 focus:outline-none shadow-lg hover:shadow-xl"
				>
					<span class="flex items-center justify-center">
						<svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
						</svg>
						Delete Forever
					</span>
				</button>
			</div>
		</div>
	</div>
{/if}

<Toast bind:this={toast} />
