import { API_BASE } from "./config";
import { fetchWithAuth } from '$lib/utils/fetchWithAuth';

// Helper to set JWT token in cookies
function setTokenCookie(token: string) {
    // Set cookie for 7 days, secure, sameSite strict
    document.cookie = `access_token=${token}; path=/; max-age=${7 * 24 * 60 * 60}; samesite=none`; // secure; 
}

// Redirects user to backend Google OAuth endpoint
export function signInWithGoogle() {
    window.location.href = `${API_BASE}/api/auth/google-login?returnUrl=` + encodeURIComponent(window.location.origin + '/dashboard');
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

export async function login(email: string, password: string): Promise<{ 
    success: boolean; 
    user?: { id: string; email: string; name?: string, username?: string }; 
    message?: string; 
    twoFactorRequired?: boolean; 
}> {
    let response;
    try {
        response = await fetchWithAuth(`${API_BASE}/api/users/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });
    } catch (err) {
        console.log(`Error during login fetch: ${err}`);
        return { success: false, message: 'Network error during login' };
    }

    const contentType = response.headers.get('content-type') || '';
    if (!contentType.includes('application/json')) {
        return { success: false, message: 'Unexpected response from server.' };
    }

    if (response.ok) {
        try {
            const data = await response.json();
            // if (data.token) {
            //     setTokenCookie(data.token);
            // }
            return { success: true, user: data.user };
        } catch (err) {
            console.log(`Error parsing login response: ${err}`);
            // Fallback: treat as success but no user info
            return { success: true };
        }
    } else {
        let errorMsg = 'Unknown error';
        let twoFactorRequired = false;
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
            twoFactorRequired = data.twoFactorRequired || false;
        } catch (err) {
            console.log(`Error logging in: ${err}`);
        }
        return { success: false, message: errorMsg, twoFactorRequired };
    }
}

export async function register(email: string, password: string, name?: string): Promise<{ success: boolean; message?: string }> {
    const response = await fetchWithAuth(`${API_BASE}/api/users/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password, name: name ?? '' })
    });
    if (response.ok) {
        try {
            const data = await response.json();
            // if (data.token) {
            //     setTokenCookie(data.token);
            // }
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

export async function verifyOtp(email: string, otp: string): Promise<{ 
    success: boolean; 
    user?: { id: string; email: string; name?: string; username?: string }; 
    message?: string; 
}> {
    let response;
    try {
        response = await fetchWithAuth(`${API_BASE}/api/users/verify-otp`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, otp }),
            credentials: 'include'
        });
    } catch (err) {
        console.log(`Error during OTP verification fetch: ${err}`);
        return { success: false, message: 'Network error during OTP verification' };
    }

    const contentType = response.headers.get('content-type') || '';
    if (!contentType.includes('application/json')) {
        return { success: false, message: 'Unexpected response from server.' };
    }

    if (response.ok) {
        try {
            const data = await response.json();
            // if (data.token) {
            //     setTokenCookie(data.token);
            // }
            return { 
                success: true, 
                user: { 
                    id: data.id, 
                    email: email, 
                    username: data.username 
                } 
            };
        } catch (err) {
            console.log(`Error parsing OTP verification response: ${err}`);
            return { success: false, message: 'Error processing server response' };
        }
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error verifying OTP: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

export async function resendOtp(email: string): Promise<{ 
    success: boolean; 
    message?: string; 
}> {
    let response;
    try {
        response = await fetchWithAuth(`${API_BASE}/api/users/resend-otp`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(email),
            credentials: 'include'
        });
    } catch (err) {
        console.log(`Error during resend OTP fetch: ${err}`);
        return { success: false, message: 'Network error during OTP resend' };
    }

    const contentType = response.headers.get('content-type') || '';
    if (!contentType.includes('application/json')) {
        return { success: false, message: 'Unexpected response from server.' };
    }

    if (response.ok) {
        try {
            const data = await response.json();
            return { 
                success: true, 
                message: data.message || 'OTP sent to your email.' 
            };
        } catch (err) {
            console.log(`Error parsing resend OTP response: ${err}`);
            return { success: true, message: 'OTP sent to your email.' };
        }
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error resending OTP: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

export async function verifyEmail(token: string): Promise<{ success: boolean; message?: string }> {
    const response = await fetchWithAuth(`${API_BASE}/api/users/verify-email?token=${encodeURIComponent(token)}`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    });
    if (response.ok) {
        return { success: true };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error verifying email: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

export async function requestPasswordReset(email: string): Promise<{ 
    success: boolean; 
    message?: string; 
}> {
    let response;
    try {
        response = await fetchWithAuth(`${API_BASE}/api/users/request-password-reset`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(email)
        });
    } catch (err) {
        console.log(`Error during password reset request fetch: ${err}`);
        return { success: false, message: 'Network error during password reset request' };
    }

    const contentType = response.headers.get('content-type') || '';
    if (!contentType.includes('application/json')) {
        return { success: false, message: 'Unexpected response from server.' };
    }

    if (response.ok) {
        try {
            const data = await response.json();
            return { 
                success: true, 
                message: data.message || 'If the email exists, a password reset link will be sent.' 
            };
        } catch (err) {
            console.log(`Error parsing password reset request response: ${err}`);
            return { success: true, message: 'If the email exists, a password reset link will be sent.' };
        }
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error requesting password reset: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

export async function resetPassword(token: string, newPassword: string): Promise<{ 
    success: boolean; 
    message?: string; 
}> {
    let response;
    try {
        response = await fetchWithAuth(`${API_BASE}/api/users/reset-password`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ token, newPassword })
        });
    } catch (err) {
        console.log(`Error during password reset fetch: ${err}`);
        return { success: false, message: 'Network error during password reset' };
    }

    const contentType = response.headers.get('content-type') || '';
    if (!contentType.includes('application/json')) {
        return { success: false, message: 'Unexpected response from server.' };
    }

    if (response.ok) {
        try {
            const data = await response.json();
            return { 
                success: true, 
                message: data.message || 'Password has been reset successfully.' 
            };
        } catch (err) {
            console.log(`Error parsing password reset response: ${err}`);
            return { success: true, message: 'Password has been reset successfully.' };
        }
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error resetting password: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}


