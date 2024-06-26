﻿using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ContrViewPolicy")]
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

        //при создании договора, автоматически создается запись в таблице "Процедура выбора" с Видом закупки,
        // поэтому необходимо найти созданную для данного договора проц.выбора и добавить все данные
        [Authorize(Policy = "ContrAdminPolicy")]
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
        [Authorize(Policy = "ContrAdminPolicy")]
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
                    return RedirectToAction("Index", "Contracts");
                }
            }
            return View(selectProcedure);
        }

        [Authorize(Policy = "ContrEditPolicy")]
        public ActionResult Edit(int id, int? contractId = null)
        {
            ViewBag.contractId = contractId;
            return View(_mapper.Map<SelectionProcedureViewModel>(_selectProcedureService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContrEditPolicy")]
        public async Task<IActionResult> Edit(SelectionProcedureViewModel selectProcedure)
        {
            if (selectProcedure is not null)
            {
                try
                {
                    _selectProcedureService.Update(_mapper.Map<SelectionProcedureDTO>(selectProcedure));
                }
                catch
                {
                    return View();
                }
            }
            if (selectProcedure?.ContractId is not null && selectProcedure.ContractId > 0)
            {
                return RedirectToAction(nameof(GetByContractId), new { contractId = selectProcedure.ContractId });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Policy = "ContrAdminPolicy")]
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
