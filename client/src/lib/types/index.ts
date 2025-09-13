// Form validation utility for vault entry
export type VaultEntryForm = {
	siteName?: string;
	url?: string;
	username?: string;
	password?: string;
	notes?: string;
};

export type VaultEntryErrors = {
	siteName?: string;
	username?: string;
	password?: string;
};

export type VaultEntry = {
	siteName: string;
	url?: string;
	username: string;
	password: string;
	notes?: string;
	[key: string]: string | undefined | unknown;
};

export type UserSettings = {
	theme: string;
	language: string;
	notificationsEnabled: boolean;
};

// Dashboard vault item type
export interface VaultItem {
	id: number;
	siteName: string;
	siteUrl: string;
	username: string;
	password: string;
	notes?: string;
	createdAt: string;
	updatedAt: string;
}

// Define a type for support requests
export interface SupportRequest {
	id: string;
	username?: string;
	email?: string;
	reason?: string;
	status: string;
	createdAt: string;
}
