import { useState, useEffect } from 'react';
import { logsAPI, habitsAPI } from '../../api/client'; // Добавили habitsAPI

const frequencyLabels = { 0: 'Ежедневно', 1: 'Раз в неделю', 2: 'По дням' };
const dayNamesShort = { 1: 'Пн', 2: 'Вт', 3: 'Ср', 4: 'Чт', 5: 'Пт', 6: 'Сб', 0: 'Вс' };

export default function HabitCard({ habit, onEdit, onDelete, onComplete, onStatusReport }) {
    const [completing, setCompleting] = useState(false);
    const [isCompletedToday, setIsCompletedToday] = useState(false);
    const [showLogs, setShowLogs] = useState(false);
    const [logs, setLogs] = useState([]);
    const [logsLoading, setLogsLoading] = useState(false);

    useEffect(() => {
        const checkTodayStatus = async () => {
            try {
                const history = await logsAPI.getByHabit(habit.id);
                const today = new Date().toISOString().split('T')[0];
                const done = history.some(log => log.date?.startsWith(today) && log.isCompleted);

                setIsCompletedToday(done);
                setLogs(history.sort((a, b) => new Date(b.date) - new Date(a.date)));

                if (onStatusReport) onStatusReport(habit.id, done);
            } catch (err) {
                console.error("Ошибка проверки статуса:", err);
            }
        };
        checkTodayStatus();
    }, [habit.id, onStatusReport]);

    const handleComplete = async () => {
        if (isCompletedToday || completing) return;
        setCompleting(true);
        try {
            await logsAPI.create(habit.id, true, '');
            setIsCompletedToday(true);
            if (onStatusReport) onStatusReport(habit.id, true);
            if (onComplete) onComplete();
        } catch (err) {
            alert('Ошибка отметки');
        } finally {
            setCompleting(false);
        }
    };

    // НОВАЯ ФУНКЦИЯ УДАЛЕНИЯ
    const handleDeleteClick = async () => {
        if (window.confirm(`Удалить привычку "${habit.name}"?`)) {
            try {
                await habitsAPI.delete(habit.id);
                if (onDelete) onDelete(); // Вызываем loadHabits в родителе
            } catch (err) {
                alert('Не удалось удалить привычку');
            }
        }
    };

    return (
        <>
            <div className={`habit-card ${isCompletedToday ? 'completed' : ''}`}>
                <div className="habit-content">
                    <div className="habit-header">
                        <h3 className="habit-title">
                            {isCompletedToday && <span className="check-icon">✅ </span>}
                            {habit.name}
                        </h3>
                        <div className="habit-actions">
                            <button className="icon-btn" onClick={() => setShowLogs(true)} title="История">📋</button>
                            <button className="icon-btn" onClick={() => onEdit(habit)}>✎</button>
                            <button className="icon-btn danger" onClick={handleDeleteClick} title="Удалить">🗑</button>
                        </div>
                    </div>
                    {habit.description && <p className="habit-desc">{habit.description}</p>}
                    <div className="habit-meta">
                        <span className="freq-label">{frequencyLabels[habit.frequency]}</span>
                        {habit.frequency === 2 && Array.isArray(habit.customDays) && (
                            <span className="days-list"> ({habit.customDays.map(d => dayNamesShort[d]).join(', ')})</span>
                        )}
                    </div>
                </div>

                <button
                    className={`btn btn-success complete-btn ${isCompletedToday ? 'completed' : ''}`}
                    onClick={handleComplete}
                    disabled={completing || isCompletedToday}
                >
                    {completing ? '...' : isCompletedToday ? '✨ Выполнено' : 'Отметить выполнение'}
                </button>
            </div>

            {showLogs && (
                <div className="modal-overlay" onClick={() => setShowLogs(false)}>
                    <div className="modal-content" onClick={e => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2>История: {habit.name}</h2>
                            <button className="close-btn" onClick={() => setShowLogs(false)}>&times;</button>
                        </div>
                        <ul className="logs-list">
                            {logs.map(log => (
                                <li key={log.id} className={log.isCompleted ? 'completed' : ''}>
                                    <span>{new Date(log.date).toLocaleDateString('ru-RU')}</span>
                                    <span>{log.isCompleted ? '✅' : '❌'}</span>
                                </li>
                            ))}
                        </ul>
                        <button className="btn btn-primary" style={{ marginTop: '10px' }} onClick={() => setShowLogs(false)}>Закрыть</button>
                    </div>
                </div>
            )}
        </>
    );
}
