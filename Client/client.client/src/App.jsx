import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Layout from './components/Layout/Layout';
import Login from './components/Auth/Login';
import Register from './components/Auth/Register';
import HabitsPage from './pages/HabitsPage';
import CalendarPage from './pages/CalendarPage'; // Добавили импорт новой страницы

// 1. Обертка для защиты роутов
function ProtectedRoute({ children }) {
    const { user, loading } = useAuth();

    // Пока идет проверка авторизации, показываем спиннер
    if (loading) return (
        <div style={{ display: 'flex', justifyContent: 'center', marginTop: '50px' }}>
            <div className="loading-spinner"></div>
        </div>
    );

    // Если пользователя нет — на логин, если есть — показываем контент
    return user ? children : <Navigate to="/login" replace />;
}

// 2. Конфигурация роутера
const router = createBrowserRouter([
    {
        path: "/login",
        element: <Login />,
    },
    {
        path: "/register",
        element: <Register />,
    },
    {
        // Все роуты внутри этой секции будут обернуты в Layout и защищены ProtectedRoute
        element: (
            <ProtectedRoute>
                <Layout />
            </ProtectedRoute>
        ),
        children: [
            {
                path: "/",
                element: <HabitsPage />,
            },
            {
                path: "/calendar", // Новый маршрут для календаря
                element: <CalendarPage />,
            },
        ],
    },
    // Редирект со всех несуществующих страниц на главную
    {
        path: "*",
        element: <Navigate to="/" replace />,
    }
]);

function App() {
    return (
        <AuthProvider>
            <RouterProvider router={router} />
        </AuthProvider>
    );
}

export default App;
