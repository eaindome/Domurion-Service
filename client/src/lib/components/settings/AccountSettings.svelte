<script lang="ts">
	import { updateUser } from '$lib/api/users';
	import { generatePassword } from '$lib/api/settings';
    import { authStore } from '$lib/stores/authStore';
	import { createAutoSave, getStatusText, getStatusColor, type SaveStatus } from '$lib/utils/autoSave';
	import { fetchUserPreferences } from '$lib/api/settings';
	import { onMount, onDestroy } from 'svelte';

	export let user: {
		id: string;
		name: string;
		email: string;
		username: string;
		profilePictureUrl?: string;
	};

	export let isLoading = false;
	export const successMessage = '';
	export const errorMessage = '';

	// Create event dispatcher
	import { createEventDispatcher } from 'svelte';
	const dispatch = createEventDispatcher<{
		success: { message: string };
		error: { message: string };
		loading: { isLoading: boolean };
	}>();


	let accountForm = {
		username: user.username,
		email: user.email,
		currentPassword: '',
		newPassword: '',
		confirmPassword: '',
		profilePicture: null as File | null
	};

	let editMode = false;
	let originalData = { username: '', email: '' };
	let isGeneratingPassword = false;
	let profilePicturePreview = user.profilePictureUrl || null;
	let showNewPassword = false;
	let showConfirmPassword = false;

	// Auto-save functionality
	let autoSaveEnabled = false;
	let saveStatus: SaveStatus = 'saved';
	let autoSaveInstance: ReturnType<typeof createAutoSave> | null = null;
	let isInitializing = true;

	// Load user preferences and setup auto-save
	onMount(async () => {
		try {
			const preferences = await fetchUserPreferences();
			autoSaveEnabled = preferences.autoSaveEntries ?? false;

			if (autoSaveEnabled && editMode) {
				setupAutoSave();
			}
		} catch (error) {
			console.warn('Failed to load user preferences for auto-save:', error);
		} finally {
			isInitializing = false;
		}
	});

	// Clean up auto-save on component destroy
	onDestroy(() => {
		autoSaveInstance?.destroy();
	});

	function setupAutoSave() {
		if (!autoSaveEnabled) return;

		autoSaveInstance = createAutoSave({
			storageKey: 'account-settings-draft',
			delay: 2000,
			enabled: autoSaveEnabled,
			saveFunction: async () => {
				// Save draft data (excluding sensitive fields)
				const draftData = {
					username: accountForm.username,
					email: accountForm.email
					// Note: We don't save password fields for security
				};
				autoSaveInstance?.saveDraft(draftData);
			},
			onStatusChange: (status) => {
				saveStatus = status;
			}
		});

		// Try to load draft
		const draft = autoSaveInstance.loadDraft() as { username?: string; email?: string } | null;
		if (draft && (draft.username !== user.username || draft.email !== user.email)) {
			const restore = confirm(
				'Found unsaved changes from a previous session. Would you like to restore them?'
			);
			if (restore) {
				accountForm.username = draft.username || accountForm.username;
				accountForm.email = draft.email || accountForm.email;
				dispatch('success', { message: 'Draft changes restored!' });
			} else {
				autoSaveInstance.clearDraft();
			}
		}
	}

	// Trigger auto-save when form data changes (but only for non-sensitive fields)
	$: if (autoSaveInstance && autoSaveEnabled && editMode) {
		const draftData = {
			username: accountForm.username,
			email: accountForm.email
		};
		autoSaveInstance.saveDraft(draftData);
		autoSaveInstance.trigger();
	}

	// Reactively update form fields when user changes - but only after initialization
	$: if (user && !isInitializing) {
		accountForm.username = user.username;
		accountForm.email = user.email;
		originalData = { username: user.username, email: user.email };
		profilePicturePreview = user.profilePictureUrl || null;
	}

	$: passwordMatch =
		accountForm.newPassword &&
		accountForm.confirmPassword &&
		accountForm.newPassword === accountForm.confirmPassword;

	function enableEditMode() {
		editMode = true;
		originalData = { username: accountForm.username, email: accountForm.email };
		
		// Setup auto-save when entering edit mode
		if (autoSaveEnabled) {
			setupAutoSave();
		}
	}

	function cancelEdit() {
		editMode = false;
		// Reset form to original values
		accountForm.username = originalData.username;
		accountForm.email = originalData.email;
		accountForm.currentPassword = '';
		accountForm.newPassword = '';
		accountForm.confirmPassword = '';
		accountForm.profilePicture = null;
		profilePicturePreview = user.profilePictureUrl || null;
		showNewPassword = false;
		showConfirmPassword = false;

		// Clean up auto-save
		autoSaveInstance?.destroy();
		autoSaveInstance?.clearDraft();
		autoSaveInstance = null;
	}

	function handleProfilePictureChange(event: Event) {
		const target = event.target as HTMLInputElement;
		const file = target.files?.[0];
		
		if (file) {
			// Validate file type
			if (!file.type.startsWith('image/')) {
				dispatch('error', { message: 'Please select a valid image file.' });
				return;
			}
			
			// Validate file size (5MB limit)
			if (file.size > 5 * 1024 * 1024) {
				dispatch('error', { message: 'Profile picture must be smaller than 5MB.' });
				return;
			}
			
			accountForm.profilePicture = file;
			
			// Create preview
			const reader = new FileReader();
			reader.onload = (e) => {
				profilePicturePreview = e.target?.result as string;
			};
			reader.readAsDataURL(file);
		}
	}

	function removeProfilePicture() {
		accountForm.profilePicture = null;
		profilePicturePreview = null;
		// Clear the file input
		const fileInput = document.getElementById('profile-picture-input') as HTMLInputElement;
		if (fileInput) fileInput.value = '';
	}

	async function handleGeneratePassword() {
		isGeneratingPassword = true;
		try {
			const generatedPassword = await generatePassword();
			accountForm.newPassword = generatedPassword;
			accountForm.confirmPassword = generatedPassword;
			// Show the generated password so user can see and copy it
			showNewPassword = true;
			showConfirmPassword = true;
		} catch (error) {
			dispatch('error', { message: 'Failed to generate password. Please try again.' });
			console.error('Error generating password:', error);
		} finally {
			isGeneratingPassword = false;
		}
	}

	function copyPasswordToClipboard() {
		if (accountForm.newPassword) {
			navigator.clipboard.writeText(accountForm.newPassword).then(() => {
				dispatch('success', { message: 'Password copied to clipboard!' });
			}).catch(() => {
				dispatch('error', { message: 'Failed to copy password to clipboard.' });
			});
		}
	}

	async function updateAccount() {
        console.log('Updating account with form data:', accountForm);
		if (accountForm.newPassword && !passwordMatch) {
			dispatch('error', { message: 'Passwords do not match' });
			return;
		}

		dispatch('loading', { isLoading: true });

		try {
			const result = await updateUser(
				accountForm.username,
				accountForm.newPassword ? accountForm.newPassword : undefined,
				accountForm.username, // or accountForm.name if you add a name field
				accountForm.profilePicture || undefined
			);
            console.log('Update user result:', result);
			if (result.success && result.user) {
				// Update the user object
				user.username = result.user.username ?? user.username;
				user.profilePictureUrl = result.user.profilePictureUrl ?? user.profilePictureUrl;
				accountForm.username = user.username;
				
				// Clear form fields and exit edit mode
				accountForm.currentPassword = '';
				accountForm.newPassword = '';
				accountForm.confirmPassword = '';
				accountForm.profilePicture = null;
				profilePicturePreview = user.profilePictureUrl || null;
				showNewPassword = false;
				showConfirmPassword = false;
				editMode = false;

				// Clear auto-save draft on successful save
				autoSaveInstance?.clearDraft();
				autoSaveInstance?.destroy();
				autoSaveInstance = null;

				dispatch('success', { message: 'Account updated successfully!' });
                // Update authStore with new user data
				authStore.setUser({ 
					...user, 
					username: result.user.username,
					profilePictureUrl: result.user.profilePictureUrl
				});
			} else {
				dispatch('error', { message: result.message || 'Failed to update account.' });
			}
		} catch (error) {
			dispatch('error', { message: 'Failed to update account. Please try again.' });
			console.error('Error updating account:', error);
		} finally {
			dispatch('loading', { isLoading: false });
		}
	}
</script>

<div class="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
	<h2 class="mb-6 text-xl font-semibold text-gray-900">Account Information</h2>

	{#if isInitializing}
		<!-- Loading skeleton while initializing -->
		<div class="space-y-6 animate-pulse">
			<!-- Profile Picture Skeleton -->
			<div class="flex items-center space-x-6">
				<div class="h-20 w-20 bg-gray-200 rounded-full"></div>
				<div>
					<div class="h-6 bg-gray-200 rounded w-32 mb-2"></div>
				</div>
			</div>
			
			<!-- Account Details Skeleton -->
			<div class="space-y-4">
				<div>
					<div class="h-4 bg-gray-200 rounded w-20 mb-2"></div>
					<div class="h-12 bg-gray-100 rounded-xl w-full"></div>
				</div>
				<div>
					<div class="h-4 bg-gray-200 rounded w-28 mb-2"></div>
					<div class="h-12 bg-gray-100 rounded-xl w-full"></div>
				</div>
			</div>
			
			<!-- Button Skeleton -->
			<div class="flex justify-end">
				<div class="h-12 bg-gray-200 rounded-xl w-32"></div>
			</div>
		</div>
	{:else if !editMode}
		<!-- View Mode -->
		<div class="space-y-6">
			<!-- Profile Picture -->
			<div class="flex items-center space-x-6">
				<div class="flex h-20 w-20 items-center justify-center rounded-full bg-indigo-100 overflow-hidden">
					{#if user.profilePictureUrl}
						<img 
							src={user.profilePictureUrl} 
							alt={user.username}
							class="h-full w-full object-cover"
							on:error={(e) => {
								// Fallback to initials if image fails to load
								const imgElement = e.target as HTMLImageElement;
								const fallbackSpan = imgElement.nextElementSibling as HTMLSpanElement;
								if (imgElement && fallbackSpan) {
									imgElement.style.display = 'none';
									fallbackSpan.style.display = 'flex';
								}
							}}
						/>
						<span class="text-2xl font-semibold text-indigo-600 hidden items-center justify-center h-full w-full">
							{user.username.charAt(0).toUpperCase()}
						</span>
					{:else}
						<span class="text-2xl font-semibold text-indigo-600">
							{user.username.charAt(0).toUpperCase()}
						</span>
					{/if}
				</div>
				<div>
					<h3 class="text-lg font-medium text-gray-900">{user.username}</h3>
				</div>
			</div>

			<!-- Account Details -->
			<div class="space-y-4">
				<div>
					<div class="mb-2 block text-sm font-medium text-gray-700">Username</div>
					<div class="w-full rounded-xl border border-gray-200 bg-gray-50 px-4 py-3 text-gray-900">
						{user.username}
					</div>
				</div>

				<div>
					<div class="mb-2 block text-sm font-medium text-gray-700">Email Address</div>
					<div class="w-full rounded-xl border border-gray-200 bg-gray-50 px-4 py-3 text-gray-900">
						{user.email}
					</div>
				</div>

                
			</div>

			<div class="flex justify-end">
				<button
					type="button"
					class="rounded-xl bg-indigo-600 px-6 py-3 text-white transition-colors hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
					on:click={enableEditMode}
				>
					Edit Profile
				</button>
			</div>
		</div>
	{:else}
		<!-- Edit Mode -->
		<form on:submit|preventDefault={updateAccount} class="space-y-6" autocomplete="off">
			<!-- Auto-save Status Indicator -->
			{#if autoSaveEnabled && autoSaveInstance && editMode}
				<div class="flex items-center justify-between rounded-lg bg-gray-50 px-3 py-2 text-sm">
					<div class="flex items-center space-x-2">
						<svg class="h-4 w-4 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
						</svg>
						<span class="text-gray-600">Auto-save draft</span>
					</div>
					<div class="flex items-center space-x-2">
						{#if saveStatus === 'saving'}
							<svg class="h-3 w-3 animate-spin {getStatusColor(saveStatus)}" fill="none" viewBox="0 0 24 24">
								<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
								<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
							</svg>
						{:else if saveStatus === 'saved'}
							<svg class="h-3 w-3 {getStatusColor(saveStatus)}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
							</svg>
						{:else if saveStatus === 'error'}
							<svg class="h-3 w-3 {getStatusColor(saveStatus)}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
							</svg>
						{/if}
						<span class="{getStatusColor(saveStatus)} font-medium">
							{getStatusText(saveStatus)}
						</span>
					</div>
				</div>
			{/if}
			
			<!-- Hidden fields to prevent autofill -->
			<input type="text" style="display:none" autocomplete="off" />
			<input type="password" style="display:none" autocomplete="off" />
			
			<!-- Profile Picture -->
			<div class="flex items-center space-x-6">
				<div class="flex h-20 w-20 items-center justify-center rounded-full bg-indigo-100 overflow-hidden">
					{#if profilePicturePreview}
						<img 
							src={profilePicturePreview} 
							alt="Profile preview"
							class="h-full w-full object-cover"
						/>
					{:else}
						<span class="text-2xl font-semibold text-indigo-600">
							{accountForm.username.charAt(0).toUpperCase()}
						</span>
					{/if}
				</div>
				<div class="flex flex-col space-y-2">
					<label 
						for="profile-picture-input"
						class="cursor-pointer rounded-lg bg-indigo-50 px-4 py-2 text-sm text-indigo-600 hover:bg-indigo-100 focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-colors"
					>
						{profilePicturePreview ? 'Change Picture' : 'Upload Picture'}
					</label>
					<input
						id="profile-picture-input"
						type="file"
						accept="image/*"
						on:change={handleProfilePictureChange}
						class="hidden"
					/>
					{#if profilePicturePreview}
						<button
							type="button"
							on:click={removeProfilePicture}
							class="text-sm text-red-600 hover:text-red-800 transition-colors"
						>
							Remove Picture
						</button>
					{/if}
					<p class="text-xs text-gray-500">
						JPG, PNG or GIF (max. 5MB)
					</p>
				</div>
			</div>

			<!-- Username Field -->
			<div>
				<label for="edit-username" class="mb-2 block text-sm font-medium text-gray-700">Username</label>
				<input
					id="edit-username"
					name="edit-username"
					type="text"
					bind:value={accountForm.username}
					required
					class="w-full rounded-xl border border-gray-200 px-4 py-3 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
					autocomplete="off"
					spellcheck="false"
				/>
			</div>

			<!-- Email Field -->
			<div>
				<label for="edit-email" class="mb-2 block text-sm font-medium text-gray-700">Email Address</label>
				<input
					id="edit-email"
					name="edit-email"
					type="email"
					bind:value={accountForm.email}
					required
					class="w-full rounded-xl border border-gray-200 px-4 py-3 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
					autocomplete="off"
					spellcheck="false"
				/>
			</div>

			<!-- Change Password Section -->
			<div class="border-t border-gray-100 pt-6">
				<h3 class="mb-4 text-lg font-medium text-gray-900">Change Password (Optional)</h3>
				<p class="mb-4 text-sm text-gray-600">Leave password fields blank to keep your current password</p>
				
				{#if accountForm.newPassword && showNewPassword}
					<div class="mb-4 rounded-lg border border-amber-200 bg-amber-50 p-4">
						<div class="flex items-center space-x-2">
							<svg class="h-5 w-5 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
							</svg>
							<p class="text-sm font-medium text-amber-800">Important: Save your new password!</p>
						</div>
						<p class="mt-1 text-sm text-amber-700">
							Make sure to copy and save your new password before continuing. You'll need it to log in again.
						</p>
					</div>
				{/if}

				<div class="space-y-4">
					<div class="grid grid-cols-1 gap-4 md:grid-cols-2">
						<div>
							<label for="edit-new-password" class="mb-2 block text-sm font-medium text-gray-700">New Password</label>
							<div class="relative">
								<input
									id="edit-new-password"
									name="edit-new-password"
									type={showNewPassword ? "text" : "password"}
									bind:value={accountForm.newPassword}
									class="w-full rounded-xl border border-gray-200 px-4 py-3 pr-20 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
									placeholder="Enter new password"
									autocomplete="new-password"
								/>
								<div class="absolute right-2 top-1/2 -translate-y-1/2 flex space-x-1">
									{#if accountForm.newPassword && showNewPassword}
										<button
											type="button"
											class="rounded-lg bg-green-100 p-2 text-xs text-green-600 hover:bg-green-200 focus:outline-none focus:ring-2 focus:ring-green-500"
											on:click={copyPasswordToClipboard}
											title="Copy password to clipboard"
                                            aria-label="Copy password to clipboard"
										>
											<svg class="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
												<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"></path>
											</svg>
										</button>
									{/if}
									{#if accountForm.newPassword}
										<button
											type="button"
											class="rounded-lg bg-gray-100 p-2 text-xs text-gray-600 hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-indigo-500"
											on:click={() => showNewPassword = !showNewPassword}
											title={showNewPassword ? "Hide password" : "Show password"}
										>
											{#if showNewPassword}
												<svg class="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
													<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21"></path>
												</svg>
											{:else}
												<svg class="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
													<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
													<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
												</svg>
											{/if}
										</button>
									{/if}
									<button
										type="button"
										class="rounded-lg bg-gray-100 p-2 text-xs text-gray-600 hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-indigo-500"
										on:click={handleGeneratePassword}
										disabled={isGeneratingPassword}
										title="Generate secure password based on your preferences"
									>
										{#if isGeneratingPassword}
											<svg class="h-3 w-3 animate-spin" fill="none" viewBox="0 0 24 24">
												<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
												<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
											</svg>
										{:else}
											<svg class="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
												<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
											</svg>
										{/if}
									</button>
								</div>
							</div>
						</div>

						<div>
							<label for="edit-confirm-password" class="mb-2 block text-sm font-medium text-gray-700">Confirm New Password</label>
							<div class="relative">
								<input
									id="edit-confirm-password"
									name="edit-confirm-password"
									type={showConfirmPassword ? "text" : "password"}
									bind:value={accountForm.confirmPassword}
									class="w-full rounded-xl border border-gray-200 px-4 py-3 pr-12 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
									placeholder="Confirm new password"
									autocomplete="new-password"
								/>
								{#if accountForm.confirmPassword}
									<button
										type="button"
										class="absolute right-2 top-1/2 -translate-y-1/2 rounded-lg bg-gray-100 p-2 text-xs text-gray-600 hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-indigo-500"
										on:click={() => showConfirmPassword = !showConfirmPassword}
										title={showConfirmPassword ? "Hide password" : "Show password"}
									>
										{#if showConfirmPassword}
											<svg class="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
												<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21"></path>
											</svg>
										{:else}
											<svg class="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
												<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
												<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
											</svg>
										{/if}
									</button>
								{/if}
							</div>
						</div>
					</div>

					{#if accountForm.newPassword && accountForm.confirmPassword && !passwordMatch}
						<p class="text-sm text-red-600">Passwords do not match</p>
					{/if}
				</div>
			</div>

			<div class="flex justify-end space-x-3">
				<button
					type="button"
					class="rounded-xl border border-gray-300 px-6 py-3 text-gray-700 transition-colors hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
					on:click={cancelEdit}
				>
					Cancel
				</button>
				<button
					type="submit"
					disabled={isLoading || (!!accountForm.newPassword && !passwordMatch)}
					class="rounded-xl bg-indigo-600 px-6 py-3 text-white transition-colors hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50"
				>
					{isLoading ? 'Saving...' : 'Save Changes'}
				</button>
			</div>
		</form>
	{/if}
</div>