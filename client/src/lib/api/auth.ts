import { API_BASE } from "./config";

// Helper to set JWT token in cookies
function setTokenCookie(token: string) {
    // Set cookie for 7 days, secure, sameSite strict
    document.cookie = `token=${token}; path=/; max-age=${7 * 24 * 60 * 60}; secure; samesite=strict`;
}

// Redirects user to backend Google OAuth endpoint
export function signInWithGoogle() {
    window.location.href = '/api/auth/google-login?returnUrl=' + encodeURIComponent(window.location.origin + '/dashboard');
}

// Call this after Google OAuth redirect to extract token from URL and store in cookies
export function handleGoogleOAuthRedirect() {
    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get('token');
    if (token) {
        setTokenCookie(token);
        // Optionally, remove token from URL
        window.history.replaceState({}, document.title, window.location.pathname);
    }
    return token;
}

export async function login(email: string, password: string): Promise<{ success: boolean; user?: { id: string; email: string; name?: string }; message?: string }> {
    const response = await fetch('/api/users/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
    });
    if (response.ok) {
        try {
            const data = await response.json();
            // If backend returns a token, store it in cookies
            if (data.token) {
                setTokenCookie(data.token);
            }
            // Expecting backend to return { user: { id, email, name? } }
            return { success: true, user: data.user };
        } catch (err) {
            console.log(`Error parsing login response: ${err}`);
            // Fallback: treat as success but no user info
            return { success: true };
        }
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.message || errorMsg;
        } catch (err) {
            console.log(`Error logging in: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

export async function register(email: string, password: string, name?: string): Promise<{ success: boolean; message?: string }> {
    const response = await fetch(`${API_BASE}/api/users/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password, name: name ?? '' })
    });
    if (response.ok) {
        try {
            const data = await response.json();
            if (data.token) {
                setTokenCookie(data.token);
            }
            return { success: true };
        } catch (err) {
            console.log(`Error during registration: ${err}`)
            // If no JSON, just treat as success
            return { success: true };
        }
    }
    else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.message || errorMsg;
        } catch (err) {
            console.log(`Error registering: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}





