
class ScrollManager {
    constructor(controller, selector = '.full-width', storageKey = null) {
        this.controller = controller;
        this.selector = selector;
        this.storageKey = storageKey || `scroll_${Date.now()}`;
        this.scrollPosition = 0;
        this.isRestoring = false;
    }

    // Получить элемент с прокруткой
    getElement() {
        return document.querySelector(this.selector);
    }

    // Получить текущую позицию скролла
    getPosition() {
        const el = this.getElement();
        return el ? el.scrollTop : 0;
    }

    // Получить максимальную позицию скролла
    getMaxPosition() {
        const el = this.getElement();
        return el ? el.scrollHeight - el.clientHeight : 0;
    }

    // Установить позицию скролла
    setPosition(position) {
        const el = this.getElement();
        if (el) {
            el.scrollTop = position;
            this.scrollPosition = position;
        }
    }

    // Сброс скролла в начало
    reset() {
        this.setPosition(0);
        console.log('🧹 Скролл сброшен в положение 0');
        return true;
    }

    // Сброс скролла с анимацией
    resetWithAnimation() {
        const el = this.getElement();
        if (el) {
            el.style.scrollBehavior = 'smooth';
            el.scrollTop = 0;
            setTimeout(() => {
                el.style.scrollBehavior = '';
            }, 500);
            console.log('🎯 Анимированный сброс скролла');
            return true;
        }
        return false;
    }

    // Сохранить текущую позицию скролла
    savePosition() {
        const position = this.getPosition();
        try {
            const currentState = JSON.parse(sessionStorage.getItem(this.storageKey) || '{}');
            currentState.scrollPosition = position;
            currentState.timestamp = new Date().getTime();
            sessionStorage.setItem(this.storageKey, JSON.stringify(currentState));
            return true;
        } catch (e) {
            console.warn('Failed to save scroll position:', e);
            return false;
        }
    }

    // Восстановить позицию скролла
    restorePosition(position = null) {
        if (position !== null) {
            this.setPosition(position);
            return true;
        }

        try {
            const saved = sessionStorage.getItem(this.storageKey);
            if (saved) {
                const state = JSON.parse(saved);
                if (state.scrollPosition) {
                    this.setPosition(state.scrollPosition);
                    console.log('📜 Скролл восстановлен:', state.scrollPosition);
                    return true;
                }
            }
        } catch (e) {
            console.warn('Failed to restore scroll position:', e);
        }
        return false;
    }

    // Восстановить скролл с задержкой (несколько попыток)
    restoreWithDelay(position, delays = [100, 300, 500, 1000]) {
        delays.forEach(delay => {
            setTimeout(() => {
                const el = this.getElement();
                if (el && position) {
                    el.scrollTop = position;
                    console.log(`📜 Скролл восстановлен (${delay}ms):`, position);
                }
            }, delay);
        });
    }

    // Настройка автосохранения скролла при прокрутке
    enableAutoSave(debounceTime = 100) {
        const el = this.getElement();
        if (el) {
            let scrollTimer;
            el.addEventListener('scroll', () => {
                clearTimeout(scrollTimer);
                scrollTimer = setTimeout(() => {
                    this.savePosition();
                }, debounceTime);
            });
            console.log('✅ Автосохранение скролла включено');
        }
    }

    // Отключить автосохранение скролла
    disableAutoSave() {
        const el = this.getElement();
        if (el) {
            const newEl = el.cloneNode(true);
            el.parentNode.replaceChild(newEl, el);
            console.log('⏹ Автосохранение скролла отключено');
        }
    }

    // Проверить, находится ли скролл вверху
    isAtTop() {
        return this.getPosition() === 0;
    }

    // Проверить, находится ли скролл внизу
    isAtBottom() {
        const el = this.getElement();
        return el ? el.scrollTop + el.clientHeight >= el.scrollHeight : false;
    }

    // Прокрутить к элементу
    scrollToElement(elementSelector, offset = 0) {
        const target = document.querySelector(elementSelector);
        const container = this.getElement();

        if (target && container) {
            const targetPosition = target.offsetTop - container.offsetTop - offset;
            container.scrollTop = targetPosition;
            return true;
        }
        return false;
    }

    // Прокрутить на определенное количество пикселей
    scrollBy(amount) {
        const el = this.getElement();
        if (el) {
            el.scrollTop += amount;
            return true;
        }
        return false;
    }

    // Получить информацию о скролле
    getInfo() {
        const el = this.getElement();
        return {
            position: this.getPosition(),
            maxPosition: this.getMaxPosition(),
            height: el?.scrollHeight || 0,
            clientHeight: el?.clientHeight || 0,
            isAtTop: this.isAtTop(),
            isAtBottom: this.isAtBottom(),
            scrollable: el ? el.scrollHeight > el.clientHeight : false
        };
    }

    // Настройка обработчиков для ссылок с сохранением скролла
    setupScrollLinks(storageKey) {
        document.addEventListener('click', (e) => {
            const link = e.target.closest('a.save-scroll');
            if (link && link.href) {
                e.preventDefault();

                // Сохраняем позицию скролла
                this.savePosition();

                // Сохраняем полное состояние через контроллер, если есть
                if (this.controller && typeof this.controller.saveState === 'function') {
                    this.controller.saveState();
                }

                window.location.href = link.href;
            }
        });
    }

    // Очистить сохраненную позицию
    clearSavedPosition() {
        try {
            sessionStorage.removeItem(this.storageKey);
            console.log('🧹 Сохраненная позиция скролла очищена');
        } catch (e) {
            console.warn('Failed to clear scroll position:', e);
        }
    }

    // Обновить ключ хранилища
    updateStorageKey(newKey) {
        this.storageKey = newKey;
    }
}