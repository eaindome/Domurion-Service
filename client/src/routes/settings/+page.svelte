<script lang=ts>
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
  $: passwordMatch = accountForm.newPassword && accountForm.confirmPassword && 
                     accountForm.newPassword === accountForm.confirmPassword;
  $: passwordTooShort = accountForm.newPassword && accountForm.newPassword.length < 8;
  
  onMount(() => {
    // Load user settings from API
    loadSettings();
  });
  
  async function loadSettings() {
    try {
      // TODO: Replace with actual API calls
      // const response = await fetch('/api/user/settings');
      // const settings = await response.json();
      // Update local state with fetched settings
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
      const updateData: any = {
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
      errorMessage = 'Failed to update account. Please try again.';
    } finally {
      isLoading = false;
      setTimeout(() => { successMessage = ''; errorMessage = ''; }, 3000);
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
      
      successMessage = 'Security settings updated successfully!';
    } catch (error) {
      errorMessage = 'Failed to update security settings.';
    } finally {
      isLoading = false;
      setTimeout(() => { successMessage = ''; errorMessage = ''; }, 3000);
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
      errorMessage = 'Failed to update vault preferences.';
    } finally {
      isLoading = false;
      setTimeout(() => { successMessage = ''; errorMessage = ''; }, 3000);
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
      errorMessage = 'Failed to export vault.';
    } finally {
      exportInProgress = false;
      setTimeout(() => { successMessage = ''; errorMessage = ''; }, 3000);
    }
  }
  
  function deleteAccount() {
    if (confirm('Are you sure you want to delete your account? This action cannot be undone and will permanently delete all your vault entries.')) {
      if (confirm('This will permanently delete ALL your passwords and data. Are you absolutely sure?')) {
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
  <nav class="bg-white shadow-sm border-b border-gray-100">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center h-16">
        <!-- Logo and Brand -->
        <div class="flex items-center">
          <a href="/dashboard" class="flex items-center">
            <div class="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center">
              <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
              </svg>
            </div>
            <span class="ml-3 text-xl font-semibold text-gray-900">Vault</span>
          </a>
        </div>
        
        <!-- User Menu -->
        <div class="flex items-center space-x-4">
          <a href="/dashboard" class="text-sm text-gray-600 hover:text-gray-900 transition-colors">
            ← Back to Dashboard
          </a>
          <button
            on:click={logout}
            class="text-sm text-gray-600 hover:text-gray-900 transition-colors"
          >
            Sign out
          </button>
        </div>
      </div>
    </div>
  </nav>

  <!-- Main Content -->
  <div class="max-w-4xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-3xl font-semibold text-gray-900">Account Settings</h1>
      <p class="mt-2 text-gray-600">Manage your account, security, and vault preferences</p>
    </div>

    <!-- Status Messages -->
    {#if successMessage}
      <div class="mb-6 bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-xl text-sm">
        {successMessage}
      </div>
    {/if}
    
    {#if errorMessage}
      <div class="mb-6 bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-xl text-sm">
        {errorMessage}
      </div>
    {/if}

    <div class="flex flex-col lg:flex-row gap-8">
      <!-- Sidebar Navigation -->
      <div class="lg:w-64 flex-shrink-0">
        <nav class="bg-white rounded-2xl shadow-sm border border-gray-100 p-2">
          <div class="space-y-1">
            <button
              on:click={() => activeTab = 'account'}
              class="w-full text-left px-4 py-3 rounded-xl text-sm font-medium transition-all duration-200 {activeTab === 'account' ? 'bg-indigo-50 text-indigo-700' : 'text-gray-600 hover:bg-gray-50'}"
            >
              <div class="flex items-center">
                <svg class="w-5 h-5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                </svg>
                Account
              </div>
            </button>
            
            <button
              on:click={() => activeTab = 'security'}
              class="w-full text-left px-4 py-3 rounded-xl text-sm font-medium transition-all duration-200 {activeTab === 'security' ? 'bg-indigo-50 text-indigo-700' : 'text-gray-600 hover:bg-gray-50'}"
            >
              <div class="flex items-center">
                <svg class="w-5 h-5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"/>
                </svg>
                Security
              </div>
            </button>
            
            <button
              on:click={() => activeTab = 'vault'}
              class="w-full text-left px-4 py-3 rounded-xl text-sm font-medium transition-all duration-200 {activeTab === 'vault' ? 'bg-indigo-50 text-indigo-700' : 'text-gray-600 hover:bg-gray-50'}"
            >
              <div class="flex items-center">
                <svg class="w-5 h-5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                </svg>
                Preferences
              </div>
            </button>
            
            <button
              on:click={() => activeTab = 'data'}
              class="w-full text-left px-4 py-3 rounded-xl text-sm font-medium transition-all duration-200 {activeTab === 'data' ? 'bg-indigo-50 text-indigo-700' : 'text-gray-600 hover:bg-gray-50'}"
            >
              <div class="flex items-center">
                <svg class="w-5 h-5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M9 19l3 3m0 0l3-3m-3 3V10"/>
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
          <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-xl font-semibold text-gray-900 mb-6">Account Information</h2>
            
            <form on:submit|preventDefault={updateAccount} class="space-y-6">
              <!-- Profile Picture -->
              <div class="flex items-center space-x-6">
                <div class="w-20 h-20 bg-indigo-100 rounded-full flex items-center justify-center">
                  <span class="text-2xl font-semibold text-indigo-600">{user.name.charAt(0)}</span>
                </div>
                <div>
                  <h3 class="text-lg font-medium text-gray-900">{user.name}</h3>
                  <p class="text-sm text-gray-500">Member since {new Date(user.createdAt).toLocaleDateString()}</p>
                  <p class="text-sm text-gray-500">{user.vaultCount} vault entries • {user.subscriptionTier} plan</p>
                </div>
              </div>
              
              <!-- Name Field -->
              <div>
                <label for="name" class="block text-sm font-medium text-gray-700 mb-2">Full Name</label>
                <input
                  id="name"
                  type="text"
                  bind:value={accountForm.name}
                  required
                  class="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                />
              </div>
              
              <!-- Email Field -->
              <div>
                <label for="email" class="block text-sm font-medium text-gray-700 mb-2">Email Address</label>
                <input
                  id="email"
                  type="email"
                  bind:value={accountForm.email}
                  required
                  class="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                />
              </div>
              
              <!-- Change Password Section -->
              <div class="border-t border-gray-100 pt-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4">Change Password</h3>
                <div class="space-y-4">
                  <div>
                    <label for="currentPassword" class="block text-sm font-medium text-gray-700 mb-2">Current Password</label>
                    <input
                      id="currentPassword"
                      type="password"
                      bind:value={accountForm.currentPassword}
                      class="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                      placeholder="Enter current password to change"
                    />
                  </div>
                  
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label for="newPassword" class="block text-sm font-medium text-gray-700 mb-2">New Password</label>
                      <input
                        id="newPassword"
                        type="password"
                        bind:value={accountForm.newPassword}
                        class="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                        placeholder="Enter new password"
                      />
                    </div>
                    
                    <div>
                      <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-2">Confirm New Password</label>
                      <input
                        id="confirmPassword"
                        type="password"
                        bind:value={accountForm.confirmPassword}
                        class="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
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
                class="w-full md:w-auto px-6 py-3 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {isLoading ? 'Updating...' : 'Update Account'}
              </button>
            </form>
          </div>
        {/if}

        <!-- Security Tab -->
        {#if activeTab === 'security'}
          <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-xl font-semibold text-gray-900 mb-6">Security Settings</h2>
            
            <form on:submit|preventDefault={updateSecuritySettings} class="space-y-6">
              <!-- Two-Factor Authentication -->
              <div class="flex items-center justify-between p-4 bg-gray-50 rounded-xl">
                <div>
                  <h3 class="text-sm font-medium text-gray-900">Two-Factor Authentication</h3>
                  <p class="text-sm text-gray-500">Add an extra layer of security to your account</p>
                </div>
                <label class="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    bind:checked={securitySettings.twoFactorEnabled}
                    class="sr-only peer"
                  />
                  <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-indigo-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-indigo-600"></div>
                </label>
              </div>
              
              <!-- Session Timeout -->
              <div>
                <label for="sessionTimeout" class="block text-sm font-medium text-gray-700 mb-2">
                  Session Timeout (minutes)
                </label>
                <select
                  id="sessionTimeout"
                  bind:value={securitySettings.sessionTimeout}
                  class="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                >
                  <option value={15}>15 minutes</option>
                  <option value={30}>30 minutes</option>
                  <option value={60}>1 hour</option>
                  <option value={120}>2 hours</option>
                  <option value={0}>Never</option>
                </select>
              </div>
              
              <!-- Auto-lock -->
              <div class="flex items-center justify-between p-4 bg-gray-50 rounded-xl">
                <div>
                  <h3 class="text-sm font-medium text-gray-900">Auto-lock Vault</h3>
                  <p class="text-sm text-gray-500">Automatically lock when browser is idle</p>
                </div>
                <label class="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    bind:checked={securitySettings.autoLock}
                    class="sr-only peer"
                  />
                  <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-indigo-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-indigo-600"></div>
                </label>
              </div>
              
              <!-- Login Notifications -->
              <div class="flex items-center justify-between p-4 bg-gray-50 rounded-xl">
                <div>
                  <h3 class="text-sm font-medium text-gray-900">Login Notifications</h3>
                  <p class="text-sm text-gray-500">Get notified of new sign-ins to your account</p>
                </div>
                <label class="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    bind:checked={securitySettings.loginNotifications}
                    class="sr-only peer"
                  />
                  <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-indigo-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-indigo-600"></div>
                </label>
              </div>
              
              <button
                type="submit"
                disabled={isLoading}
                class="w-full md:w-auto px-6 py-3 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {isLoading ? 'Updating...' : 'Update Security Settings'}
              </button>
            </form>
          </div>
        {/if}

        <!-- Vault Preferences Tab -->
        {#if activeTab === 'vault'}
          <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
            <h2 class="text-xl font-semibold text-gray-900 mb-6">Vault Preferences</h2>
            
            <form on:submit|preventDefault={updateVaultPreferences} class="space-y-6">
              <!-- Password Generation -->
              <div>
                <h3 class="text-lg font-medium text-gray-900 mb-4">Password Generation</h3>
                <div class="space-y-4">
                  <div>
                    <label for="passwordLength" class="block text-sm font-medium text-gray-700 mb-2">
                      Default Password Length: {vaultPreferences.defaultPasswordLength}
                    </label>
                    <input
                      id="passwordLength"
                      type="range"
                      min="8"
                      max="64"
                      bind:value={vaultPreferences.defaultPasswordLength}
                      class="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer slider"
                    />
                    <div class="flex justify-between text-xs text-gray-500 mt-1">
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
                <h3 class="text-lg font-medium text-gray-900 mb-4">Vault Behavior</h3>
                <div class="space-y-4">
                  <div class="flex items-center justify-between p-4 bg-gray-50 rounded-xl">
                    <div>
                      <h4 class="text-sm font-medium text-gray-900">Auto-save entries</h4>
                      <p class="text-sm text-gray-500">Automatically save changes without confirmation</p>
                    </div>
                    <label class="relative inline-flex items-center cursor-pointer">
                      <input
                        type="checkbox"
                        bind:checked={vaultPreferences.autoSave}
                        class="sr-only peer"
                      />
                      <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-indigo-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-indigo-600"></div>
                    </label>
                    </div>
                    <div class="flex items-center justify-between p-4 bg-gray-50 rounded-xl">
                      <div>
                        <h4 class="text-sm font-medium text-gray-900">Show password strength</h4>
                        <p class="text-sm text-gray-500">Display password strength meter when creating passwords</p>
                      </div>
                      <label class="relative inline-flex items-center cursor-pointer">
                        <input
                          type="checkbox"
                          bind:checked={vaultPreferences.showPasswordStrength}
                          class="sr-only peer"
                        />
                        <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-indigo-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-indigo-600"></div>
                      </label>
                    </div>
                  </div>
                </div>
                <button
                  type="submit"
                  disabled={isLoading}
                  class="w-full md:w-auto px-6 py-3 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors mt-4"
                >
                  {isLoading ? 'Updating...' : 'Update Preferences'}
                </button>
              </form>
            </div>
        {/if}

        <!-- Data & Export Tab -->
        {#if activeTab === 'data'}
            <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                <h2 class="text-xl font-semibold text-gray-900 mb-6">Data & Export</h2>
                <div class="space-y-6">
                <div>
                    <h3 class="text-lg font-medium text-gray-900 mb-2">Export Vault Data</h3>
                    <p class="text-sm text-gray-500 mb-4">Download all your vault entries as a JSON file for backup or migration.</p>
                    <button
                    on:click={exportVault}
                    disabled={exportInProgress}
                    class="px-6 py-3 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                    {exportInProgress ? 'Exporting...' : 'Export Vault'}
                    </button>
                </div>
                <div class="border-t border-gray-100 pt-6">
                    <h3 class="text-lg font-medium text-gray-900 mb-2">Import Vault Data</h3>
                    <p class="text-sm text-gray-500 mb-4">Restore your vault from a previously exported JSON file.</p>
                    <input
                    type="file"
                    accept="application/json"
                    on:change={e => {
                      const target = e.target as HTMLInputElement | null;
                      importFile = target && target.files ? target.files[0] : null;
                    }}
                    class="mb-2"
                    />
                    <button
                    type="button"
                    disabled={!importFile}
                    class="px-6 py-3 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    on:click={() => {
                        // TODO: Implement import logic
                        successMessage = 'Import feature coming soon!';
                        setTimeout(() => { successMessage = ''; }, 2000);
                    }}
                    >
                    Import Vault
                    </button>
                </div>
                <div class="border-t border-gray-100 pt-6">
                    <h3 class="text-lg font-medium text-red-700 mb-2">Delete Account</h3>
                    <p class="text-sm text-red-500 mb-4">This action is irreversible. All your data will be permanently deleted.</p>
                    <button
                    type="button"
                    on:click={deleteAccount}
                    class="px-6 py-3 bg-red-600 text-white rounded-xl hover:bg-red-700 transition-colors"
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