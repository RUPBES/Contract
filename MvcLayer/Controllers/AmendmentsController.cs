using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    public class AmendmentsController : Controller
    {
        private readonly IAmendmentService _amendment;        
        private readonly IMapper _mapper;

        public AmendmentsController(IAmendmentService amendment, IMapper mapper)
        {
           _amendment = amendment;
            _mapper = mapper;           
        }
               
        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<AmendmentViewModel>>(_amendment.GetAll()));
        }

        public ActionResult Create()
        {
            return View();
        }

        // POST: AmendmentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AmendmentViewModel amendment)
        {
            try
            {
                _amendment.Create(_mapper.Map<AmendmentDTO>(amendment));
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AmendmentsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AmendmentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AmendmentsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AmendmentsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
