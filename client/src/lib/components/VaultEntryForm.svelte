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
  let passwordStrength = 0;
  let isCopied = false;
  let isGenerating = false;

  // Password strength calculation
  $: {
    passwordStrength = calculatePasswordStrength(formData.password);
  }

  function calculatePasswordStrength(password: string): number {
    if (!password) return 0;
    
    let score = 0;
    if (password.length >= 8) score += 25;
    if (password.length >= 12) score += 25;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score += 20;
    if (/\d/.test(password)) score += 15;
    if (/[^a-zA-Z\d]/.test(password)) score += 15;
    
    return Math.min(score, 100);
  }

  function getPasswordStrengthColor(): string {
    if (passwordStrength < 40) return 'bg-red-500';
    if (passwordStrength < 70) return 'bg-yellow-500';
    return 'bg-green-500';
  }

  function getPasswordStrengthText(): string {
    if (passwordStrength < 40) return 'Weak';
    if (passwordStrength < 70) return 'Medium';
    return 'Strong';
  }

  async function handleGeneratePassword() {
    isGenerating = true;
    // Add a slight delay for better UX feedback
    await new Promise(resolve => setTimeout(resolve, 300));
    formData.password = generatePassword();
    isGenerating = false;
    dispatch('toast', { message: 'New password generated!', type: 'success' });
  }

  async function handleCopyPassword() {
    if (formData.password) {
      await copyToClipboard(formData.password);
      isCopied = true;
      dispatch('toast', { message: 'Password copied to clipboard!', type: 'success' });
      
      // Reset copied state after 2 seconds
      setTimeout(() => {
        isCopied = false;
      }, 2000);
    }
  }

  function handleUrlInput(event: Event) {
    const target = event.target as HTMLInputElement;
    let value = target.value;
    
    // Auto-add https:// if user enters a domain without protocol
    if (value && !value.includes('://') && value.includes('.')) {
      formData.url = `https://${value}`;
    }
  }

  // Auto-focus next field on Enter (except for textarea)
  function handleKeyDown(event: KeyboardEvent, nextFieldId?: string) {
    if (event.key === 'Enter' && nextFieldId) {
      event.preventDefault();
      const nextField = document.getElementById(nextFieldId);
      nextField?.focus();
    }
  }
</script>

<form on:submit|preventDefault={onSubmit} class="p-6 space-y-6">
  <!-- Site Name -->
  <div class="space-y-2">
    <label for="siteName" class="block text-sm font-semibold text-gray-800 mb-1">
      Site Name *
    </label>
    <div class="relative group">
      <input
        type="text"
        id="siteName"
        bind:value={formData.siteName}
        on:keydown={(e) => handleKeyDown(e, 'url')}
        placeholder="e.g., Google, GitHub, Facebook"
        class="w-full px-4 py-3.5 border-2 border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all duration-200 bg-white group-hover:border-gray-300 placeholder-gray-400"
        class:border-red-300={errors.siteName}
        class:focus:ring-red-500={errors.siteName}
        class:focus:border-red-500={errors.siteName}
        class:border-indigo-200={formData.siteName && !errors.siteName}
      />
      {#if formData.siteName && !errors.siteName}
        <div class="absolute inset-y-0 right-0 flex items-center pr-3">
          <svg class="w-5 h-5 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
          </svg>
        </div>
      {/if}
    </div>
    {#if errors.siteName}
      <p class="text-sm text-red-600 flex items-center space-x-1">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
        </svg>
        <span>{errors.siteName}</span>
      </p>
    {/if}
  </div>

  <!-- Website URL -->
  <div class="space-y-2">
    <label for="url" class="block text-sm font-semibold text-gray-800 mb-1">
      Website URL
      <span class="text-gray-500 font-normal text-xs">(optional)</span>
    </label>
    <div class="relative group">
      <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
        <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9v-9m0-9v9"></path>
        </svg>
      </div>
      <input
        type="url"
        id="url"
        bind:value={formData.url}
        on:blur={handleUrlInput}
        on:keydown={(e) => handleKeyDown(e, 'username')}
        placeholder="https://example.com"
        class="w-full pl-10 pr-4 py-3.5 border-2 border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all duration-200 bg-white group-hover:border-gray-300 placeholder-gray-400"
        class:border-indigo-200={formData.url}
      />
    </div>
  </div>

  <!-- Username/Email -->
  <div class="space-y-2">
    <label for="username" class="block text-sm font-semibold text-gray-800 mb-1">
      Username/Email *
    </label>
    <div class="relative group">
      <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
        <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
        </svg>
      </div>
      <input
        type="text"
        id="username"
        bind:value={formData.username}
        on:keydown={(e) => handleKeyDown(e, 'password')}
        placeholder="your-username or email@example.com"
        class="w-full pl-10 pr-4 py-3.5 border-2 border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all duration-200 bg-white group-hover:border-gray-300 placeholder-gray-400"
        class:border-red-300={errors.username}
        class:focus:ring-red-500={errors.username}
        class:focus:border-red-500={errors.username}
        class:border-indigo-200={formData.username && !errors.username}
      />
      {#if formData.username && !errors.username}
        <div class="absolute inset-y-0 right-0 flex items-center pr-3">
          <svg class="w-5 h-5 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
          </svg>
        </div>
      {/if}
    </div>
    {#if errors.username}
      <p class="text-sm text-red-600 flex items-center space-x-1">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
        </svg>
        <span>{errors.username}</span>
      </p>
    {/if}
  </div>

  <!-- Password -->
  <div class="space-y-2">
    <label for="password" class="block text-sm font-semibold text-gray-800 mb-1">
      Password *
    </label>
    <div class="relative group">
      <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
        <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"></path>
        </svg>
      </div>
      <input
        type={showPassword ? 'text' : 'password'}
        id="password"
        bind:value={formData.password}
        on:keydown={(e) => handleKeyDown(e, 'notes')}
        placeholder="Enter or generate a secure password"
        class="w-full pl-10 pr-24 py-3.5 border-2 border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all duration-200 bg-white group-hover:border-gray-300 placeholder-gray-400"
        class:border-red-300={errors.password}
        class:focus:ring-red-500={errors.password}
        class:focus:border-red-500={errors.password}
        class:border-indigo-200={formData.password && !errors.password}
      />
      <div class="absolute inset-y-0 right-0 flex items-center space-x-1 pr-3">
        <button
          type="button"
          on:click={() => showPassword = !showPassword}
          class="p-1.5 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-all duration-200"
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
            class="p-1.5 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-all duration-200 relative"
            class:text-green-600={isCopied}
            class:hover:text-green-700={isCopied}
            title="Copy password"
            aria-label="Copy password"
          >
            {#if isCopied}
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
              </svg>
            {:else}
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
              </svg>
            {/if}
          </button>
        {/if}
      </div>
    </div>
    
    <!-- Password Strength Indicator -->
    {#if formData.password}
      <div class="space-y-2">
        <div class="flex items-center justify-between text-xs">
          <span class="text-gray-600">Password strength:</span>
          <span class="font-medium" class:text-red-600={passwordStrength < 40} class:text-yellow-600={passwordStrength >= 40 && passwordStrength < 70} class:text-green-600={passwordStrength >= 70}>
            {getPasswordStrengthText()}
          </span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-2 overflow-hidden">
          <div 
            class="h-full rounded-full transition-all duration-500 {getPasswordStrengthColor()}"
            style="width: {passwordStrength}%"
          ></div>
        </div>
      </div>
    {/if}
    
    {#if errors.password}
      <p class="text-sm text-red-600 flex items-center space-x-1">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
        </svg>
        <span>{errors.password}</span>
      </p>
    {/if}
    
    <div class="flex items-center space-x-4">
      <button
        type="button"
        on:click={handleGeneratePassword}
        disabled={isGenerating}
        class="inline-flex items-center text-sm text-indigo-600 hover:text-indigo-700 font-medium transition-colors bg-indigo-50 hover:bg-indigo-100 px-3 py-1.5 rounded-lg disabled:opacity-50"
      >
        {#if isGenerating}
          <svg class="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          Generating...
        {:else}
          <svg class="w-4 h-4 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
          </svg>
          Generate {mode === 'add' ? 'secure' : 'new'} password
        {/if}
      </button>
    </div>
  </div>

  <!-- Notes -->
  <div class="space-y-2">
    <label for="notes" class="block text-sm font-semibold text-gray-800 mb-1">
      Notes
      <span class="text-gray-500 font-normal text-xs">(optional)</span>
    </label>
    <div class="relative group">
      <div class="absolute top-3 left-3 pointer-events-none">
        <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
        </svg>
      </div>
      <textarea
        id="notes"
        bind:value={formData.notes}
        placeholder="Additional information, security questions, or recovery codes..."
        rows="3"
        class="w-full pl-10 pr-4 py-3.5 border-2 border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all duration-200 bg-white resize-none group-hover:border-gray-300 placeholder-gray-400"
        class:border-indigo-200={formData.notes}
      ></textarea>
    </div>
  </div>

  <!-- Action Buttons -->
  <div class="flex flex-col-reverse sm:flex-row sm:justify-end sm:space-x-3 space-y-3 space-y-reverse sm:space-y-0 pt-6 border-t-2 border-gray-100">
    <button
      type="button"
      on:click={onCancel}
      class="w-full sm:w-auto px-6 py-3.5 text-sm font-semibold text-gray-700 bg-white border-2 border-gray-300 rounded-xl hover:bg-gray-50 hover:border-gray-400 focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-all duration-200"
    >
      Cancel
    </button>
    <button
      type="submit"
      disabled={isSubmitting}
      class="w-full sm:w-auto px-8 py-3.5 text-sm font-semibold text-white bg-indigo-600 rounded-xl hover:bg-indigo-700 focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200 shadow-lg hover:shadow-xl"
    >
      {#if isSubmitting}
        <span class="flex items-center justify-center">
          <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          {mode === 'add' ? 'Saving Entry...' : 'Saving Changes...'}
        </span>
      {:else}
        <span class="flex items-center justify-center">
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
          </svg>
          {mode === 'add' ? 'Save Entry' : 'Save Changes'}
        </span>
      {/if}
    </button>
  </div>
</form>

<style>
  input:focus, textarea:focus {
    outline: none;
  }
  
  input, textarea, button {
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  }
  
  input, textarea {
    -webkit-appearance: none;
    appearance: none;
  }

  /* Enhanced hover animations */
  .group:hover input,
  .group:hover textarea {
    transform: translateY(-1px);
    box-shadow: 0 4px 12px -2px rgba(0, 0, 0, 0.05);
  }

  /* Custom scrollbar for textarea */
  textarea::-webkit-scrollbar {
    width: 6px;
  }
  
  textarea::-webkit-scrollbar-track {
    background: #f1f5f9;
    border-radius: 3px;
  }
  
  textarea::-webkit-scrollbar-thumb {
    background: #cbd5e1;
    border-radius: 3px;
  }
  
  textarea::-webkit-scrollbar-thumb:hover {
    background: #94a3b8;
  }

  /* Password strength animation */
  .password-strength-bar {
    transform-origin: left;
    animation: fillBar 0.5s ease-out;
  }

  @keyframes fillBar {
    from {
      transform: scaleX(0);
    }
    to {
      transform: scaleX(1);
    }
  }
</style>