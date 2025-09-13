// Redirects user to backend Google OAuth endpoint
export function signInWithGoogle() {
    window.location.href = '/api/auth/google-login?returnUrl=' + encodeURIComponent(window.location.origin + '/dashboard');
}

export async function login(email: string, password: string): Promise<{ success: boolean; message?: string }> {
    const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
    });
    if (response.ok) {
        return { success: true };
    }
    else {
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

export async function register(name: string, email: string, password: string): Promise<{ success: boolean; message?: string }> {
    const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, email, password })
    });
    if (response.ok) {
        return { success: true };
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

export async function fetchCurrentUser(): Promise<{ success: boolean; user?: { id: string; email: string; name?: string }; message?: string }> {
    const response = await fetch('/api/auth/me', {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    });
    if (response.ok) {
        const data = await response.json();
        return { success: true, user: data.user };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.message || errorMsg;
        } catch (err) {
            console.log(`Error fetching current user: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

export async function logout(): Promise<{ success: boolean; message?: string }> {
    const response = await fetch('/api/auth/logout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    });
    if (response.ok) {
        return { success: true };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.message || errorMsg;
        } catch (err) {
            console.log(`Error logging out: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

