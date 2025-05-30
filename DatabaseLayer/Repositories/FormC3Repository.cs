using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class FormC3Repository : IRepository<FormC3a>
    {
        private readonly ContractsContext _context;
        public FormC3Repository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(FormC3a entity)
        {
            if (entity is not null)
            {
                _context.FormC3as.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            FormC3a form = _context.FormC3as.Find(id);

            if (form is not null)
            {
                _context.FormC3as.Remove(form);
            }
        }

        public IEnumerable<FormC3a> Find(Func<FormC3a, bool> predicate)
        {
            return _context.FormC3as.Where(predicate).ToList();
        }

        public IEnumerable<FormC3a> Find(Func<FormC3a, bool> where, Func<FormC3a, FormC3a> select)
        {
            return _context.FormC3as.Where(where).Select(select).ToList();
        }        

        public IEnumerable<FormC3a> GetAll()
        {
            return _context.FormC3as.ToList();
        }

        public FormC3a GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.FormC3as.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(FormC3a entity)
        {
            if (entity is not null)
            {
                var form = _context.FormC3as.Find(entity.Id);

                if (form is not null)
                {
                    form.Period = entity.Period;
                    form.DateSigning = entity.DateSigning;
                    form.SmrCost = entity.SmrCost;
                    form.PnrCost = entity.PnrCost;
                    form.EquipmentCost = entity.EquipmentCost;
                    form.OtherExpensesCost = entity.OtherExpensesCost;
                    form.AdditionalCost = entity.AdditionalCost;
                    form.MaterialCost = entity.MaterialCost;
                    form.GenServiceCost = entity.GenServiceCost;

                    form.OffsetTargetPrepayment = entity.OffsetTargetPrepayment;
                    form.OffsetCurrentPrepayment = entity.OffsetCurrentPrepayment;

                    form.SmrContractCost = entity.SmrContractCost ;
                    form.MaterialClientCost = entity.MaterialClientCost;
                    form.EquipmentClientCost = entity.EquipmentClientCost;
                    form.IsExemptFromVAT = entity.IsExemptFromVAT ?? false;
                    form.CostToConstructionIndustryFund = entity.CostToConstructionIndustryFund;
                    form.CostStatisticReportOfContractor = entity.CostStatisticReportOfContractor;

                    form.AdditionalNdsCost = entity.AdditionalNdsCost;
                    form.SmrNdsCost = entity.SmrNdsCost;

                    form.PnrNdsCost = entity.PnrNdsCost;
                    form.EquipmentNdsCost = entity.EquipmentNdsCost;
                    form.AdditionalContractCost = entity.AdditionalContractCost;
                    form.EquipmentContractCost = entity.EquipmentContractCost;

                    form.PnrContractCost = entity.PnrContractCost;
                    form.OtherExpensesNdsCost = entity.OtherExpensesNdsCost;
                    form.Reserve = entity.Reserve;

                    _context.FormC3as.Update(form);
                }
            }
        }
    }
}

