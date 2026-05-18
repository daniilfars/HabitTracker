import { useState, useEffect } from 'react';
import { calendarAPI } from '../api/client';

export default function CalendarPage() {
    const [data, setData] = useState(null);
    const [streak, setStreak] = useState({ currentStreak: 0, bestStreak: 0 });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        async function fetchData() {
            try {
                const [cal, str] = await Promise.all([calendarAPI.getCalendar(), calendarAPI.getStreak()]);
                setData(cal);
                setStreak(str);
            } catch (err) { console.error('Ошибка загрузки календаря'); }
            finally { setLoading(false); }
        }
        fetchData();
    }, []);

    if (loading) return <div className="loading-spinner"></div>;

    const sortedDays = [...(data?.days || [])].sort((a, b) =>
        new Date(b.date) - new Date(a.date)
    );

    const months = {};
    sortedDays.forEach(day => {
        const date = new Date(day.date.split('T'));
        const key = `${date.getFullYear()}-${date.getMonth()}`;
        if (!months[key]) months[key] = [];
        months[key].push(day); 
    });

    const monthKeys = Object.keys(months);

    return (
        <div className="calendar-page">
            <div className="page-header">
                <h1>Календарь</h1>
                <div className="streak-info">
                    <div className="streak-current">🔥 Серия: {streak.currentStreak}</div>
                    <div className="streak-best">🏆 Рекорд: {streak.bestStreak}</div>
                </div>
            </div>

            <div className="calendar-container">
                {monthKeys.map(key => {
                    const days = months[key];
                    const [y, m] = key.split('-');
                    const monthName = new Date(y, m).toLocaleString('ru', { month: 'long' });

                    return (
                        <div key={key} className="calendar-month">
                            <h3>{monthName.charAt(0).toUpperCase() + monthName.slice(1)} {y}</h3>
                            <div className="calendar-grid">
                                {days.map(day => {
                                    const date = new Date(day.date.split('T'));
                                    const intensity = day.totalCount > 0 ? day.completedCount / day.totalCount : 0;
                                    const bgColor = day.totalCount === 0 ? '#f3f4f6' : `rgba(16, 185, 129, ${0.1 + intensity * 0.9})`;

                                    return (
                                        <div
                                            key={day.date}
                                            className="calendar-day"
                                            style={{ backgroundColor: bgColor }}
                                            data-tooltip={day.totalCount === 0 ? "Нет задач" : `Выполнено: ${day.completedCount}/${day.totalCount}`}
                                        >
                                            {date.getDate()}
                                        </div>
                                    );
                                })}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
