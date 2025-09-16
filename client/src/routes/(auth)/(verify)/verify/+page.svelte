<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  import { get } from 'svelte/store';
  import { verifyEmail } from '$lib/api/auth';
  import { toast } from '$lib/stores/toast';

  let email = '';
  let token = '';
  let isLoading = true;
  let isError = false;
  let errorMsg = '';
  let isResending = false;
  let isResent = false;
  let cooldown = 0;
  let cooldownInterval: NodeJS.Timeout | null = null;

  // Read query params
  onMount(async () => {
    const url = new URL(get(page).url);
    email = url.searchParams.get('email') || '';
    token = url.searchParams.keys().next().value || '';
    if (!token) {
      isError = true;
      errorMsg = 'Invalid verification link.';
      isLoading = false;
      return;
    }
    // Call backend verify endpoint
    try {
      const result = await verifyEmail(token);
      if (result.success) {
        toast.show('Email verified successfully', 'success');
        // Success: redirect to login
        setTimeout(() => goto('/login'), 1000);
      } else {
        isError = true;
        toast.show('Email verification failed', 'error');
        errorMsg = 'Verification failed. You can resend the verification link below.';
      }
    } catch (e) {
      isError = true;
      errorMsg = 'Network error. Please try again.';
    } finally {
      isLoading = false;
    }
  });

  async function handleResend() {
    if (cooldown > 0) return;
    isResending = true;
    isResent = false;
    errorMsg = '';
    try {
      const res = await fetch('/api/resend-verification', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email })
      });
      if (res.ok) {
        isResent = true;
        startCooldown(30); // 30 seconds cooldown
      } else {
        errorMsg = 'Failed to resend verification email.';
      }
    } catch (e) {
      errorMsg = 'Network error. Please try again.';
    } finally {
      isResending = false;
    }
  }

  function startCooldown(seconds: number) {
    cooldown = seconds;
    if (cooldownInterval) clearInterval(cooldownInterval);
    cooldownInterval = setInterval(() => {
      cooldown -= 1;
      if (cooldown <= 0 && cooldownInterval) {
        clearInterval(cooldownInterval);
        cooldownInterval = null;
      }
    }, 1000);
  }
</script>

<div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
  <div class="w-full max-w-md bg-white rounded-2xl shadow-lg p-8 border border-gray-100">
    {#if isLoading}
      <div class="text-center text-gray-600">Verifying your email...</div>
    {:else if isError}
      <h1 class="text-2xl font-bold text-gray-900 mb-2">Email Verification Failed</h1>
      <p class="text-gray-600 mb-4">{errorMsg}</p>
      <button
        class="w-full rounded-xl bg-indigo-600 text-white font-semibold py-2 mt-2 transition hover:bg-indigo-700 focus:ring-2 focus:ring-indigo-400 focus:outline-none shadow"
        on:click={handleResend}
        disabled={isResending || !email || cooldown > 0}
      >
        {#if isResending}
          Sending...
        {:else if cooldown > 0}
          Resend available in {cooldown}s
        {:else if isResent}
          Verification sent!
        {:else}
          Resend verification email
        {/if}
      </button>
      <button
        class="w-full mt-4 text-sm text-gray-500 hover:text-indigo-600 transition"
        on:click={() => goto('/login')}
        type="button"
      >
        Back to login
      </button>
    {/if}
  </div>
</div>

<style>
  /* Add any additional custom styles here if needed */
</style>
