import type { Handle } from "@sveltejs/kit";
import { jwtDecode } from "jwt-decode";
import type { User } from "$lib/types/index";

export const handle: Handle = async ({ event, resolve }) => {
    const token = event.cookies.get('access_token');
    if (token) {
        try {
            const user = jwtDecode<User>(token);
            event.locals.user = user;
            // console.log('Decoded user:', event.locals.user);
        } catch (err) {
            console.log(`Error decoding token: ${err}`);
            event.locals.user = null;
        }
    } else {
        event.locals.user = null;
    }
    return resolve(event);
};