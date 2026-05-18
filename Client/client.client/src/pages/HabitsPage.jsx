import { useState, useEffect, useCallback } from 'react';
import { habitsAPI } from '../api/client';
import HabitCard from '../components/Habits/HabitCard';
import HabitForm from '../components/Habits/HabitForm';

export default function HabitsPage() {
    const [habits, setHabits] = useState([]);
    const [completionMap, setCompletionMap] = useState({});
    const [showForm, setShowForm] = useState(false);
    const [editingHabit, setEditingHabit] = useState(null);

    const loadHabits = async () => {
        try {
            const data = await habitsAPI.getAll();
            setHabits(data);
        } catch (err) {
            console.error('Ошибка загрузки:', err);
        }
    };

    useEffect(() => {
        loadHabits();
    }, []);

    const handleStatusReport = useCallback((id, isCompleted) => {
        setCompletionMap(prev => {
            if (prev[id] === isCompleted) return prev;
            return { ...prev, [id]: isCompleted };
        });
    }, []);

    // При удалении вызываем это, чтобы почистить карту статусов
    const handleRefresh = async () => {
        setCompletionMap({}); // Сбрасываем карту, чтобы она пересобралась
        await loadHabits();
    };

    const sortedHabits = [...habits].sort((a, b) => {
        const aDone = completionMap[a.id] || false;
        const bDone = completionMap[b.id] || false;
        if (aDone === bDone) return 0;
        return aDone ? 1 : -1;
    });

    return (
        <div className="container">
            <header className="page-header">
                <h1>Мои привычки</h1>
                <button onClick={() => { setEditingHabit(null); setShowForm(true); }} className="btn btn-primary full-mobile">
                    + Новая привычка
                </button>
            </header>

            {showForm && (
                <div className="modal-overlay" onClick={() => { setShowForm(false); setEditingHabit(null); }}>
                    <div className="modal-content" onClick={e => e.stopPropagation()}>
                        <HabitForm
                            habit={editingHabit}
                            onSuccess={async () => {
                                await handleRefresh();
                                setShowForm(false);
                            }}
                            onCancel={() => { setShowForm(false); setEditingHabit(null); }}
                        />
                    </div>
                </div>
            )}

            <div className="habits-grid">
                {sortedHabits.map(habit => (
                    <HabitCard
                        key={habit.id}
                        habit={habit}
                        onEdit={(h) => { setEditingHabit(h); setShowForm(true); }}
                        onDelete={handleRefresh}
                        onComplete={handleRefresh}
                        onStatusReport={handleStatusReport}
                    />
                ))}
            </div>
        </div>
    );
}
