<script lang="ts">
	import '../../app.css';
	import favicon from '$lib/assets/favIcon.png';
	import Toast from '$lib/components/Toast.svelte';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} class="w-20 h-20"/>
</svelte:head>

<Toast />
{@render children?.()}