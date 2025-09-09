<script lang=ts>
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  
  // Mock data - replace with actual API calls
  let vaultItems = [
    {
      id: 1,
      siteName: 'Google',
      siteUrl: 'google.com',
      username: 'john.doe@email.com',
      password: 'MySecurePassword123!',
      notes: 'Main Google account',
      createdAt: '2024-01-15',
      updatedAt: '2024-01-20'
    },
    {
      id: 2,
      siteName: 'GitHub',
      siteUrl: 'github.com',
      username: 'johndoe',
      password: 'GitHubSecure456@',
      notes: 'Development account',
      createdAt: '2024-01-10',
      updatedAt: '2024-01-18'
    },
    {
      id: 3,
      siteName: 'Netflix',
      siteUrl: 'netflix.com',
      username: 'john.doe@email.com',
      password: 'Netflix789#',
      notes: 'Family subscription',
      createdAt: '2024-01-05',
      updatedAt: '2024-01-12'
    }
  ];
  
  let searchQuery = '';
  let showDeleteModal = false;
  let itemToDelete: typeof vaultItems[0] | null = null;
  let isLoading = false;
  
  // User info - you'll get this from your auth store
  let user = {
    email: 'john.doe@email.com',
    name: 'John Doe'
  };
  
  // Filter vault items based on search
  $: filteredItems = vaultItems.filter(item => 
    item.siteName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    item.username.toLowerCase().includes(searchQuery.toLowerCase()) ||
    item.siteUrl.toLowerCase().includes(searchQuery.toLowerCase())
  );
  
  onMount(async () => {
    // TODO: Fetch vault items from API
    await loadVaultItems();
  });
  
  async function loadVaultItems() {
    isLoading = true;
    try {
      // TODO: Replace with actual API call
      // const response = await fetch('/api/vault');
      // vaultItems = await response.json();
    } catch (error) {
      console.error('Failed to load vault items:', error);
    } finally {
      isLoading = false;
    }
  }
  
  function maskPassword(password: string) {
    return '●'.repeat(password.length);
  }
  
  function copyToClipboard(text: string, type: string) {
    navigator.clipboard.writeText(text).then(() => {
      // You can show a toast notification here
      console.log(`${type} copied to clipboard`);
    });
  }
  
  function confirmDelete(item: typeof vaultItems[0]) {
    itemToDelete = item;
    showDeleteModal = true;
  }
  
  async function deleteItem() {
    if (!itemToDelete) return;
    
    try {
      // TODO: Replace with actual API call
      // await fetch(`/api/vault/${itemToDelete.id}`, { method: 'DELETE' });
      
      vaultItems = vaultItems.filter(item => item.id !== itemToDelete!.id);
      showDeleteModal = false;
      itemToDelete = null;
    } catch (error) {
      console.error('Failed to delete item:', error);
    }
  }
  
  function logout() {
    // TODO: Clear auth store and redirect
    goto('/login');
  }
  
  function getSiteFavicon(siteUrl: string) {
    return `https://www.google.com/s2/favicons?domain=${siteUrl}&sz=32`;
  }
</script>

<svelte:head>
  <title>Dashboard - Vault</title>
</svelte:head>

<div class="min-h-screen bg-gray-50">
  <!-- Navigation Header -->
  <nav class="bg-white shadow-sm border-b border-gray-100">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center h-16">
        <!-- Logo and Brand -->
        <div class="flex items-center">
          <div class="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center">
            <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
            </svg>
          </div>
          <span class="ml-3 text-xl font-semibold text-gray-900">Vault</span>
        </div>
        
        <!-- User Menu -->
        <div class="flex items-center space-x-4">
          <span class="text-sm text-gray-700">Welcome, {user.name}</span>
          <div class="relative">
            <button
              class="flex items-center text-sm rounded-full focus:outline-none focus:ring-2 focus:ring-indigo-500"
            >
              <div class="w-8 h-8 bg-indigo-100 rounded-full flex items-center justify-center">
                <span class="text-indigo-600 font-medium">{user.name.charAt(0)}</span>
              </div>
            </button>
          </div>
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
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <!-- Header Section -->
    <div class="px-4 sm:px-0">
      <div class="flex justify-between items-center mb-8">
        <div>
          <h1 class="text-3xl font-semibold text-gray-900">Your Vault</h1>
          <p class="mt-2 text-gray-600">Manage your passwords and credentials securely</p>
        </div>
        
        <a 
          href="/vault/add"
          class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-xl text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-all duration-200"
        >
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"/>
          </svg>
          Add New Entry
        </a>
      </div>
      
      <!-- Search and Stats -->
      <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 mb-6">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between space-y-4 sm:space-y-0">
          <!-- Search Bar -->
          <div class="relative flex-1 max-w-md">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
              </svg>
            </div>
            <input
              type="text"
              bind:value={searchQuery}
              placeholder="Search your vault..."
              class="block w-full pl-10 pr-3 py-2.5 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-sm"
            />
          </div>
          
          <!-- Stats -->
          <div class="flex items-center space-x-6 text-sm text-gray-600">
            <div class="flex items-center">
              <div class="w-2 h-2 bg-indigo-500 rounded-full mr-2"></div>
              <span>{vaultItems.length} total entries</span>
            </div>
            <div class="flex items-center">
              <div class="w-2 h-2 bg-green-500 rounded-full mr-2"></div>
              <span>{filteredItems.length} shown</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Vault Items -->
    <div class="px-4 sm:px-0">
      {#if isLoading}
        <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-12">
          <div class="text-center">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600 mx-auto"></div>
            <p class="mt-4 text-gray-600">Loading your vault...</p>
          </div>
        </div>
      {:else if filteredItems.length === 0}
        <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-12">
          <div class="text-center">
            <svg class="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
            </svg>
            <h3 class="text-lg font-medium text-gray-900 mb-2">
              {searchQuery ? 'No matching entries' : 'Your vault is empty'}
            </h3>
            <p class="text-gray-600 mb-6">
              {searchQuery ? 'Try adjusting your search terms' : 'Start by adding your first password entry'}
            </p>
            {#if !searchQuery}
              <a 
                href="/vault/add"
                class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-xl text-white bg-indigo-600 hover:bg-indigo-700 transition-colors"
              >
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"/>
                </svg>
                Add Your First Entry
              </a>
            {/if}
          </div>
        </div>
      {:else}
        <div class="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
          <div class="divide-y divide-gray-100">
            {#each filteredItems as item (item.id)}
              <div class="p-6 hover:bg-gray-50 transition-colors">
                <div class="flex items-center justify-between">
                  <div class="flex items-center space-x-4 flex-1">
                    <!-- Site Icon -->
                    <div class="flex-shrink-0">
                      <img 
                        src={getSiteFavicon(item.siteUrl)} 
                        alt="{item.siteName} favicon"
                        class="w-10 h-10 rounded-lg border border-gray-200"
                        on:error={(e) => {
                          const target = e.target as HTMLImageElement | null;
                          if (target) {
                            target.src = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23e5e7eb'%3E%3Cpath d='M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z'/%3E%3C/svg%3E";
                          }
                        }}
                      />
                    </div>
                    
                    <!-- Item Details -->
                    <div class="flex-1 min-w-0">
                      <div class="flex items-center space-x-2">
                        <h3 class="text-sm font-medium text-gray-900 truncate">{item.siteName}</h3>
                        <span class="text-xs text-gray-500">•</span>
                        <span class="text-xs text-gray-500 truncate">{item.siteUrl}</span>
                      </div>
                      <div class="mt-1 flex items-center space-x-4">
                        <div class="flex items-center space-x-2">
                          <span class="text-xs text-gray-500">Username:</span>
                          <span class="text-sm text-gray-900">{item.username}</span>
                          <button
                            on:click={() => copyToClipboard(item.username, 'Username')}
                            class="text-gray-400 hover:text-gray-600 transition-colors"
                            title="Copy username"
                            aria-label="Copy username"
                          >
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
                            </svg>
                          </button>
                        </div>
                        <div class="flex items-center space-x-2">
                          <span class="text-xs text-gray-500">Password:</span>
                          <span class="text-sm text-gray-900 font-mono">{maskPassword(item.password)}</span>
                          <button
                            on:click={() => copyToClipboard(item.password, 'Password')}
                            class="text-gray-400 hover:text-gray-600 transition-colors"
                            title="Copy password"
                            aria-label="Copy password"
                          >
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
                            </svg>
                          </button>
                        </div>
                      </div>
                      {#if item.notes}
                        <p class="mt-1 text-xs text-gray-500 truncate">{item.notes}</p>
                      {/if}
                    </div>
                  </div>
                  
                  <!-- Actions -->
                  <div class="flex items-center space-x-2 ml-4">
                    <a
                      href="/vault/{item.id}/edit"
                      class="p-2 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors"
                      title="Edit entry"
                      aria-label="Edit entry"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
                      </svg>
                    </a>
                    <button
                      on:click={() => confirmDelete(item)}
                      class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                      title="Delete entry"
                      aria-label="Delete entry"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                      </svg>
                    </button>
                  </div>
                </div>
              </div>
            {/each}
          </div>
        </div>
      {/if}
    </div>
  </div>
</div>

<!-- Delete Confirmation Modal -->
{#if showDeleteModal}
  <div class="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center p-4 z-50">
    <div class="bg-white rounded-2xl shadow-xl max-w-md w-full p-6">
      <div class="flex items-center justify-center w-12 h-12 mx-auto bg-red-100 rounded-full mb-4">
        <svg class="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"/>
        </svg>
      </div>
      
      <h3 class="text-lg font-semibold text-gray-900 text-center mb-2">
        Delete Entry
      </h3>
      
      <p class="text-sm text-gray-600 text-center mb-6">
        Are you sure you want to delete the entry for <strong>{itemToDelete?.siteName}</strong>? This action cannot be undone.
      </p>
      
      <div class="flex space-x-3">
        <button
          on:click={() => { showDeleteModal = false; itemToDelete = null; }}
          class="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-xl transition-colors"
        >
          Cancel
        </button>
        <button
          on:click={deleteItem}
          class="flex-1 px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-xl transition-colors"
        >
          Delete
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