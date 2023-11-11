﻿using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    public class FilesController : Controller
    {
        private readonly IFileService _file;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public FilesController(IFileService file, IWebHostEnvironment env, IMapper mapper)
        {
            _file = file;
            _env = env;
            _mapper = mapper;
        }

        public ActionResult Index()
        {
            return View(_file.GetAll());
        }

        public ActionResult AddFile(int entityId, FolderEnum fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null)
        {
            ViewBag.redirectAction = redirectAction;
            ViewBag.redirectController = redirectController;
            ViewBag.fileCategory = fileCategory;
            ViewBag.entityId = entityId;
            ViewBag.contractId = contractId;

            return View();
        }

        [HttpPost]
        public ActionResult AddFile(IFormCollection collection, int entityId, FolderEnum fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null)
        {

            int fileId = (int)_file.Create(collection.Files, fileCategory);
            _file.AttachFileToEntity(fileId, entityId, fileCategory);

            return RedirectToAction("GetByContractId", "Files", new { id = contractId, redirectAction = redirectAction, redirectController= redirectController, fileCategory = fileCategory });
        }

        [HttpGet]
        public ActionResult GetByContractId(int id, FolderEnum fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null)
        {
            ViewBag.redirectAction = redirectAction;
            ViewBag.redirectController = redirectController;
            ViewBag.entityId = id;
            ViewBag.contractId = contractId;
            var files = _file.GetFilesOfEntity(id, fileCategory).ToList();
            return View(files);
        }


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                _file.Create(collection.Files, folder: FolderEnum.Other);

                return Redirect($"/Home/Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(int id, FolderEnum fileCategory, string redirectAction = null, string redirectController = null, int? contractId = null)
        {
            try
            {
                _file.Delete(id);

                if (redirectController is not null && redirectAction is not null)
                {
                    return RedirectToAction(redirectAction, redirectController, new {id = contractId, fileCategory = fileCategory });
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult GetFile(int id)
        {
            if (id != 0)
            {
                var file = _file.GetById(id);
                return PhysicalFile(_env.WebRootPath + file.FilePath, file.FileType, file.FileName);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }       
    }
}