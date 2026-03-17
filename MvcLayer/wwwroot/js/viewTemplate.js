function setContractTableRow(contractItems, permissions, isEngineering) {
    return contractItems.map(contractItem => `
    <tr class="${contractItem.DateEndWork && new Date(contractItem.DateEndWork) < new Date() ? `overdue` : ``} ">                           
            <td><span class="table_span-numberanddate">
                    <a href="/Contracts/Details/${contractItem.Id}" title="Просмотр детальной информации" class="save-page-state">
                        ${contractItem.Number ?? ``} от ${contractItem.Date ?? ``}
                    </a>
                </span></td>
            <td><span class="table_span-nameobject">${contractItem.NameObject ?? ``}</span></td>
            <td><span class="table_span-customer">${contractItem.Client ?? ``}</span></td>
            <td><div><span class="table_span-contractor">${contractItem.GenContractor ?? ``}</span></div>                                   
                    ${contractItem.ResponsibleForWork ?
            `<div><span class="table_span-contractor"><hr /><span>Ответственный за производство работ:</span><br />${contractItem.ResponsibleForWork}</span></div>` : ``}                                                                        
                </td>                            
            <td><span class="table_span-deadlines">
                    ${contractItem.DateBeginWork || contractItem.DateEndWork ? `<b>выполнения работ:</b><br><span>${contractItem.DateBeginWork ?? ``} - ${contractItem?.DateEndWork ?? ``}</span>` : ``} 
                    ${contractItem.EnteringTerm ? `<br><b>ввода:</b><br><span>${contractItem?.EnteringTerm ?? ``}</span>` : ``}                                    
                </span>
            </td>
            <td><span class="table_span-conditions">
                    <a href="/Prepayments/GetByContractId?contractId=${contractItem.Id}" > ${contractItem.PaymentСonditionsAvans ?? ``}</a><br /><br />
                    <a href="/Payments/GetByContractId?contractId=${contractItem.Id}">${contractItem.PaymentСonditionsRaschet ?? ``}</a>
                    </span>
            </td>           
            ${!isEngineering ? `<td><span class="table_span-work">${contractItem.WorkType ?? ``}</span></td>` : ``}
                                        
            <td class="text-end"><span class="table_span-contractprice">${contractItem.ContractPrice ?? `0.00`} ${contractItem.Сurrency ?? ``}</span></td>
            <td class="text-end"><span class="table_span-realization">${contractItem.PreYearSum ?? `0.00`} ${contractItem.Сurrency ?? ``}</span></td>                           
            <td class="text-end"><span class="table_span-remains">${contractItem.RemainingSum ?? `0.00`} ${contractItem.Сurrency ?? ``}</span></td>
            <td><span class="table_span-volume">${contractItem.ThisYearSum ?? `0.00`} ${contractItem.Сurrency ?? ``}</span></td>
            <td><span class="table_span-action-main">            
                <button class="action-btn"><svg class="ic ic-18"><use href="#ic-more-vert"/></svg></button>
                   <div class="action-menu">
                    <a class="menu-item save-page-state" href="/Contracts/Details/${contractItem.Id}" title="Просмотр детальной информации">
                        <span class="menu-icon"><svg class="ic ic-15"><use href="#ic-open"></use></svg></span>
                        <span class="menu-label">Открыть</span>
                    </a>

                    ${(contractItem.Author === permissions.company) || (permissions.company === `ContrOrgBes`) ? `
                        ${!isEngineering && permissions.groupeName.includes(`GRP_Estimate`) ?
                        `<a href="/Estimate/Index?contractId=${contractItem.Id}" class="menu-item" title="Просмотр смет договора">
                            <span class="menu-icon"><svg ><use href="#ic-calculate"></use></svg></span>
                            <span class="menu-label">Сметы</span>
                         </a> 
                        `: ``}   

                        ${permissions.groupeName.includes(`GRP_Contract`) ?
                            `
                            ${(permissions.isReader && permissions.groupeName.includes(`GRP_Report`)) ?
                             `<div class="menu-item-wrap">
                                <a class="menu-item">
                                    <span class="menu-icon"><svg ><use href="#ic-export"></use></svg> </span>
                                    <span class="menu-label">Экспорт</span>
                                    <span class="menu-arrow">›</span>
                                </a>
                                <div class="action-submenu">
                                    <a href="/Report/Print/Contracts/Details?contractId=${contractItem.Id}" class="submenu-item" title="Экспортировать в Excel, детальную информацию по договору">
                                        <span class="menu-icon"> <svg ><use href="#ic-excel-file"></use></svg></span>
                                        <span class="menu-label">Детальная информация</span>
                                    </a>
                                    <a href="/Report/Print/Scopes?contractId=${contractItem.Id}&numberContr=${contractItem?.Number}" class="submenu-item" title="Экспортировать в Excel, объем работ по договору">
                                        <span class="menu-icon"> <svg><use href="#ic-excel-file"></use></svg></span>
                                        <span class="menu-label">Объем работ</span>
                                    </a>
                                </div>
                            </div>
                            ` : ``}  

                             ${!contractItem.IsExpired && !contractItem.IsClosed && permissions?.isEditor ?
                             `<div class="menu-item-wrap">
                                <a class="menu-item">
                                    <span class="menu-icon"><svg ><use href="#ic-swap-horiz"></use></svg> </span>
                                    <span class="menu-label">Изменить статус</span>
                                    <span class="menu-arrow">›</span>
                                </a>
                                <div class="action-submenu">
                                    ${contractItem.IsExpired || contractItem.IsClosed ?
                                           ` <span class="menu-label">
                                                ${contractItem.IsExpired ? `<div class="icon expired" title="Договор просрочен"><div></div></div>` : ``} 
                                                ${contractItem.IsClosed ? `<div class="icon closed" title="Договор закрыт, ожидается акт ввода"><div></div></div>` : ``} 
                                            </span>
                                            <div class="menu-divider"></div>
                                    `: ``}
                                    
                                    <a href="/Contracts/ChangeStatus?contrId=${contractItem.Id}&status=expired" class="submenu-item" title="Изменить статус договора на - "договор просрочен"">
                                       
                                        <span class="menu-label">Просрочен</span>
                                    </a>
                                    <a href="/Contracts/ChangeStatus?contrId=${contractItem.Id}&status=closed" class="submenu-item" title="Изменить статус договора на - "договор закрыт, ожидается акта ввода"">
                                        
                                        <span class="menu-label">Закрыт, ожидается акта ввода</span>
                                    </a>
                                </div>
                            </div>
                            ` : ``}

                            ${contractItem.WorkflowRef ?
                            `<a href="#" data-link="${contractItem.WorkflowRef ?? ``}" class="menu-item copy-link-btn" title="Скопировать ссылку на договор в 1C">
                                <span class="menu-icon"> <svg ><use href="#ic-duplicate"></use></svg></span>
                                <span class="menu-label">Ссылка в 1C</span>
                            </a>
                            ` : ``}

                            ${permissions.company === `ContrOrgBes` && permissions.isEditor ?
                            `<a href="/Contracts/ChangeOwner?contrId=${contractItem.Id}" class="menu-item" title="Передать договор филиалу">
                                <span class="menu-icon"> <svg ><use href="#ic-share"></use></svg></span>
                                <span class="menu-label">Передать договор</span>
                            </a>
                            ` : ``}

                            ${(!contractItem.IsArchive && permissions?.isEditor && permissions?.isAdmin) ?
                                `<a href="/Contracts/ChangeStatus?contrId=${contractItem.Id}&status=archive&isEngineering=false" class="menu-item modal-link" title="Переместить договор в архив" data-message="Договор будет перемещен в архив. Продолжить?">
                                    <span class="menu-icon"><svg ><use href="#ic-archive"></use></svg></span>
                                    <span class="menu-label">В архив</span>
                                </a>
                                ` : ``} 

                            ${permissions.isEditor ?
                                `<a href="/Contracts/Edit/${contractItem.Id}" class="menu-item" title="Редактировать договор">
                                        <span class="menu-icon"><svg ><use href="#ic-edit"></use></svg></span>
                                        <span class="menu-label">Редактировать</span>
                                </a> 
                                ` : ``}
                            ${permissions.isDeleter ? 
                            `<div class="menu-divider"></div>
                                <a href="/Contracts/Delete/${contractItem.Id}" class="menu-item danger" title="Удалить договор">
                                    <span class="menu-icon"> <svg ><use href="#ic-delete"></use></svg></span>
                                    <span class="menu-label">Удалить</span>
                                </a>
                            ` : ``} 
                            </div>

                        `: ``} 
                    `: ``}
                </span>
            </td>
    </tr>`
    ).join('');
}


function setContractArchiveTableRow(contractItems, permissions, isEngineering) {
    return contractItems.map(contractItem => `
    <tr>                           
            <td><span class="table_span-numberanddate">
                    <a href="/archive/Contracts/Details/?id=${contractItem.Id}" title="Просмотр детальной информации">
                        ${contractItem.Number ?? ``} от ${contractItem.Date ?? ``}
                    </a>
                </span>
            </td>
            <td><span class="table_span-nameobject">${contractItem.NameObject ?? ``}</span></td>
            <td><span class="table_span-customer">${contractItem.Client ?? ``}</span></td>   
             ${!isEngineering ?
            `<td><div><span class="table_span-contractor">${contractItem.GenContractor ?? ``}</span></div>                                   
                            ${contractItem.ResponsibleForWork ?
                `<div><span class="table_span-contractor"><hr /><span>Ответственный за производство работ:</span><br />${contractItem.ResponsibleForWork}</span></div>` : ``}                                                                        
                </td> `
            : ``}

            <td><span class="table_span-deadlines">
                    ${contractItem.DateBeginWork || contractItem.DateEndWork ? `<b>выполнения работ:</b><br><span>${contractItem.DateBeginWork ?? ``} - ${contractItem?.DateEndWork ?? ``}</span>` : ``} 
                    ${contractItem.EnteringTerm ? `<br><b>ввода:</b><br><span>${contractItem?.EnteringTerm ?? ``}</span>` : ``}                                    
                </span>
            </td>
            <td><span class="table_span-conditions">
                    <a href="/Prepayments/GetByContractId?contractId=${contractItem.Id}" > ${contractItem.PaymentСonditionsAvans ?? ``}</a><br /><br />
                    <a href="/Payments/GetByContractId?contractId=${contractItem.Id}">${contractItem.PaymentСonditionsRaschet ?? ``}</a>
                    </span>
            </td>           
            ${!isEngineering ? `<td><span class="table_span-work">${contractItem.WorkType ?? ``}</span></td>` : ` <td><span class="table_span-work">${contractItem.PaymentСonditionsPrice ?? ``}</span></td>`}
                                        
            <td class="text-end"><span class="table_span-contractprice">${contractItem.ContractPrice ?? `0.00`} ${contractItem.Сurrency ?? ``}</span></td>
            <td class="text-end"><span class="table_span-realization">${contractItem.PreYearSum ?? `0.00`} ${contractItem.Сurrency ?? ``}</span></td>                           
            <td class="text-end"><span class="table_span-remains">${contractItem.RemainingSum ?? `0.00`} ${contractItem.Сurrency ?? ``}</span></td>
            <td><span class="table_span-volume">${contractItem.ThisYearSum ?? `0.00`} ${contractItem.Сurrency ?? ``}</span></td>
            
            <td><span class="table_span-action-main">
                    <div class="icon info" title="Детальная информация"><a href="/archive/Contracts/Details/?id=${contractItem.Id}"></a></div>                        
               ${!isEngineering && permissions.groupeName.includes(`GRP_Estimate`) ? `<div class="icon estimate" title="Сметы"><a href="/archive/Estimates?contractId=${contractItem.Id}"></a></div>` : ``}   
                
                </span>
        </td>
    </tr>`
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