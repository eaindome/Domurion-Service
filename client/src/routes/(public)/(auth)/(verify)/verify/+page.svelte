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
      console.log(`Verification result:`, result);
      if (result.success) {
        toast.show('Email verified successfully', 'success');
        // Success: redirect to login
        setTimeout(() => goto('/login'), 1000);
      } else {
        isError = true;
        console.log('Verification failed:', result.message);
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

<!-- Animated gradient background -->
<div class="min-h-screen relative overflow-hidden bg-gradient-to-br from-indigo-50 via-white to-purple-50">
  <!-- Floating background elements -->
  <div class="absolute inset-0 overflow-hidden pointer-events-none">
    <div class="absolute -top-40 -right-40 w-80 h-80 bg-gradient-to-br from-indigo-400/20 to-purple-600/20 rounded-full blur-3xl animate-float"></div>
    <div class="absolute -bottom-40 -left-40 w-80 h-80 bg-gradient-to-br from-blue-400/20 to-indigo-600/20 rounded-full blur-3xl animate-float-delayed"></div>
    <div class="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-96 h-96 bg-gradient-to-br from-purple-400/10 to-pink-600/10 rounded-full blur-3xl animate-pulse-slow"></div>
  </div>

  <!-- Main content -->
  <div class="relative flex items-center justify-center min-h-screen px-4 py-12">
    <div class="w-full max-w-md">
      {#if isLoading}
        <!-- Loading state with elegant animation -->
        <div class="bg-white/80 backdrop-blur-xl rounded-3xl shadow-2xl border border-white/20 p-8 animate-scale-in">
          <div class="text-center">
            <!-- Animated verification icon -->
            <div class="mx-auto mb-6 w-20 h-20 relative">
              <div class="absolute inset-0 bg-gradient-to-r from-indigo-500 to-purple-600 rounded-full animate-pulse"></div>
              <div class="absolute inset-2 bg-white rounded-full flex items-center justify-center">
                <svg class="w-8 h-8 text-indigo-600 animate-spin" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              </div>
            </div>
            <h2 class="text-2xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent mb-2">Verifying Email</h2>
            <p class="text-gray-600 animate-pulse">Please wait while we verify your email address...</p>
            
            <!-- Progress bar -->
            <div class="mt-6 w-full bg-gray-200 rounded-full h-2 overflow-hidden">
              <div class="h-full bg-gradient-to-r from-indigo-500 to-purple-600 rounded-full animate-progress"></div>
            </div>
          </div>
        </div>
      {:else if isError}
        <!-- Error state with better visual hierarchy -->
        <div class="bg-white/80 backdrop-blur-xl rounded-3xl shadow-2xl border border-white/20 p-8 animate-scale-in">
          <!-- Error icon -->
          <div class="text-center mb-6">
            <div class="mx-auto w-20 h-20 bg-gradient-to-br from-red-100 to-red-200 rounded-full flex items-center justify-center mb-4 animate-bounce-subtle">
              <svg class="w-10 h-10 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
              </svg>
            </div>
            <h1 class="text-3xl font-bold bg-gradient-to-r from-gray-800 to-gray-600 bg-clip-text text-transparent mb-3">Verification Failed</h1>
            <p class="text-gray-600 leading-relaxed">{errorMsg}</p>
          </div>

          <!-- Action buttons with improved styling -->
          <div class="space-y-4">
            <button
              class="group relative w-full overflow-hidden rounded-2xl bg-gradient-to-r from-indigo-600 to-purple-600 px-6 py-4 text-white font-semibold shadow-lg transition-all duration-300 hover:shadow-xl hover:scale-[1.02] focus:ring-4 focus:ring-indigo-300 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
              on:click={handleResend}
              disabled={isResending || !email || cooldown > 0}
            >
              <!-- Button background animation -->
              <div class="absolute inset-0 bg-gradient-to-r from-indigo-700 to-purple-700 opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
              
              <!-- Button content -->
              <div class="relative flex items-center justify-center space-x-2">
                {#if isResending}
                  <svg class="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  <span>Sending...</span>
                {:else if cooldown > 0}
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                  </svg>
                  <span>Resend available in {cooldown}s</span>
                {:else if isResent}
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                  </svg>
                  <span>Verification sent!</span>
                {:else}
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path>
                  </svg>
                  <span>Resend verification email</span>
                {/if}
              </div>
            </button>

            <!-- Back to login button -->
            <button
              class="group w-full rounded-2xl border-2 border-gray-200 bg-white/50 px-6 py-3 text-gray-700 font-medium transition-all duration-300 hover:border-indigo-300 hover:bg-indigo-50 hover:text-indigo-700 focus:ring-4 focus:ring-indigo-100 focus:outline-none"
              on:click={() => goto('/login')}
              type="button"
            >
              <div class="flex items-center justify-center space-x-2">
                <svg class="w-4 h-4 transition-transform group-hover:-translate-x-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"></path>
                </svg>
                <span>Back to login</span>
              </div>
            </button>
          </div>
        </div>
      {:else}
        <!-- Success state (if needed) -->
        <div class="bg-white/80 backdrop-blur-xl rounded-3xl shadow-2xl border border-white/20 p-8 animate-scale-in">
          <div class="text-center">
            <div class="mx-auto w-20 h-20 bg-gradient-to-br from-green-100 to-green-200 rounded-full flex items-center justify-center mb-4 animate-bounce-subtle">
              <svg class="w-10 h-10 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
              </svg>
            </div>
            <h1 class="text-3xl font-bold bg-gradient-to-r from-green-600 to-emerald-600 bg-clip-text text-transparent mb-3">Email Verified!</h1>
            <p class="text-gray-600 mb-6">Your email has been successfully verified. Redirecting you to login...</p>
          </div>
        </div>
      {/if}
    </div>
  </div>
</div>

<style>
  /* Custom animations */
  @keyframes float {
    0%, 100% {
      transform: translateY(0px) rotate(0deg);
    }
    50% {
      transform: translateY(-20px) rotate(180deg);
    }
  }

  @keyframes float-delayed {
    0%, 100% {
      transform: translateY(0px) rotate(0deg);
    }
    50% {
      transform: translateY(-15px) rotate(-180deg);
    }
  }

  @keyframes scale-in {
    0% {
      opacity: 0;
      transform: scale(0.9) translateY(20px);
    }
    100% {
      opacity: 1;
      transform: scale(1) translateY(0);
    }
  }

  @keyframes bounce-subtle {
    0%, 100% {
      transform: translateY(0);
    }
    50% {
      transform: translateY(-5px);
    }
  }

  @keyframes progress {
    0% {
      width: 0%;
    }
    50% {
      width: 70%;
    }
    100% {
      width: 100%;
    }
  }

  @keyframes pulse-slow {
    0%, 100% {
      opacity: 0.1;
    }
    50% {
      opacity: 0.2;
    }
  }

  .animate-float {
    animation: float 6s ease-in-out infinite;
  }

  .animate-float-delayed {
    animation: float-delayed 8s ease-in-out infinite;
    animation-delay: 2s;
  }

  .animate-scale-in {
    animation: scale-in 0.6s cubic-bezier(0.4, 0, 0.2, 1) forwards;
  }

  .animate-bounce-subtle {
    animation: bounce-subtle 2s ease-in-out infinite;
  }

  .animate-progress {
    animation: progress 3s ease-out infinite;
  }

  .animate-pulse-slow {
    animation: pulse-slow 4s ease-in-out infinite;
  }

  /* Glass morphism effect */
  .backdrop-blur-xl {
    backdrop-filter: blur(20px);
    -webkit-backdrop-filter: blur(20px);
  }

  /* Gradient text support for older browsers */
  .bg-clip-text {
    -webkit-background-clip: text;
    background-clip: text;
  }

  /* Smooth transitions for all interactive elements */
  button {
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  }

  /* Custom focus styles */
  button:focus {
    transform: translateY(-1px);
  }

  /* Hover effects */
  button:hover:not(:disabled) {
    transform: translateY(-2px);
  }

  button:active:not(:disabled) {
    transform: translateY(0);
  }
</style>
