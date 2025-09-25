/**
 * Auto-save utility functions for forms
 * Provides debounced saving, localStorage backup, and status management
 */

export type SaveStatus = 'saved' | 'saving' | 'error' | 'pending';

export interface AutoSaveOptions {
	/** Debounce delay in milliseconds (default: 2000) */
	delay?: number;
	/** LocalStorage key for draft backup */
	storageKey: string;
	/** Save function that returns a promise */
	saveFunction: () => Promise<void>;
	/** Callback when save status changes */
	onStatusChange?: (status: SaveStatus) => void;
	/** Whether auto-save is enabled (from user preferences) */
	enabled?: boolean;
}

export interface AutoSaveInstance<T = Record<string, unknown>> {
	/** Trigger auto-save (debounced) */
	trigger: () => void;
	/** Save draft to localStorage immediately */
	saveDraft: (data: T) => void;
	/** Load draft from localStorage */
	loadDraft: () => T | null;
	/** Clear draft from localStorage */
	clearDraft: () => void;
	/** Clean up timers and listeners */
	destroy: () => void;
	/** Current save status */
	status: SaveStatus;
}

/**
 * Creates an auto-save instance for a form
 */
export function createAutoSave<T = Record<string, unknown>>(options: AutoSaveOptions): AutoSaveInstance<T> {
	let saveTimeout: NodeJS.Timeout | null = null;
	let currentStatus: SaveStatus = 'saved';
	const delay = options.delay ?? 2000;

	const updateStatus = (status: SaveStatus) => {
		currentStatus = status;
		options.onStatusChange?.(status);
	};

	const triggerSave = () => {
		if (!options.enabled) return;

		// Clear existing timeout
		if (saveTimeout) {
			clearTimeout(saveTimeout);
		}

		// Update status to pending
		updateStatus('pending');

		// Set new timeout for auto-save
		saveTimeout = setTimeout(async () => {
			updateStatus('saving');
			try {
				await options.saveFunction();
				updateStatus('saved');
			} catch (error) {
				console.error('Auto-save failed:', error);
				updateStatus('error');
			}
		}, delay);
	};

	const saveDraft = (data: T) => {
		try {
			localStorage.setItem(options.storageKey, JSON.stringify({
				data,
				timestamp: Date.now()
			}));
		} catch (error) {
			console.warn('Failed to save draft to localStorage:', error);
		}
	};

	const loadDraft = (): T | null => {
		try {
			const stored = localStorage.getItem(options.storageKey);
			if (!stored) return null;

			const parsed = JSON.parse(stored);
			// Check if draft is not too old (24 hours)
			const maxAge = 24 * 60 * 60 * 1000;
			if (Date.now() - parsed.timestamp > maxAge) {
				clearDraft();
				return null;
			}

			return parsed.data as T;
		} catch (error) {
			console.warn('Failed to load draft from localStorage:', error);
			return null;
		}
	};

	const clearDraft = () => {
		try {
			localStorage.removeItem(options.storageKey);
		} catch (error) {
			console.warn('Failed to clear draft from localStorage:', error);
		}
	};

	const destroy = () => {
		if (saveTimeout) {
			clearTimeout(saveTimeout);
			saveTimeout = null;
		}
	};

	return {
		trigger: triggerSave,
		saveDraft,
		loadDraft,
		clearDraft,
		destroy,
		get status() { return currentStatus; }
	};
}

/**
 * Status indicator text for UI display
 */
export function getStatusText(status: SaveStatus): string {
	switch (status) {
		case 'saved': return 'Saved';
		case 'saving': return 'Saving...';
		case 'error': return 'Save failed';
		case 'pending': return 'Typing...';
		default: return '';
	}
}

/**
 * Status indicator color classes for Tailwind
 */
export function getStatusColor(status: SaveStatus): string {
	switch (status) {
		case 'saved': return 'text-green-600';
		case 'saving': return 'text-blue-600';
		case 'error': return 'text-red-600';
		case 'pending': return 'text-gray-500';
		default: return 'text-gray-400';
	}
}

/**
 * Debounced function utility
 */
export function debounce<T extends (...args: never[]) => unknown>(
	func: T,
	delay: number
): (...args: Parameters<T>) => void {
	let timeoutId: NodeJS.Timeout | null = null;

	return (...args: Parameters<T>) => {
		if (timeoutId) {
			clearTimeout(timeoutId);
		}
		
		timeoutId = setTimeout(() => {
			func(...args);
		}, delay);
	};
}