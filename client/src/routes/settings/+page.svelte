<script lang="ts">
	import { settings } from '$lib/stores/settings';
	import { fetchUserSettings, updateUserSettings, generatePassword } from '$lib/api/settings';
	import { fetchCurrentUser, updateUser, deleteUser } from '$lib/api/users';
	import { get2FAStatus } from '$lib/api/2fa';
	import { goto } from '$app/navigation';
	import { onMount, onDestroy } from 'svelte';
	import navLogo from '$lib/assets/navLogo.png';
	import { authStore } from '$lib/stores/authStore';

	let user = {
	  id: '',
	  name: '',
	  email: '',
	  username: '',
	  // Remove createdAt, lastLogin, vaultCount, subscriptionTier for now
	};

	let activeTab = 'account';
	let isLoading = false;
	let successMessage = '';
	let errorMessage = '';

	let accountForm = {
		username: '',
		email: '',
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

	let vaultPreferences = {
		defaultPasswordLength: 16,
		includeUppercase: true,
		includeLowercase: true,
		includeNumbers: true,
		includeSymbols: true,
		autoSave: true,
		showPasswordStrength: true
	};

	let exportInProgress = false;
	let importFile: File | null = null;

	$: passwordMatch =
		accountForm.newPassword &&
		accountForm.confirmPassword &&
		accountForm.newPassword === accountForm.confirmPassword;
	let showUserMenu = false;
	function setTimeoutMenuClose() {
		setTimeout(() => (showUserMenu = false), 120);
	}

	$: isAuthenticated = $authStore.isAuthenticated;
	$: isLoadingAuth = $authStore.loading;

	$: if (!isLoadingAuth && isAuthenticated) {
	    loadSettings();
	}
	

	async function loadSettings() {
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
	      };
	      accountForm.username = user.username;
	      accountForm.email = user.email;
	    }
	    // Fetch user settings
	    const settingsRes = await fetchUserSettings();
		console.log(`Fetched settings:`, settingsRes);
		if (settingsRes) {
			// Only assign properties that exist on settingsRes, and cast to correct types
			if ('autoLock' in settingsRes) securitySettings.autoLock = Boolean(settingsRes.autoLock);
			if ('sessionTimeout' in settingsRes) securitySettings.sessionTimeout = Number(settingsRes.sessionTimeout);
			if ('loginNotifications' in settingsRes) securitySettings.loginNotifications = Boolean(settingsRes.loginNotifications);
			if ('defaultPasswordLength' in settingsRes) vaultPreferences.defaultPasswordLength = Number(settingsRes.defaultPasswordLength);
			if ('includeUppercase' in settingsRes) vaultPreferences.includeUppercase = Boolean(settingsRes.includeUppercase);
			if ('includeLowercase' in settingsRes) vaultPreferences.includeLowercase = Boolean(settingsRes.includeLowercase);
			if ('includeNumbers' in settingsRes) vaultPreferences.includeNumbers = Boolean(settingsRes.includeNumbers);
			if ('includeSymbols' in settingsRes) vaultPreferences.includeSymbols = Boolean(settingsRes.includeSymbols);
			if ('autoSave' in settingsRes) vaultPreferences.autoSave = Boolean(settingsRes.autoSave);
			if ('showPasswordStrength' in settingsRes) vaultPreferences.showPasswordStrength = Boolean(settingsRes.showPasswordStrength);
	    }
	    // Fetch 2FA status
	    const twoFARes = await get2FAStatus();
	    if (twoFARes && 'enabled' in twoFARes) securitySettings.twoFactorEnabled = twoFARes.enabled;
	  } catch (error) {
	    errorMessage = 'Failed to load settings.';
	    console.error('Failed to load settings:', error);
	  } finally {
	    isLoading = false;
	  }
	}

	async function updateAccount() {
		if (accountForm.newPassword && !passwordMatch) {
			errorMessage = 'Passwords do not match';
			return;
		}
		isLoading = true;
		errorMessage = '';
		successMessage = '';
		try {
			const result = await updateUser(
				user.id,
				undefined,
				accountForm.newPassword ? accountForm.newPassword : undefined,
				accountForm.username
			);
			if (result.success && result.user) {
				user.username = result.user.username ?? user.username;
				successMessage = 'Account updated successfully!';
				accountForm.currentPassword = '';
				accountForm.newPassword = '';
				accountForm.confirmPassword = '';
			} else {
				errorMessage = result.message || 'Failed to update account.';
			}
		} catch (error) {
			errorMessage = 'Failed to update account. Please try again.';
			console.error('Error updating account:', error);
		} finally {
			isLoading = false;
			setTimeout(() => {
				successMessage = '';
				errorMessage = '';
			}, 3000);
		}
	}

	async function updateSecuritySettings() {
		isLoading = true;
		errorMessage = '';
		successMessage = '';
		try {
				const updated = await updateUserSettings({
					theme: 'light', // or use a variable if available
					language: 'en', // or use a variable if available
					notificationsEnabled: securitySettings.loginNotifications
				});
			settings.update((s) => ({ ...s, autoLock: securitySettings.autoLock }));
			successMessage = 'Security settings updated successfully!';
		} catch (error) {
			errorMessage = 'Failed to update security settings.';
			console.error('Error updating security settings:', error);
		} finally {
			isLoading = false;
			setTimeout(() => {
				successMessage = '';
				errorMessage = '';
			}, 3000);
		}
	}

	async function updateVaultPreferences() {
		isLoading = true;
		errorMessage = '';
		successMessage = '';
		try {
				const updated = await updateUserSettings({
					theme: 'light', // or use a variable if available
					language: 'en', // or use a variable if available
					notificationsEnabled: vaultPreferences.autoSave // or another relevant property
				});
			successMessage = 'Vault preferences updated successfully!';
		} catch (error) {
			errorMessage = 'Failed to update vault preferences.';
			console.error('Error updating vault preferences:', error);
		} finally {
			isLoading = false;
			setTimeout(() => {
				successMessage = '';
				errorMessage = '';
			}, 3000);
		}
	}

	async function exportVault() {
		exportInProgress = true;
		try {
			// Replace with actual API call if available
			// const response = await fetch('/api/vault/export');
			// const data = await response.blob();
			// For now, use mock data
			const exportData = {
				exportDate: new Date().toISOString(),
				version: '1.0',
				entries: [] // TODO: fetch real entries
			};
			const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' });
			const url = URL.createObjectURL(blob);
			const a = document.createElement('a');
			a.href = url;
			a.download = `vault-export-${new Date().toISOString().split('T')[0]}.json`;
			a.click();
			URL.revokeObjectURL(url);
			successMessage = 'Vault exported successfully!';
		} catch (error) {
			errorMessage = 'Failed to export vault.';
			console.error('Error exporting vault:', error);
		} finally {
			exportInProgress = false;
			setTimeout(() => {
				successMessage = '';
				errorMessage = '';
			}, 3000);
		}
	}

	async function deleteAccount() {
		if (
			confirm(
				'Are you sure you want to delete your account? This action cannot be undone and will permanently delete all your vault entries.'
			)
		) {
			if (
				confirm(
					'This will permanently delete ALL your passwords and data. Are you absolutely sure?'
				)
			) {
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
				}
			}
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
									<div class="flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-indigo-600 shadow-sm ring-2 ring-white">
										<span class="text-sm font-semibold text-white">{user.username.charAt(0)}</span>
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
											<div class="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-indigo-600">
												<span class="font-semibold text-white">{user.username.charAt(0)}</span>
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
						<div class="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
							<h2 class="mb-6 text-xl font-semibold text-gray-900">Account Information</h2>

							<form on:submit|preventDefault={updateAccount} class="space-y-6">
								<!-- Profile Picture -->
								<div class="flex items-center space-x-6">
									<div class="flex h-20 w-20 items-center justify-center rounded-full bg-indigo-100">
										<span class="text-2xl font-semibold text-indigo-600">{user.username.charAt(0)}</span>
									</div>
									<div>
										<h3 class="text-lg font-medium text-gray-900">{user.username}</h3>
									</div>
								</div>

								<!-- Name Field -->
								<div>
									<label for="name" class="mb-2 block text-sm font-medium text-gray-700"
										>Username</label
									>
									<input
										id="username"
										type="text"
										bind:value={accountForm.username}
										required
										class="w-full rounded-xl border border-gray-200 px-4 py-3 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
									/>
								</div>

								<!-- Email Field -->
								<div>
									<label for="email" class="mb-2 block text-sm font-medium text-gray-700"
										>Email Address</label
									>
									<input
										id="email"
										type="email"
										bind:value={accountForm.email}
										required
										class="w-full rounded-xl border border-gray-200 px-4 py-3 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
									/>
								</div>

								<!-- Change Password Section -->
								<div class="border-t border-gray-100 pt-6">
									<h3 class="mb-4 text-lg font-medium text-gray-900">Change Password</h3>
									<div class="space-y-4">
										<div>
											<label
												for="currentPassword"
												class="mb-2 block text-sm font-medium text-gray-700">Current Password</label
											>
											<input
												id="currentPassword"
												type="password"
												bind:value={accountForm.currentPassword}
												class="w-full rounded-xl border border-gray-200 px-4 py-3 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
												placeholder="Enter current password to change"
											/>
										</div>

										<div class="grid grid-cols-1 gap-4 md:grid-cols-2">
											<div>
												<label for="newPassword" class="mb-2 block text-sm font-medium text-gray-700"
													>New Password</label
												>
												<input
													id="newPassword"
													type="password"
													bind:value={accountForm.newPassword}
													class="w-full rounded-xl border border-gray-200 px-4 py-3 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
													placeholder="Enter new password"
												/>
											</div>

											<div>
												<label
													for="confirmPassword"
													class="mb-2 block text-sm font-medium text-gray-700"
													>Confirm New Password</label
												>
												<input
													id="confirmPassword"
													type="password"
													bind:value={accountForm.confirmPassword}
													class="w-full rounded-xl border border-gray-200 px-4 py-3 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
													placeholder="Confirm new password"
												/>
											</div>
										</div>

										{#if accountForm.newPassword && accountForm.confirmPassword && !passwordMatch}
											<p class="text-sm text-red-600">Passwords do not match</p>
										{/if}
									</div>
								</div>

								<button
									type="submit"
									disabled={isLoading || (!!accountForm.newPassword && !passwordMatch)}
									class="w-full rounded-xl bg-indigo-600 px-6 py-3 text-white transition-colors hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50 md:w-auto"
								>
									{isLoading ? 'Updating...' : 'Update Account'}
								</button>
							</form>
						</div>
					{/if}

					<!-- Security Tab -->
					{#if activeTab === 'security'}
						<div class="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
							<h2 class="mb-6 text-xl font-semibold text-gray-900">Security Settings</h2>

							<form on:submit|preventDefault={updateSecuritySettings} class="space-y-6">
								<!-- Two-Factor Authentication -->
								<div class="flex items-center justify-between rounded-xl bg-gray-50 p-4">
									<div>
										<h3 class="text-sm font-medium text-gray-900">Two-Factor Authentication</h3>
										<p class="text-sm text-gray-500">
											Add an extra layer of security to your account
										</p>
									</div>
									<label class="relative inline-flex cursor-pointer items-center">
										<input
											type="checkbox"
											bind:checked={securitySettings.twoFactorEnabled}
											class="peer sr-only"
										/>
										<div
											class="peer h-6 w-11 rounded-full bg-gray-200 peer-checked:bg-indigo-600 peer-focus:ring-4 peer-focus:ring-indigo-300 peer-focus:outline-none after:absolute after:top-[2px] after:left-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-gray-300 after:bg-white after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-white"
										></div>
									</label>
								</div>

								<!-- Session Timeout -->
								<div>
									<label for="sessionTimeout" class="mb-2 block text-sm font-medium text-gray-700">
										Session Timeout (minutes)
									</label>
									<select
										id="sessionTimeout"
										bind:value={securitySettings.sessionTimeout}
										class="w-full rounded-xl border border-gray-200 px-4 py-3 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
									>
										<option value={15}>15 minutes</option>
										<option value={30}>30 minutes</option>
										<option value={60}>1 hour</option>
										<option value={120}>2 hours</option>
										<option value={0}>Never</option>
									</select>
								</div>

								<!-- Auto-lock -->
								<div class="flex items-center justify-between rounded-xl bg-gray-50 p-4">
									<div>
										<h3 class="text-sm font-medium text-gray-900">Auto-lock Vault</h3>
										<p class="text-sm text-gray-500">Automatically lock when browser is idle</p>
									</div>
									<label class="relative inline-flex cursor-pointer items-center">
										<input
											type="checkbox"
											bind:checked={securitySettings.autoLock}
											class="peer sr-only"
										/>
										<div
											class="peer h-6 w-11 rounded-full bg-gray-200 peer-checked:bg-indigo-600 peer-focus:ring-4 peer-focus:ring-indigo-300 peer-focus:outline-none after:absolute after:top-[2px] after:left-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-gray-300 after:bg-white after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-white"
										></div>
									</label>
								</div>

								<!-- Login Notifications -->
								<div class="flex items-center justify-between rounded-xl bg-gray-50 p-4">
									<div>
										<h3 class="text-sm font-medium text-gray-900">Login Notifications</h3>
										<p class="text-sm text-gray-500">Get notified of new sign-ins to your account</p>
									</div>
									<label class="relative inline-flex cursor-pointer items-center">
										<input
											type="checkbox"
											bind:checked={securitySettings.loginNotifications}
											class="peer sr-only"
										/>
										<div
											class="peer h-6 w-11 rounded-full bg-gray-200 peer-checked:bg-indigo-600 peer-focus:ring-4 peer-focus:ring-indigo-300 peer-focus:outline-none after:absolute after:top-[2px] after:left-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-gray-300 after:bg-white after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-white"
										></div>
									</label>
								</div>

								<button
									type="submit"
									disabled={isLoading}
									class="w-full rounded-xl bg-indigo-600 px-6 py-3 text-white transition-colors hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50 md:w-auto"
								>
									{isLoading ? 'Updating...' : 'Update Security Settings'}
								</button>
							</form>
						</div>
					{/if}

					<!-- Vault Preferences Tab -->
					{#if activeTab === 'vault'}
						<div class="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
							<h2 class="mb-6 text-xl font-semibold text-gray-900">Vault Preferences</h2>

							<form on:submit|preventDefault={updateVaultPreferences} class="space-y-6">
								<!-- Password Generation -->
								<div>
									<h3 class="mb-4 text-lg font-medium text-gray-900">Password Generation</h3>
									<div class="space-y-4">
										<div>
											<label
												for="passwordLength"
												class="mb-2 block text-sm font-medium text-gray-700"
											>
												Default Password Length: {vaultPreferences.defaultPasswordLength}
											</label>
											<input
												id="passwordLength"
												type="range"
												min="8"
												max="64"
												bind:value={vaultPreferences.defaultPasswordLength}
												class="slider h-2 w-full cursor-pointer appearance-none rounded-lg bg-gray-200"
											/>
											<div class="mt-1 flex justify-between text-xs text-gray-500">
												<span>8</span>
												<span>64</span>
											</div>
										</div>

										<div class="grid grid-cols-2 gap-4">
											<label class="flex items-center">
												<input
													type="checkbox"
													bind:checked={vaultPreferences.includeUppercase}
													class="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
												/>
												<span class="ml-2 text-sm text-gray-700">Uppercase letters (A-Z)</span>
											</label>

											<label class="flex items-center">
												<input
													type="checkbox"
													bind:checked={vaultPreferences.includeLowercase}
													class="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
												/>
												<span class="ml-2 text-sm text-gray-700">Lowercase letters (a-z)</span>
											</label>

											<label class="flex items-center">
												<input
													type="checkbox"
													bind:checked={vaultPreferences.includeNumbers}
													class="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
												/>
												<span class="ml-2 text-sm text-gray-700">Numbers (0-9)</span>
											</label>

											<label class="flex items-center">
												<input
													type="checkbox"
													bind:checked={vaultPreferences.includeSymbols}
													class="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
												/>
												<span class="ml-2 text-sm text-gray-700">Symbols (!@#$%^&*)</span>
											</label>
										</div>
									</div>
								</div>

								<!-- Vault Behavior -->
								<div class="border-t border-gray-100 pt-6">
									<h3 class="mb-4 text-lg font-medium text-gray-900">Vault Behavior</h3>
									<div class="space-y-4">
										<div class="flex items-center justify-between rounded-xl bg-gray-50 p-4">
											<div>
												<h4 class="text-sm font-medium text-gray-900">Auto-save entries</h4>
												<p class="text-sm text-gray-500">
													Automatically save changes without confirmation
												</p>
											</div>
											<label class="relative inline-flex cursor-pointer items-center">
												<input
													type="checkbox"
													bind:checked={vaultPreferences.autoSave}
													class="peer sr-only"
												/>
												<div
													class="peer h-6 w-11 rounded-full bg-gray-200 peer-checked:bg-indigo-600 peer-focus:ring-4 peer-focus:ring-indigo-300 peer-focus:outline-none after:absolute after:top-[2px] after:left-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-gray-300 after:bg-white after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-white"
												></div>
											</label>
										</div>
										<div class="flex items-center justify-between rounded-xl bg-gray-50 p-4">
											<div>
												<h4 class="text-sm font-medium text-gray-900">Show password strength</h4>
												<p class="text-sm text-gray-500">
													Display password strength meter when creating passwords
												</p>
											</div>
											<label class="relative inline-flex cursor-pointer items-center">
												<input
													type="checkbox"
													bind:checked={vaultPreferences.showPasswordStrength}
													class="peer sr-only"
												/>
												<div
													class="peer h-6 w-11 rounded-full bg-gray-200 peer-checked:bg-indigo-600 peer-focus:ring-4 peer-focus:ring-indigo-300 peer-focus:outline-none after:absolute after:top-[2px] after:left-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-gray-300 after:bg-white after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-white"
												></div>
											</label>
										</div>
									</div>
								</div>
								<button
									type="submit"
									disabled={isLoading}
									class="mt-4 w-full rounded-xl bg-indigo-600 px-6 py-3 text-white transition-colors hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50 md:w-auto"
								>
									{isLoading ? 'Updating...' : 'Update Preferences'}
								</button>
							</form>
						</div>
					{/if}

					<!-- Data & Export Tab -->
					{#if activeTab === 'data'}
						<div class="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
							<h2 class="mb-6 text-xl font-semibold text-gray-900">Data & Export</h2>
							<div class="space-y-6">
								<div>
									<h3 class="mb-2 text-lg font-medium text-gray-900">Export Vault Data</h3>
									<p class="mb-4 text-sm text-gray-500">
										Download all your vault entries as a JSON file for backup or migration.
									</p>
									<button
										on:click={exportVault}
										disabled={exportInProgress}
										class="rounded-xl bg-indigo-600 px-6 py-3 text-white transition-colors hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50"
									>
										{exportInProgress ? 'Exporting...' : 'Export Vault'}
									</button>
								</div>
								<div class="border-t border-gray-100 pt-6">
									<h3 class="mb-2 text-lg font-medium text-gray-900">Import Vault Data</h3>
									<p class="mb-4 text-sm text-gray-500">
										Restore your vault from a previously exported JSON file.
									</p>
									<input
										type="file"
										accept="application/json"
										on:change={(e) => {
											const target = e.target as HTMLInputElement | null;
											importFile = target && target.files ? target.files[0] : null;
										}}
										class="mb-2"
									/>
									<button
										type="button"
										disabled={!importFile}
										class="rounded-xl bg-indigo-600 px-6 py-3 text-white transition-colors hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50"
										on:click={() => {
											// TODO: Implement import logic
											successMessage = 'Import feature coming soon!';
											setTimeout(() => {
												successMessage = '';
											}, 2000);
										}}
									>
										Import Vault
									</button>
								</div>
								<div class="border-t border-gray-100 pt-6">
									<h3 class="mb-2 text-lg font-medium text-red-700">Delete Account</h3>
									<p class="mb-4 text-sm text-red-500">
										This action is irreversible. All your data will be permanently deleted.
									</p>
									<button
										type="button"
										on:click={deleteAccount}
										class="rounded-xl bg-red-600 px-6 py-3 text-white transition-colors hover:bg-red-700"
									>
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
