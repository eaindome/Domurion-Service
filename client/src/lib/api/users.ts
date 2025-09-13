export async function fetchCurrentUser(): Promise<{ success: boolean; user?: { id: string; username: string; name?: string; authProvider?: string; googleId?: string; twoFactorEnabled?: boolean }; message?: string }> {
    const response = await fetch('/api/users/me', {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    });
    if (response.ok) {
        const data = await response.json();
        // Backend returns user fields directly, not wrapped in 'user'
        return { success: true, user: data };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error fetching current user: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

// Update user info (username, password, name)
export async function updateUser(
    userId: string,
    newUsername?: string,
    newPassword?: string,
    name?: string
): Promise<{
    success: boolean;
    user?: { id: string; username: string; name?: string; authProvider?: string; googleId?: string; twoFactorEnabled?: boolean };
    message?: string;
}> {
    const params = new URLSearchParams();
    if (name) params.append('name', name);
    const url = `/api/users/update?${params.toString()}`;
    const body: { userId: string; newUsername?: string; newPassword?: string } = { userId };
    if (newUsername) body.newUsername = newUsername;
    if (newPassword) body.newPassword = newPassword;
    const response = await fetch(url, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    if (response.ok) {
        const data = await response.json();
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
    const url = `/api/users/delete?userId=${encodeURIComponent(userId)}`;
    const response = await fetch(url, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' }
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

// Generate password
export async function generatePassword(length: number = 16): Promise<{ success: boolean; password?: string; message?: string }> {
    const url = `/api/users/generate-password?length=${length}`;
    const response = await fetch(url, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    });
    if (response.ok) {
        const data = await response.json();
        return { success: true, password: data.password };
    } else {
        let errorMsg = 'Unknown error';
        try {
            const data = await response.json();
            errorMsg = data.error || data.message || errorMsg;
        } catch (err) {
            console.log(`Error generating password: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}

// Link Google account
export async function linkGoogle(googleId: string): Promise<{ success: boolean; message?: string }> {
    const response = await fetch('/api/users/link-google', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(googleId)
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
    const response = await fetch('/api/users/unlink-google', {
        method: 'POST',
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
            console.log(`Error unlinking Google: ${err}`);
        }
        return { success: false, message: errorMsg };
    }
}