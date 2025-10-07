<script lang="ts">
import { onMount } from 'svelte';
import { listSharedCredentials, acceptShareInvitation, rejectShareInvitation } from '$lib/api/shared';
import { toast } from '$lib/stores/toast';
import { authStore } from '$lib/stores/authStore';
import type { VaultItem } from '$lib/types';

let sharedItems: VaultItem[] = [];
let pendingInvites: any[] = [];
let isLoading = false;
let user = { id: '', email: '', name: '' };

onMount(async () => {
	const unsubscribe = authStore.subscribe((u) => {
		if (u) user = { ...u };
	});
	await loadShared();
	unsubscribe();
});

async function loadShared() {
	isLoading = true;
	try {
		const result = await listSharedCredentials();
		if (result.success) {
			sharedItems = result.shared || [];
			pendingInvites = result.pending || [];
		} else {
			toast.show(result.error || 'Failed to load shared credentials', 'error');
		}
	} catch (e) {
		toast.show('Failed to load shared credentials', 'error');
	} finally {
		isLoading = false;
	}
}

async function handleAccept(inviteId: string) {
	const res = await acceptShareInvitation(inviteId);
	if (res.success) {
		toast.show('Credential accepted!', 'success');
		await loadShared();
	} else {
		toast.show(res.error || 'Failed to accept', 'error');
	}
}

async function handleReject(inviteId: string) {
	const res = await rejectShareInvitation(inviteId);
	if (res.success) {
		toast.show('Credential rejected.', 'success');
		await loadShared();
	} else {
		toast.show(res.error || 'Failed to reject', 'error');
	}
}
</script>

<svelte:head>
	<title>Shared Credentials</title>
</svelte:head>

<div class="max-w-3xl mx-auto py-10">
	<h1 class="text-2xl font-bold mb-6">Credentials Shared With You</h1>

	{#if isLoading}
		<p>Loading...</p>
	{:else}
		{#if sharedItems.length === 0 && pendingInvites.length === 0}
			<p class="text-gray-500">No credentials have been shared with you yet.</p>
		{/if}

		{#if pendingInvites.length > 0}
			<h2 class="text-lg font-semibold mt-8 mb-2">Pending Invitations</h2>
			<ul class="space-y-4">
				{#each pendingInvites as invite}
					<li class="rounded-lg border p-4 flex flex-col md:flex-row md:items-center md:justify-between bg-yellow-50">
						<div>
							<strong>{invite.siteName}</strong> shared by <span class="text-indigo-700">{invite.ownerEmail}</span>
							<div class="text-xs text-gray-500">{invite.username}</div>
						</div>
						<div class="flex space-x-2 mt-2 md:mt-0">
							<button class="px-3 py-1 rounded bg-green-600 text-white hover:bg-green-700" on:click={() => handleAccept(invite.id)}>Accept</button>
							<button class="px-3 py-1 rounded bg-red-500 text-white hover:bg-red-600" on:click={() => handleReject(invite.id)}>Reject</button>
						</div>
					</li>
				{/each}
			</ul>
		{/if}

		{#if sharedItems.length > 0}
			<h2 class="text-lg font-semibold mt-8 mb-2">Accepted Shared Credentials</h2>
			<ul class="space-y-4">
				{#each sharedItems as item}
					<li class="rounded-lg border p-4 bg-white flex flex-col md:flex-row md:items-center md:justify-between">
						<div>
							<strong>{item.siteName}</strong>
							<div class="text-xs text-gray-500">{item.username}</div>
						</div>
						<div class="flex space-x-2 mt-2 md:mt-0">
							<a class="px-3 py-1 rounded bg-indigo-600 text-white hover:bg-indigo-700" href={item.siteUrl} target="_blank">Visit</a>
						</div>
					</li>
				{/each}
			</ul>
		{/if}
	{/if}
</div>
