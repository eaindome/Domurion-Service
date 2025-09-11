export function maskPassword(password: string): string {
	return '●'.repeat(password.length);
}

export function getSiteFavicon(siteUrl: string): string {
	return `https://www.google.com/s2/favicons?domain=${siteUrl}&sz=32`;
}
