<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import { generatePassword, copyToClipboard } from '../../utils/helpers';
  import type { VaultEntryErrors } from '$lib/types';

  export let formData: {
    siteName: string;
    url?: string;
    username: string;
    password: string;
    notes?: string;
  };
  export let errors: VaultEntryErrors = {};
  export let isSubmitting: boolean = false;
  export let mode: 'add' | 'edit' = 'add';

  // Handlers passed from parent
  export let onSubmit: () => void;
  export let onCancel: () => void;

  const dispatch = createEventDispatcher();

  let showPassword = false;

  function handleGeneratePassword() {
    formData.password = generatePassword();
  }

  function handleCopyPassword() {
    if (formData.password) {
      copyToClipboard(formData.password);
      dispatch('toast', { message: 'Password copied!', type: 'success' });
    }
  }
</script>

<form on:submit|preventDefault={onSubmit} class="p-6 space-y-6">
  <!-- Site Name -->
  <div>
    <label for="siteName" class="block text-sm font-medium text-gray-700 mb-2">
      Site Name *
    </label>
    <input
      type="text"
      id="siteName"
      bind:value={formData.siteName}
      placeholder="e.g., Google, GitHub, Facebook"
      class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors bg-white"
      class:border-red-300={errors.siteName}
      class:focus:ring-red-500={errors.siteName}
      class:focus:border-red-500={errors.siteName}
    />
    {#if errors.siteName}
      <p class="mt-1 text-sm text-red-600">{errors.siteName}</p>
    {/if}
  </div>

  <!-- Website URL -->
  <div>
    <label for="url" class="block text-sm font-medium text-gray-700 mb-2">
      Website URL
      <span class="text-gray-500 font-normal">(optional)</span>
    </label>
    <input
      type="url"
      id="url"
      bind:value={formData.url}
      placeholder="https://example.com"
      class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors bg-white"
    />
  </div>

  <!-- Username/Email -->
  <div>
    <label for="username" class="block text-sm font-medium text-gray-700 mb-2">
      Username/Email *
    </label>
    <input
      type="text"
      id="username"
      bind:value={formData.username}
      placeholder="your-username or email@example.com"
      class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors bg-white"
      class:border-red-300={errors.username}
      class:focus:ring-red-500={errors.username}
      class:focus:border-red-500={errors.username}
    />
    {#if errors.username}
      <p class="mt-1 text-sm text-red-600">{errors.username}</p>
    {/if}
  </div>

  <!-- Password -->
  <div>
    <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
      Password *
    </label>
    <div class="relative">
      <input
        type={showPassword ? 'text' : 'password'}
        id="password"
        bind:value={formData.password}
        placeholder="Enter or generate a secure password"
        class="w-full px-4 py-3 pr-20 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors bg-white"
        class:border-red-300={errors.password}
        class:focus:ring-red-500={errors.password}
        class:focus:border-red-500={errors.password}
      />
      <div class="absolute inset-y-0 right-0 flex items-center space-x-1 pr-3">
        <button
          type="button"
          on:click={() => showPassword = !showPassword}
          class="p-1 text-gray-400 hover:text-gray-600 transition-colors"
          title={showPassword ? 'Hide password' : 'Show password'}
        >
          {#if showPassword}
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21" />
            </svg>
          {:else}
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
            </svg>
          {/if}
        </button>
        {#if formData.password}
          <button
            type="button"
            on:click={handleCopyPassword}
            class="p-1 text-gray-400 hover:text-gray-600 transition-colors"
            title="Copy password"
            aria-label="Copy password"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
            </svg>
          </button>
        {/if}
      </div>
    </div>
    {#if errors.password}
      <p class="mt-1 text-sm text-red-600">{errors.password}</p>
    {/if}
    <button
      type="button"
      on:click={handleGeneratePassword}
      class="mt-2 text-sm text-indigo-600 hover:text-indigo-700 font-medium transition-colors"
    >
      Generate {mode === 'add' ? 'secure' : 'new'} password
    </button>
  </div>

  <!-- Notes -->
  <div>
    <label for="notes" class="block text-sm font-medium text-gray-700 mb-2">
      Notes
      <span class="text-gray-500 font-normal">(optional)</span>
    </label>
    <textarea
      id="notes"
      bind:value={formData.notes}
      placeholder="Additional information or security questions..."
      rows="3"
      class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors bg-white resize-none"
    ></textarea>
  </div>

  <!-- Action Buttons -->
  <div class="flex flex-col-reverse sm:flex-row sm:justify-end sm:space-x-3 space-y-3 space-y-reverse sm:space-y-0 pt-4 border-t border-gray-200">
    <button
      type="button"
      on:click={onCancel}
      class="w-full sm:w-auto px-6 py-3 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors"
    >
      Cancel
    </button>
    <button
      type="submit"
      disabled={isSubmitting}
      class="w-full sm:w-auto px-6 py-3 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
    >
      {#if isSubmitting}
        <span class="flex items-center justify-center">
          <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          {mode === 'add' ? 'Saving...' : 'Saving changes...'}
        </span>
      {:else}
        {mode === 'add' ? 'Save Entry' : 'Save Changes'}
      {/if}
    </button>
  </div>
</form>

<style>
  input:focus, textarea:focus {
    outline: none;
  }
  input, textarea, button {
    transition: all 0.2s ease-in-out;
  }
  input, textarea {
    -webkit-appearance: none;
    appearance: none;
  }
</style>
