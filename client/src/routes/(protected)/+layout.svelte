<script lang="ts">
	import '../../app.css';
	import favicon from '$lib/assets/favIcon.png';
	import Toast from '$lib/components/Toast.svelte';
	import { onMount } from 'svelte';
	import { authStore } from '$lib/stores/authStore';

	// --- Auto-lock vault global logic ---
	import { writable } from 'svelte/store';
	import { settings } from '$lib/stores/settings';
	const isVaultLocked = writable(false);
	let autoLockTimer: ReturnType<typeof setTimeout> | null = null;
	const AUTO_LOCK_IDLE_MINUTES = 10; // Can be made user-configurable

	function resetAutoLockTimer() {
		let autoLockValue;
		const unsubscribe = settings.subscribe((s) => (autoLockValue = s.autoLock));
		if (autoLockTimer) clearTimeout(autoLockTimer);
		autoLockTimer = setTimeout(
			() => {
				console.log('Auto-lock timer triggered');
				isVaultLocked.set(false);
			},
			AUTO_LOCK_IDLE_MINUTES * 60 * 1000
		);
		unsubscribe();
	}

	function unlockVault() {
		isVaultLocked.set(true);
		resetAutoLockTimer();
	}

	function handleVisibilityChange() {
		let autoLockValue;
		const unsubscribe = settings.subscribe((s) => (autoLockValue = s.autoLock));
		if (document.hidden && autoLockValue) {
			isVaultLocked.set(true);
			if (autoLockTimer) clearTimeout(autoLockTimer);
		}
		unsubscribe();
	}

	function setupAutoLockListeners() {
		const events = ['mousemove', 'keydown', 'mousedown', 'touchstart'];
		events.forEach((event) => window.addEventListener(event, resetAutoLockTimer));
		document.addEventListener('visibilitychange', handleVisibilityChange);
	}

	function removeAutoLockListeners() {
		const events = ['mousemove', 'keydown', 'mousedown', 'touchstart'];
		events.forEach((event) => window.removeEventListener(event, resetAutoLockTimer));
		document.removeEventListener('visibilitychange', handleVisibilityChange);
	}

	onMount(() => {
		// Hydrate auth state from cookie/session on app load
		authStore.hydrateAuth();
		authStore.validateSession();

		let autoLockValue;
		const unsubscribe = settings.subscribe((s) => (autoLockValue = s.autoLock));
		if (autoLockValue) {
			console.log('Auto-lock enabled');
			setupAutoLockListeners();
			resetAutoLockTimer();
		}
		unsubscribe();
		// Listen for changes to autoLock and update listeners/timer accordingly
		const unsubSettings = settings.subscribe((s) => {
			if (s.autoLock) {
				setupAutoLockListeners();
				resetAutoLockTimer();
			} else {
				if (autoLockTimer) clearTimeout(autoLockTimer);
				removeAutoLockListeners();
				isVaultLocked.set(false);
			}
		});
		return () => {
			if (autoLockTimer) clearTimeout(autoLockTimer);
			removeAutoLockListeners();
			unsubSettings();
		};
	});

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} class="w-20 h-20"/>
</svelte:head>

<Toast />
{#if $authStore.loading}
	<div class="flex min-h-screen items-center justify-center">
		<svg class="animate-spin h-10 w-10 text-indigo-600" fill="none" viewBox="0 0 24 24">
			<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
			<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
		</svg>
	</div>
{:else}
	<div aria-hidden={$isVaultLocked}>
		{@render children?.()}
	</div>
{/if}