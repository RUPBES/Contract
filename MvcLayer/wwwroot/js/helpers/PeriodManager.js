class PeriodManager {
    constructor() {
        this.rangeStartSW = null;
        this.rangeEndSW = null;

        this.rangeStartEW = null;
        this.rangeEndEW = null;

        this.rangeStartET = null;
        this.rangeEndET = null;

        this.sortDir = 'desc';

        this.initWidgets();
        this.setupEventListeners();
    }

    initWidgets() {
        //// Виджет для одиночной даты
        //this.singleWidget = new MonthYearWidget(
        //    'singleMonthInput',
        //    'singleMonthPicker',
        //    (value) => {
        //        this.singleDate = value;
        //        this.updateSelectedValue();
        //    }
        //);
        //this.singleWidget.setCurrentMonth();

        // Виджет для начала периода START WORK
        this.rangeStartSW = new MonthYearWidget(
            'rangeFromInput',
            'rangeFromPicker',
            (value) => {
                this.rangeStartSW = value;
                if (this.rangeStartSW && this.rangeEndSW) {
                    this.validateRange();
                }
                //this.updateSelectedValue();
            }
        );

        // Виджет для конца периода START WORK
        this.rangeEndSW = new MonthYearWidget(
            'rangeToInput',
            'rangeToPicker',
            (value) => {
                this.rangeEndSW = value;
                if (this.rangeStartSW && this.rangeEndSW) {
                    this.validateRange();
                }
                //this.updateSelectedValue();
            }
        );



        // Виджет для начала периода END WORK
        this.rangeStartEW = new MonthYearWidget(
            'rangeEWFromInput',
            'rangeEWFromPicker',
            (value) => {
                this.rangeStartEW = value;
                if (this.rangeStartEW && this.rangeEndEW) {
                    this.validateRange();
                }
                //this.updateSelectedValue();
            }
        );

        // Виджет для конца периода END WORK
        this.rangeEndEW = new MonthYearWidget(
            'rangeEWToInput',
            'rangeEWToPicker',
            (value) => {
                this.rangeEndEW = value;
                if (this.rangeStartEW && this.rangeEndEW) {
                    this.validateRange();
                }
                //this.updateSelectedValue();
            }
        );



        // Виджет для начала периода ENTERING WORK
        this.rangeStartET = new MonthYearWidget(
            'rangeEDFromInput',
            'rangeEDFromPicker',
            (value) => {
                this.rangeStartET = value;
                if (this.rangeStartET && this.rangeEndET) {
                    this.validateRange();
                }
                //this.updateSelectedValue();
            }
        );

        // Виджет для конца периода ENTERING WORK
        this.rangeEndET = new MonthYearWidget(
            'rangeEDToInput',
            'rangeEDToPicker',
            (value) => {
                this.rangeEndET = value;
                if (this.rangeStartET && this.rangeEndET) {
                    this.validateRange();
                }
                //this.updateSelectedValue();
            }
        );


        //// Устанавливаем начальные значения для периода
        //const now = new Date();
        //const currentYear = now.getFullYear();
        //const currentMonth = now.getMonth() + 1;

        //this.rangeFromWidget.setValue(`${currentYear}-${String(currentMonth).padStart(2, '0')}`);
        //this.rangeToWidget.setValue(`${currentYear}-${String(currentMonth).padStart(2, '0')}`);
    }

    setupEventListeners() {
        // Переключатели режима
        document.getElementById('sortDesc')?.addEventListener('click', () => {
            this.setSort('desc');
        });

        document.getElementById('sortAsc')?.addEventListener('click', () => {
            this.setSort('asc');
        });
    }

    setSort(mode) {
        if (this.sortDir === mode) return;
        this.sortDir = mode;

        // Обновляем активные кнопки
        const singleBtn = document.getElementById('sortDesc');
        const rangeBtn = document.getElementById('sortAsc');
        singleBtn.classList.toggle('active', mode === 'desc');
        rangeBtn.classList.toggle('active', mode === 'asc');
    }

    validateRange() {
        if (!this.rangeStart || !this.rangeEnd) return;

        const [startYear, startMonth] = this.rangeStart.split('-').map(Number);
        const [endYear, endMonth] = this.rangeEnd.split('-').map(Number);

        const startDate = new Date(startYear, startMonth - 1);
        const endDate = new Date(endYear, endMonth - 1);

        // Если начало периода больше конца, меняем их местами
        if (startDate > endDate) {
            const temp = this.rangeStart;
            this.rangeStart = this.rangeEnd;
            this.rangeEnd = temp;

            this.rangeFromWidget.setValue(this.rangeStart);
            this.rangeToWidget.setValue(this.rangeEnd);
        }
    }

    getPeriod() {
        return {
            sort: this.sortDir,
            startSW: this.rangeStartSW,
            endSW: this.rangeEndSW,
            startEW: this.rangeStartEW,
            endEW: this.rangeEndEW,
            startET: this.rangeStartET,
            endET: this.rangeEndET
        };
    }

    reset() {
        document.querySelectorAll('input.md3-month-year-input').forEach((elemInput) => {
            elemInput.value = null;
            elemInput.removeAttribute('data-value');
        });

        document.querySelectorAll('.md3-month-year-picker.show').forEach((elemInput) => {
            elemInput.classList.remove('show');
        });

        this.rangeStartSW = null;
        this.rangeEndSW = null;

        this.rangeStartEW = null;
        this.rangeEndEW = null;

        this.rangeStartET = null;
        this.rangeEndET = null;
        this.sortDir = 'desc';
    }
}
