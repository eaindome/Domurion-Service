<script lang="ts">
  import { toast } from '../stores/toast';
  import { fly } from 'svelte/transition';
  import { quintOut, backOut } from 'svelte/easing';

  let isVisible = false;

  // Enhanced animation timing
  $: if ($toast) {
    isVisible = true;
  }

  function getToastStyles(type: string | undefined) {
    const baseClasses = 'backdrop-blur-sm border-2';
    
    switch (type) {
      case 'success':
        return `${baseClasses} bg-green-50/95 border-green-200/60 text-green-800 shadow-green-100/50`;
      case 'error':
        return `${baseClasses} bg-red-50/95 border-red-200/60 text-red-800 shadow-red-100/50`;
      case 'warning':
        return `${baseClasses} bg-amber-50/95 border-amber-200/60 text-amber-800 shadow-amber-100/50`;
      case 'info':
        return `${baseClasses} bg-blue-50/95 border-blue-200/60 text-blue-800 shadow-blue-100/50`;
      default:
        return `${baseClasses} bg-white/95 border-gray-200/60 text-gray-800 shadow-gray-100/50`;
    }
  }

  function getIconColor(type: string | undefined) {
    switch (type) {
      case 'success': return 'text-green-600';
      case 'error': return 'text-red-600';
      case 'warning': return 'text-amber-600';
      case 'info': return 'text-blue-600';
      default: return 'text-gray-600';
    }
  }

  function dismissToast() {
    toast.clear();
  }
</script>

{#if $toast}
  <div
    class="fixed bottom-6 left-1/2 transform -translate-x-1/2 z-50 pointer-events-auto"
    in:fly={{ y: 100, duration: 400, easing: backOut }}
    out:fly={{ y: 50, duration: 250, easing: quintOut }}
    role="status"
    aria-live="polite"
  >
    <div
  class="flex items-center space-x-3 px-5 py-4 rounded-2xl shadow-2xl max-w-sm mx-4 relative overflow-hidden group cursor-pointer transition-all duration-300 hover:scale-105 active:scale-95 {getToastStyles($toast.type ?? 'info')}"
      on:click={dismissToast}
      on:keydown={(e) => (e.key === 'Enter' || e.key === ' ') && dismissToast()}
      role="button"
      aria-label="Dismiss notification"
      tabindex="0"
    >
      <!-- Background gradient overlay -->
      <div class="absolute inset-0 bg-gradient-to-br from-white/20 to-transparent pointer-events-none"></div>
      
      <!-- Progress bar for auto-dismiss -->
      <div class="absolute bottom-0 left-0 h-1 bg-current opacity-30 animate-shrink origin-left"></div>
      
      <!-- Icon with enhanced styling -->
      <div class="flex-shrink-0 relative">
        <div class="w-8 h-8 rounded-full flex items-center justify-center {$toast.type === 'success' ? 'bg-green-100' : $toast.type === 'error' ? 'bg-red-100' : $toast.type === 'warning' ? 'bg-amber-100' : $toast.type === 'info' ? 'bg-blue-100' : 'bg-gray-100'}">
          {#if $toast.type === 'success'}
            <svg class="w-5 h-5 {getIconColor($toast.type ?? 'info')}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7" />
            </svg>
          {:else if $toast.type === 'error'}
            <svg class="w-5 h-5 {getIconColor($toast.type ?? 'info')}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12" />
            </svg>
          {:else if $toast.type === 'warning'}
            <svg class="w-5 h-5 {getIconColor($toast.type ?? 'info')}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
          {:else}
            <svg class="w-5 h-5 {getIconColor($toast.type ?? 'info')}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          {/if}
        </div>
        
        <!-- Pulse animation for success -->
        {#if $toast.type === 'success'}
          <div class="absolute inset-0 w-8 h-8 rounded-full bg-green-400 animate-ping opacity-20"></div>
        {/if}
      </div>
      
      <!-- Message with better typography -->
      <div class="flex-1 min-w-0">
        <p class="text-sm font-semibold leading-5 truncate">
          {$toast.message}
        </p>
        {#if $toast.description}
          <p class="text-xs opacity-80 mt-1 leading-4">
            {$toast.description}
          </p>
        {/if}
      </div>
      
      <!-- Dismiss button -->
      <button
        type="button"
        class="flex-shrink-0 w-6 h-6 rounded-full flex items-center justify-center hover:bg-black/10 transition-colors duration-200 opacity-60 hover:opacity-100"
        on:click|stopPropagation={dismissToast}
        aria-label="Dismiss notification"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
      
      <!-- Subtle shine effect -->
      <div class="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent -skew-x-12 opacity-0 group-hover:opacity-100 group-hover:animate-shine pointer-events-none"></div>
    </div>
  </div>
{/if}

<style>
  /* Auto-dismiss progress bar animation */
  @keyframes shrink {
    from {
      transform: scaleX(1);
    }
    to {
      transform: scaleX(0);
    }
  }

  .animate-shrink {
    animation: shrink 4s linear forwards;
  }

  /* Subtle shine effect */
  @keyframes shine {
    0% {
      transform: translateX(-100%) skewX(-12deg);
    }
    100% {
      transform: translateX(200%) skewX(-12deg);
    }
  }

  .animate-shine {
    animation: shine 0.8s ease-out;
  }

  /* Enhanced pulse animation */
  @keyframes ping {
    75%, 100% {
      transform: scale(1.2);
      opacity: 0;
    }
  }

  .animate-ping {
    animation: ping 1.5s cubic-bezier(0, 0, 0.2, 1) infinite;
  }

  /* Custom focus styles */
  .group:focus {
    outline: 2px solid currentColor;
    outline-offset: 2px;
  }

  /* Backdrop blur support */
  .backdrop-blur-sm {
    backdrop-filter: blur(8px);
    -webkit-backdrop-filter: blur(8px);
  }

  /* Responsive sizing */
  @media (max-width: 640px) {
    .max-w-sm {
      max-width: calc(100vw - 2rem);
    }
  }

  /* Improved transitions */
  .group {
    transform-origin: center bottom;
  }

  /* Enhanced hover effects */
  .group:hover {
    transform: translateY(-2px) scale(1.02);
  }

  .group:active {
    transform: translateY(0) scale(0.98);
  }
</style>