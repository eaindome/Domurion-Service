
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import type { VaultItem } from '$lib/types';
  import { maskPassword, getSiteFavicon } from '$lib/utils/helpers';

  export let item: VaultItem;

  const dispatch = createEventDispatcher();

  let showDetails = false;

  function handleCopy(value: string, type: string) {
    dispatch('copy', { value, type });
  }
  function handleEdit() {
    dispatch('edit', { id: item.id });
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

<div class="p-6 hover:bg-gray-50 transition-colors">
  <div class="grid grid-cols-6 gap-4 items-center">
    <!-- Site Icon -->
    <div class="flex justify-center">
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

    <!-- Site Details -->
    <div class="min-w-0">
      <div class="flex items-center space-x-2">
        <h3 class="text-sm font-medium text-gray-900 truncate">{item.siteName}</h3>
        <span class="text-xs text-gray-500">•</span>
        <span class="text-xs text-gray-500 truncate">{item.siteUrl}</span>
      </div>
      {#if item.notes}
        <p class="mt-1 text-xs text-gray-500 truncate">{item.notes}</p>
      {/if}
    </div>

    <!-- Username -->
    <div class="flex items-center space-x-2">
      <span class="text-xs text-gray-500">Username:</span>
      <span class="text-sm text-gray-900 truncate">{item.username}</span>
      <button
        on:click={() => handleCopy(item.username, 'Username')}
        class="text-gray-400 hover:text-gray-600 transition-colors"
        title="Copy username"
        aria-label="Copy username"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
        </svg>
      </button>
    </div>

    <!-- Password -->
    <div class="flex items-center space-x-2">
      <span class="text-xs text-gray-500">Password:</span>
      <span class="text-sm text-gray-900 font-mono truncate">{maskPassword(item.password)}</span>
      <button
        on:click={() => handleCopy(item.password, 'Password')}
        class="text-gray-400 hover:text-gray-600 transition-colors"
        title="Copy password"
        aria-label="Copy password"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
        </svg>
      </button>
    </div>

    <!-- Notes (hidden on small screens, already shown above for details) -->
    <div class="hidden md:block">
      {#if item.notes}
        <span class="text-xs text-gray-500 truncate">{item.notes}</span>
      {/if}
    </div>

    <!-- Actions -->
    <div class="flex items-center space-x-2 justify-end">
      <!-- View (eye) icon -->
      <button
        on:click={handleView}
        class="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
        title="View details"
        aria-label="View details"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.477 0 8.268 2.943 9.542 7-1.274 4.057-5.065 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
        </svg>
      </button>
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
        on:click={handleDelete}
        class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
        title="Delete entry"
        aria-label="Delete entry"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
        </svg>
      </button>
    </div>

    <!-- Details Modal -->
    {#if showDetails}
      <div class="fixed inset-0 z-50 flex items-center justify-center backdrop-blur-sm bg-white/10">
        <div class="bg-white/80 backdrop-blur-lg rounded-2xl shadow-2xl max-w-md w-full p-8 relative border border-gray-200">
          <button
            class="absolute top-3 right-3 text-gray-400 hover:text-gray-700 focus:outline-none"
            on:click={closeModal}
            aria-label="Close details"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
          <div class="flex flex-col items-center gap-2 mb-6">
            <img
              src={getSiteFavicon(item.siteUrl)}
              alt="{item.siteName} favicon"
              class="w-14 h-14 rounded-lg border border-gray-200 shadow-sm mb-1"
            />
            <h2 class="text-xl font-bold text-gray-900">{item.siteName}</h2>
            <span class="text-xs text-gray-500">{item.siteUrl}</span>
          </div>
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <span class="font-medium text-gray-700">Username:</span>
              <span class="text-gray-900 break-all">{item.username}</span>
            </div>
            <div class="flex items-center justify-between">
              <span class="font-medium text-gray-700">Password:</span>
              <span class="text-gray-900 font-mono break-all">{item.password}</span>
            </div>
            {#if item.notes}
              <div class="flex items-start justify-between">
                <span class="font-medium text-gray-700">Notes:</span>
                <span class="text-gray-900 text-right break-words max-w-[60%]">{item.notes}</span>
              </div>
            {/if}
            <div class="flex items-center justify-between">
              <span class="font-medium text-gray-700">Created:</span>
              <span class="text-gray-900">{item.createdAt}</span>
            </div>
            <div class="flex items-center justify-between">
              <span class="font-medium text-gray-700">Updated:</span>
              <span class="text-gray-900">{item.updatedAt}</span>
            </div>
          </div>
        </div>
      </div>
    {/if}
  </div>
</div>
