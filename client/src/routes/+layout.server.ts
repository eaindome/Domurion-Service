import type { LayoutServerLoad } from './$types';

export const load: LayoutServerLoad = async () => {
	// Root layout - no auth logic needed here
	// Auth is handled by the (protected) group layout
	return {};
};
