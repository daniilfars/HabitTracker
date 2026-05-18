import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

export default function Layout() {
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    const handleLogout = async () => {
        await logout();
        navigate('/login');
    };

    return (
        <div className="app-layout">
            <header className="navbar">
                <div className="nav-container">
                    {/* Логотип */}
                    <Link to="/" className="nav-logo">
                        <span className="logo-icon">🌱</span>
                        <span>HabitTracker</span>
                    </Link>

                    {/* Объединенное меню: ссылки + пользователь */}
                    <div className="nav-links">
                        <Link to="/" className="nav-link">Привычки</Link>
                        <Link to="/calendar" className="nav-link">Календарь</Link>

                        {/* Имя и кнопка в одной группе */}
                        <div className="user-group">
                            <span className="user-name">{user?.userName || user?.name || 'Гость'}</span>
                            <button onClick={handleLogout} className="btn-logout-small">
                                Выйти
                            </button>
                        </div>
                    </div>
                </div>
            </header>

            <main className="app-main">
                <div className="container">
                    <Outlet />
                </div>
            </main>
        </div>
    );
}
