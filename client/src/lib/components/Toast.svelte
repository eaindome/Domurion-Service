<script lang="ts">
  import { toast } from '../stores/toast';
  import { fly, fade } from 'svelte/transition';
</script>

{#if $toast}
  <div
    class="fixed bottom-6 left-1/2 transform -translate-x-1/2 z-50 px-4 py-3 rounded-lg shadow-lg flex items-center space-x-2
      bg-white border border-gray-200
      { $toast.type === 'success' ? 'text-green-700 border-green-200 bg-green-50' : '' }
      { $toast.type === 'error' ? 'text-red-700 border-red-200 bg-red-50' : '' }
      { $toast.type === 'warning' ? 'text-amber-700 border-amber-200 bg-amber-50' : '' }
      { $toast.type === 'info' ? 'text-blue-700 border-blue-200 bg-blue-50' : '' }
    "
    in:fly={{ y: 32, duration: 200 }}
    out:fade={{ duration: 200 }}
    role="status"
    aria-live="polite"
  >
    {#if $toast.type === 'success'}
      <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
      </svg>
    {:else if $toast.type === 'error'}
      <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
    {:else if $toast.type === 'warning'}
      <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z" />
      </svg>
    {:else}
      <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01" />
      </svg>
    {/if}
    <span>{$toast.message}</span>
  </div>
{/if}

<style>
  .fixed {
    min-width: 220px;
    max-width: 90vw;
    pointer-events: none;
    font-size: 1rem;
  }
</style>