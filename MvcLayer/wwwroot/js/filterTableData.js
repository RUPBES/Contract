$(document).ready(function () {
    // Состояние фильтров
    let filterState = {};
    let activeColumn = null;
    let originalCheckboxes = new Set();

    // Получаем элементы DOM
    const modal = document.getElementById('filterModal');
    const filterCheckboxes = document.getElementById('filterCheckboxes');
    const filterSearch = document.getElementById('filterSearch');
    const closeBtn = document.querySelector('.close');
    const selectAllBtn = document.getElementById('selectAll');
    const clearAllBtn = document.getElementById('clearAll');
    const applyFilterBtn = document.getElementById('applyFilter');
    const cancelFilterBtn = document.getElementById('cancelFilter');

    // Инициализация состояния фильтров на основе data-filter атрибутов
    function initFilterState() {
        const headers = document.querySelectorAll('#dataTable th[data-filter]');
        headers.forEach(header => {
            const filterName = header.getAttribute('data-filter');
            filterState[filterName] = new Set();
        });
    }

    // Получение уникальных значений столбца
    function getUniqueValues(column) {
        const values = new Set();
        const tbody = document.querySelector('#dataTable tbody');
        const rows = tbody.querySelectorAll('tr:not(.summary-row):not(.summary-row-total):not([colspan])'); //:not([style*="display: none"])
        let countColumn = document.querySelectorAll('#dataTable > thead > tr > th').length;
        rows.forEach(row => {
            const headerIndex = Array.from(row.parentElement.querySelectorAll('tr')).indexOf(row);
            const cells = row.cells;
            let value;

            // Определяем значение в зависимости от типа столбца
            switch (column) {
                case 'bCode':
                    value = cells[0].textContent.trim();
                    if (row.cells[0].hasAttribute('rowspan')) {
                        values.add(value);
                    }
                    break;
                case 'bName':
                    let index = Math.abs(countColumn - cells.length - 1);
                    value = cells[index].textContent.trim();
                    if (row.cells[index] && row.cells[index].hasAttribute('rowspan')) {
                        values.add(value);
                    }
                    break;
                case 'drwKit':
                    let index2 = Math.abs(countColumn - cells.length - 2);
                    value = cells[index2] ? cells[index2].textContent.trim() : '';
                    if (value) values.add(value);
                    break;
                case 'drwName':
                    let index3 = Math.abs(countColumn - cells.length - 3);
                    value = cells[index3] ? cells[index3].textContent.trim() : '';
                    if (value) values.add(value);
                    break;
                // Добавьте другие столбцы по необходимости
            }
        });

        return [...values].filter(v => v).sort();
    }

    // Создание списка чекбоксов для фильтра
    function createFilterCheckboxes(column) {
        const values = getUniqueValues(column);
        filterCheckboxes.innerHTML = values.map(value => `
        <div class="checkbox-item">
            <input type="checkbox" id="filter-${value}" value="${value}" 
                   ${filterState[column].has(value) ? 'checked' : ''}>
            <label for="filter-${value}">${value}</label>
        </div>
    `).join('');

        originalCheckboxes = new Set(values);
    }

    // Проверка соответствия строки фильтрам
    function matchesFilters(row) {
        for (const [column, values] of Object.entries(filterState)) {
            if (values.size === 0) continue;

            let cellValue = '';
            let matches = false;

            // Получаем значение ячейки в зависимости от типа столбца
            switch (column) {
                case 'bCode':
                    cellValue = row.cells[0].textContent.trim();
                    if (!row.cells[0].hasAttribute('rowspan')) {
                        const prevRow = row.previousElementSibling;
                        if (prevRow) {
                            cellValue = prevRow.cells[0].textContent.trim();
                        }
                    }
                    break;
                case 'bName':
                    if (row.cells[1] && row.cells[1].hasAttribute('rowspan')) {
                        cellValue = row.cells[1].textContent.trim();
                    } else {
                        const prevRow = row.previousElementSibling;
                        if (prevRow) {
                            cellValue = prevRow.cells[1].textContent.trim();
                        }
                    }
                    break;
                default:
                    const headerIndex = Array.from(document.querySelectorAll('#dataTable th'))
                        .findIndex(th => th.getAttribute('data-filter') === column);
                    if (headerIndex !== -1 && row.cells[headerIndex]) {
                        cellValue = row.cells[headerIndex].textContent.trim();
                    }
            }

            if (!values.has(cellValue)) {
                return false;
            }
        }

        return true;
    }

    // Применение фильтров
    function applyFilters() {
        const tbody = document.querySelector('#dataTable tbody');
        const rows = tbody.querySelectorAll('tr');
        let currentGroupVisible = false;
        let lastGroupRow = null;

        rows.forEach((row, index) => {
            // Пропускаем строки с colspan (итоги)
            if (row.querySelector('td[colspan]')) {
                //row.style.display = '';
                return;
            }

            // Проверяем, является ли строка началом новой группы
            const isGroupStart = row.cells[0].hasAttribute('rowspan');

            if (isGroupStart) {
                currentGroupVisible = matchesFilters(row);
                lastGroupRow = row;
            }

            // Показываем/скрываем строку в зависимости от результатов фильтрации
            if (currentGroupVisible /*&& matchesFilters(row)*/) {
                row.style.display = '';
                if (lastGroupRow) {
                    lastGroupRow.style.display = '';

                    let nextElem = row.nextElementSibling;
                    while (nextElem.classList.contains("summary-row")) {
                        nextElem.style.display = '';
                        nextElem = nextElem.nextElementSibling;
                    }
                }
            } else {
                row.style.display = 'none';
                let nextElem = row.nextElementSibling;
                while (nextElem.classList.contains("summary-row")) {
                    nextElem.style.display = 'none';
                    nextElem = nextElem.nextElementSibling;
                }
            }
        });

        // Подсветка отфильтрованных столбцов
        //highlightFilteredColumns();
    }

    // Подсветка отфильтрованных столбцов
    //function highlightFilteredColumns() {
    //    const headers = document.querySelectorAll('#dataTable th[data-filter]');
    //    headers.forEach(header => {
    //        const filterName = header.getAttribute('data-filter');
    //        const columnIndex = Array.from(header.parentElement.children).indexOf(header);
    //        const cells = document.querySelectorAll(`#dataTable td:nth-child(${columnIndex + 1})`);

    //        if (filterState[filterName].size > 0) {
    //            header.classList.add('filtered-column');
    //            cells.forEach(cell => {
    //                if (cell.closest('tr').style.display !== 'none') {
    //                    cell.classList.add('filtered-column');
    //                }
    //            });
    //        } else {
    //            header.classList.remove('filtered-column');
    //            cells.forEach(cell => cell.classList.remove('filtered-column'));
    //        }
    //    });
    //}

    // Обработчики событий
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const header = e.target.closest('th');
            activeColumn = header.getAttribute('data-filter');
            createFilterCheckboxes(activeColumn);
            modal.style.display = 'block';
        });
    });

    closeBtn.onclick = () => modal.style.display = 'none';
    cancelFilterBtn.onclick = () => modal.style.display = 'none';

    window.onclick = (e) => {
        if (e.target === modal) {
            modal.style.display = 'none';
        }
    };

    // Поиск по значениям в фильтре
    filterSearch.addEventListener('input', (e) => {
        const searchText = e.target.value.toLowerCase();
        const checkboxes = filterCheckboxes.querySelectorAll('.checkbox-item');

        checkboxes.forEach(item => {
            const label = item.querySelector('label').textContent.toLowerCase();
            item.style.display = label.includes(searchText) ? '' : 'none';
        });
    });

    // Выбрать все/Очистить все
    selectAllBtn.onclick = () => {
        const checkboxes = filterCheckboxes.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(checkbox => checkbox.checked = true);
    };

    clearAllBtn.onclick = () => {
        const checkboxes = filterCheckboxes.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(checkbox => checkbox.checked = false);
    };

    // Применить фильтр
    applyFilterBtn.onclick = () => {
        const checkboxes = filterCheckboxes.querySelectorAll('input[type="checkbox"]:checked');
        filterState[activeColumn] = new Set([...checkboxes].map(cb => cb.value));
        applyFilters();
        modal.style.display = 'none';
    };

    // Инициализация при загрузке страницы
    initFilterState();
});