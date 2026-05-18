import { useState } from 'react';
import { habitsAPI } from '../../api/client';

// В .NET DayOfWeek: Sunday = 0, Monday = 1, ..., Saturday = 6
// Наш массив кнопок: Пн(1), Вт(2), Ср(3), Чт(4), Пт(5), Сб(6), Вс(0)
const weekDays = [
    { id: 1, label: 'Пн' },
    { id: 2, label: 'Вт' },
    { id: 3, label: 'Ср' },
    { id: 4, label: 'Чт' },
    { id: 5, label: 'Пт' },
    { id: 6, label: 'Сб' },
    { id: 0, label: 'Вс' }
];

export default function HabitForm({ habit, onSuccess, onCancel }) {
    const [name, setName] = useState(habit?.name || '');
    const [description, setDescription] = useState(habit?.description || '');
    const [frequency, setFrequency] = useState(habit?.frequency ?? 0);
    const [customDays, setCustomDays] = useState(habit?.customDays || []);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const handleDayToggle = (dayId) => {
        setCustomDays(prev =>
            prev.includes(dayId)
                ? prev.filter(id => id !== dayId)
                : [...prev, dayId]
        );
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        // Валидация для Custom частоты
        if (frequency === 2 && customDays.length === 0) {
            setError('Выберите хотя бы один день недели');
            return;
        }

        setLoading(true);
        try {
            const payload = {
                name,
                description: description || null,
                frequency: Number(frequency),
                // Если частота не Custom (2), то customDays должен быть строго null
                customDays: frequency === 2 ? customDays : null,
                userId: 0 // Бэкенд перезапишет из токена
            };

            if (habit) {
                await habitsAPI.update(habit.id, payload);
            } else {
                await habitsAPI.create(payload);
            }
            onSuccess();
        } catch (err) {
            // Если бэк вернул JSON с ошибками, выводим их
            console.error(err);
            setError('Ошибка сохранения. Проверьте правильность заполнения полей.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="habit-form">
            <h2>{habit ? 'Редактировать привычку' : 'Новая привычка'}</h2>

            <div className="form-group">
                <label>Название</label>
                <input
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    required
                    maxLength={100}
                    placeholder="Напр: Заправить постель"
                />
            </div>

            <div className="form-group">
                <label>Описание (необязательно)</label>
                <textarea
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    maxLength={500}
                    rows={3}
                    placeholder="Зачем вам эта привычка?"
                />
            </div>

            <div className="form-group">
                <label>Частота</label>
                <select
                    value={frequency}
                    onChange={(e) => {
                        const val = Number(e.target.value);
                        setFrequency(val);
                        if (val !== 2) setCustomDays([]); // Сбрасываем дни, если не Custom
                    }}
                >
                    <option value={0}>Ежедневно</option>
                    <option value={1}>Раз в неделю</option>
                    <option value={2}>По выбранным дням</option>
                </select>
            </div>

            {frequency === 2 && (
                <div className="form-group">
                    <label>Дни недели</label>
                    <div className="days-selector">
                        {weekDays.map((day) => (
                            <button
                                key={day.id}
                                type="button"
                                className={`day-btn ${customDays.includes(day.id) ? 'active' : ''}`}
                                onClick={() => handleDayToggle(day.id)}
                            >
                                {day.label}
                            </button>
                        ))}
                    </div>
                </div>
            )}

            {error && <div className="error-text" style={{ color: 'red', marginTop: '10px' }}>{error}</div>}

            <div className="form-actions" style={{ marginTop: '20px', display: 'flex', gap: '10px' }}>
                <button type="button" onClick={onCancel} className="btn btn-outline">
                    Отмена
                </button>
                <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? 'Сохранение...' : (habit ? 'Сохранить' : 'Создать')}
                </button>
            </div>
        </form>
    );
}
