// Form validation utility for vault entry
export type VaultEntryForm = {
	site?: string;
	siteUrl?: string;
	email?: string;
	password?: string;
	notes?: string;
};

export type VaultEntryErrors = {
	site?: string;
	siteUrl?: string;
	email?: string;
	password?: string;
};

export type VaultEntry = {
	site: string;
	siteUrl?: string;
	email: string;
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
	id: string;
	siteName: string;
	siteUrl: string;
	email: string;
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

// Create a share invitation (by username or email)
export interface ShareInvitation {
	// Define the expected properties of a share invitation here
	id: string;
	to: string;
	from: string;
	status: string;
	createdAt: string;
	// Add other fields as needed
}
