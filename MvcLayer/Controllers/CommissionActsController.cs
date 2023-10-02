﻿using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    public class CommissionActsController : Controller
    {
        private readonly ICommissionActService _commissionActService;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public CommissionActsController(ICommissionActService commissionActService, IMapper mapper, IFileService fileService)
        {
            _commissionActService = commissionActService;
            _mapper = mapper;
            _fileService = fileService;
        }

        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<CommissionActViewModel>>(_commissionActService.GetAll()));
        }

        public IActionResult GetByContractId(int id)
        {
            return View(_mapper.Map<IEnumerable<CommissionActViewModel>>(_commissionActService.Find(x => x.ContractId == id)));
        }

        public ActionResult Create(int contractId)
        {
            ViewData["contractId"] = contractId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CommissionActViewModel commissionAct)
        {
            try
            {
                int fileId = (int)_fileService.Create(commissionAct.FilesEntity, FolderEnum.CommissionActs);
                int commissionActId = (int)_commissionActService.Create(_mapper.Map<CommissionActDTO>(commissionAct));
                _commissionActService.AddFile(commissionActId, fileId);
               
                //если запрос пришел с детальной инфы по договору, тогда редиректим туда же, если нет - на список всех
                if (commissionAct.ContractId is not null && commissionAct.ContractId > 0)
                {
                    return RedirectToAction(nameof(GetByContractId), new { id = commissionAct.ContractId });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }               
            }
            catch
            {
                return View();
            }
        }

        //public ActionResult Edit(int id, int? contractId = null)
        //{
        //    ViewBag.contractId = contractId;
        //    return View(_mapper.Map<CommissionActViewModel>(_commissionActService.GetById(id)));
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(CommissionActViewModel commissionAct)
        //{
        //    if (commissionAct is not null)
        //    {
        //        try
        //        {
        //            _commissionActService.Update(_mapper.Map<CommissionActDTO>(commissionAct));
        //        }
        //        catch
        //        {
        //            return View();
        //        }
        //    }

        //    if (commissionAct?.ContractId is not null && commissionAct.ContractId > 0)
        //    {
        //        return RedirectToAction(nameof(GetByContractId), new { id = commissionAct.ContractId });
        //    }
        //    else
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        public ActionResult Delete(int id, int? contractId = null)
        {
            try
            {
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.CommissionActs))
                {
                    _fileService.Delete(item.Id);
                }

                _commissionActService.Delete(id);

                if (contractId is not null && contractId > 0)
                {
                    return RedirectToAction(nameof(GetByContractId), new { id = contractId });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                return View();
            }
        }
    }
}