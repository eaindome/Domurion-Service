import type { VaultEntry } from "$lib/types";

export async function getVaultEntry(id: string): Promise<VaultEntry | null> {
	const response = await fetch(`/api/vault/${id}`);
	if (response.ok) {
		return await response.json();
	}
	return null;
}

export async function createVaultEntry(entry: VaultEntry): Promise<{ success: boolean; error?: string }> {
	const response = await fetch('/api/vault', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(entry)
	});
    if (response.ok) {
        return { success: true };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.message || errorMsg;
        } catch (err) {
            console.log(`Error creating entry: ${err}`)
        }
        return { success: false, error: errorMsg };
    }
	
}

export async function updateVaultEntry(id: string, entry: VaultEntry): Promise<{ success: boolean; error?: string }> {
	const response = await fetch(`/api/vault/${id}`, {
		method: 'PUT',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(entry)
	});
	if (response.ok) {
        return { success: true };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.message || errorMsg;
        } catch (err) {
            console.log(`Error updating entry: ${err}`)
        }
        return { success: false, error: errorMsg };
    }
}

export async function deleteVaultEntry(id: string): Promise<{ success: boolean; error?: string }> {
	const response = await fetch(`/api/vault/${id}`, {
		method: 'DELETE'
	});
	if (response.ok) {
        return { success: true };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.message || errorMsg;
        } catch (err) {
            console.log(`Error deleting entry: ${err}`)
        }
        return { success: false, error: errorMsg };
    }
}
