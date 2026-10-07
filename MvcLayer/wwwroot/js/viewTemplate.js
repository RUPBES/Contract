function setContractTableRow(contractItems, permissions, isEngineering) {
    const now = new Date();
    const isBes = permissions.company === 'ContrOrgBes';
    const hasEstimate = !isEngineering && permissions.groupeName.includes('GRP_Estimate');
    const hasContract = permissions.groupeName.includes('GRP_Contract');
    const hasReport = permissions.isReader && permissions.groupeName.includes('GRP_Report');
    const canEdit = permissions?.isEditor;
    const canDelete = permissions.isDeleter;
    const canArchive = canEdit && permissions?.isAdmin;
    const canTransfer = isBes && canEdit;


    return contractItems.map(item => {
        const isOverdue = (() => {
            if (!item.DateEndWork) return false;

            const [day, month, year] = item.DateEndWork.split('.').map(Number);
            const endDate = new Date(year, month - 1, day);

            return endDate < now;
        })();
        const isAuthorAllowed = item.Author === permissions.company || isBes;
        const avans = item.PaymentСonditionsAvans ?? '';

        const hasNone = avans.includes('Без авансов');
        const hasCurrent = avans.includes('С предоставлением текущего аванса');
        const hasTarget = avans.includes('С предоставлением целевого аванса');

        return `<tr>
        <td>
          <a class="contract-num" href="/Contracts/Details/${item.Id}">№ ${item.Number ?? ''}</a>
          <span class="contract-date">от ${item.Date ?? ''}</span>
          <span class="status-chip ${isOverdue ? 'status-overdue' : 'status-active'}"><span class="dot"></span>${isOverdue ? 'Просрочен' : 'Активен'}</span>
        </td>
        <td>
          <div class="tt-wrap">
            <div class="clamp-2">${item.NameObject ?? ''}</div>
            <div class="tt-popover">${item.NameObject ?? ''}</div>
          </div>
        </td>
        <td>${item.Client ?? ''}</td>
        ${!isEngineering ? `
        <td>
          <span class="gen-name">${item.GenContractor ?? ''}</span>
          ${item.ResponsibleForWork
                    ? `
          <span class="subline"><b>Отв. за произв. работ:</b><br>${item.ResponsibleForWork}</span>`
                    : ''}
        </td>` : ''}

        <td class="deadline-block">
        ${item.DateBeginWork || item.DateEndWork
                ? `
          <div class="deadline-label">выполнения работ</div>
          <div class="deadline-range">${item.DateBeginWork ?? ''} - ${item.DateEndWork ?? ''}</div>`
                : ''}
        ${item.EnteringTerm ? `  
          <div class="deadline-input">
            <div class="deadline-label">ввод</div>
            <div class="val">${item.EnteringTerm}</div>
          </div>`
                : ''}
        </td>
        <td>
          ${hasNone
                ? `<span class="pay-chip pc-none">Без авансов</span>`
                : ''
            }

        ${hasCurrent && hasTarget
                ? `<span class="pay-chip pc-current">
                        <a href="/Prepayments/GetByContractId?contractId=${item.Id}">
                            С текущим + целевым авансом
                        </a>
                    </span>`
                :
            (hasCurrent && !hasTarget) ? `<span class="pay-chip pc-target">
                        <a href="/Prepayments/GetByContractId?contractId=${item.Id}">
                            С текущим авансом
                        </a>
                    </span>`
                : (!hasCurrent && hasTarget) ? `<span class="pay-chip pc-target">
                        <a href="/Prepayments/GetByContractId?contractId=${item.Id}">
                           С целевым авансом
                        </a>
                    </span>`
                    : ''
            }
            
        
          
          <div class="tt-wrap">
            <div class="pay-terms">${item.PaymentСonditionsRaschet ?? ''}</div>
            <div class="tt-popover">${item.PaymentСonditionsRaschet ?? ''}</div>
          </div>
          <span class="work-type">${!isEngineering ? `${item.WorkType ?? ''}` : `${item.PaymentСonditionsPrice ?? ''}`}</span>
        </td>
        <td >
          <div class="fin-grid">
            <div class="fin-cell main" >
              <span class="lbl">Всего по договору с НДС</span>
              <span class="val">${item.ContractPrice ?? ''}<span class="fin-cur">${item.Сurrency ?? ''}</span></span>
            </div>
            <div class="fin-cell" ><span class="lbl">Выполнено на 01.01</span><span class="val">${item.PreYearSum ?? ''}</span></div>
            <div class="fin-cell" ><span class="lbl">Объём на год</span><span class="val">${item.RemainingSum ?? ''}</span></div>
            <div class="fin-cell" ><span class="lbl">Остаток</span><span class="val pos">${item.ThisYearSum ?? ''}</span></div>
          </div>
        </td>
        <td >
          <button class="action-btn"><svg class="ic ic-18"><use href="#ic-more-vert"/></svg></button>
            <div class="action-menu">
                <a class="menu-item save-page-state" href="/Contracts/Details/${item.Id}">
                    <span class="menu-icon"><svg class="ic ic-15"><use href="#ic-open"></use></svg></span>
                    <span class="menu-label">Открыть</span>
                </a>
                ${isAuthorAllowed ? buildAuthorMenu(item, hasEstimate, hasContract, hasReport, canEdit, canDelete, canArchive, canTransfer, isOverdue) : ''}
            </div>
        </td>
      </tr>`;
    }).join('');
}


function setContractArchiveTableRow(contractItems, permissions, isEngineering) {
    const isBes = permissions.company === 'ContrOrgBes';
    const hasEstimate = !isEngineering && permissions.groupeName.includes('GRP_Estimate');
    const hasContract = permissions.groupeName.includes('GRP_Contract');
    const hasReport = permissions.isReader && permissions.groupeName.includes('GRP_Report'); 

    return contractItems.map(contractItem => {       
        const isAuthorAllowed = contractItem.Author === permissions.company || isBes;
        const avans = contractItem.PaymentСonditionsAvans ?? '';
        const hasNone = avans.includes('Без авансов');
        const hasCurrent = avans.includes('С предоставлением текущего аванса');
        const hasTarget = avans.includes('С предоставлением целевого аванса');

        return `<tr>
        <td>
          <a class="contract-num" href="/archive/Contracts/Details/${contractItem.Id}">№ ${contractItem.Number ?? ''}</a>
          <span class="contract-date">от ${contractItem.Date ?? ''}</span>          
        </td>
        <td>
          <div class="tt-wrap">
            <div class="clamp-2">${contractItem.NameObject ?? ''}</div>
            <div class="tt-popover">${contractItem.NameObject ?? ''}</div>
          </div>
        </td>
        <td>${contractItem.Client ?? ''}</td>
        ${
            !isEngineering ? `
        <td>
          <span class="gen-name">${contractItem.GenContractor ?? ''}</span>
          ${contractItem.ResponsibleForWork
                    ? `
          <span class="subline"><b>Отв. за произв. работ:</b><br>${contractItem.ResponsibleForWork}</span>`
                    : ''}
        </td>` : ''
        }

        <td class="deadline-block">
        ${contractItem.DateBeginWork || contractItem.DateEndWork
                ? `
          <div class="deadline-label">выполнения работ</div>
          <div class="deadline-range">${contractItem.DateBeginWork ?? ''} - ${contractItem.DateEndWork ?? ''}</div>`
                : ''}
        ${contractItem.EnteringTerm ? `  
          <div class="deadline-input">
            <div class="deadline-label">ввод</div>
            <div class="val">${contractItem.EnteringTerm}</div>
          </div>`
                : ''}
        </td>
        <td>
          ${hasNone
                ? `<span class="pay-chip pc-none">Без авансов</span>`
                : ''
            }

        ${hasCurrent && hasTarget
                ? `<span class="pay-chip pc-current">
                        <a href="/Prepayments/GetArchByContractId?contractId=${contractItem.Id}">
                            С текущим + целевым авансом
                        </a>
                    </span>`
                :
            (hasCurrent && !hasTarget) ? `<span class="pay-chip pc-target">
                        <a href="/Prepayments/GetArchByContractId?contractId=${contractItem.Id}">
                            С текущим авансом
                        </a>
                    </span>`
                : (!hasCurrent && hasTarget) ? `<span class="pay-chip pc-target">
                        <a href="/Prepayments/GetArchByContractId?contractId=${contractItem.Id}">
                           С целевым авансом
                        </a>
                    </span>`
                    : ''
            }
            
        
          
          <div class="tt-wrap">
            <div class="pay-terms">${contractItem.PaymentСonditionsRaschet ?? ''}</div>
            <div class="tt-popover">${contractItem.PaymentСonditionsRaschet ?? ''}</div>
          </div>
          <span class="work-type">${!isEngineering ? `${contractItem.WorkType ?? ''}` : `${contractItem.PaymentСonditionsPrice ?? ''}`}</span>
        </td>
        <td >
          <div class="fin-grid">
            <div class="fin-cell main" >
              <span class="lbl">Всего по договору с НДС</span>
              <span class="val">${contractItem.ContractPrice ?? ''}<span class="fin-cur">${contractItem.Сurrency ?? ''}</span></span>
            </div>
            <div class="fin-cell" ><span class="lbl">Выполнено на 01.01</span><span class="val">${contractItem.PreYearSum ?? ''}</span></div>
            <div class="fin-cell" ><span class="lbl">Объём на год</span><span class="val">${contractItem.RemainingSum ?? ''}</span></div>
            <div class="fin-cell" ><span class="lbl">Остаток</span><span class="val pos">${contractItem.ThisYearSum ?? ''}</span></div>
          </div>
        </td>
        <td >
          <button class="action-btn"><svg class="ic ic-18"><use href="#ic-more-vert"/></svg></button>
            <div class="action-menu">
                <a class="menu-item save-page-state" href="/archive/Contracts/Details/${contractItem.Id}">
                    <span class="menu-icon"><svg class="ic ic-15"><use href="#ic-open"></use></svg></span>
                    <span class="menu-label">Открыть</span>
                </a>
                ${isAuthorAllowed ? buildAuthorMenu(contractItem, hasEstimate, hasContract, false, false, false, false, false, false) : ''}
            </div>
        </td>
      </tr > `
    }
    ).join('');
}

function setPayableCashTableRow(contractItems) {
    return contractItems.map(contractItem => `
   <tr>
                        <td>
                             <a href="/Payments/DetailsPayableCash?contractId=${contractItem.Id}" class="save-page-state"> ${contractItem.Number ?? ``} <br> от<br> ${contractItem.Date ?? ``} </a>                           
                        </td>
                        <td>${contractItem.NameObject ?? ``}</td>
                        <td>${contractItem.Client ?? ``}</td>
                        <td>${contractItem.GenContractor ?? ``}</td>
                        <td>
                            ${contractItem.DateBeginWork || contractItem.DateEndWork ? `<b>выполнения работ:</b><br><span>${contractItem.DateBeginWork ?? ``} - ${contractItem?.DateEndWork ?? ``}</span>` : ``} 
                            ${contractItem.EnteringTerm ? `<br><b>ввода:</b><br><span>${contractItem?.EnteringTerm ?? ``}</span>` : ``}                                    
                            
                        </td>
                        <td>${contractItem.Сurrency ?? ``}</td>
                        <td class="text-end">${contractItem.ContractPrice ?? ``}</td>
                        <td class="text-end">${calculateDifference(contractItem)
        }</td>
                        <td class="text-end">${contractItem.ThisYearSum ?? ``}</td>
                        <td class="text-end">${contractItem.FactSum ?? ``}</td>
                        <td class="text-end">${contractItem.ReserveSum ?? ``}</td>
                    </tr>`
    ).join('');
}

function setOrganizationTableRow(organizations, permissions) {
    return organizations.map(organization => `
    <tr>
        <td><span class="column_400"> ${organization.Name ?? ``}</span></td>
        
        <td><span class="column_250">
                <span class="p-1 k-mr-5 text-break">${organization.FullAddress ?? ``}</span>
             </span>
        </td>
        <td><span class="column_140">
                <span class="p-1 k-mr-5 text-break">${organization.PhoneNumbers ?? ``}</span>
            </span>
        </td>
        <td>          
                ${permissions.isReader ? `<div class="icon info" title="Детальная информация"><a href="/Organizations/Details/${organization.Id}"></a></div>` : ``}                
                 ${permissions.isEditor ? `<div class="icon edit" title="Редактировать"><a href="/Organizations/Edit/${organization.Id}"></a></div>` : ``} 
                  
                 ${permissions.isDeleter || permissions.isAdmin ? `<div class="icon delete" title="Удалить">
                 <a href="/Organizations/Delete/${organization.Id}" 
                        class="modal-link"
                        data-message="Организация будет удалена. Продолжить?">
                 </a></div>`
            : ``}          
        </td>
    </tr>`
    ).join('');
}


function setEmployeesTableRow(employees, permissions) {
    return employees.map(employee => `
    <tr>
        <td><span class="column_300">${employee.FullName ?? ``}</span></td>
        <td><span class="column_250">${employee.Email ?? ``}</span></td>
        <td><span class="column_130">${employee.PhoneNumbers ?? ``}</span></td>
        <td>
            <div class="d-flex flex-row">
            ${permissions.isReader ? `<div class="icon info" title="Детальная информация"><a href="/Employees/Details/${employee.Id}"></a></div>` : ``}
            ${permissions.isEditor ? `<div class="icon edit" title="Редактировать"><a href="/Employees/Edit/${employee.Id}"></a></div>` : ``}     
             ${permissions.isDeleter || permissions.isAdmin ? `<div class="icon delete" title="Удалить">
                 <a href="/Employees/Delete/${employee.Id}"
                        class="modal-link"
                        data-message="Сотрудник будет удален. Продолжить?">
                 </a></div>`
            : ``}
            </div>
        </td>
    </tr>`
    ).join('');
}

function calculateDifference(contractItem) {
    const price = parseCurrency(contractItem?.ContractPrice);
    const fact = parseCurrency(contractItem?.FactSum);
    const diff = price - fact;

    // Форматируем результат с двумя знаками после запятой
    return diff.toLocaleString('ru-RU', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function parseCurrency(value) {
    if (value == null) return 0;

    const str = String(value).trim();
    if (str === '') return 0;

    // Убираем пробелы, заменяем запятую на точку
    let cleaned = str.replace(/\s/g, '').replace(',', '.');

    // Убираем лишние символы, кроме цифр, точки и минуса
    cleaned = cleaned.replace(/[^\d.-]/g, '');

    const parsed = parseFloat(cleaned);
    return isNaN(parsed) ? 0 : parsed;
}


function buildAuthorMenu(item, hasEstimate, hasContract, hasReport, canEdit, canDelete, canArchive, canTransfer, isOverdue) {
    const parts = [];

    if (hasEstimate) {
        parts.push(`
            <a href="/Estimate/Index?contractId=${item.Id}" class="menu-item">
                <span class="menu-icon"><svg><use href="#ic-calculate"></use></svg></span>
                <span class="menu-label">Сметы</span>
            </a>`);
    }

    if (hasContract) {
        if (hasReport) {
            parts.push(`
                <div class="menu-item-wrap">
                    <a class="menu-item">
                        <span class="menu-icon"><svg><use href="#ic-export"></use></svg></span>
                        <span class="menu-label">Экспорт</span>
                        <span class="menu-arrow">›</span>
                    </a>
                    <div class="action-submenu">
                        <a href="/Report/Print/Contracts/Details?contractId=${item.Id}" class="submenu-item">
                            <span class="menu-icon"><svg><use href="#ic-excel-file"></use></svg></span>
                            <span class="menu-label">Детальная информация</span>
                        </a>
                        <a href="/Report/Print/Scopes?contractId=${item.Id}&numberContr=${item.Number ?? ''}" class="submenu-item">
                            <span class="menu-icon"><svg><use href="#ic-excel-file"></use></svg></span>
                            <span class="menu-label">Объем работ</span>
                        </a>
                    </div>
                </div>`);
        }

        if (!item.IsExpired && !item.IsClosed && canEdit && isOverdue) {
            parts.push(`
        <div class="menu-item-wrap">
            <a class="menu-item">
                <span class="menu-icon"><svg><use href="#ic-swap-horiz"></use></svg></span>
                <span class="menu-label">Изменить статус</span>
                <span class="menu-arrow">›</span>
            </a>
            <div class="action-submenu">
                <a href="/Contracts/ChangeStatus?contrId=${item.Id}&status=expired" class="submenu-item modal-link"
                   data-message="Статус договора будет изменен на 'ПРОСРОЧЕН'. Продолжить?">
                    <span class="menu-label">Просрочен</span>
                </a>
                <a href="/Contracts/ChangeStatus?contrId=${item.Id}&status=closed" class="submenu-item modal-link"
                   data-message="Статус договора будет изменен на 'ОЖИДАЕТСЯ АКТ ВВОДА'. Продолжить?">
                    <span class="menu-label">Закрыт, ожидается акт ввода</span>
                </a>
            </div>
        </div>`);
        }

        if (item.WorkflowRef) {
            parts.push(`
                <a href="#" data-link="${item.WorkflowRef}" class="menu-item copy-link-btn">
                    <span class="menu-icon"><svg><use href="#ic-duplicate"></use></svg></span>
                    <span class="menu-label">Ссылка в 1C</span>
                </a>`);
        }

        if (canTransfer) {
            parts.push(`
                <a href="/Contracts/ChangeOwner?contrId=${item.Id}" class="menu-item">
                    <span class="menu-icon"><svg><use href="#ic-share"></use></svg></span>
                    <span class="menu-label">Передать договор</span>
                </a>`);
        }

        if (!item.IsArchive && canArchive) {
            parts.push(`
                <a href="/Contracts/ChangeStatus?contrId=${item.Id}&status=archive&isEngineering=false" 
                   class="menu-item modal-link" 
                   data-message="Договор будет перемещен в архив. Продолжить?">
                    <span class="menu-icon"><svg><use href="#ic-archive"></use></svg></span>
                    <span class="menu-label">В архив</span>
                </a>`);
        }

        if (canEdit) {
            parts.push(`
                <a href="/Contracts/Edit/${item.Id}" class="menu-item">
                    <span class="menu-icon"><svg><use href="#ic-edit"></use></svg></span>
                    <span class="menu-label">Редактировать</span>
                </a>`);
        }

        if (canDelete) {
            parts.push(`
                <div class="menu-divider"></div>
                <a href="/Contracts/Delete/${item.Id}" class="menu-item danger">
                    <span class="menu-icon"><svg><use href="#ic-delete"></use></svg></span>
                    <span class="menu-label">Удалить</span>
                </a>`);
        }
    }

    return parts.join('');
}