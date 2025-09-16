import type { LayoutServerLoad } from './$types';
import { redirect } from '@sveltejs/kit';

export const load: LayoutServerLoad = async ({ locals, url }) => {
	const publicRoutes = ['/login', '/register'];
	const currentPath = url.pathname;

	// If not authenticated and not on a public route, redirect to login
	if (!locals.user && !publicRoutes.includes(currentPath)) {
		throw redirect(302, '/login');
	}

	return {};
};
