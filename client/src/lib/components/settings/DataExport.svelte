<script lang="ts">
	export let isLoading = false;

	// Create event dispatcher
	import { createEventDispatcher } from 'svelte';
	const dispatch = createEventDispatcher<{
		success: { message: string };
		error: { message: string };
		loading: { isLoading: boolean };
	}>();

	let importFile: FileList | null = null;

	async function exportData() {
		dispatch('loading', { isLoading: true });

		try {
			// Simulate export process
			await new Promise(resolve => setTimeout(resolve, 2000));
			
			// In a real implementation, this would call an API to export vault data
			const exportData = {
				vaultItems: [],
				settings: {},
				exportDate: new Date().toISOString()
			};

			const dataStr = JSON.stringify(exportData, null, 2);
			const dataBlob = new Blob([dataStr], { type: 'application/json' });
			const url = URL.createObjectURL(dataBlob);
			
			const link = document.createElement('a');
			link.href = url;
			link.download = `domurion-vault-export-${new Date().toISOString().split('T')[0]}.json`;
			link.click();
			
			URL.revokeObjectURL(url);
			
			dispatch('success', { message: 'Vault data exported successfully!' });
		} catch (error) {
			dispatch('error', { message: 'Failed to export vault data.' });
			console.error('Error exporting data:', error);
		} finally {
			dispatch('loading', { isLoading: false });
		}
	}

	async function importData() {
		if (!importFile || importFile.length === 0) {
			dispatch('error', { message: 'Please select a file to import.' });
			return;
		}

		dispatch('loading', { isLoading: true });

		try {
			const file = importFile[0];
			const text = await file.text();
			const data = JSON.parse(text);

			// Validate the data structure
			if (!data.vaultItems || !Array.isArray(data.vaultItems)) {
				throw new Error('Invalid file format');
			}

			// In a real implementation, this would call an API to import the data
			await new Promise(resolve => setTimeout(resolve, 2000));

			dispatch('success', { message: `Successfully imported ${data.vaultItems.length} vault items!` });
			
			// Reset file input
			importFile = null;
		} catch (error) {
			dispatch('error', { message: 'Failed to import vault data. Please check the file format.' });
			console.error('Error importing data:', error);
		} finally {
			dispatch('loading', { isLoading: false });
		}
	}

	async function deleteAllData() {
		const confirmed = confirm(
			'Are you sure you want to delete all your vault data? This action cannot be undone.'
		);
		
		if (!confirmed) return;

		dispatch('loading', { isLoading: true });

		try {
			// In a real implementation, this would call an API to delete all data
			await new Promise(resolve => setTimeout(resolve, 1500));
			
			dispatch('success', { message: 'All vault data has been deleted.' });
		} catch (error) {
			dispatch('error', { message: 'Failed to delete vault data.' });
			console.error('Error deleting data:', error);
		} finally {
			dispatch('loading', { isLoading: false });
		}
	}
</script>

<div class="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
	<h2 class="mb-6 text-xl font-semibold text-gray-900">Data Management</h2>

	<div class="space-y-6">
		<!-- Export Data -->
		<div class="rounded-xl bg-green-50 p-4">
			<h3 class="mb-2 text-sm font-medium text-green-900">Export Vault Data</h3>
			<p class="mb-4 text-sm text-green-700">
				Download a backup of all your vault data in JSON format.
			</p>
			<button
				on:click={exportData}
				disabled={isLoading}
				class="rounded-xl bg-green-600 px-4 py-2 text-white transition-all duration-200 hover:bg-green-700 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-green-600 flex items-center justify-center"
			>
				{#if isLoading}
					<svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
						<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
						<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
					</svg>
					Exporting...
				{:else}
					Export Data
				{/if}
			</button>
		</div>

		<!-- Import Data -->
		<div class="rounded-xl bg-blue-50 p-4">
			<h3 class="mb-2 text-sm font-medium text-blue-900">Import Vault Data</h3>
			<p class="mb-4 text-sm text-blue-700">
				Import vault data from a previously exported JSON file.
			</p>
			<div class="space-y-3">
				<input
					type="file"
					accept=".json"
					bind:files={importFile}
					class="block w-full text-sm text-blue-900 file:mr-4 file:rounded-xl file:border-0 file:bg-blue-100 file:px-4 file:py-2 file:text-sm file:font-medium file:text-blue-700 hover:file:bg-blue-200"
				/>
				<button
					on:click={importData}
					disabled={isLoading || !importFile}
					class="rounded-xl bg-blue-600 px-4 py-2 text-white transition-all duration-200 hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-blue-600 flex items-center justify-center"
				>
					{#if isLoading}
						<svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
							<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
							<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
						</svg>
						Importing...
					{:else}
						Import Data
					{/if}
				</button>
			</div>
		</div>

		<!-- Delete All Data -->
		<div class="rounded-xl bg-red-50 p-4">
			<h3 class="mb-2 text-sm font-medium text-red-900">Delete All Data</h3>
			<p class="mb-4 text-sm text-red-700">
				Permanently delete all your vault data. This action cannot be undone.
			</p>
			<button
				on:click={deleteAllData}
				disabled={isLoading}
				class="rounded-xl bg-red-600 px-4 py-2 text-white transition-all duration-200 hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-red-600 flex items-center justify-center"
			>
				{#if isLoading}
					<svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
						<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
						<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
					</svg>
					Deleting...
				{:else}
					Delete All Data
				{/if}
			</button>
		</div>
	</div>
</div>