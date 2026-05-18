import { createContext, useContext, useState, useEffect } from 'react';
import { authAPI, userAPI } from '../api/client';

const AuthContext = createContext();

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    // Проверка при загрузке страницы: стучимся за /user/me
    useEffect(() => {
        async function checkAuth() {
            try {
                const userData = await userAPI.getMe();
                setUser(userData);
            } catch {
                setUser(null);
            } finally {
                setLoading(false);
            }
        }
        checkAuth();
    }, []);

    const login = async (email, password) => {
        const response = await authAPI.login(email, password);
        // После логина получаем профиль
        const userData = await userAPI.getMe();
        setUser(userData);
        return response;
    };

    const logout = async () => {
        await authAPI.logout();
        setUser(null);
    };

    const register = async (name, email, password) => {
        const response = await authAPI.register(name, email, password);
        const userData = await userAPI.getMe();
        setUser(userData);
        return response;
    };

    return (
        <AuthContext.Provider
            value={{ user, loading, login, logout, register, setUser }}
        >
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    return useContext(AuthContext);
}