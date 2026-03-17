
class MonthYearWidget {
    constructor(inputId, pickerId, onSelectCallback = null) {
        this.input = document.getElementById(inputId);
        this.picker = document.getElementById(pickerId);
        this.currentYear = new Date().getFullYear();
        this.selectedMonth = null;
        this.selectedYear = null;
        this.onSelectCallback = onSelectCallback;

        this.monthNames = [
            'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
            'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'
        ];

        this.shortMonthNames = [
            'Янв', 'Фев', 'Мар', 'Апр', 'Май', 'Июн',
            'Июл', 'Авг', 'Сен', 'Окт', 'Ноя', 'Дек'
        ];

        this.init();
    }

    init() {
        this.renderMonths();
        this.setupEventListeners();
    }

    renderMonths() {
        const monthsGrid = this.picker.querySelector('.md3-months-grid');
        const currentYearElement = this.picker.querySelector('.md3-current-year');

        monthsGrid.innerHTML = '';

        this.monthNames.forEach((month, index) => {
            const button = document.createElement('button');
            button.className = 'month-btn';
            button.dataset.month = index + 1;
            button.textContent = this.shortMonthNames[index];

            // Проверяем, является ли месяц текущим
            const currentDate = new Date();
            if (this.currentYear === currentDate.getFullYear() &&
                (index + 1) === (currentDate.getMonth() + 1)) {
                button.classList.add('current-month');
            }

            // Проверяем, выбран ли месяц
            if (this.selectedYear === this.currentYear &&
                this.selectedMonth === (index + 1)) {
                button.classList.add('selected');
            }

            monthsGrid.appendChild(button);
        });

        if (currentYearElement) {
            currentYearElement.textContent = this.currentYear;
        }
    }

    setupEventListeners() {
        // Клик по полю ввода
        this.input.addEventListener('click', (e) => {
            e.stopPropagation();
            this.togglePicker();
        });

        // Клик по кнопкам месяцев
        this.picker.addEventListener('click', (e) => {
            if (e.target.classList.contains('month-btn')) {
                this.selectMonth(parseInt(e.target.dataset.month));
            }
        });

        // Навигация по годам
        const prevBtn = this.picker.querySelector('.md3-prev-year');
        const nextBtn = this.picker.querySelector('.md3-next-year');

        if (prevBtn) {
            prevBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.currentYear--;
                this.renderMonths();
            });
        }

        if (nextBtn) {
            nextBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.currentYear++;
                this.renderMonths();
            });
        }

        // Кнопка "Текущий"
        const todayBtn = this.picker.querySelector('.md3-today-btn');
        if (todayBtn) {
            todayBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                const now = new Date();
                this.currentYear = now.getFullYear();
                this.selectMonth(now.getMonth() + 1);
            });
        }

        // Кнопка "Очистить"
        const clearBtn = this.picker.querySelector('.md3-clear-btn');
        if (clearBtn) {
            clearBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.clearSelection();
            });
        }

        // Закрытие при клике вне виджета
        document.addEventListener('click', (e) => {
            if (!this.input.contains(e.target) && !this.picker.contains(e.target)) {
                this.hidePicker();
            }
        });

        // Закрытие по Escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.hidePicker();
            }
        });
    }

    togglePicker() {
        this.picker.classList.toggle('show');
        if (this.picker.classList.contains('show')) {
            this.currentYear = this.selectedYear || new Date().getFullYear();
            this.renderMonths();
        }
    }

    showPicker() {
        this.picker.classList.add('show');
        this.currentYear = this.selectedYear || new Date().getFullYear();
        this.renderMonths();
    }

    hidePicker() {
        this.picker.classList.remove('show');
    }

    selectMonth(month) {
        this.selectedMonth = month;
        this.selectedYear = this.currentYear;

        this.updateDisplay();
        this.hidePicker();

        if (this.onSelectCallback) {
            this.onSelectCallback(this.getValue());
        }

        this.triggerChangeEvent();
    }

    setCurrentMonth() {
        const now = new Date();
        this.selectedMonth = now.getMonth() + 1;
        this.selectedYear = now.getFullYear();
        this.updateDisplay();
    }

    clearSelection() {
        this.selectedMonth = null;
        this.selectedYear = null;
        this.updateDisplay();
        this.hidePicker();

        if (this.onSelectCallback) {
            this.onSelectCallback(null);
        }

        this.triggerChangeEvent();
    }

    updateDisplay() {
        if (this.selectedMonth && this.selectedYear) {
            const formattedMonth = String(this.selectedMonth).padStart(2, '0');
            const displayText = `${this.monthNames[this.selectedMonth - 1]} ${this.selectedYear}`;

            this.input.value = displayText;
            this.input.dataset.value = `${this.selectedYear}-${formattedMonth}`;
        } else {
            this.input.value = '';
            this.input.dataset.value = '';
        }
    }

    triggerChangeEvent() {
        const event = new Event('change', {
            bubbles: true,
            cancelable: true
        });
        this.input.dispatchEvent(event);
    }

    getValue() {
        if (this.selectedMonth && this.selectedYear) {
            const formattedMonth = String(this.selectedMonth).padStart(2, '0');
            return `${this.selectedYear}-${formattedMonth}`;
        }
        return null;
    }

    setValue(yearMonthString) {
        if (yearMonthString) {
            const [year, month] = yearMonthString.split('-').map(Number);
            if (year && month >= 1 && month <= 12) {
                this.selectedYear = year;
                this.selectedMonth = month;
                this.updateDisplay();
            }
        }
    }
}