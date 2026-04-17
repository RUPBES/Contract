// Утилиты форматирования
const Fmt = {
    // Дата: "2026-02-19T00:00:00" → "19.02.2026"
    date(val) {
        if (!val) return '';
        const d = new Date(val);
        return `${String(d.getDate()).padStart(2, '0')}.${String(d.getMonth() + 1).padStart(2, '0')}.${d.getFullYear()}`;
    },

    // Число: 550000.5 → "550 000,50"
    decimal(val) {
        if (val == null) return '0.00';
        return Number(val).toLocaleString('ru-RU', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }
};
class DataTableController {
    constructor(options) {
        this.container = document.querySelector(options.container);
        this.apiUrl = options.apiUrl;
        this.apiFilterModalUrl = options.apiFilterModalUrl || '';
        this.itemsPerPage = options.itemsPerPage || 50;
        this.isEngineering = options.isEngineering || false;
        this.isArchive = options.isArchive || false;
        this.permissions = options.permissions || {
            isAdmin: false,
            isLeadAdmin: false,
            isReader: false,
            isCreator: false,
            isEditor: false,
            isDeleter: false,
            company: '',
            groupeName: []
        };

        // Состояние
        this.data = [];
        this.currentPage = 1;
        this.totalPage = 1;
        this.selectedField = '';
        this.sortDirection = 'desc';
        this.searchText = '';
        this.count = 0

        //пагинация
        this.visiblePagBtnCount = 2;
        this.ellipsisPage = 4;

        //фильтрация
        this.startSW = '';
        this.endSW = '';
        this.startEW = '';
        this.endEW = '';
        this.startET = '';
        this.endET = '';

        // ============ Инициализация состояния скролла ============
        this.tableSelector = '.full-width'; // селектор контейнера с прокруткой
        this.storageKey = `contractsTable_${options.container}`;
        this.isRestoring = false;

        this.checkReturnPath = 'Details';
        // ===================================================================


        // ============ ИНИЦИАЛИЗАЦИЯ МЕНЕДЖЕРОВ ============
        // Инициализация менеджера пагинации
        this.pagination = new PaginationManager(this);

        // Инициализация менеджера скролла
        this.scroll = new ScrollManager(this,  this.tableSelector, this.storageKey);
        // =================================================

        this.initGlobalFilterHandlers();
        this.init();
    }

    //// Инициализация
    //init() {

    //    // ============ Восстановление состояния и настройка истории ============
    //    this.setupHistoryHandling();
    //    this.restoreState();
    //    // ===============================================================================

    //    this.loadData();
    //    this.setupEvents();
    //}

    init() {
        this.setupHistoryHandling();

        const restored = this.restoreState();

        if (!restored) {
            this.loadData();
        }

        this.setupEvents();
    }

    // ============  Настройка истории браузера ============
    setupHistoryHandling() {
        // Сохраняем при уходе со страницы
        window.addEventListener('beforeunload', () => this.saveState());

        // Обработка кнопок браузера
        window.addEventListener('popstate', (e) => {
            if (e.state?.tableState) {
                this.isRestoring = true;
                this.restoreStateFromHistory(e.state.tableState);
            }
        });

        // Сохраняем начальное состояние
        const initialState = this.getCurrentState();
        history.replaceState({ tableState: initialState }, '', window.location.href);
    }

    // ДОБАВЛЕНО: Получить текущее состояние
    getCurrentState() {
        return {
            currentPage: this.currentPage,
            selectedField: this.selectedField,
            sortDirection: this.sortDirection,
            searchText: this.searchText,
            scrollPosition: this.scroll.getPosition(), // Используем менеджер скролла
            filters: {
                startSW: this.startSW,
                endSW: this.endSW,
                startEW: this.startEW,
                endEW: this.endEW,
                startET: this.startET,
                endET: this.endET
            },
            isEngineering: this.isEngineering,
            isArchive: this.isArchive,
            path: window.location.pathname,
            timestamp: new Date().getTime()
        };
    }

    // ДОБАВЛЕНО: Сохранить состояние
    saveState() {
        const state = this.getCurrentState();
        sessionStorage.setItem(this.storageKey, JSON.stringify(state));
        history.replaceState({ tableState: state }, '', window.location.href);
    }

    // ДОБАВЛЕНО: Восстановить состояние из истории
    restoreStateFromHistory(state) {
        if (!state) return;

        this.currentPage = state.currentPage || 1;
        this.selectedField = state.selectedField || '';
        this.sortDirection = state.sortDirection || 'asc';
        this.searchText = state.searchText || '';

        if (state.filters) {
            this.startSW = state.filters.startSW || '';
            this.endSW = state.filters.endSW || '';
            this.startEW = state.filters.startEW || '';
            this.endEW = state.filters.endEW || '';
            this.startET = state.filters.startET || '';
            this.endET = state.filters.endET || '';
        }

        // Загружаем данные с восстановленными параметрами
        this.loadData();

        // Восстанавливаем скролл после загрузки через менеджер
        if (state.scrollPosition) {
            this.scroll.restoreWithDelay(state.scrollPosition);
        }

        setTimeout(() => {
            this.isRestoring = false;
        }, 300);
    }

    // ДОБАВЛЕНО: Восстановить состояние при загрузке
    restoreState() {
        try {
            const saved = sessionStorage.getItem(this.storageKey);
            if (saved) {
                const state = JSON.parse(saved);

                if (document.referrer.includes(this.checkReturnPath)) {
                    const isRecent = (new Date().getTime() - state.timestamp) < 600000; // 10 минут
                    if (isRecent) {
                        this.isRestoring = true;
                        this.restoreStateFromHistory(state);
                        return true;
                    }
                }
            }

            if (history.state?.tableState) {
                this.isRestoring = true;
                this.restoreStateFromHistory(history.state.tableState);
                return true;
            }
        } catch (e) {
            console.warn('Failed to restore state:', e);
        }
        return false;
    }

    // Простой сброс (вызов в одну строку)
    resetScrollToUp() {
        return this.scroll.reset();
    }

    // Загрузка данных
    async loadData() {
        try {

            //// ============ ДОБАВЛЕНО: Сохраняем состояние перед загрузкой ============
            //if (!this.isRestoring) {
            //    this.saveState();
            //}
            //// ========================================================================

            // Формируем URL с параметрами
            const params = new URLSearchParams({
                isEngineering: this.isEngineering,
                isArchive: this.isArchive,
                selectedField: this.selectedField,
                page: this.currentPage,
                pageSize: this.itemsPerPage,
                sortDirection: this.sortDirection,
                searchText: this.searchText,
                startSW: this.startSW,
                endSW: this.endSW,
                startEW: this.startEW,
                endEW: this.endEW,
                startET: this.startET,
                endET: this.endET,
            });

            const url = `${this.apiUrl}?${params.toString()}`;
            const response = await fetch(url);
            const data = await response.json();


            //this.data = data.Objects;

            // Нормализуем весь массив один раз — O(n) до рендера
            this.data = this.normalizeContracts(data.Objects);

            // Обновляем состояние пагинации через менеджер
            this.pagination.updateState(data.PageViewModel.TotalPages, data.PageViewModel.Count);

            this.currentPage = data.PageViewModel.PageNumber;
            this.totalPage = data.PageViewModel.TotalPages;
            this.count = data.PageViewModel.Count;

            this.renderTable();

            // Сохраняем только один раз и только если не восстанавливаемся
            if (!this.isRestoring) {
                this.saveState();
            }

            //// ============ ДОБАВЛЕНО: Сохраняем состояние после загрузки ============
            //setTimeout(() => {
            //    if (!this.isRestoring) {
            //        this.saveState();
            //    }
            //}, 100);
            //// =======================================================================

        } catch (error) {
            console.error('Ошибка загрузки:', error);
        }
    }

    normalizeContracts(items) {
        return items.map(item => ({
            ...item,
            // Даты
            Date: Fmt.date(item.Date),
            DateBeginWork: Fmt.date(item.DateBeginWork),
            DateEndWork: Fmt.date(item.DateEndWork),
            EnteringTerm: Fmt.date(item.EnteringTerm),
            // Числа
            ContractPrice: Fmt.decimal(item.ContractPrice),
            PreYearSum: Fmt.decimal(item.PreYearSum),
            RemainingSum: Fmt.decimal(item.RemainingSum),
            ThisYearSum: Fmt.decimal(item.ThisYearSum),
            // Сравнение дат для overdue — сохраняем сырое значение
            _isOverdue: item.DateEndWork && new Date(item.DateEndWork) < new Date()
        }));
    }

    async openFilterModalWindow() {
        const filterContainer = document.querySelector('.contract-filter-container');
        if (!filterContainer) return;

        // Инициализация стилей и показ с анимацией
        this.initFilterContainer(filterContainer);
        await this.showFilter(filterContainer);

        // Загрузка контента
        filterContainer.innerHTML = await this.loadPartialFiltering();
        
        // Управление фильтром
        await this.initFilterControls();

        // Обработчик закрытия по клику вне
        this.setupOutsideClickHandler(filterContainer);
    }

    // Загрузка модального окна фильтрации
    async loadPartialFiltering() {
        try {
            const response = await fetch(`/Contracts/GetHTMLModalFiltering`);
            if (!response.ok) console.log(`Ошибка HTTP: ${response.status}`);
            return await response.text();
        } catch (error) {
            console.error('Ошибка загрузки:', error);
        }
    }

    // Настройка событий
    setupEvents() {
        // СОРТИРОВКА по клику на заголовок
        document.querySelectorAll('[data-content-sort]').forEach((item) => item.addEventListener('click', (e) => {
            if (item) {
                const currentField = this.selectedField;
                this.selectedField = item.getAttribute('data-content-sort');
                if (currentField !== this.selectedField) {
                    this.searchText = '';
                }
                this.setSortMode(item);
                this.loadData();
                this.showResetButton();
            }
        }));

        // ПАГИНАЦИЯ - через менеджер
        this.pagination.setPaginationEvents();

        // Поиск в модальном окне шапки таблицы
        this.setSearchFormsEvents();

        /**
         * открытие окна для Фильтрации
         */
        document.querySelector('.filtering-contract_btn').addEventListener('click', this.openFilterModalWindow.bind(this));

        // ============  Настройка ссылок с сохранением скролла ============
        this.scroll.setupScrollLinks(this.storageKey);

        // Автосохранение скролла при прокрутке
        this.scroll.enableAutoSave(100);

        // Инициализация контекстных меню таблицы
        this.setupActionMenus();
        this.setupAlertHandlers();
    }

    // Установка состояния кнопок сортировки
    setSortMode(item) {
        let isDescSort = item.classList.contains('sort-desc');
        let isAscSort = item.classList.contains('sort-asc');
        let isNotSort = !item.classList.contains('sort-asc') && !item.classList.contains('sort-desc');
        const listSortBtn = document.querySelectorAll('.sort-btn.sort-asc, .sort-btn.sort-desc');

        if (!isNotSort) {
            if (isAscSort && !isDescSort) {
                this.sortDirection = 'desc';
                item.classList.remove('sort-asc', 'sort-desc');
                item.classList.add('sort-desc');
            }
            if (!isAscSort && isDescSort) {
                this.sortDirection = 'asc';
                item.classList.remove('sort-asc', 'sort-desc');
                this.selectedField = '';
            }
        }
        if (isNotSort) {
            this.sortDirection = 'asc';

            listSortBtn.forEach((elem) => {
                elem.classList.remove('sort-asc', 'sort-desc');
            });
            item.classList.toggle('sort-asc');
        }
    }

    // Сброс всего
    resetAll() {
        this.data = [];
        this.currentPage = 1;
        this.totalPage = 1;
        this.selectedField = '';
        this.sortDirection = 'desc';
        this.searchText = '';
        this.count = 0;

        this.startSW = '';
        this.endSW = '';
        this.startEW = '';
        this.endEW = '';
        this.startET = '';
        this.endET = '';

        //Скрытие кнопки сброса
        const resetBtn = document.querySelector('.reset-btn');
        resetBtn.setAttribute('style', 'display:none !important');

        //Сброс подсветки сортировки в таблице
        document.querySelectorAll('.sort-btn.sort-asc, .sort-btn.sort-desc').forEach((item) => {
            item.classList.remove('sort-asc', 'sort-desc');
        });

        // Сброс скролла
        this.scroll.reset();

        this.loadData();
        this.renderTable();
    }

    // Рендер таблицы
    renderTable() {
        // Обновляем тело таблицы
        this.renderBody();

        // Обновляем пагинацию
        //this.renderPagination();

        // Рендер пагинации через менеджер
        this.pagination.renderPagination();

        //this.handleAlert();
    }

    // Рендер тела таблицы
    renderBody() {
        const tbody = this.container.querySelector('.table_tbody');
        if (this.data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="100%">Нет данных</td></tr>';
            return;
        }
        //if (this.isArchive) {
        //    tbody.innerHTML = setContractArchiveTableRow(this.data, this.permissions, this.isEngineering);
        //} else {
        //    tbody.innerHTML = setContractTableRow(this.data, this.permissions, this.isEngineering);
        //}

        //====================================================================================

        const html = this.isArchive
            ? setContractArchiveTableRow(this.data, this.permissions, this.isEngineering)
            : setContractTableRow(this.data, this.permissions, this.isEngineering);

        // Парсим через template — быстрее прямого innerHTML на tbody
        const template = document.createElement('template');
        template.innerHTML = html;

        // Один DOM-удар вместо инкрементальной вставки
        tbody.replaceChildren(template.content);


        //=====================================================================================

        //// Переинициализация меню после перерисовки строк
        //this.setupActionMenus();
    }

    setupActionMenus() {
        const positionMenu = (btn, menu) => {
            // Сначала показываем, чтобы получить реальные размеры
            menu.style.visibility = 'hidden';
            menu.style.display = 'block';

            const btnRect = btn.getBoundingClientRect();
            const menuWidth = menu.offsetWidth || 180;
            const menuHeight = menu.offsetHeight || 250;
            const vw = window.innerWidth;
            const vh = window.innerHeight;
            const gap = 4;

            // position: fixed — координаты viewport, scrollY не нужен
            let top = btnRect.bottom + gap;
            let left = btnRect.right - menuWidth;

            // Выходит за правый край — прижимаем
            if (left < 8) left = 8;
            if (left + menuWidth > vw - 8) left = vw - menuWidth - 8;

            // Выходит за нижний край — открываем вверх
            if (top + menuHeight > vh - 8) {
                top = btnRect.top - menuHeight - gap;
            }
            if (top < 8) top = 8;

            menu.style.position = 'fixed';
            menu.style.top = top + 'px';
            menu.style.left = left + 'px';
            menu.style.zIndex = '9999';
            menu.style.visibility = '';
            menu.style.display = '';
        };

        const positionSubmenu = (wrap) => {
            const submenu = wrap.querySelector('.action-submenu');
            if (!submenu) return;

            // Получаем родительское .action-menu для точной левой границы
            const parentMenu = wrap.closest('.action-menu');
            const itemEl = wrap.querySelector('.menu-item');
            if (!itemEl || !parentMenu) return;

            submenu.style.visibility = 'hidden';
            submenu.style.display = 'block';

            const parentRect = parentMenu.getBoundingClientRect();
            const itemRect = itemEl.getBoundingClientRect();
            const submenuWidth = submenu.offsetWidth || 180;
            const submenuHeight = submenu.offsetHeight || 150;
            const vw = window.innerWidth;
            const vh = window.innerHeight;
            const gap = 4;

            // По умолчанию — правее родительского меню
            let left = parentRect.right + gap;
            let top = itemRect.top;

            // Не помещается справа — открываем левее родительского меню
            if (left + submenuWidth > vw - 8) {
                left = parentRect.left - submenuWidth - gap;
            }

            // Не помещается снизу — прижимаем к нижнему краю
            if (top + submenuHeight > vh - 8) {
                top = vh - submenuHeight - 8;
            }
            if (top < 8) top = 8;

            submenu.style.top = top + 'px';
            submenu.style.left = left + 'px';
            submenu.style.position = 'fixed';
            submenu.style.zIndex = '10000';
            submenu.style.visibility = '';
            submenu.style.display = '';
        };

        const closeAll = () => {
            document.querySelectorAll('.action-menu.open').forEach(m => {
                m.classList.remove('open');
                // Снимаем подсветку с кнопки
                const btn = m.previousElementSibling;
                if (btn?.classList.contains('action-btn')) {
                    btn.classList.remove('active');
                }
            });
        };

        //if (!this.container.dataset.menuInitialized) {
        //    this.container.dataset.menuInitialized = 'true';

        //    // Открытие / закрытие меню
        //    this.container.addEventListener('click', (e) => {
        //        const btn = e.target.closest('.action-btn');
        //        if (!btn) return;

        //        e.stopPropagation();

        //        const menu = btn.nextElementSibling?.classList.contains('action-menu')
        //            ? btn.nextElementSibling
        //            : null;
        //        if (!menu) return;

        //        const isOpen = menu.classList.contains('open');
        //        closeAll();

        //        if (!isOpen) {
        //            menu.classList.add('open');
        //            btn.classList.add('active');
        //            positionMenu(btn, menu);
        //        }
        //    });

        //    // Закрытие по клику вне меню
        //    document.addEventListener('click', closeAll);

        //    // Закрытие по Escape
        //    document.addEventListener('keydown', (e) => {
        //        if (e.key === 'Escape') closeAll();
        //    });

        //    // Закрытие при прокрутке (любой элемент, capture чтобы поймать всё)
        //    window.addEventListener('scroll', closeAll, { capture: true, passive: true });

        //    // Перепозиционирование при ресайзе
        //    window.addEventListener('resize', () => {
        //        const openMenu = document.querySelector('.action-menu.open');
        //        if (!openMenu) return;
        //        const btn = openMenu.previousElementSibling;
        //        if (btn?.classList.contains('action-btn')) positionMenu(btn, openMenu);
        //    });
        //}

        // Позиционирование подменю при наведении
        // Заменить оба блока mouseout на этот код:

        //-----
        if (this.container.dataset.menuInitialized) return; // выходим целиком
        this.container.dataset.menuInitialized = 'true';

        //-----

        const submenuTimers = new WeakMap();


        //----
        // Открытие / закрытие меню
        this.container.addEventListener('click', (e) => {
            const btn = e.target.closest('.action-btn');
            if (!btn) return;

            e.stopPropagation();

            const menu = btn.nextElementSibling?.classList.contains('action-menu')
                ? btn.nextElementSibling
                : null;
            if (!menu) return;

            const isOpen = menu.classList.contains('open');
            closeAll();

            if (!isOpen) {
                menu.classList.add('open');
                btn.classList.add('active');
                positionMenu(btn, menu);
            }
        });

        
        ///-----

        this.container.addEventListener('mouseout', (e) => {
            const wrap = e.target.closest('.menu-item-wrap');
            const submenuEl = e.target.closest('.action-submenu');

            const target = wrap || submenuEl;
            if (!target) return;

            // Находим submenu
            let submenu;
            if (wrap) {
                submenu = wrap.querySelector('.action-submenu');
            } else {
                submenu = submenuEl;
            }
            if (!submenu) return;

            // Даём 100мс — если мышь зашла в submenu или wrap, отменяем закрытие
            const timer = setTimeout(() => {
                submenu.style.display = 'none';
            }, 100);

            submenuTimers.set(submenu, timer);
        });

        this.container.addEventListener('mouseover', (e) => {
            // Отменяем закрытие если навели на wrap или submenu
            const wrap = e.target.closest('.menu-item-wrap');
            const submenuEl = e.target.closest('.action-submenu');

            let submenu;
            if (wrap) {
                submenu = wrap.querySelector('.action-submenu');
            } else if (submenuEl) {
                submenu = submenuEl;
            }

            if (submenu && submenuTimers.has(submenu)) {
                clearTimeout(submenuTimers.get(submenu));
                submenuTimers.delete(submenu);
            }

            // --- остальной код позиционирования из прошлого шага ---
            if (!wrap) return;

            const parentMenu = wrap.closest('.action-menu');
            if (parentMenu) {
                parentMenu.querySelectorAll('.action-submenu').forEach(s => {
                    if (s !== submenu) s.style.display = 'none';
                });
            }

            if (!submenu) return;

            const itemEl = wrap.querySelector('.menu-item');
            if (!itemEl || !parentMenu) return;

            submenu.style.visibility = 'hidden';
            submenu.style.display = 'block';

            const parentRect = parentMenu.getBoundingClientRect();
            const itemRect = itemEl.getBoundingClientRect();
            const submenuWidth = submenu.offsetWidth || 180;
            const submenuHeight = submenu.offsetHeight || 150;
            const vw = window.innerWidth;
            const vh = window.innerHeight;
            const gap = 4;

            let left = parentRect.right + gap;
            let top = itemRect.top;

            if (left + submenuWidth > vw - 8) {
                left = parentRect.left - submenuWidth - gap;
            }
            if (top + submenuHeight > vh - 8) {
                top = vh - submenuHeight - 8;
            }
            if (top < 8) top = 8;

            submenu.style.position = 'fixed';
            submenu.style.top = top + 'px';
            submenu.style.left = left + 'px';
            submenu.style.zIndex = '10000';
            submenu.style.visibility = 'visible';
        });

        // Закрытие по клику вне меню
        document.addEventListener('click', closeAll);

        // Закрытие по Escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') closeAll();
        });

        // Закрытие при прокрутке (любой элемент, capture чтобы поймать всё)
        window.addEventListener('scroll', closeAll, { capture: true, passive: true });

        // Перепозиционирование при ресайзе
        window.addEventListener('resize', () => {
            const openMenu = document.querySelector('.action-menu.open');
            if (!openMenu) return;
            const btn = openMenu.previousElementSibling;
            if (btn?.classList.contains('action-btn')) positionMenu(btn, openMenu);
        });
    }

    initGlobalFilterHandlers() {
        // Обработчик для Escape (глобальный)
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                const container = document.querySelector('.contract-filter-container');
                if (container && container.style.display === 'flex') {
                    this.hideFilter(container);
                }
            }
        });
    }

    // Инициализация стилей контейнера
    initFilterContainer(container) {
        if (container.hasAttribute('data-initialized')) return;

        container.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 2000;
        opacity: 0;
        transition: opacity 0.2s ease;
    `;
        container.setAttribute('data-initialized', 'true');

        // Обработчик Escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && container.style.display === 'flex') {
                this.hideFilter(container);
            }
        });
    }

    // Показать фильтр с анимацией
    showFilter(container) {
        return new Promise(resolve => {
            container.style.display = 'flex';
            requestAnimationFrame(() => {
                container.style.opacity = '1';
                document.body.style.overflow = 'hidden';
                resolve();
            });
        });
    }

    // Скрыть фильтр с анимацией
    hideFilter(container) {
        return new Promise(resolve => {
            container.style.opacity = '0';
            setTimeout(() => {
                container.style.display = 'none';
                container.innerHTML = '';
                document.body.style.overflow = '';
                resolve();
            }, 200);
        });
    }

    // Обработчик клика вне карточки
    setupOutsideClickHandler(container) {
        const handleClick = (e) => {
            if (e.target === container) {
                this.hideFilter(container);
                container.removeEventListener('click', handleClick);
            }
        };

        // Используем setTimeout для предотвращения немедленного срабатывания
        setTimeout(() => {
            container.addEventListener('click', handleClick);
        }, 100);
    }

    // Инициализация управления фильтром
    async initFilterControls() {
        const periodManager = new PeriodManager();
        const searchKeyword = document.getElementById('searchKeyword');

        // Обработчик Enter в поле поиска
        searchKeyword?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                this.applyFilter(periodManager, searchKeyword);
            }
        });

        // Кнопка применения
        document.getElementById('applyButton')?.addEventListener('click', () => {
            this.applyFilter(periodManager, searchKeyword);
        });

        // Кнопка сброса
        document.getElementById('resetButton')?.addEventListener('click', () => {
            document.getElementById('filterField').value = '';
            searchKeyword.value = '';
            periodManager.reset();
        });

        // Кнопка закрытия
        document.getElementById('closeButton')?.addEventListener('click', () => {
            const container = document.querySelector('.contract-filter-container');
            this.hideFilter(container);
        });
    }

    // Применить фильтр и закрыть окно
    async applyFilter(periodManager, searchKeyword) {
        const paramsFilter = this.performSearch(periodManager, searchKeyword);

        // Сохранение параметров фильтра
        this.saveFilterParams(paramsFilter);

        // Загрузка данных с новыми фильтрами
        await this.loadData();

        // ============ ДОБАВЛЕНО: Сохраняем состояние после применения фильтров ============
        this.saveState();
        // ===============================================================================


        // Закрытие окна фильтра
        const container = document.querySelector('.contract-filter-container');
        this.showResetButton();
        await this.hideFilter(container);
    }

    // Выполнить поиск
    performSearch(periodManager, searchKeyword) {
        const filterField = document.getElementById('filterField')?.value || '';

        return {
            field: filterField,
            keyword: searchKeyword?.value.trim() || '',
            period: periodManager.getPeriod()
        };
    }

    // Сохранение параметров фильтра
    saveFilterParams(paramsFilter) {
        this.sortDirection = paramsFilter.period.sort;
        this.selectedField = paramsFilter.field;
        this.searchText = paramsFilter.keyword;

        // Сохранение периодов с проверкой
        const periodFields = ['startSW', 'endSW', 'startEW', 'endEW', 'startET', 'endET'];
        periodFields.forEach(field => {
            if (paramsFilter.period && paramsFilter.period[field]) {
                this[field] = typeof paramsFilter.period[field] === 'string'
                    ? paramsFilter.period[field]
                    : '';
            } else {
                this[field] = '';
            }
        });
    }


    //**** ПОИСКОВИК по столбцам ===============
    openSearchForm(button) {
        const formWrapper = this.container.querySelector('.search-form-wrapper');
        if (!formWrapper) return;

        this.positionForm(button, formWrapper);
        const filed = button.getAttribute('data-content-search');
        const input = formWrapper.querySelector('input[type="text"]');

        if (this.selectedField !== filed) {
            this.selectedField = filed;
            if (input) input.value = '';
            document.querySelector('#clearButton.clear-btn').style.display = 'none';
        }

        formWrapper.style.display = 'block';
        formWrapper.style.opacity = '0';
        formWrapper.style.transform = 'translateY(-40px)';

        setTimeout(() => {
            formWrapper.style.opacity = '1';
            formWrapper.style.transform = 'translateY(0)';
            formWrapper.style.transition = 'all 0.3s ease';

            if (input) input.focus();
        }, 10);
    }

    closeSearchForm(formWrapper) {
        formWrapper.style.opacity = '0';
        formWrapper.style.transform = 'translateY(-40px)';

        setTimeout(() => {
            formWrapper.style.display = 'none';
        }, 300);
    }

    closeAllSearchForms() {
        const allForms = document.querySelectorAll('.search-form-wrapper');
        allForms.forEach(form => {
            if (form.style.display === 'block') {
                this.closeSearchForm(form);
            }
        });
    }

    positionForm(button, formWrapper) {
        const rect = button.getBoundingClientRect();
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

        //formWrapper.style.position = 'absolute';
        formWrapper.style.top = (rect.bottom + scrollTop + 25) + 'px';
        formWrapper.style.left = (rect.left + scrollLeft - 120) + 'px';
        formWrapper.style.zIndex = '1000';
        formWrapper.style.minWidth = '280px';

        if (window.innerWidth < 768) {
            formWrapper.style.width = 'calc(100vw - 40px)';
            formWrapper.style.left = '20px';
            formWrapper.style.right = '20px';
        }
    }

    setSearchFormsEvents() {
        /** Открытие модального окна ПОИСКА */
        document.querySelectorAll('[data-content-search]').forEach(button => {
            button.addEventListener('click', () => this.handleSearchButtonClick(button));
        });

        /**
         * поиск в модальном окне!
         * оброботкичи событий кнопки посика и кнопки очистки поля ввода
         */
        const searchInput = document.getElementById('searchKeyword');
        const clearButton = document.getElementById('clearButton');

        // Обновить видимость кнопки
        function updateClearButton() {
            clearButton.style.display = searchInput.value.trim() ? 'block' : 'none';
        }

        // Очистить поле
        function clearInput() {
            searchInput.value = '';
            searchInput.focus();
            updateClearButton();
        }

        // События
        searchInput.addEventListener('input', updateClearButton);
        clearButton.addEventListener('click', clearInput);

        // Инициализация
        updateClearButton();

        /**
         * обработка событий при срабатывании которых закрываем Окно поиска
         */
        document.addEventListener('click', this.handleOutsideClick.bind(this));
        document.addEventListener('keydown', this.handleEscapeKey.bind(this));
        document.addEventListener('keydown', this.handleEnterKey.bind(this));

        /**
         * запрос загрузки данных по поисковому окну
         */
        this.container.querySelector('button#search-button').addEventListener('click', this.handleSearchBtn.bind(this));
    }
    //******=====================================


    // Кнопка RESET ==================
    showResetButton() {
        const resetBtn = document.querySelector('.reset-btn');
        if (resetBtn.style.display === 'none !important') {
            return
        }
        resetBtn.addEventListener('click', () => {
            this.resetAll();
        });
        resetBtn.style.display = 'flex';
    }

    // Обработчики событий
    handleSearchButtonClick(button) {
        this.updateFormPositions(button);
        this.openSearchForm(button);
    }

    handleOutsideClick(event) {
        if (!event || !event.target) return;

        const clickedButton = event.target.closest('[data-content-search]');
        const clickedForm = event.target.closest('.search-form-wrapper');

        // Если клик был НЕ по кнопке и НЕ по форме
        if (!clickedButton && !clickedForm) this.closeAllSearchForms();
    }

    handleEscapeKey(event) {
        if (event.key === 'Escape') {
            this.closeAllSearchForms();
        }
    }

    //поиск по нажатию на клавишу Enter
    handleEnterKey(event) {
        if (event.key === 'Enter' || event.keyCode === 13) {
            event.preventDefault();
            const input = this.container.querySelector('input[type="text"].input-sarch-filter');
            if (input) {
                this.currentPage = 1;
                this.searchText = input.value;
                this.showResetButton();
                this.closeAllSearchForms();
                this.loadData();
            }
        }
    }

    //поиск по нажатию на кнопку
    handleSearchBtn() {
        const input = this.container.querySelector('input[type="text"].input-sarch-filter');
        if (input) {
            this.currentPage = 1;
            this.searchText = input.value;
            this.closeAllSearchForms();
            this.showResetButton();
            this.loadData();
        }
    }

    updateFormPositions(button) {
        const openForms = document.querySelectorAll('.search-form-wrapper[style*="display: none"]');

        openForms.forEach(formWrapper => {
            if (button) {
                this.positionForm(button, formWrapper);
            }
        });
    }

    // Вызвать один раз в setupEvents(), убрать из renderTable()
    setupAlertHandlers() {
        let currentUrl = '';

        // Один обработчик на весь документ вместо N обработчиков на каждую ссылку
        document.addEventListener('click', (e) => {
            const link = e.target.closest('.modal-link');
            if (!link) return;
            e.preventDefault();

            currentUrl = link.href;
            document.getElementById('alert-modal_title').textContent =
                link.getAttribute('data-title') || 'Предупреждение';
            document.getElementById('alert-modal_message').textContent =
                link.getAttribute('data-message') || 'Вы уверены?';
            document.getElementById('alert-modal').style.display = 'block';
        });

        document.getElementById('alert-modal_confirm').addEventListener('click', () => {
            window.location.href = currentUrl;
        });

        document.getElementById('alert-modal_cancel').addEventListener('click', () => {
            document.getElementById('alert-modal').style.display = 'none';
        });
    }

    //handleAlert() {
    //    let currentUrl = '';

    //    document.querySelectorAll('.modal-link').forEach(link => {
    //        link.addEventListener('click', function (e) {
    //            e.preventDefault();

    //            currentUrl = this.href;
    //            document.getElementById('alert-modal_title').textContent = this.getAttribute('data-title') || 'Предупреждение';
    //            document.getElementById('alert-modal_message').textContent = this.getAttribute('data-message') || 'Вы уверены?';

    //            document.getElementById('alert-modal').style.display = 'block';
    //        });
    //    });

    //    document.getElementById('alert-modal_confirm').addEventListener('click', function () {
    //        window.location.href = currentUrl;
    //    });

    //    document.getElementById('alert-modal_cancel').addEventListener('click', function () {
    //        document.getElementById('alert-modal').style.display = 'none';
    //    });
    //}
}

class DataPayableCashTableController {
    constructor(options) {
        this.container = document.querySelector(options.container);
        this.apiUrl = options.apiUrl;
        this.apiFilterModalUrl = options.apiFilterModalUrl || '';
        this.itemsPerPage = options.itemsPerPage || 50;

        // Состояние
        this.data = [];
        this.currentPage = 1;
        this.totalPage = 1;
        this.selectedField = '';
        this.sortDirection = 'desc';
        this.searchText = '';
        this.count = 0

        //пагинация
        this.visiblePagBtnCount = 2;
        this.ellipsisPage = 4;

        //фильтрация
        this.startSW = '';
        this.endSW = '';
        this.startEW = '';
        this.endEW = '';
        this.startET = '';
        this.endET = '';

        // ============ Инициализация состояния скролла ============
        this.tableSelector = '.wrapper-table'; // селектор контейнера с прокруткой
        this.storageKey = `payableCashTable_${options.container}`;
        this.isRestoring = false;
        this.checkReturnPath = 'DetailsPayableCash';
        // ===================================================================

        // Инициализация менеджера пагинации
        this.pagination = new PaginationManager(this);
        // Инициализация менеджера скролла
        this.scroll = new ScrollManager(this, this.tableSelector, this.storageKey);
        // =================================================

        this.initGlobalFilterHandlers();
        this.init();
    }

    // Инициализация
    init() {
        this.setupHistoryHandling();

        const restored = this.restoreState();

        if (!restored) {
            this.loadData();
        }

        this.setupEvents();
    }

    // ============  Настройка истории браузера ============
    setupHistoryHandling() {
        // Сохраняем при уходе со страницы
        window.addEventListener('beforeunload', () => this.saveState());

        // Обработка кнопок браузера
        window.addEventListener('popstate', (e) => {
            if (e.state?.tableState) {
                this.isRestoring = true;
                this.restoreStateFromHistory(e.state.tableState);
            }
        });

        // Сохраняем начальное состояние
        const initialState = this.getCurrentState();
        history.replaceState({ tableState: initialState }, '', window.location.href);
    }

    // ДОБАВЛЕНО: Получить текущее состояние
    getCurrentState() {
        return {
            currentPage: this.currentPage,
            selectedField: this.selectedField,
            sortDirection: this.sortDirection,
            searchText: this.searchText,
            scrollPosition: this.scroll.getPosition(), // Используем менеджер скролла
            filters: {
                startSW: this.startSW,
                endSW: this.endSW,
                startEW: this.startEW,
                endEW: this.endEW,
                startET: this.startET,
                endET: this.endET
            },

            path: window.location.pathname,
            timestamp: new Date().getTime()
        };
    }

    // ДОБАВЛЕНО: Сохранить состояние
    saveState() {
        const state = this.getCurrentState();
        sessionStorage.setItem(this.storageKey, JSON.stringify(state));
        history.replaceState({ tableState: state }, '', window.location.href);
    }

    // ДОБАВЛЕНО: Восстановить состояние из истории
    restoreStateFromHistory(state) {
        if (!state) return;

        this.currentPage = state.currentPage || 1;
        this.selectedField = state.selectedField || '';
        this.sortDirection = state.sortDirection || 'asc';
        this.searchText = state.searchText || '';

        if (state.filters) {
            this.startSW = state.filters.startSW || '';
            this.endSW = state.filters.endSW || '';
            this.startEW = state.filters.startEW || '';
            this.endEW = state.filters.endEW || '';
            this.startET = state.filters.startET || '';
            this.endET = state.filters.endET || '';
        }

        // Загружаем данные с восстановленными параметрами
        this.loadData();

        // Восстанавливаем скролл после загрузки через менеджер
        if (state.scrollPosition) {
            this.scroll.restoreWithDelay(state.scrollPosition);
        }

        setTimeout(() => {
            this.isRestoring = false;
        }, 300);
    }

    // ДОБАВЛЕНО: Восстановить состояние при загрузке
    restoreState() {
        try {
            const saved = sessionStorage.getItem(this.storageKey);
            if (saved) {
                const state = JSON.parse(saved);

                if (document.referrer.includes(this.checkReturnPath)) {
                    const isRecent = (new Date().getTime() - state.timestamp) < 600000; // 10 минут
                    if (isRecent) {
                        this.isRestoring = true;
                        this.restoreStateFromHistory(state);
                        return true;
                    }
                }
            }

            if (history.state?.tableState) {
                this.isRestoring = true;
                this.restoreStateFromHistory(history.state.tableState);
                return true;
            }
        } catch (e) {
            console.warn('Failed to restore state:', e);
        }
        return false;
    }

    // Простой сброс (вызов в одну строку)
    resetScrollToUp() {
        return this.scroll.reset();
    }

    // Загрузка данных
    async loadData() {
        try {
            // Формируем URL с параметрами
            const params = new URLSearchParams({
                selectedField: this.selectedField,
                page: this.currentPage,
                pageSize: this.itemsPerPage,
                sortDirection: this.sortDirection,
                searchText: this.searchText,
                startSW: this.startSW,
                endSW: this.endSW,
                startEW: this.startEW,
                endEW: this.endEW,
                startET: this.startET,
                endET: this.endET,
            });

            const url = `${this.apiUrl}?${params.toString()}`;
            const response = await fetch(url);
            const data = await response.json();

            this.data = data.Objects;
            // Обновляем состояние пагинации через менеджер
            this.pagination.updateState(data.PageViewModel.TotalPages, data.PageViewModel.Count);

            this.currentPage = data.PageViewModel.PageNumber;
            this.totalPage = data.PageViewModel.TotalPages;
            this.count = data.PageViewModel.Count;

            this.renderTable();

            // Сохраняем только один раз и только если не восстанавливаемся
            if (!this.isRestoring) {
                this.saveState();
            }

        } catch (error) {
            console.error('Ошибка загрузки:', error);
        }
    }

    async openFilterModalWindow() {
        const filterContainer = document.querySelector('.contract-filter-container');
        if (!filterContainer) return;

        // Инициализация стилей и показ с анимацией
        this.initFilterContainer(filterContainer);
        await this.showFilter(filterContainer);

        // Загрузка контента
        filterContainer.innerHTML = await this.loadPartialFiltering();

        // Управление фильтром
        await this.initFilterControls();

        // Обработчик закрытия по клику вне
        this.setupOutsideClickHandler(filterContainer);
    }

    // Загрузка модального окна фильтрации
    async loadPartialFiltering() {
        try {
            const response = await fetch(this.apiFilterModalUrl);
            if (!response.ok) console.log(`Ошибка HTTP: ${response.status}`);
            return await response.text();
        } catch (error) {
            console.error('Ошибка загрузки:', error);
        }
    }

    // Настройка событий
    setupEvents() {
        // СОРТИРОВКА по клику на заголовок
        document.querySelectorAll('[data-content-sort]').forEach((item) => item.addEventListener('click', (e) => {
            if (item) {
                const currentField = this.selectedField;
                this.selectedField = item.getAttribute('data-content-sort');
                if (currentField !== this.selectedField) {
                    this.searchText = '';
                }
                this.setSortMode(item);
                this.loadData();
                this.showResetButton();
            }
        }));

        // ПАГИНАЦИЯ - через менеджер
        this.pagination.setPaginationEvents();

        // Поиск в модальном окне шапки таблицы
        this.setSearchFormsEvents();

        /**
         * открытие окна для Фильтрации
         */
        document.querySelector('.filtering-contract_btn').addEventListener('click', this.openFilterModalWindow.bind(this));

        // ============  Настройка ссылок с сохранением скролла ============
        this.scroll.setupScrollLinks(this.storageKey);

        // Автосохранение скролла при прокрутке
        this.scroll.enableAutoSave(100);

        //this.setupActionMenus();
    }

    // Установка состояния кнопок сортировки
    setSortMode(item) {
        let isDescSort = item.classList.contains('sort-desc');
        let isAscSort = item.classList.contains('sort-asc');
        let isNotSort = !item.classList.contains('sort-asc') && !item.classList.contains('sort-desc');
        const listSortBtn = document.querySelectorAll('.sort-btn.sort-asc, .sort-btn.sort-desc');

        if (!isNotSort) {
            if (isAscSort && !isDescSort) {
                this.sortDirection = 'desc';
                item.classList.remove('sort-asc', 'sort-desc');
                item.classList.add('sort-desc');
            }
            if (!isAscSort && isDescSort) {
                this.sortDirection = 'asc';
                item.classList.remove('sort-asc', 'sort-desc');
                this.selectedField = '';
            }
        }
        if (isNotSort) {
            this.sortDirection = 'asc';

            listSortBtn.forEach((elem) => {
                elem.classList.remove('sort-asc', 'sort-desc');
            });
            item.classList.toggle('sort-asc');
        }
    }

    // Сброс всего
    resetAll() {
        this.data = [];
        this.currentPage = 1;
        this.totalPage = 1;
        this.selectedField = '';
        this.sortDirection = 'desc';
        this.searchText = '';
        this.count = 0;

        this.startSW = '';
        this.endSW = '';
        this.startEW = '';
        this.endEW = '';
        this.startET = '';
        this.endET = '';

        //Скрытие кнопки сброса
        const resetBtn = document.querySelector('.reset-btn');
        resetBtn.setAttribute('style', 'display:none !important');

        //Сброс подсветки сортировки в таблице
        document.querySelectorAll('.sort-btn.sort-asc, .sort-btn.sort-desc').forEach((item) => {
            item.classList.remove('sort-asc', 'sort-desc');
        });

        // Сброс скролла
        this.scroll.reset();

        this.loadData();
        this.renderTable();
    }

    // Рендер таблицы
    renderTable() {
        // Обновляем тело таблицы
        this.renderBody();

        // Обновляем пагинацию
        //this.renderPagination();

        // Рендер пагинации через менеджер
        this.pagination.renderPagination();

        
    }

    // Рендер тела таблицы
    renderBody() {
        const tbody = this.container.querySelector('.table_tbody');
        if (this.data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="100%">Нет данных</td></tr>';
            return;
        }
        tbody.innerHTML = setPayableCashTableRow(this.data);
    }


    initGlobalFilterHandlers() {
        // Обработчик для Escape (глобальный)
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                const container = document.querySelector('.contract-filter-container');
                if (container && container.style.display === 'flex') {
                    this.hideFilter(container);
                }
            }
        });
    }

    // Инициализация стилей контейнера
    initFilterContainer(container) {
        if (container.hasAttribute('data-initialized')) return;

        container.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 2000;
        opacity: 0;
        transition: opacity 0.2s ease;
    `;
        container.setAttribute('data-initialized', 'true');

        // Обработчик Escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && container.style.display === 'flex') {
                this.hideFilter(container);
            }
        });
    }

    // Показать фильтр с анимацией
    showFilter(container) {
        return new Promise(resolve => {
            container.style.display = 'flex';
            requestAnimationFrame(() => {
                container.style.opacity = '1';
                document.body.style.overflow = 'hidden';
                resolve();
            });
        });
    }

    // Скрыть фильтр с анимацией
    hideFilter(container) {
        return new Promise(resolve => {
            container.style.opacity = '0';
            setTimeout(() => {
                container.style.display = 'none';
                container.innerHTML = '';
                document.body.style.overflow = '';
                resolve();
            }, 200);
        });
    }

    // Обработчик клика вне карточки
    setupOutsideClickHandler(container) {
        const handleClick = (e) => {
            if (e.target === container) {
                this.hideFilter(container);
                container.removeEventListener('click', handleClick);
            }
        };

        // Используем setTimeout для предотвращения немедленного срабатывания
        setTimeout(() => {
            container.addEventListener('click', handleClick);
        }, 100);
    }

    // Инициализация управления фильтром
    async initFilterControls() {
        const periodManager = new PeriodManager();
        const searchKeyword = document.getElementById('searchKeyword');

        // Обработчик Enter в поле поиска
        searchKeyword?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                this.applyFilter(periodManager, searchKeyword);
            }
        });

        // Кнопка применения
        document.getElementById('applyButton')?.addEventListener('click', () => {
            this.applyFilter(periodManager, searchKeyword);
        });

        // Кнопка сброса
        document.getElementById('resetButton')?.addEventListener('click', () => {
            document.getElementById('filterField').value = '';
            searchKeyword.value = '';
            periodManager.reset();
        });

        // Кнопка закрытия
        document.getElementById('closeButton')?.addEventListener('click', () => {
            const container = document.querySelector('.contract-filter-container');
            this.hideFilter(container);
        });
    }

    // Применить фильтр и закрыть окно
    async applyFilter(periodManager, searchKeyword) {
        const paramsFilter = this.performSearch(periodManager, searchKeyword);

        // Сохранение параметров фильтра
        this.saveFilterParams(paramsFilter);

        // Загрузка данных с новыми фильтрами
        await this.loadData();

        // ============ ДОБАВЛЕНО: Сохраняем состояние после применения фильтров ============
        this.saveState();
        // ===============================================================================


        // Закрытие окна фильтра
        const container = document.querySelector('.contract-filter-container');
        this.showResetButton();
        await this.hideFilter(container);
    }

    // Выполнить поиск
    performSearch(periodManager, searchKeyword) {
        const filterField = document.getElementById('filterField')?.value || '';

        return {
            field: filterField,
            keyword: searchKeyword?.value.trim() || '',
            period: periodManager.getPeriod()
        };
    }

    // Сохранение параметров фильтра
    saveFilterParams(paramsFilter) {
        this.sortDirection = paramsFilter.period.sort;
        this.selectedField = paramsFilter.field;
        this.searchText = paramsFilter.keyword;

        // Сохранение периодов с проверкой
        const periodFields = ['startSW', 'endSW', 'startEW', 'endEW', 'startET', 'endET'];
        periodFields.forEach(field => {
            if (paramsFilter.period && paramsFilter.period[field]) {
                this[field] = typeof paramsFilter.period[field] === 'string'
                    ? paramsFilter.period[field]
                    : '';
            } else {
                this[field] = '';
            }
        });
    }


    //**** ПОИСКОВИК по столбцам ===============
    openSearchForm(button) {
        const formWrapper = this.container.querySelector('.search-form-wrapper');
        if (!formWrapper) return;

        this.positionForm(button, formWrapper);
        const filed = button.getAttribute('data-content-search');
        const input = formWrapper.querySelector('input[type="text"]');

        if (this.selectedField !== filed) {
            this.selectedField = filed;
            if (input) input.value = '';
            document.querySelector('#clearButton.clear-btn').style.display = 'none';
        }

        formWrapper.style.display = 'block';
        formWrapper.style.opacity = '0';
        formWrapper.style.transform = 'translateY(-40px)';

        setTimeout(() => {
            formWrapper.style.opacity = '1';
            formWrapper.style.transform = 'translateY(0)';
            formWrapper.style.transition = 'all 0.3s ease';

            if (input) input.focus();
        }, 10);
    }

    closeSearchForm(formWrapper) {
        formWrapper.style.opacity = '0';
        formWrapper.style.transform = 'translateY(-40px)';

        setTimeout(() => {
            formWrapper.style.display = 'none';
        }, 300);
    }

    closeAllSearchForms() {
        const allForms = document.querySelectorAll('.search-form-wrapper');
        allForms.forEach(form => {
            if (form.style.display === 'block') {
                this.closeSearchForm(form);
            }
        });
    }

    positionForm(button, formWrapper) {
        const rect = button.getBoundingClientRect();
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

        //formWrapper.style.position = 'absolute';
        formWrapper.style.top = (rect.bottom + scrollTop + 25) + 'px';
        formWrapper.style.left = (rect.left + scrollLeft - 120) + 'px';
        formWrapper.style.zIndex = '1000';
        formWrapper.style.minWidth = '280px';

        if (window.innerWidth < 768) {
            formWrapper.style.width = 'calc(100vw - 40px)';
            formWrapper.style.left = '20px';
            formWrapper.style.right = '20px';
        }
    }

    setSearchFormsEvents() {
        /** Открытие модального окна ПОИСКА */
        document.querySelectorAll('[data-content-search]').forEach(button => {
            button.addEventListener('click', () => this.handleSearchButtonClick(button));
        });

        /**
         * поиск в модальном окне!
         * оброботкичи событий кнопки посика и кнопки очистки поля ввода
         */
        const searchInput = document.getElementById('searchKeyword');
        const clearButton = document.getElementById('clearButton');

        // Обновить видимость кнопки
        function updateClearButton() {
            clearButton.style.display = searchInput.value.trim() ? 'block' : 'none';
        }

        // Очистить поле
        function clearInput() {
            searchInput.value = '';
            searchInput.focus();
            updateClearButton();
        }

        // События
        searchInput.addEventListener('input', updateClearButton);
        clearButton.addEventListener('click', clearInput);

        // Инициализация
        updateClearButton();

        /**
         * обработка событий при срабатывании которых закрываем Окно поиска
         */
        document.addEventListener('click', this.handleOutsideClick.bind(this));
        document.addEventListener('keydown', this.handleEscapeKey.bind(this));
        document.addEventListener('keydown', this.handleEnterKey.bind(this));

        /**
         * запрос загрузки данных по поисковому окну
         */
        this.container.querySelector('button#search-button').addEventListener('click', this.handleSearchBtn.bind(this));
    }
    //******=====================================


    // Кнопка RESET ==================
    showResetButton() {
        const resetBtn = document.querySelector('.reset-btn');
        if (resetBtn.style.display === 'none !important') {
            return
        }
        resetBtn.addEventListener('click', () => {
            this.resetAll();
        });
        resetBtn.style.display = 'flex';
    }

    // Обработчики событий
    handleSearchButtonClick(button) {
        this.updateFormPositions(button);
        this.openSearchForm(button);
    }

    handleOutsideClick(event) {
        if (!event || !event.target) return;

        const clickedButton = event.target.closest('[data-content-search]');
        const clickedForm = event.target.closest('.search-form-wrapper');

        // Если клик был НЕ по кнопке и НЕ по форме
        if (!clickedButton && !clickedForm) this.closeAllSearchForms();
    }

    handleEscapeKey(event) {
        if (event.key === 'Escape') {
            this.closeAllSearchForms();
        }
    }

    //поиск по нажатию на клавишу Enter
    handleEnterKey(event) {
        if (event.key === 'Enter' || event.keyCode === 13) {
            event.preventDefault();
            const input = this.container.querySelector('input[type="text"].input-sarch-filter');
            if (input) {
                this.currentPage = 1;
                this.searchText = input.value;
                this.showResetButton();
                this.closeAllSearchForms();
                this.loadData();
            }
        }
    }

    //поиск по нажатию на кнопку
    handleSearchBtn() {
        const input = this.container.querySelector('input[type="text"].input-sarch-filter');
        if (input) {
            this.currentPage = 1;
            this.searchText = input.value;
            this.closeAllSearchForms();
            this.showResetButton();
            this.loadData();
        }
    }

    updateFormPositions(button) {
        const openForms = document.querySelectorAll('.search-form-wrapper[style*="display: none"]');

        openForms.forEach(formWrapper => {
            if (button) {
                this.positionForm(button, formWrapper);
            }
        });
    }

    
}


class DataWithoutDatesTableController {
    constructor(options) {
        this.container = document.querySelector(options.container);
        this.apiUrl = options.apiUrl;
        this.apiFilterModalUrl = options.apiFilterModalUrl || '';
        this.itemsPerPage = options.itemsPerPage || 50;
        this.permissions = options.permissions || {
            isAdmin: false,
            isLeadAdmin: false,
            isReader: false,
            isCreator: false,
            isEditor: false,
            isDeleter: false,
            company: '',
            groupeName: []
        };
        this.tableType = options.table || 'organization';

        // Состояние
        this.data = [];
        this.currentPage = 1;
        this.totalPage = 1;
        this.selectedField = '';
        this.sortDirection = 'desc';
        this.searchText = '';
        this.count = 0

        //пагинация
        this.visiblePagBtnCount = 2;
        this.ellipsisPage = 4;      

        // ============ Инициализация состояния скролла ============
        this.tableSelector = '.wrapper-table'; // селектор контейнера с прокруткой
        this.storageKey = `guidebookTable_${options.container}`;
        this.isRestoring = false;
        this.checkReturnPath = 'Details';
        // ===================================================================

        // Инициализация менеджера пагинации
        this.pagination = new PaginationManager(this);
        // Инициализация менеджера скролла
        this.scroll = new ScrollManager(this, this.tableSelector, this.storageKey);
        // =================================================

        this.initGlobalFilterHandlers();
        this.init();
    }

    // Инициализация
    init() {
        this.setupHistoryHandling();
        const restored = this.restoreState(); // получаем флаг

        if (!restored) {               // loadData только если не восстановились
            this.loadData();
        }
        this.setupEvents();
    }

    // ============  Настройка истории браузера ============
    setupHistoryHandling() {
        // Сохраняем при уходе со страницы
        window.addEventListener('beforeunload', () => this.saveState());

        // Обработка кнопок браузера
        window.addEventListener('popstate', (e) => {
            if (e.state?.tableState) {
                this.isRestoring = true;
                this.restoreStateFromHistory(e.state.tableState);
            }
        });

        // Сохраняем начальное состояние
        const initialState = this.getCurrentState();
        history.replaceState({ tableState: initialState }, '', window.location.href);
    }

    // Получить текущее состояние
    getCurrentState() {
        return {
            currentPage: this.currentPage,
            selectedField: this.selectedField,
            sortDirection: this.sortDirection,
            searchText: this.searchText,
            scrollPosition: this.scroll.getPosition(), // Используем менеджер скролла
            path: window.location.pathname,
            timestamp: new Date().getTime()
        };
    }

    // Сохранить состояние
    saveState() {
        const state = this.getCurrentState();
        sessionStorage.setItem(this.storageKey, JSON.stringify(state));
        history.replaceState({ tableState: state }, '', window.location.href);
    }

    // Восстановить состояние из истории
    restoreStateFromHistory(state) {
        if (!state) return;

        this.currentPage = state.currentPage || 1;
        this.selectedField = state.selectedField || '';
        this.sortDirection = state.sortDirection || 'asc';
        this.searchText = state.searchText || '';

        // Загружаем данные с восстановленными параметрами
        this.loadData();

        // Восстанавливаем скролл после загрузки через менеджер
        if (state.scrollPosition) {
            this.scroll.restoreWithDelay(state.scrollPosition);
        }

        setTimeout(() => {
            this.isRestoring = false;
        }, 300);
    }

    // Восстановить состояние при загрузке
    restoreState() {
        try {
            const saved = sessionStorage.getItem(this.storageKey);
            if (saved) {
                const state = JSON.parse(saved);

                if (document.referrer.includes(this.checkReturnPath)) {
                    const isRecent = (new Date().getTime() - state.timestamp) < 600000; // 10 минут
                    if (isRecent) {
                        this.isRestoring = true;
                        this.restoreStateFromHistory(state);
                        return true;
                    }
                }
            }

            if (history.state?.tableState) {
                this.isRestoring = true;
                this.restoreStateFromHistory(history.state.tableState);
                return true;
            }
        } catch (e) {
            console.warn('Failed to restore state:', e);
        }
        return false;
    }

    // Простой сброс (вызов в одну строку)
    resetScrollToUp() {
        return this.scroll.reset();
    }

    // Загрузка данных
    async loadData() {
        try {
            // Формируем URL с параметрами
            const params = new URLSearchParams({
                selectedField: this.selectedField,
                page: this.currentPage,
                pageSize: this.itemsPerPage,
                sortDirection: this.sortDirection,
                searchText: this.searchText,                
            });

            const url = `${this.apiUrl}?${params.toString()}`;
            const response = await fetch(url);
            const data = await response.json();

            this.data = data.Objects;
            // Обновляем состояние пагинации через менеджер
            this.pagination.updateState(data.PageViewModel.TotalPages, data.PageViewModel.Count);

            this.currentPage = data.PageViewModel.PageNumber;
            this.totalPage = data.PageViewModel.TotalPages;
            this.count = data.PageViewModel.Count;

            this.renderTable();

            // Сохраняем только один раз и только если не восстанавливаемся
            if (!this.isRestoring) {
                this.saveState();
            }

        } catch (error) {
            console.error('Ошибка загрузки:', error);
        }
    }

    async openFilterModalWindow() {
        const filterContainer = document.querySelector('.contract-filter-container');
        if (!filterContainer) return;

        // Инициализация стилей и показ с анимацией
        this.initFilterContainer(filterContainer);
        await this.showFilter(filterContainer);

        // Загрузка контента
        filterContainer.innerHTML = await this.loadPartialFiltering();
        this.sortDirection = 'desc';
        // Управление фильтром
        await this.initFilterControls();

        // Обработчик закрытия по клику вне
        this.setupOutsideClickHandler(filterContainer);
    }

    // Загрузка модального окна фильтрации
    async loadPartialFiltering() {
        try {
            const response = await fetch(this.apiFilterModalUrl);
            if (!response.ok) console.log(`Ошибка HTTP: ${response.status}`);
            return await response.text();
        } catch (error) {
            console.error('Ошибка загрузки:', error);
        }
    }

    // Настройка событий
    setupEvents() {
        // СОРТИРОВКА по клику на заголовок
        document.querySelectorAll('[data-content-sort]').forEach((item) => item.addEventListener('click', (e) => {
            if (item) {
                const currentField = this.selectedField;
                this.selectedField = item.getAttribute('data-content-sort');
                if (currentField !== this.selectedField) {
                    this.searchText = '';
                }
                this.setSortMode(item);
                this.loadData();
                this.showResetButton();
            }
        }));

        // ПАГИНАЦИЯ - через менеджер
        this.pagination.setPaginationEvents();

        // Поиск в модальном окне шапки таблицы
        this.setSearchFormsEvents();

        /**
         * открытие окна для Фильтрации
         */
        document.querySelector('.filtering-contract_btn').addEventListener('click', this.openFilterModalWindow.bind(this));

        // ============  Настройка ссылок с сохранением скролла ============
        this.scroll.setupScrollLinks(this.storageKey);

        // Автосохранение скролла при прокрутке
        this.scroll.enableAutoSave(100);
    }

    // Установка состояния кнопок сортировки
    setSortMode(item) {
        let isDescSort = item.classList.contains('sort-desc');
        let isAscSort = item.classList.contains('sort-asc');
        let isNotSort = !item.classList.contains('sort-asc') && !item.classList.contains('sort-desc');
        const listSortBtn = document.querySelectorAll('.sort-btn.sort-asc, .sort-btn.sort-desc');

        if (!isNotSort) {
            if (isAscSort && !isDescSort) {
                this.sortDirection = 'desc';
                item.classList.remove('sort-asc', 'sort-desc');
                item.classList.add('sort-desc');
            }
            if (!isAscSort && isDescSort) {
                this.sortDirection = 'asc';
                item.classList.remove('sort-asc', 'sort-desc');
                this.selectedField = '';
            }
        }
        if (isNotSort) {
            this.sortDirection = 'asc';

            listSortBtn.forEach((elem) => {
                elem.classList.remove('sort-asc', 'sort-desc');
            });
            item.classList.toggle('sort-asc');
        }
    }

    // Сброс всего
    resetAll() {
        this.data = [];
        this.currentPage = 1;
        this.totalPage = 1;
        this.selectedField = '';
        this.sortDirection = 'desc';
        this.searchText = '';
        this.count = 0;

        //Скрытие кнопки сброса
        const resetBtn = document.querySelector('.reset-btn');
        resetBtn.setAttribute('style', 'display:none !important');

        //Сброс подсветки сортировки в таблице
        document.querySelectorAll('.sort-btn.sort-asc, .sort-btn.sort-desc').forEach((item) => {
            item.classList.remove('sort-asc', 'sort-desc');
        });

        this.scroll.reset();
        this.loadData();
        this.renderTable();
    }

    // Рендер таблицы
    renderTable() {
        // Обновляем тело таблицы
        this.renderBody();

        // Рендер пагинации через менеджер
        this.pagination.renderPagination();

        this.handleAlert();
    }

    // Рендер тела таблицы
    renderBody() {
        const tbody = this.container.querySelector('.table_tbody');
        if (this.data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="100%">Нет данных</td></tr>';
            return;
        }
        switch (this.tableType) {
            case 'organization':
                tbody.innerHTML = setOrganizationTableRow(this.data, this.permissions);
                break;
            case 'employees':
                tbody.innerHTML = setEmployeesTableRow(this.data, this.permissions);
                break;
            default:
                tbody.innerHTML = '<tr><td colspan="100%">Нет данных</td></tr>';
                break;
        }        
    }


    initGlobalFilterHandlers() {
        // Обработчик для Escape (глобальный)
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                const container = document.querySelector('.contract-filter-container');
                if (container && container.style.display === 'flex') {
                    this.hideFilter(container);
                }
            }
        });
    }

    // Инициализация стилей контейнера
    initFilterContainer(container) {
        if (container.hasAttribute('data-initialized')) return;

        container.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 2000;
        opacity: 0;
        transition: opacity 0.2s ease;
    `;
        container.setAttribute('data-initialized', 'true');

        // Обработчик Escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && container.style.display === 'flex') {
                this.hideFilter(container);
            }
        });
    }

    // Показать фильтр с анимацией
    showFilter(container) {
        return new Promise(resolve => {
            container.style.display = 'flex';
            requestAnimationFrame(() => {
                container.style.opacity = '1';
                document.body.style.overflow = 'hidden';
                resolve();
            });
        });
    }

    // Скрыть фильтр с анимацией
    hideFilter(container) {
        return new Promise(resolve => {
            container.style.opacity = '0';
            setTimeout(() => {
                container.style.display = 'none';
                container.innerHTML = '';
                document.body.style.overflow = '';
                resolve();
            }, 200);
        });
    }

    // Обработчик клика вне карточки
    setupOutsideClickHandler(container) {
        const handleClick = (e) => {
            if (e.target === container) {
                this.hideFilter(container);
                container.removeEventListener('click', handleClick);
            }
        };

        // Используем setTimeout для предотвращения немедленного срабатывания
        setTimeout(() => {
            container.addEventListener('click', handleClick);
        }, 100);
    }

    // Инициализация управления фильтром
    async initFilterControls() {
        
        const searchKeyword = document.getElementById('searchKeyword');

        // Обработчик Enter в поле поиска
        searchKeyword?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                this.applyFilter(searchKeyword);
            }
        });

        // Переключатели режима
        document.getElementById('sortDesc').addEventListener('click', () => {
            //setSort('desc');
            if (this.sortDirection === 'desc') return;
            this.sortDirection = 'desc';

            // Обновляем активные кнопки
            const singleBtn = document.getElementById('sortDesc');
            const rangeBtn = document.getElementById('sortAsc');

            singleBtn.classList.add('active');
            rangeBtn.classList.remove('active');
        });

        document.getElementById('sortAsc').addEventListener('click', () => {
            //setSort('asc');
            if (this.sortDirection === 'asc') return;
            this.sortDirection = 'asc';
            // Обновляем активные кнопки
            const singleBtn = document.getElementById('sortDesc');
            const rangeBtn = document.getElementById('sortAsc');

            singleBtn.classList.remove('active');
            rangeBtn.classList.add('active');
        });


        // Кнопка применения
        document.getElementById('applyButton')?.addEventListener('click', () => {
            this.applyFilter( searchKeyword);
        });

        // Кнопка сброса
        document.getElementById('resetButton')?.addEventListener('click', () => {
            document.getElementById('filterField').value = '';
            searchKeyword.value = '';
            this.sortDirection = 'desc';
        });

        // Кнопка закрытия
        document.getElementById('closeButton')?.addEventListener('click', () => {
            const container = document.querySelector('.contract-filter-container');
            this.hideFilter(container);
        });
    }

    // Применить фильтр и закрыть окно
    async applyFilter(searchKeyword) {
        const paramsFilter = this.performSearch( searchKeyword);

        // Сохранение параметров фильтра
        this.saveFilterParams(paramsFilter);

        // Загрузка данных с новыми фильтрами
        await this.loadData();

        // Сохраняем состояние после применения фильтров ============
        this.saveState();

        // Закрытие окна фильтра
        const container = document.querySelector('.contract-filter-container');
        this.showResetButton();
        await this.hideFilter(container);
    }

    // Выполнить поиск
    performSearch(searchKeyword) {
        const filterField = document.getElementById('filterField')?.value || '';

        return {
            field: filterField,
            keyword: searchKeyword?.value.trim() || '',           
        };
    }

    // Сохранение параметров фильтра
    saveFilterParams(paramsFilter) {

        this.selectedField = paramsFilter.field;
        this.searchText = paramsFilter.keyword;
    }


    //**** ПОИСКОВИК по столбцам ===============
    openSearchForm(button) {
        const formWrapper = this.container.querySelector('.search-form-wrapper');
        if (!formWrapper) return;

        this.positionForm(button, formWrapper);
        const filed = button.getAttribute('data-content-search');
        const input = formWrapper.querySelector('input[type="text"]');

        if (this.selectedField !== filed) {
            this.selectedField = filed;
            if (input) input.value = '';
            document.querySelector('#clearButton.clear-btn').style.display = 'none';
        }

        formWrapper.style.display = 'block';
        formWrapper.style.opacity = '0';
        formWrapper.style.transform = 'translateY(-40px)';

        setTimeout(() => {
            formWrapper.style.opacity = '1';
            formWrapper.style.transform = 'translateY(0)';
            formWrapper.style.transition = 'all 0.3s ease';

            if (input) input.focus();
        }, 10);
    }

    closeSearchForm(formWrapper) {
        formWrapper.style.opacity = '0';
        formWrapper.style.transform = 'translateY(-40px)';

        setTimeout(() => {
            formWrapper.style.display = 'none';
        }, 300);
    }

    closeAllSearchForms() {
        const allForms = document.querySelectorAll('.search-form-wrapper');
        allForms.forEach(form => {
            if (form.style.display === 'block') {
                this.closeSearchForm(form);
            }
        });
    }

    positionForm(button, formWrapper) {
        const rect = button.getBoundingClientRect();
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

        //formWrapper.style.position = 'absolute';
        formWrapper.style.top = (rect.bottom + scrollTop + 25) + 'px';
        formWrapper.style.left = (rect.left + scrollLeft - 120) + 'px';
        formWrapper.style.zIndex = '2000';
        formWrapper.style.minWidth = '280px';

        if (window.innerWidth < 768) {
            formWrapper.style.width = 'calc(100vw - 40px)';
            formWrapper.style.left = '20px';
            formWrapper.style.right = '20px';
        }
    }

    setSearchFormsEvents() {
        /** Открытие модального окна ПОИСКА */
        document.querySelectorAll('[data-content-search]').forEach(button => {
            button.addEventListener('click', () => this.handleSearchButtonClick(button));
        });

        /**
         * поиск в модальном окне!
         * оброботкичи событий кнопки поиска и кнопки очистки поля ввода
         */
        const searchInput = document.getElementById('searchKeyword');
        const clearButton = document.getElementById('clearButton');

        // Обновить видимость кнопки
        function updateClearButton() {
            clearButton.style.display = searchInput.value.trim() ? 'block' : 'none';
        }

        // Очистить поле
        function clearInput() {
            searchInput.value = '';
            searchInput.focus();
            updateClearButton();
        }

        // События
        searchInput.addEventListener('input', updateClearButton);
        clearButton.addEventListener('click', clearInput);

        // Инициализация
        updateClearButton();

        /**
         * обработка событий при срабатывании которых закрываем Окно поиска
         */
        document.addEventListener('click', this.handleOutsideClick.bind(this));
        document.addEventListener('keydown', this.handleEscapeKey.bind(this));
        document.addEventListener('keydown', this.handleEnterKey.bind(this));

        /**
         * запрос загрузки данных по поисковому окну
         */
        this.container.querySelector('button#search-button').addEventListener('click', this.handleSearchBtn.bind(this));
    }
    //******=====================================


    // Кнопка RESET ==================
    showResetButton() {
        const resetBtn = document.querySelector('.reset-btn');
        if (resetBtn.style.display === 'none !important') {
            return
        }
        resetBtn.addEventListener('click', () => {
            this.resetAll();
        });
        resetBtn.style.display = 'flex';
    }

    // Обработчики событий
    handleSearchButtonClick(button) {
        this.updateFormPositions(button);
        this.openSearchForm(button);
    }

    handleOutsideClick(event) {
        if (!event || !event.target) return;

        const clickedButton = event.target.closest('[data-content-search]');
        const clickedForm = event.target.closest('.search-form-wrapper');

        // Если клик был НЕ по кнопке и НЕ по форме
        if (!clickedButton && !clickedForm) this.closeAllSearchForms();
    }

    handleEscapeKey(event) {
        if (event.key === 'Escape') {
            this.closeAllSearchForms();
        }
    }

    //поиск по нажатию на клавишу Enter
    handleEnterKey(event) {
        if (event.key === 'Enter' || event.keyCode === 13) {
            event.preventDefault();
            const input = this.container.querySelector('input[type="text"].input-sarch-filter');
            if (input) {
                this.currentPage = 1;
                this.searchText = input.value;
                this.showResetButton();
                this.closeAllSearchForms();
                this.loadData();
            }
        }
    }

    //поиск по нажатию на кнопку
    handleSearchBtn() {
        const input = this.container.querySelector('input[type="text"].input-sarch-filter');
        if (input) {
            this.currentPage = 1;
            this.searchText = input.value;
            this.closeAllSearchForms();
            this.showResetButton();
            this.loadData();
        }
    }

    updateFormPositions(button) {
        const openForms = document.querySelectorAll('.search-form-wrapper[style*="display: none"]');

        openForms.forEach(formWrapper => {
            if (button) {
                this.positionForm(button, formWrapper);
            }
        });
    }

    handleAlert() {
        let currentUrl = '';

        document.querySelectorAll('.modal-link').forEach(link => {
            link.addEventListener('click', function (e) {
                e.preventDefault();

                currentUrl = this.href;
                document.getElementById('alert-modal_title').textContent = this.getAttribute('data-title') || 'Предупреждение';
                document.getElementById('alert-modal_message').textContent = this.getAttribute('data-message') || 'Вы уверены?';

                document.getElementById('alert-modal').style.display = 'block';
            });
        });

        document.getElementById('alert-modal_confirm').addEventListener('click', function () {
            window.location.href = currentUrl;
        });

        document.getElementById('alert-modal_cancel').addEventListener('click', function () {
            document.getElementById('alert-modal').style.display = 'none';
        });
    }
}