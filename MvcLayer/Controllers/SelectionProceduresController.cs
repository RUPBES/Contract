using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using Newtonsoft.Json;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "ViewPolicy")]
    public class SelectionProceduresController : Controller
    {
        private readonly IContractService _contractService;
        private readonly ISelectionProcedureService _selectProcedureService;
        private readonly IMapper _mapper;

        private readonly IFileService _fileService;
        public SelectionProceduresController(IContractService contractService, IMapper mapper, ISelectionProcedureService selectionProcedureService,
            IFileService fileService)
        {
            _selectProcedureService = selectionProcedureService;
            _contractService = contractService;
            _mapper = mapper;
            _fileService = fileService;
        }

        public IActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<SelectionProcedureViewModel>>(_selectProcedureService.GetAll()));
        }

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<SelectionProcedureViewModel>(_selectProcedureService.Find(x => x.ContractId == contractId).LastOrDefault()));
        }

        //при создании договора, автоматически создается запись в таблице "Процедура выбора" с Видом закупки,
        // поэтому необходимо найти созданную для данного договора проц.выбора и добавить все данные
        [Authorize(Policy = "CreatePolicy")]
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
        [Authorize(Policy = "CreatePolicy")]
        public IActionResult Create(SelectionProcedureViewModel selectProcedure)
        {
            if (selectProcedure is not null)
            {
                _selectProcedureService.Update(_mapper.Map<SelectionProcedureDTO>(selectProcedure));

                NotificationHelper.SetNotification(TempData, $"Данные процедуры выбора обновлены", NotificationType.Info);
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

        [Authorize(Policy = "EditPolicy")]
        public ActionResult Edit(int id, int? contractId = null)
        {
            ViewBag.contractId = contractId;
            return View(_mapper.Map<SelectionProcedureViewModel>(_selectProcedureService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditPolicy")]
        public IActionResult Edit(SelectionProcedureViewModel selectProcedure)
        {
            if (selectProcedure is not null)
            {
                try
                {
                    _selectProcedureService.Update(_mapper.Map<SelectionProcedureDTO>(selectProcedure));
                    NotificationHelper.SetNotification(TempData, $"Данные процедуры выбора обновлены", NotificationType.Info);
                    if (selectProcedure.FilesEntity != null && selectProcedure.FilesEntity.Count() > 0)
                    {
                        int fileId = (int)_fileService.Create(selectProcedure.FilesEntity, Folder.SelectionProcedures, selectProcedure.Id, selectProcedure?.ContractId?.ToString());
                        _selectProcedureService.AddFile(selectProcedure.Id, fileId);
                    }
                }
                catch
                {
                    NotificationHelper.SetNotification(TempData, $"Ошибка обновления", NotificationType.Error);
                    return View();
                }
            }
            if (selectProcedure?.ContractId is not null && selectProcedure.ContractId > 0)
            {
                return RedirectToAction(nameof(GetByContractId), new { contractId = selectProcedure.ContractId });
            }
            else
            {
                NotificationHelper.SetNotification(TempData, "Некорректные данные", NotificationType.Warning);
                return RedirectToAction(nameof(Index));
            }
        }

        [Route("/SelectionProcedures/Delete/{id}/{contractId}")]
        public ActionResult Delete(int? id, int contractId)
        {
            if (id == null)
            {
                return NotFound();
            }

            _selectProcedureService.Delete((int)id);
            NotificationHelper.SetNotification(TempData, $"Процедура выбора удалена", NotificationType.Info);
            return RedirectToAction("Details", "Contracts", new { id = contractId});
        }


        [Route("/archive/SelectionProcedure/")]
        public IActionResult GetArchByContractId(int contractId)
        {
            return View(_mapper.Map<SelectionProcedureViewModel>(_selectProcedureService.Find(x => x.ContractId == contractId, useArchiveData:true).LastOrDefault()));
        }
    }
}
