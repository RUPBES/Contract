// ==================== КЛАСС ПАГИНАЦИИ ====================
class PaginationManager {
    constructor(controller) {
        this.controller = controller;
        this.visiblePagBtnCount = controller.visiblePagBtnCount || 2;
        this.ellipsisPage = controller.ellipsisPage || 4;
        this.container = controller.container;
    }

    // Рендер пагинации
    renderPagination() {
        const pagPanel = this.container.querySelector('ul.panel-pagination');
        const prevBtn = this.container.querySelector('.prev_btn');
        const nextBtn = this.container.querySelector('.next_btn');
        const pageInfo = this.container.querySelector('.page-info');
        const firstLi = pagPanel.querySelector('li.pagination-items_block');
        const paginationItems = this.createPaginationItems(this.controller.totalPage);

        firstLi.replaceChildren();
        paginationItems.forEach(item => {
            firstLi.appendChild(item);
        });

        prevBtn.disabled = this.controller.currentPage === 1;
        nextBtn.disabled = this.controller.currentPage === this.controller.totalPage;
        pageInfo.textContent = (this.controller.itemsPerPage >= this.controller.count) ?
            `1 - ${this.controller.count} из ${this.controller.count} строк` :
            `${this.controller.itemsPerPage * (this.controller.currentPage - 1) + 1} - ${(this.controller.itemsPerPage * this.controller.currentPage) > this.controller.count ? this.controller.count : (this.controller.itemsPerPage * this.controller.currentPage)} из ${this.controller.count} строк`;

        this.updatePaginationButtons();
    }

    // Обновление видимости кнопок пагинации
    updatePaginationButtons() {
        if (this.controller.totalPage >= 6) {
            let leftVisible = (this.controller.currentPage - this.visiblePagBtnCount) < 1 ? 1 : (this.controller.currentPage - this.visiblePagBtnCount);
            let rigthVisible = (this.controller.currentPage + this.visiblePagBtnCount) > this.controller.totalPage ? this.controller.totalPage : (this.controller.currentPage + this.visiblePagBtnCount);

            const divs = this.container.querySelectorAll('div[data-value]:not(.start-page, .end-page)');
            const filtered = Array.from(divs).filter(div => {
                const value = +(div.dataset.value);
                return value < leftVisible || value > rigthVisible;
            });

            if (filtered && filtered.length > 0) {
                filtered.forEach(elem => {
                    if (elem) {
                        elem.hidden = true;
                    }
                });
            }

            if ((leftVisible - this.visiblePagBtnCount) >= 1) {
                const firstElement = this.container.querySelector('.pagination-items_block__item.start-page');
                if (firstElement) {
                    firstElement.insertAdjacentElement('afterend', this.createPaginationItem('', true, 'item-ellipsis__prev'));
                }
            }

            if ((this.controller.totalPage - rigthVisible) >= this.visiblePagBtnCount) {
                const lastElement = this.container.querySelector('.pagination-items_block__item.end-page');
                if (lastElement) {
                    lastElement.insertAdjacentElement('beforebegin', this.createPaginationItem('', true));
                }
            }
        }
    }

    // Установка обработчиков событий пагинации
    setPaginationEvents() {
        const prevBtn = this.container.querySelector('.prev_btn');
        const nextBtn = this.container.querySelector('.next_btn');

        prevBtn.addEventListener('click', () => {
            if (this.controller.currentPage > 1) {
                this.controller.currentPage--;
                this.controller.resetScrollToUp();
                this.controller.loadData();
                this.controller.renderTable();
            }
        });

        nextBtn.addEventListener('click', () => {
            const totalPages = Math.ceil(this.controller.count / this.controller.itemsPerPage);
            if (this.controller.currentPage < totalPages) {
                this.controller.currentPage++;
                this.controller.resetScrollToUp();
                this.controller.loadData();
                this.controller.renderTable();
            }
        });
    }

    // Создание элементов пагинации
    createPaginationItems(count) {
        const items = [];
        for (let i = 1; i <= count; i++) {
            const pageNumber = i;
            const item = this.createPaginationItem(pageNumber);
            items.push(item);
        }
        return items;
    }

    // Создание отдельного элемента пагинации
    createPaginationItem(pageNumber, isNextBtn = false, classEllipsis = 'item-ellipsis__next') {
        const div = document.createElement('div');

        if (isNextBtn) {
            div.classList.add(`pagination-items_block__item`);
            div.classList.add(classEllipsis);

            if (classEllipsis !== 'item-ellipsis__next') {
                div.setAttribute('title', `Вперед на ${this.ellipsisPage} страницы`);
            }
            if (classEllipsis === 'item-ellipsis__next') {
                div.setAttribute('title', `Назад на ${this.ellipsisPage} страницы`);
            }

            div.addEventListener('click', (e) => {
                if (classEllipsis === 'item-ellipsis__next') {
                    this.controller.currentPage = (this.controller.currentPage + this.visiblePagBtnCount) > this.controller.totalPage ?
                        this.controller.totalPage :
                        (this.controller.currentPage + this.ellipsisPage);
                }

                if (classEllipsis !== 'item-ellipsis__next') {
                    this.controller.currentPage = (this.controller.currentPage - this.visiblePagBtnCount) < 1 ?
                        1 :
                        (this.controller.currentPage - this.ellipsisPage);
                }

                this.controller.loadData();
            });
        }

        if (!isNextBtn) {
            div.setAttribute('title', pageNumber.toString());
            div.classList.add('pagination-items_block__item');
            div.setAttribute('data-value', pageNumber);

            if (+pageNumber === 1) {
                div.classList.add('start-page');
            }
            if (this.controller.totalPage === +pageNumber) {
                div.classList.add('end-page');
            }
            if (this.controller.currentPage === +pageNumber) {
                div.classList.add('active-btn-pag');
            }

            div.addEventListener('click', (e) => {
                this.controller.currentPage = pageNumber;
                this.controller.resetScrollToUp();
                this.controller.loadData();
            });
        }

        div.textContent = pageNumber;
        return div;
    }

    // Переход на конкретную страницу
    goToPage(page) {
        if (page < 1 || page > this.controller.totalPage) return;
        this.controller.currentPage = page;
        this.controller.resetScrollToUp();
        this.controller.loadData();
    }

    // Переход на следующую страницу
    nextPage() {
        if (this.controller.currentPage < this.controller.totalPage) {
            this.controller.currentPage++;
            this.controller.resetScrollToUp();
            this.controller.loadData();
        }
    }

    // Переход на предыдущую страницу
    prevPage() {
        if (this.controller.currentPage > 1) {
            this.controller.currentPage--;
            this.controller.resetScrollToUp();
            this.controller.loadData();
        }
    }

    // Переход на первую страницу
    firstPage() {
        if (this.controller.currentPage !== 1) {
            this.controller.currentPage = 1;
            this.controller.resetScrollToUp();
            this.controller.loadData();
        }
    }

    // Переход на последнюю страницу
    lastPage() {
        if (this.controller.currentPage !== this.controller.totalPage) {
            this.controller.currentPage = this.controller.totalPage;
            this.controller.resetScrollToUp();
            this.controller.loadData();
        }
    }

    // Сброс пагинации на первую страницу
    reset() {
        this.controller.currentPage = 1;
    }

    // Обновление состояния пагинации
    updateState(totalPages, totalItems) {
        this.controller.totalPage = totalPages;
        this.controller.count = totalItems;
    }
}
