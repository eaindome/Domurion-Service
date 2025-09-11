
<script lang="ts">
	import '../app.css';
	import favicon from '$lib/assets/favicon.svg';
	import Toast from '$lib/components/Toast.svelte';
	import { onMount } from 'svelte';

	// --- Auto-lock vault global logic ---

	import { writable } from 'svelte/store';
	import { settings } from '$lib/stores/settings';
	const isVaultLocked = writable(false);
	let autoLockTimer: ReturnType<typeof setTimeout> | null = null;
	const AUTO_LOCK_IDLE_MINUTES = 2; // Can be made user-configurable

	function resetAutoLockTimer() {
		let autoLockValue;
		const unsubscribe = settings.subscribe(s => autoLockValue = s.autoLock);
		if (!autoLockValue) {
			unsubscribe();
			return;
		}
		if (autoLockTimer) clearTimeout(autoLockTimer);
		autoLockTimer = setTimeout(() => {
			console.log('Auto-lock timer triggered');
			isVaultLocked.set(true);
		}, (AUTO_LOCK_IDLE_MINUTES * 60 * 1000));
		unsubscribe();
	}

	function unlockVault() {
		isVaultLocked.set(false);
		resetAutoLockTimer();
	}

	function handleVisibilityChange() {
		let autoLockValue;
		const unsubscribe = settings.subscribe(s => autoLockValue = s.autoLock);
		if (document.hidden && autoLockValue) {
			isVaultLocked.set(true);
			if (autoLockTimer) clearTimeout(autoLockTimer);
		}
		unsubscribe();
	}

	function setupAutoLockListeners() {
		const events = ['mousemove', 'keydown', 'mousedown', 'touchstart'];
		events.forEach(event => window.addEventListener(event, resetAutoLockTimer));
		document.addEventListener('visibilitychange', handleVisibilityChange);
	}

	function removeAutoLockListeners() {
		const events = ['mousemove', 'keydown', 'mousedown', 'touchstart'];
		events.forEach(event => window.removeEventListener(event, resetAutoLockTimer));
		document.removeEventListener('visibilitychange', handleVisibilityChange);
	}

	onMount(() => {
		let autoLockValue;
		const unsubscribe = settings.subscribe(s => autoLockValue = s.autoLock);
		if (autoLockValue) {
			console.log('Auto-lock enabled');
			setupAutoLockListeners();
			resetAutoLockTimer();
		}
		unsubscribe();
		// Listen for changes to autoLock and update listeners/timer accordingly
		const unsubSettings = settings.subscribe(s => {
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
	<link rel="icon" href={favicon} />
</svelte:head>


{#if $isVaultLocked}
	<!-- Vault Lock Screen Modal (global) -->
	<div class="fixed inset-0 z-50 flex items-center justify-center p-4">
		<div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
		<div class="relative bg-white/80 backdrop-blur-xl border border-white/30 shadow-2xl rounded-3xl max-w-md w-full p-8 flex flex-col items-center animate-modal-enter">
			<svg class="w-12 h-12 text-indigo-500 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 11c1.657 0 3-1.343 3-3V7a3 3 0 10-6 0v1c0 1.657 1.343 3 3 3zm6 2v6a2 2 0 01-2 2H8a2 2 0 01-2-2v-6a2 2 0 012-2h8a2 2 0 012 2z" />
			</svg>
			<h2 class="text-2xl font-bold text-gray-900 mb-2">Vault Locked</h2>
			<p class="text-gray-600 mb-6 text-center">Your vault has been locked due to inactivity or tab hidden.<br/>Please re-authenticate to continue.</p>
			<button
				onclick={unlockVault}
				class="px-6 py-3 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 transition-colors"
			>
				Unlock Vault
			</button>
		</div>
	</div>
{/if}

<Toast />
<div aria-hidden={$isVaultLocked}>
	{@render children?.()}
</div>
