<script lang="ts">
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';

	// User data - you'll get this from your auth store
	let user = {
		name: 'John Doe',
		email: 'john.doe@email.com',
		createdAt: '2024-01-15',
		lastLogin: '2024-09-09',
		vaultCount: 12,
		subscriptionTier: 'Free'
	};

	// Form states
	let activeTab = 'account';
	let isLoading = false;
	let successMessage = '';
	let errorMessage = '';

	// Account settings form
	let accountForm = {
		name: user.name,
		email: user.email,
		currentPassword: '',
		newPassword: '',
		confirmPassword: ''
	};

	import { settings } from '$lib/stores/settings';
	// Security settings
	let securitySettings = {
		twoFactorEnabled: false,
		sessionTimeout: 30, // minutes
		autoLock: true,
		loginNotifications: true
	};

	// Vault preferences
	let vaultPreferences = {
		defaultPasswordLength: 16,
		includeUppercase: true,
		includeLowercase: true,
		includeNumbers: true,
		includeSymbols: true,
		autoSave: true,
		showPasswordStrength: true
	};

	// Export/Import
	let exportInProgress = false;
	let importFile: File | null = null;

	// Password validation
	$: passwordMatch =
		accountForm.newPassword &&
		accountForm.confirmPassword &&
		accountForm.newPassword === accountForm.confirmPassword;

	onMount(() => {
		// Load user settings from API
		loadSettings();
	});

	async function loadSettings() {
		try {
			// TODO: Replace with actual API calls
			// const response = await fetch('/api/user/settings');
			// const settingsData = await response.json();
			// Update local state with fetched settings
			// securitySettings.autoLock = settingsData.autoLock;
			// settings.set({ ...$settings, autoLock: settingsData.autoLock });
		} catch (error) {
			console.error('Failed to load settings:', error);
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
			// TODO: Replace with actual API call
			const updateData: Record<string, string> = {
				name: accountForm.name,
				email: accountForm.email
			};

			if (accountForm.newPassword) {
				updateData.currentPassword = accountForm.currentPassword;
				updateData.newPassword = accountForm.newPassword;
			}

			// const response = await fetch('/api/user/profile', {
			//   method: 'PUT',
			//   headers: { 'Content-Type': 'application/json' },
			//   body: JSON.stringify(updateData)
			// });

			successMessage = 'Account updated successfully!';
			accountForm.currentPassword = '';
			accountForm.newPassword = '';
			accountForm.confirmPassword = '';

			// Update user object
			user.name = accountForm.name;
			user.email = accountForm.email;
		} catch (error) {
			console.log(`Error: ${error}`);
			errorMessage = 'Failed to update account. Please try again.';
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
			// TODO: Replace with actual API call
			// await fetch('/api/user/security', {
			//   method: 'PUT',
			//   headers: { 'Content-Type': 'application/json' },
			//   body: JSON.stringify(securitySettings)
			// });
			// Sync global store
			settings.update((s) => ({ ...s, autoLock: securitySettings.autoLock }));
			successMessage = 'Security settings updated successfully!';
		} catch (error) {
			console.log(`Error: ${error}`);
			errorMessage = 'Failed to update security settings.';
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
			// TODO: Replace with actual API call
			// await fetch('/api/user/vault-preferences', {
			//   method: 'PUT',
			//   headers: { 'Content-Type': 'application/json' },
			//   body: JSON.stringify(vaultPreferences)
			// });

			successMessage = 'Vault preferences updated successfully!';
		} catch (error) {
			console.log(`Error: ${error}`);
			errorMessage = 'Failed to update vault preferences.';
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
			// TODO: Replace with actual API call
			// const response = await fetch('/api/vault/export');
			// const data = await response.blob();

			// Create mock export data for demo
			const exportData = {
				exportDate: new Date().toISOString(),
				version: '1.0',
				entries: [
					{ siteName: 'Google', username: 'user@example.com', password: 'password123' },
					{ siteName: 'GitHub', username: 'username', password: 'password456' }
				]
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
			console.log(`Error: ${error}`);
			errorMessage = 'Failed to export vault.';
		} finally {
			exportInProgress = false;
			setTimeout(() => {
				successMessage = '';
				errorMessage = '';
			}, 3000);
		}
	}

	function deleteAccount() {
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
				// TODO: Implement account deletion
				console.log('Account deletion requested');
				goto('/login');
			}
		}
	}

	function logout() {
		// TODO: Clear auth store and redirect
		goto('/login');
	}
</script>

<svelte:head>
	<title>Settings - Vault</title>
</svelte:head>

<div class="min-h-screen bg-gray-50">
	<!-- Navigation Header -->
	<nav class="border-b border-gray-100 bg-white shadow-sm">
		<div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
			<div class="flex h-16 items-center justify-between">
				<!-- Logo and Brand -->
				<div class="flex items-center">
					<a href="/dashboard" class="flex items-center">
						<div class="flex h-8 w-8 items-center justify-center rounded-lg bg-indigo-600">
							<svg class="h-5 w-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
								/>
							</svg>
						</div>
						<span class="ml-3 text-xl font-semibold text-gray-900">Vault</span>
					</a>
				</div>

				<!-- User Menu -->
				<div class="flex items-center space-x-4">
					<a href="/dashboard" class="text-sm text-gray-600 transition-colors hover:text-gray-900">
						← Back to Dashboard
					</a>
					<button
						on:click={logout}
						class="text-sm text-gray-600 transition-colors hover:text-gray-900"
					>
						Sign out
					</button>
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
									<span class="text-2xl font-semibold text-indigo-600">{user.name.charAt(0)}</span>
								</div>
								<div>
									<h3 class="text-lg font-medium text-gray-900">{user.name}</h3>
									<p class="text-sm text-gray-500">
										Member since {new Date(user.createdAt).toLocaleDateString()}
									</p>
									<p class="text-sm text-gray-500">
										{user.vaultCount} vault entries • {user.subscriptionTier} plan
									</p>
								</div>
							</div>

							<!-- Name Field -->
							<div>
								<label for="name" class="mb-2 block text-sm font-medium text-gray-700"
									>Full Name</label
								>
								<input
									id="name"
									type="text"
									bind:value={accountForm.name}
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
</div>
