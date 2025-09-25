import { API_BASE } from "./config";
import { fetchWithAuth } from '$lib/utils/fetchWithAuth';

export async function fetchCurrentUser(): Promise<{ success: boolean; user?: { id: string; username: string; email: string; name?: string; authProvider?: string; googleId?: string; twoFactorEnabled?: boolean; profilePictureUrl?: string }; message?: string }> {
    const response = await fetchWithAuth(`${API_BASE}/api/users/me`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include' // include cookies for session
    });
    // console.log(`Response: ${JSON.stringify(response)}`);
    if (response.ok) {
        const data = await response.json();
        // Backend returns user fields directly, not wrapped in 'user'
        return { success: true, user: data };
    } else {
        let errorMsg = 'Unknown error';
        try {
            // Only parse if content-type is JSON and body is not empty
            if (response.headers.get('content-type')?.includes('application/json')) {
                const text = await response.text();
                if (text) {
                    const data = JSON.parse(text);
                    errorMsg = data.error || data.message || errorMsg;
                }
            }
        } catch (err) {
            console.log(`Error fetching current user: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

// Update user info (username, password, name, profile picture)
export async function updateUser(
    newUsername?: string,
    newPassword?: string,
    name?: string,
    profilePicture?: File
): Promise<{
    success: boolean;
    user?: { id: string; username: string; name?: string; authProvider?: string; googleId?: string; twoFactorEnabled?: boolean; profilePictureUrl?: string };
    message?: string;
}> {
    const url = `${API_BASE}/api/users/update`;
    
    // Create FormData for multipart form data (supports file uploads)
    const formData = new FormData();
    if (newUsername) formData.append('NewUsername', newUsername);
    if (newPassword) formData.append('NewPassword', newPassword);
    if (name) formData.append('Name', name);
    if (profilePicture) formData.append('profilePicture', profilePicture);
    
    const response = await fetchWithAuth(url, {
        method: 'PUT',
        body: formData, // Don't set Content-Type header - let browser set it with boundary
        credentials: 'include'
    });
    if (response.ok) {
        const data = await response.json();
        console.log(`Updated user: ${JSON.stringify(data)}`);
        return { success: true, user: data };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error updating user: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

// Delete user
export async function deleteUser(userId: string): Promise<{ success: boolean; message?: string }> {
    const url = `${API_BASE}/api/users/delete?userId=${encodeURIComponent(userId)}`;
    const response = await fetchWithAuth(url, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include'
    });
    if (response.status === 204) {
        return { success: true };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error deleting user: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

// Link Google account
export async function linkGoogle(googleId: string): Promise<{ success: boolean; message?: string }> {
    const response = await fetchWithAuth(`${API_BASE}/api/users/link-google`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(googleId),
        credentials: 'include'
    });
    if (response.ok) {
        return { success: true };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error linking Google: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

// Unlink Google account
export async function unlinkGoogle(): Promise<{ success: boolean; message?: string }> {
    const response = await fetchWithAuth(`${API_BASE}/api/users/unlink-google`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include'
    });
    if (response.ok) {
        return { success: true };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error unlinking Google: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}