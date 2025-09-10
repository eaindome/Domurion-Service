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