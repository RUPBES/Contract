// wwwroot/js/release-notes-badge.js

/**
 * Обновляет badge с количеством непрочитанных уведомлений.
 * Вызывается при загрузке страницы и после действий пользователя.
 */
async function refreshUnreadBadge() {
    try {
        const response = await fetch('/release-notes/unread-count', {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const { count } = await response.json();

        const badge = document.getElementById('release-notes-badge');
        if (!badge) return;

        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.classList.remove('d-none');
        } else {
            badge.classList.add('d-none');
        }
    } catch (err) {
        console.warn('Failed to refresh badge:', err);
    }
}

// Обновляем badge при загрузке страницы
document.addEventListener('DOMContentLoaded', refreshUnreadBadge);