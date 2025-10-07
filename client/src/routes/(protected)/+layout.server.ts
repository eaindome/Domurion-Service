import type { LayoutServerLoad } from './$types';
import { redirect } from '@sveltejs/kit';

export const load: LayoutServerLoad = async ({ locals }) => {
	// All routes in (protected) group require authentication
	if (!locals.user) {
		throw redirect(302, '/login');
	}

	return {};
};