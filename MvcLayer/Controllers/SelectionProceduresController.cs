using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;

namespace MvcLayer.Controllers
{
    public class SelectionProceduresController : Controller
    {
        private readonly IContractService _contractService;
        private readonly ISelectionProcedureService _selectProcedureService;
        private readonly IMapper _mapper;

        public SelectionProceduresController(IContractService contractService, IMapper mapper, ISelectionProcedureService selectionProcedureService)
        {
            _selectProcedureService = selectionProcedureService;
            _contractService = contractService;
            _mapper = mapper;

        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<SelectionProcedureViewModel>>(_selectProcedureService.GetAll()));
        }

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<SelectionProcedureViewModel>>(_selectProcedureService.Find(x => x.ContractId == contractId)));
        }

        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _scopeWork.GetAll() == null)
        //    {
        //        return NotFound();
        //    }

        //    var scopeWork = _scopeWork.GetById((int)id);
        //    if (scopeWork == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(_mapper.Map<ScopeWorkViewModel>(scopeWork));
        //}

        //при создании договора, автоматически создается запись в таблице "Процедура выбора" с Видом закупки,
        // поэтому необходимо найти созданную для данного договора проц.выбора и добавить все данные
        public IActionResult Create(int contractId)
        {
            if (contractId > 0)
            {
                return View(_mapper.Map<SelectionProcedureViewModel>(_selectProcedureService.Find(x => x.ContractId == contractId).FirstOrDefault()));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SelectionProcedureViewModel selectProcedure)
        {
            if (selectProcedure is not null)
            {
                 _selectProcedureService.Update(_mapper.Map<SelectionProcedureDTO>(selectProcedure));

                //если запрос пришел с детальной инфы по договору, тогда редиректим проц.выбора для этого договора, если нет - на список всех проц.выбора
                if (selectProcedure.ContractId is not null)
                {
                    return RedirectToAction(nameof(GetByContractId), new { contractId = selectProcedure.ContractId });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(selectProcedure);
        }

        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _scopeWork.GetAll() == null)
        //    {
        //        return NotFound();
        //    }

        //    var scopeWork = _scopeWork.GetById((int)id);
        //    if (scopeWork == null)
        //    {
        //        return NotFound();
        //    }
        //    //ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.AgreementContractId);
        //    //ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Id", contract.SubContractId);
        //    return View(_mapper.Map<ScopeWorkViewModel>(scopeWork));
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, ScopeWorkViewModel scopeWork)
        //{
        //    if (id != scopeWork.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _scopeWork.Update(_mapper.Map<ScopeWorkDTO>(scopeWork));
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (_scopeWork.GetById((int)id) is null)
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    //ViewData["AgreementContractId"] = new SelectList(_contractService.GetAll(), "Id", "Name", scopeWork.AgreementContractId);
        //    //ViewData["SubContractId"] = new SelectList(_contractService.GetAll(), "Id", "Name", scopeWork.SubContractId);
        //    return View(scopeWork);
        //}

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _selectProcedureService.GetAll() == null)
            {
                return NotFound();
            }

            _selectProcedureService.Delete((int)id);
            return RedirectToAction("Index", "Contracts");
        }
    }
}
