<script lang="ts">
	export let isLoading = false;

	// Create event dispatcher
	import { createEventDispatcher } from 'svelte';
	import { exportVaultData, importVaultData, deleteAllVaultData } from '$lib/api/vault';
	const dispatch = createEventDispatcher<{
		success: { message: string };
		error: { message: string };
		loading: { isLoading: boolean };
	}>();

	let importFile: FileList | null = null;
	let isExporting = false;
	let isImporting = false;
	let isDeleting = false;
	let showDeleteModal = false;

	async function exportData() {
		isExporting = true;
		dispatch('loading', { isLoading: true });

			try {
				// Call backend to get actual vault export
				const res = await exportVaultData();
				if (!res.success || !res.data) {
					throw new Error(res.error || 'Failed to fetch export data');
				}

				// Build export payload
				const exportData = {
					vaultItems: res.data,
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
			isExporting = false;
			dispatch('loading', { isLoading: false });
		}
	}

	async function importData() {
		if (!importFile || importFile.length === 0) {
			dispatch('error', { message: 'Please select a file to import.' });
			return;
		}

		isImporting = true;
		dispatch('loading', { isLoading: true });

		try {
				const file = importFile[0];
				const text = await file.text();
				const data = JSON.parse(text);

				// Validate the data structure
				if (!data.vaultItems || !Array.isArray(data.vaultItems)) {
					throw new Error('Invalid file format');
				}

				// Call backend import API
				const res = await importVaultData(data.vaultItems);
				if (res.success) {
					dispatch('success', { message: `Successfully imported ${data.vaultItems.length} vault items!` });
					importFile = null;
				} else {
					dispatch('error', { message: res.error || 'Failed to import vault data.' });
				}
		} catch (error) {
			dispatch('error', { message: 'Failed to import vault data. Please check the file format.' });
			console.error('Error importing data:', error);
		} finally {
			isImporting = false;
			dispatch('loading', { isLoading: false });
		}
	}

	function openDeleteModal() {
		showDeleteModal = true;
	}

	async function deleteAllData() {
		isDeleting = true;
		dispatch('loading', { isLoading: true });

		try {
				const res = await deleteAllVaultData();
				if (res.success) {
					dispatch('success', { message: res.message || 'All vault data has been deleted.' });
				} else {
					dispatch('error', { message: res.error || 'Failed to delete vault data.' });
				}
		} catch (error) {
			dispatch('error', { message: 'Failed to delete vault data.' });
			console.error('Error deleting data:', error);
		} finally {
			isDeleting = false;
			showDeleteModal = false;
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
				disabled={isExporting || isLoading}
				class="rounded-xl bg-green-600 px-4 py-2 text-white transition-all duration-200 hover:bg-green-700 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-green-600 flex items-center justify-center"
			>
				{#if isExporting}
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
					disabled={isImporting || isLoading || !importFile}
					class="rounded-xl bg-blue-600 px-4 py-2 text-white transition-all duration-200 hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-blue-600 flex items-center justify-center"
				>
					{#if isImporting}
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
				type="button"
				on:click={openDeleteModal}
				class="rounded-xl bg-red-600 px-4 py-2 text-white transition-all duration-200 hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-red-600 flex items-center justify-center"
			>
				Delete All Data
			</button>
		</div>
	</div>
</div>

<!-- Custom Delete Data Confirmation Modal -->
{#if showDeleteModal}
	<div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
		<div class="bg-white rounded-3xl shadow-2xl border border-gray-200 max-w-md w-full p-8 transform transition-all">
			<!-- Warning Icon -->
			<div class="flex items-center justify-center w-16 h-16 mx-auto mb-6 bg-red-100 rounded-full">
				<svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L4.35 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
				</svg>
			</div>
			
			<!-- Modal Content -->
			<div class="text-center mb-8">
				<h3 class="text-2xl font-bold text-gray-900 mb-3">Delete All Data?</h3>
				<p class="text-gray-600 leading-relaxed">
					This will permanently delete <strong>ALL</strong> your data. Are you absolutely sure?
				</p>
			</div>
			
			<!-- Action Buttons -->
			<div class="flex flex-col-reverse sm:flex-row gap-3">
				<button
					on:click={() => showDeleteModal = false}
					class="flex-1 px-6 py-3 text-gray-700 bg-gray-100 rounded-xl font-medium transition-all duration-200 hover:bg-gray-200 focus:ring-4 focus:ring-gray-300 focus:outline-none"
				>
					Cancel
				</button>
				<button
					on:click={deleteAllData}
					disabled={isDeleting || isLoading}
					class="flex-1 px-6 py-3 bg-red-600 text-white rounded-xl font-medium transition-all duration-200 hover:bg-red-700 focus:ring-4 focus:ring-red-300 focus:outline-none shadow-lg hover:shadow-xl"
				>
					<span class="flex items-center justify-center">
						{#if isDeleting}
							<svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
								<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
								<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
							</svg>
							Deleting...
						{:else}
							Delete Forever
						{/if}
					</span>
				</button>
			</div>
		</div>
	</div>
{/if}