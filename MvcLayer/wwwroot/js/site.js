// получаем координаты элемента в контексте документа
function getCoords(elem) {
    let box = elem.getBoundingClientRect();

    return {
        top: box.top + window.pageYOffset,
        right: box.right + window.pageXOffset,
        bottom: box.bottom + window.pageYOffset,
        left: box.left + window.pageXOffset
    };
}
function showResultMessage(objSelector, htmlMessage) {
    $(objSelector).html('');
    $(objSelector).html(htmlMessage);
    $(objSelector).show(200);

    setTimeout(() => {
        $(objSelector).hide(400);
        //$(objSelector).html('');
    }, 2000);

}
function changeClassByClickButton(buttonClickObj, obj, removeClass, addClass) {
    $(buttonClickObj).click(() => {
        let elem = document.querySelector(obj);
        if (elem) {
            elem.classList.remove(removeClass);
            elem.classList.add(addClass);
        }

    });
}

//переход к следующему этапу заполнения смет
function GoNextStep() {
    let points = document.querySelectorAll("div.line-item");
    if (points) {
        points.forEach((e) => {
            if (!e.classList.contains("done") && !e.classList.contains("unactive-step")) {
                e.classList.add("done");
            }
        });
    }
    document.querySelector(".line-item.unactive-step").classList.remove("unactive-step");
}

//Разбиение строки по разрядам числа
//todo: пересмотреть код на необходимость использования - JS
function digits_float(target) {
    val = $(target).val().replace(/[^0-9,-]/g, '');
    val = val.replace(/\s/g, "");
    val = val.replace(/(?!^)-/g, '');
    if (val.indexOf(",") != '-1') {
        first = val.substring(0, val.indexOf(",") + 1);
        
        let checkds = +(val.substring(val.indexOf(",") + 1, val.indexOf(",") + 4));
        if (checkds.toString().length > 2) {
            second = (+('0.' + checkds)).toFixed(2).toString();
            second = second.substring(second.indexOf(".") + 1, second.indexOf(".") + 4);
        } else {
            second = val.substring(val.indexOf(",") + 1, val.indexOf(",") + 3);
        }
        second = second.replace(/[^0-9]/g, '');
        val = first + second;
    }
    val = val.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
    $(target).val(val);
}

/**
* закрываем окно с оповещением через промежуток времени
* @param {number} time - промежуток времени, после которого закрываем окно (1000 - 1 секунда)
*/
function closeNotificationModal(time) {
    // Handle notification close button
    $('.notification .close').on('click', function () {
        $(this).closest('.notification').fadeOut('slow', function () {
            $(this).remove();
        });
    });

    // Optional: Auto-hide notifications after 5 seconds
    setTimeout(function () {
        $('.notification').fadeOut('slow', function () {
            $(this).remove();
        });

    }, time);
}

/**
* устанавливаем/записываем в окно новое оповещением и через промежуток времени удаляем
* @param {string} message - Текст оповещения
* @param {string} type - Тип оповещения (Success, Error, Warning, Info (по умолчанию))
* @param {number} time - промежуток времени, после которого закрываем окно (1000 - 1 секунда)
*/
function setNotification(message, type = 'Info', time) {
    const notification = $(`
        <div class="notification ${type}">
            ${message}
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
    `);

    // Добавляем в контейнер
    $('.notification-container').append(notification);

    // Анимация появления
    notification.hide().fadeIn(300);

    closeNotificationModal(time);
}

/**
* Копирует текст в буфер обмена
* @param {string} link - Текст для копирования
* @param {function} [onSuccess] - Коллбек при успехе
* @param {function} [onError] - Коллбек при ошибке
    ПРИМЕР
    <div class="icon content-copy" title=" *** " >
        <a href="#"
            data-link=" **ссылка/текст** "
            class="copy-link-btn">
        </a>
    </div>
*/
function copyLinkToClipboard(link, onSuccess, onError) {
    // Проверяем наличие ссылки
    if (!link || typeof link !== 'string') {
        if (onError) onError('Некорректная ссылка');
        return;
    }

    // Современный метод (HTTPS или localhost)
    if (navigator.clipboard && window.isSecureContext) {
        navigator.clipboard.writeText(link)
            .then(() => {
                if (onSuccess) onSuccess(link);
            })
            .catch(err => {
                console.warn('Clipboard API failed, using fallback:', err);
                useExecCommandFallback(link, onSuccess, onError);
            });
    } else {
        // Старый метод
        useExecCommandFallback(link, onSuccess, onError);
    }

    // Старый метод для копирования в буфер данных
    function useExecCommandFallback(text, successCallback, errorCallback) {
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.opacity = '0';
        textArea.style.pointerEvents = 'none';
        textArea.style.left = '-9999px';

        document.body.appendChild(textArea);
        textArea.select();
        textArea.setSelectionRange(0, 99999); // Для мобильных

        try {
            const successful = document.execCommand('copy');
            document.body.removeChild(textArea);

            if (successful) {
                if (successCallback) successCallback(text);
            } else {
                if (errorCallback) errorCallback('Не удалось скопировать');
            }
        } catch (err) {
            document.body.removeChild(textArea);
            if (errorCallback) errorCallback('Ошибка: ' + err.message);
        }
    }
}

/**
* Суммирование всех стоимостей из input's в total summ
* @param {string} targetClass - Название класса (селектор) в инпут которого вставляется сумма
* @param {function} inputClasses - Перечисление названий классов (селекторов) из инпута которых будет суммироваться стоимостей,
                                   либо объектов со свойствами -> название класса и оператор (указатель матем.операции (сложение или вычитание)

                *** ПРИМЕР ***
sumInputs('total', 
    'income',                        // доходы (положительные)
    { class: 'expense', sign: -1 },  // расходы (отрицательные)
    { class: 'bonus', sign: 1 },     // бонусы (положительные)
);
*/
const sumInputs = (targetClass, ...inputClasses) => {
    const target = document.querySelector(`.${targetClass}`);
    if (!target) return;
    const inputs = [];


    inputClasses.forEach(param => {
        if (typeof param === 'string') {
            // Простой класс - добавляем как положительное значение
            document.querySelectorAll(`.${param}`).forEach(input => {
                inputs.push({ element: input, sign: 1, class: param });
            });
        } else if (typeof param === 'object' && param.class) {
            // Объект с настройками { class: 'discount', sign: -1 }
            document.querySelectorAll(`.${param.class}`).forEach(input => {
                inputs.push({
                    element: input,
                    sign: param.sign || 1,
                    class: param.class,
                    label: param.label
                });
            });
        }
    });

    if (inputs.length === 0) return;

    const formatter = new Intl.NumberFormat('ru-RU', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    const parseNumber = (value) => {
        if (!value && value !== '0') return 0;

        // Используем Intl для парсинга
        const thousandSeparator = formatter.format(1111).replace(/1/g, '')[0];
        const decimalSeparator = formatter.format(1.1).replace(/1/g, '')[0];

        let cleanValue = value.toString()
            .replace(new RegExp('\\' + thousandSeparator, 'g'), '')
            .replace(decimalSeparator, '.');

        const num = parseFloat(cleanValue);
        return isNaN(num) ? 0 : num;
    };

    const calculate = () => {
        const sum = inputs.reduce((acc, { element, sign }) => {
            const value = parseNumber(element.value);
            return acc + (value * sign);
        }, 0);

        target.value = formatter.format(sum);
    };

    inputs.forEach(({ element }) => {
        element.addEventListener('input', calculate);

        // Добавляем форматирование при потере фокуса
        element.addEventListener('blur', () => {
            const num = parseNumber(element.value);
            if (!isNaN(num) && element.value) {
                element.value = formatter.format(num);
            }
        });
    });

    calculate();

    return calculate;
};




/*

    **************Выполнение скриптов**********!

*/

$(document).ready(function () {
    $('.js-chosen').selectize();

    //удаление оповещения
    closeNotificationModal(8000);

    //******  копирование в буфер, добавление обработчиков клика!!
    document.querySelectorAll('.copy-link-btn').forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const link = this.getAttribute('data-link') || window.location.href;
            copyLinkToClipboard(link,
                () => setNotification('Ссылка скопирована!', 'Success', 3000),
                (err) => setNotification(err, 'Error', 4000)
            );
        });
    });

});

//чтобы не было возвожности ввести дату с клавиатуры!
$('[type="date"]').attr('inputmode', 'none');
$('[type="date"]').on('keydown', (e) => {
    e.preventDefault();
    return false;
});


window.onclick = function (event) {
    if (!event.target.matches('.dropbtn')) {
        var dropdowns = document.getElementsByClassName("dropdown-content");
        var i;
        for (i = 0; i < dropdowns.length; i++) {
            var openDropdown = dropdowns[i];
            if (openDropdown.classList.contains('show')) {
                openDropdown.classList.remove('show');
            }
        }
    }
}

//todo: проверить надо оно еще или нет - JS!
let arrayTextEnd = document.getElementsByClassName('text-end');

for (let value of arrayTextEnd) {
    let val = Number(value.textContent.replace(/\s/g, '').replace(/,/g, '.'));
    if (val != NaN && val < 0) {
        value.setAttribute('style', 'color:red');
    }
}


//выполняем суммирование/вычитание из input's с стоимостями в input итоговый
sumInputs('smr-summary', 'smr-cost');
sumInputs('pnr-summary', 'pnr-cost');
sumInputs('equipment-summary', 'equipment-cost');
sumInputs('total-sum',
    { class: 'smr-cost', sign: 1 },                 // суммируем СМР
    { class: 'pnr-cost', sign: 1 },                 // суммируем ПНР
    { class: 'equipment-cost', sign: 1 },           // суммируем стоимость оборудования
    { class: 'other-cost', sign: -1 },              // вычитаем прочие затраты

    { class: 'current-prepayment-cost', sign: -1 }, // вычитаем Зачет текущего аванса
    { class: 'target-prepayment-cost', sign: -1 },  // вычитаем Зачет целевого аванса
    { class: 'gen-material-cost', sign: -1 },       // вычитаем Стоимость материалов генподрядчика
    { class: 'gen-service-cost', sign: -1 },        // вычитаем Стоимость услуг генподрядчика
    { class: 'reserve-cost', sign: -1 },            // вычитаем зарезервированные средства
); 
