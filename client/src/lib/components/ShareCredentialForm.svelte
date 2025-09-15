<script lang="ts">
import { createShareInvitation } from '$lib/api/shared';
import { toast } from '$lib/stores/toast';
import { onMount } from 'svelte';
import type { VaultItem } from '$lib/types';

export let item: VaultItem;
export let closeShareModal: () => void;

let recipient = '';
let isSubmitting = false;
let error = '';

async function handleShare() {
  isSubmitting = true;
  error = '';
  try {
    const res = await createShareInvitation(String(item.id), recipient);
    if (res.success) {
      toast.show('Share invitation sent!', 'success');
      closeShareModal();
    } else {
      error = res.error || 'Failed to share credential.';
    }
  } catch (e) {
    error = 'Failed to share credential.';
  } finally {
    isSubmitting = false;
  }
}
</script>

<form on:submit|preventDefault={handleShare} class="space-y-4">
  <label for="recipient" class="block text-sm font-medium text-gray-700 mb-1">Share with (username or email):</label>
  <input id="recipient" type="text" bind:value={recipient} class="w-full rounded border px-3 py-2" placeholder="Enter username or email" required />
  {#if error}
    <div class="text-red-600 text-sm">{error}</div>
  {/if}
  <div class="flex justify-end space-x-2">
    <button type="button" class="px-4 py-2 rounded bg-gray-100 text-gray-700" on:click={closeShareModal}>Cancel</button>
    <button type="submit" class="px-4 py-2 rounded bg-indigo-600 text-white hover:bg-indigo-700" disabled={isSubmitting}>
      {isSubmitting ? 'Sharing...' : 'Share'}
    </button>
  </div>
</form>
