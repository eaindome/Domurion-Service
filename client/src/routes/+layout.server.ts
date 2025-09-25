import type { LayoutServerLoad } from './$types';
import { redirect } from '@sveltejs/kit';

export const load: LayoutServerLoad = async ({ locals, url }) => {
	const publicRoutes = ['/login', '/register', '/verify', '/message', '/', '/otp'];
	const currentPath = url.pathname;
	const isPublic = publicRoutes.some(route => currentPath.startsWith(route));

	// If not authenticated and not on a public route, redirect to login
	if (!locals.user && !isPublic) {
        // console.log(`locals.user: ${locals.user}`)
		throw redirect(302, '/login');
	}

	return {};
};
