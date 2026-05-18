// src/api/client.js

const API_BASE = import.meta.env.VITE_API_URL || '/api';

/**
 * Универсальная функция запроса.
 * Автоматически добавляет credentials: 'include' для кук.
 */
async function request(endpoint, options = {}) {
    const url = `${API_BASE}${endpoint}`;

    const config = {
        ...options,
        credentials: 'include', // обязательно для httpOnly cookies
        headers: {
            'Content-Type': 'application/json',
            ...options.headers,
        },
    };

    const response = await fetch(url, config);

    if (!response.ok) {
        // Пробрасываем ошибку с кодом и текстом, чтобы обработать выше
        const error = await response.text();
        throw new Error(error || `Ошибка ${response.status}`);
    }

    // Если ответ пустой (например, 204 No Content), возвращаем null
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
        return response.json();
    }
    return null;
}

// ---------- Auth ----------
export const authAPI = {
    login: (email, password) =>
        request('/Auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password }),
        }),

    register: (name, email, password) =>
        request('/Auth/register', {
            method: 'POST',
            body: JSON.stringify({
                UserName: name,
                email: email,
                password: password
            }),
        }),

    logout: () =>
        request('/Auth/logout', {
            method: 'POST',
        }),

    refresh: () =>
        request('/Auth/refresh', {
            method: 'POST',
        }),
};

// ---------- User ----------
export const userAPI = {
    getMe: () => request('/user/me'),

    updateMe: (data) =>
        request('/user/me', {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    deleteMe: () =>
        request('/user/me', {
            method: 'DELETE',
        }),
};

// ---------- Habits ----------
export const habitsAPI = {
    getAll: () => request('/habit'),

    getById: (id) => request(`/habit/${id}`),

    create: (data) =>
        request('/habit', {
            method: 'POST',
            body: JSON.stringify(data),
        }),

    update: (id, data) =>
        request(`/habit/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    delete: (id) =>
        request(`/habit/${id}`, {
            method: 'DELETE',
        }),
};

// ---------- Habit Logs ----------
export const logsAPI = {
    getByHabit: (habitId) => request(`/log/habit/${habitId}`),

    create: (habitId, isCompleted, notes) =>
        request('/log', {
            method: 'POST',
            body: JSON.stringify({ habitId, isCompleted, notes }),
        }),

    update: (logId, data) =>
        request(`/log/${logId}`, {
            method: 'PUT',
            body: JSON.stringify(data),
        }),

    delete: (logId) =>
        request(`/log/${logId}`, {
            method: 'DELETE',
        }),
};

// ---------- Calendar & Streak ----------
export const calendarAPI = {
    getCalendar: () => request('/calendar'),
    getStreak: () => request('/calendar/streak'),
};