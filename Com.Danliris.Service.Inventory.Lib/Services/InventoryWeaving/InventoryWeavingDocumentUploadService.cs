﻿using Com.Danliris.Service.Inventory.Lib.Models.InventoryWeavingModel;
using Com.Danliris.Service.Inventory.Lib.ViewModels.InventoryWeavingViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using CsvHelper.TypeConversion;
using System.Threading.Tasks;
using Com.Moonlay.Models;
using Com.Danliris.Service.Inventory.Lib.Helpers;
using Newtonsoft.Json;
using Com.Moonlay.NetCore.Lib;

namespace Com.Danliris.Service.Inventory.Lib.Services.InventoryWeaving
{
    public class InventoryWeavingDocumentUploadService : IInventoryWeavingDocumentUploadService
    {
        private string USER_AGENT = "Service";
        private const string UserAgent = "inventory-service";
        protected DbSet<InventoryWeavingDocument> DbSet;
        protected DbSet<InventoryWeavingMovement> DbSet2;
        public IIdentityService IdentityService;
        public readonly IServiceProvider ServiceProvider;
        public InventoryDbContext DbContext;

        public InventoryWeavingDocumentUploadService(IServiceProvider serviceProvider, InventoryDbContext dbContext)
        {
            DbContext = dbContext;
            ServiceProvider = serviceProvider;
            DbSet = dbContext.Set<InventoryWeavingDocument>();
            DbSet2 = dbContext.Set<InventoryWeavingMovement>();
            IdentityService = serviceProvider.GetService<IIdentityService>();
            //IdentityService = serviceProvider.GetService<IIdentityService>();
        }


        public List<string> CsvHeader { get; } = new List<string>()
        {
           // "BonNo", "Benang", "Anyaman", "Lusi", "Pakan", "Lebar", "JL", "JP", "AL", "AP", "SP", "Grade", "Piece", "Qty", "QtyPiece"

            "BonNo","Tanggal","Benang","Anyaman","Lusi","Pakan","Lebar","JL","JP","AL","AP","SP","Grade","Piece","Qty","QtyPiece"
        };

        public ReadResponse<InventoryWeavingDocument> Read(int page, int size, string order, string keyword, string filter)
        {
            IQueryable<InventoryWeavingDocument> Query = this.DbSet;


            List<string> SearchAttributes = new List<string>()
                {
                    "BonNo","Type"
                };
            Query = QueryHelper<InventoryWeavingDocument>.Search(Query, SearchAttributes, keyword);

            List<string> SelectedFields = new List<string>()
            {
                "Id", "date", "bonNo", "bonType",  "storageCode", "storageId", "storageName", "type", "_LastModifiedUtc"
            };

            Query = Query.Where( s => s.Type == "IN")
                .Select(s => new InventoryWeavingDocument
                {
                    Id = s.Id,

                    Date = s.Date,
                    BonNo = s.BonNo,
                    BonType = s.BonType,
                    StorageCode = s.StorageCode,
                    StorageId = s.StorageId,
                    StorageName = s.StorageName,
                    Type = s.Type,
                    _LastModifiedUtc = s._LastModifiedUtc,

                });


            #region OrderBy

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            Query = QueryHelper<InventoryWeavingDocument>.Order(Query, OrderDictionary);
            #endregion OrderBy

            #region Paging

            Pageable<InventoryWeavingDocument> pageable = new Pageable<InventoryWeavingDocument>(Query, page - 1, size);
            List<InventoryWeavingDocument> Data = pageable.Data.ToList<InventoryWeavingDocument>();
            int TotalData = pageable.TotalCount;

            #endregion Paging

            return new ReadResponse<InventoryWeavingDocument>(Data, TotalData, OrderDictionary, SelectedFields);
        }
        public sealed class InventoryWeavingDocumentMap : CsvHelper.Configuration.ClassMap<InventoryWeavingDocumentCsvViewModel>
        {
            public InventoryWeavingDocumentMap()
            {
                Map(p => p.ReferenceNo).Index(0);
                Map(p => p.Date).Index(1);
                Map(p => p.MaterialName).Index(2);
                Map(p => p.WovenType).Index(3);
                Map(p => p.Yarn1).Index(4);
                Map(p => p.Yarn2).Index(5);
                Map(p => p.Width).Index(6);
                Map(p => p.YarnType1).Index(7);
                Map(p => p.YarnType2).Index(8);
                Map(p => p.YarnOrigin1).Index(9);
                Map(p => p.YarnOrigin2).Index(10);
                Map(p => p.ProductionOrderNo).Index(11);
                Map(p => p.Grade).Index(12);
                Map(p => p.Piece).Index(13).TypeConverter<StringConverter>();
                Map(p => p.Qty).Index(14).TypeConverter<StringConverter>();
                Map(p => p.QtyPiece).Index(15).TypeConverter<StringConverter>();


            }
        }

        public Tuple<bool, List<object>> UploadValidate(ref List<InventoryWeavingDocumentCsvViewModel> Data, List<KeyValuePair<string, StringValues>> Body)
        {
            List<object> ErrorList = new List<object>();
            string ErrorMessage;
            bool Valid = true;

            foreach (InventoryWeavingDocumentCsvViewModel productVM in Data)
            {
                ErrorMessage = "";

                if (string.IsNullOrWhiteSpace(productVM.ReferenceNo))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "No Bon Tidak Boleh Kosong tidak boleh kosong, ");
                }

                if (productVM.Date == null)
                {
                    ErrorMessage = string.Concat(ErrorMessage, "No Bon Tidak Boleh Kosong tidak boleh kosong, ");
                }

                if (string.IsNullOrWhiteSpace(productVM.ProductionOrderNo))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "SP tidak boleh kosong, ");
                }

                if (string.IsNullOrWhiteSpace(productVM.Grade))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Grade tidak boleh kosong, ");
                }

                if (string.IsNullOrWhiteSpace(productVM.MaterialName))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Benang tidak boleh kosong, ");
                }


                if (string.IsNullOrWhiteSpace(productVM.Width))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Lebar tidak boleh kosong, ");
                }


                if (string.IsNullOrWhiteSpace(productVM.Yarn1))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Lusi tidak boleh kosong, ");
                }

                if (string.IsNullOrWhiteSpace(productVM.Yarn2))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Pakan tidak boleh kosong, ");
                }

                if (string.IsNullOrWhiteSpace(productVM.YarnType1))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Jenis Lusi tidak boleh kosong, ");
                }

                if (string.IsNullOrWhiteSpace(productVM.YarnType2))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Jenis Pakan tidak boleh kosong, ");
                }

                if (string.IsNullOrWhiteSpace(productVM.YarnOrigin1))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Asal Lusi tidak boleh kosong, ");
                }

                if (string.IsNullOrWhiteSpace(productVM.YarnOrigin2))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Asal Pakan tidak boleh kosong, ");
                }

                if (string.IsNullOrWhiteSpace(productVM.Piece))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Piece tidak boleh kosong, ");
                }

                double Qty = 0;
                if (string.IsNullOrWhiteSpace(productVM.Qty))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Qty tidak boleh kosong, ");
                }
                else if (!double.TryParse(productVM.Qty, out Qty))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Qty harus numerik, ");
                }
                else if (Qty < 0)
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Qty harus lebih besar dari 0, ");
                }
                else
                {
                    string[] QtySplit = productVM.Qty.Split('.');
                    if (QtySplit.Count().Equals(2) && QtySplit[1].Length > 2)
                    {
                        ErrorMessage = string.Concat(ErrorMessage, "Qty maksimal memiliki 2 digit dibelakang koma, ");
                    }
                }




                double QtyPiece = 0;
                if (string.IsNullOrWhiteSpace(productVM.QtyPiece))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Quantity Piece tidak boleh kosong, ");
                }
                else if (!double.TryParse(productVM.QtyPiece, out QtyPiece))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Quantity Piece numerik, ");
                }
                else if (QtyPiece < 0)
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Quantity Piece harus lebih besar dari 0, ");
                }


                if (string.IsNullOrEmpty(ErrorMessage))
                {

                }

                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    productVM.Qty = Qty;
                    productVM.QtyPiece = QtyPiece;

                }
                else
                {
                    ErrorMessage = ErrorMessage.Remove(ErrorMessage.Length - 2);
                    var Error = new ExpandoObject() as IDictionary<string, object>;

                    Error.Add("BonNo", productVM.ReferenceNo);
                    Error.Add("Tanggal", productVM.Date);
                    Error.Add("Benang", productVM.MaterialName);
                    Error.Add("Lusi", productVM.Yarn1);
                    Error.Add("Pakan", productVM.Yarn2);
                    Error.Add("Lebar", productVM.Width);
                    Error.Add("JL", productVM.YarnType1);
                    Error.Add("JP", productVM.YarnType2);
                    Error.Add("AL", productVM.YarnOrigin1);
                    Error.Add("AP", productVM.YarnOrigin2);




                    ErrorList.Add(Error);
                }
            }

            if (ErrorList.Count > 0)
            {
                Valid = false;
            }

            return Tuple.Create(Valid, ErrorList);
        }

        public async Task UploadData(InventoryWeavingDocument data, string username)
        {

            IInventoryWeavingMovementService movement = ServiceProvider.GetService<IInventoryWeavingMovementService>();
            foreach (var i in data.Items)
            {
                MoonlayEntityExtension.FlagForCreate(i, username, USER_AGENT);
                MoonlayEntityExtension.FlagForUpdate(i, username, USER_AGENT);
            }
            MoonlayEntityExtension.FlagForCreate(data, username, USER_AGENT);
            MoonlayEntityExtension.FlagForUpdate(data, username, USER_AGENT);
            DbSet.Add(data);

            var result = await DbContext.SaveChangesAsync();
            foreach (var item in data.Items)
            {
                InventoryWeavingMovement movementModel = new InventoryWeavingMovement
                {
                    ProductOrderName = item.ProductOrderName,
                    BonNo = data.BonNo,
                    ReferenceNo = item.ReferenceNo,
                    Construction = item.Construction,
                    Grade = item.Grade,
                    Piece = item.Piece,
                    MaterialName = item.MaterialName,
                    WovenType = item.WovenType,
                    Width = item.Width,
                    Yarn1 = item.Yarn1,
                    Yarn2 = item.Yarn2,
                    YarnType1 = item.YarnType1,
                    YarnType2 = item.YarnType2,
                    YarnOrigin1 = item.YarnOrigin1,
                    YarnOrigin2 = item.YarnOrigin2,
                    UomId = item.UomId,
                    UomUnit = item.UomUnit,
                    Quantity = item.Quantity,
                    QuantityPiece = item.QuantityPiece,
                    ProductRemark = item.ProductRemark,
                    Type = data.Type,
                    InventoryWeavingDocumentId = data.Id,
                    InventoryWeavingDocumentItemId = item.Id
                    //await BulkInsert(data, username);
                };
                MoonlayEntityExtension.FlagForCreate(movementModel, username, USER_AGENT);
                MoonlayEntityExtension.FlagForUpdate(movementModel, username, USER_AGENT);
                DbSet2.Add(movementModel);
            }
            var result2 = await DbContext.SaveChangesAsync();
        }


        //public async Task<InventoryWeavingDocumentViewModel> MapToViewModel(List<InventoryWeavingDocumentCsvViewModel> csv, DateTimeOffset date, string from)
        public async Task<InventoryWeavingDocumentViewModel> MapToViewModel(List<InventoryWeavingDocumentCsvViewModel> csv, string from)
        {
            List<InventoryWeavingDocumentItemViewModel> DocsItems = new List<InventoryWeavingDocumentItemViewModel>();
            //var itemx = GetItem();

            foreach (var i in csv)
            {

                //var MaterialName =  i.MaterialName.TrimEnd().Replace(" ", String.Empty);
                var MaterialName = i.MaterialName.Trim();
                var WovenType = i.WovenType.Trim();
                
                var Yarn1 = i.Yarn1.Trim();
                var Yarn2 = i.Yarn2.Trim();
                var Width = i.Width.Trim();
                var YarnType1 = i.YarnType1.Trim();
                var YarnType2 = i.YarnType2.Trim();
                var YarnOrigin1 = i.YarnOrigin1.Trim();
                var YarnOrigin2 = i.YarnOrigin2.Trim();

                var constructonC = MaterialName + "  " + WovenType + "  " + Yarn1 + "  " + Yarn2 + "  " + Width + "  "
                                   + YarnType1 + "  " + YarnType2 + "  " + YarnOrigin1 + "  " + YarnOrigin2;

                    DocsItems.Add(new InventoryWeavingDocumentItemViewModel
                    {
                        productOrderNo = i.ProductionOrderNo,
                        referenceNo = i.ReferenceNo,

                        construction = constructonC,
                        grade = i.Grade,
                        piece = i.Piece,
                        materialName = MaterialName,
                        wovenType = WovenType,
                        width = Width,
                        yarn1 = Yarn1,
                        yarn2 = Yarn2,
                        yarnType1 = YarnType1,
                        yarnType2 = YarnType2,
                        yarnOrigin1 = YarnOrigin1,
                        yarnOrigin2 = YarnOrigin2,
                        uomId = 35,
                        uomUnit = "MTR",
                        quantity = Convert.ToDouble(i.Qty),
                        quantityPiece = Convert.ToDouble(i.QtyPiece)
                       
                    });
                

            }

            InventoryWeavingDocumentViewModel sPKDocsViews = new InventoryWeavingDocumentViewModel
            {
                bonNo = GenerateBon(from, csv.FirstOrDefault().Date),
                date = csv.FirstOrDefault().Date,
                bonType = from == "PRODUKSI" ? "PRODUKSI" : from == "RETUR PACKING" ? "PACKING"
                          : from == "RETUR FINISHING" ? "FINISHING" : from == "RETUR PRINTING" ? "PRINTING"
                          : from == "RECHEKING" ? "RECHEKING" : "LAIN-LAIN",
                storageId = 105,
                storageCode = "DNWDX2GZ",
                storageName = "WEAVING 2 (EX. WEAVING 3) / WEAVING",
                remark = "",
                type = "IN",

                items = DocsItems
            };

            return sPKDocsViews;
        }


        public async Task<InventoryWeavingDocument> MapToModel(InventoryWeavingDocumentViewModel data)
        {

            InventoryWeavingDocument model = new InventoryWeavingDocument
            {
                BonNo = data.bonNo,
                BonType = data.bonType,
                Date = data.date,
                StorageId = data.storageId,
                StorageCode = data.storageCode,
                StorageName = data.storageName,
                Remark = data.remark,
                Type = data.type,
                Items = data.items.Select(item => new InventoryWeavingDocumentItem()
                {
                    Id = item.Id,
                    Active = item.Active,
                    _CreatedBy = item._CreatedBy,
                    _CreatedUtc = item._CreatedUtc,
                    _CreatedAgent = item._CreatedAgent,
                    _LastModifiedBy = item._LastModifiedBy,
                    _LastModifiedUtc = item._LastModifiedUtc,
                    _LastModifiedAgent = item._LastModifiedAgent,
                    _IsDeleted = item._IsDeleted,
                    ProductOrderName = item.productOrderNo,
                    ReferenceNo = item.referenceNo,
                    Construction = item.construction,
                    Grade = item.grade,
                    Piece = item.piece,
                    MaterialName = item.materialName,
                    WovenType = item.wovenType,
                    Width = item.width,
                    Yarn1 = item.yarn1,
                    Yarn2 = item.yarn2,
                    YarnType1 = item.yarnType1,
                    YarnType2 = item.yarnType2,
                    YarnOrigin1 = item.yarnOrigin1,
                    YarnOrigin2 = item.yarnOrigin2,

                    UomId = item.uomId,
                    UomUnit = item.uomUnit,
                    Quantity = item.quantity,
                    QuantityPiece = item.quantityPiece,
                    ProductRemark = item.productRemark,
                    InventoryWeavingDocumentId = item.InventoryWeavingDocumentId
                }).ToList()
            };
            return model;

        }

        private string GenerateBon(string from, DateTimeOffset date)
        {


            var type = from == "PRODUKSI" ? "PR" : from == "RETUR PACKING" ? "PC" : from == "RETUR FINISHING" ? "RF"
                      : from == "RETUR PRINTING" ? "RP" : from == "RECHEKING" ? "RC" : "LL";

            var totalData = DbSet.Count(s => s.BonNo.Substring(0, 2) == type && s._CreatedUtc.Year == date.Date.Year) + 1;

            if (totalData == 0)
            {
                return string.Format("{0}.{1}.{2}", type, date.ToString("yy"), "0001");

            }
            else
            {
                return string.Format("{0}.{1}.{2}", type, date.ToString("yy"), totalData.ToString().PadLeft(4, '0'));
            }
        }

        private InventoryWeavingDocumentDetailViewModel MapToViewModelById(InventoryWeavingDocument model)
        {
            var vm = new InventoryWeavingDocumentDetailViewModel()
            {
                Active = model.Active,
                Id = model.Id,
                _CreatedAgent = model._CreatedAgent,
                _CreatedBy = model._CreatedBy,
                _CreatedUtc = model._CreatedUtc,
                _IsDeleted = model._IsDeleted,
                _LastModifiedAgent = model._LastModifiedAgent,
                _LastModifiedBy = model._LastModifiedBy,
                _LastModifiedUtc = model._LastModifiedUtc,
                BonNo = model.BonNo,
                Date = model.Date,
                BonType = model.BonType,
                StorageId = model.StorageId,
                StorageCode = model.StorageCode,
                StorageName = model.StorageName,
                Type = model.Type,

                Detail = model.Items.GroupBy(x => x.ReferenceNo).Select(item => new InventoryWeavingItemDetailViewModel()
                {
                    ReferenceNo = item.Key,
                    Construction = item.First().Construction,
                    ProductOrderNo = item.First().ProductOrderName,
                    Year = item.First().ProductOrderName.Substring(item.First().ProductOrderName.Length - 4, 4),

                    ListItems = item.Select(s => new ItemListDetailViewModel()
                    {
                        Active = s.Active,
                        _CreatedAgent = s._CreatedAgent,
                        _CreatedBy = s._CreatedBy,
                        _CreatedUtc = s._CreatedUtc,

                        Id = s.Id,
                        _IsDeleted = s._IsDeleted,
                        _LastModifiedAgent = s._LastModifiedAgent,
                        _LastModifiedBy = s._LastModifiedBy,
                        _LastModifiedUtc = s._LastModifiedUtc,

                        Grade = s.Grade,
                        Piece = s.Piece,
                        MaterialName = s.MaterialName,
                        WovenType = s.WovenType,
                        Yarn1 = s.Yarn1,
                        Yarn2 = s.Yarn2,
                        YarnType1 = s.YarnType1,
                        YarnType2 = s.YarnType2,
                        YarnOrigin1 = s.YarnOrigin1,
                        YarnOrigin2 = s.YarnOrigin2,
                        YarnOrigin = s.YarnOrigin1 + " / " + s.YarnOrigin2,
                        Width = s.Width,
                        UomUnit = s.UomUnit,
                        Quantity = s.Quantity,
                        QuantityPiece = s.QuantityPiece,
                        ProductRemark = s.ProductRemark
                    }).ToList()
                }).ToList()
            };
            return vm;
        }
        public InventoryWeavingDocumentDetailViewModel ReadById(int id)
        {
            var data = this.DbSet.Where(d => d.Id.Equals(id) && d._IsDeleted.Equals(false))
                 .Include(p => p.Items).FirstOrDefault();


            if (data == null)
                return null;

            InventoryWeavingDocumentDetailViewModel DocsItems = MapToViewModelById(data);


            return DocsItems;
        }

           public ListResult<InventoryWeavingItemViewModel>ReadInputWeaving(string bonType, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int page, int size, string order, int offset)
           {

               IQueryable<InventoryWeavingDocument> Query = this.DbSet;

               List<string> SelectedFields = new List<string>()
                   {
                      "date", "bonNo", "construction", "grade", "piece", "quantity", "quantityPiece"
                   };

            DateTimeOffset DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTimeOffset)dateFrom;
            DateTimeOffset DateTo = dateTo == null ? DateTime.Now : (DateTimeOffset)dateTo;

            var query = (from a in DbContext.InventoryWeavingDocuments
                         join b in DbContext.InventoryWeavingDocumentItems on a.Id equals b.InventoryWeavingDocumentId
                         where a._IsDeleted == false
                            && b._IsDeleted == false
                            && a.BonType == (string.IsNullOrWhiteSpace(bonType) ? a.BonType : bonType)
                            && a.Type == "IN"
                              && a.Date.AddHours(offset).Date >= DateFrom.Date
                             && a.Date.AddHours(offset).Date <= DateTo.Date
                         orderby a.Date, a._CreatedUtc ascending
                         select new InventoryWeavingItemViewModel
                         {
                             Date = a.Date,
                             BonNo = a.BonNo,
                             Construction = b.Construction,
                             Grade = b.Grade,
                             Piece = b.Piece,
                             Quantity = b.Quantity,
                             QuantityPiece = b.QuantityPiece
                         })
                         ;
            var data = query.Skip((page - 1) * size).Take(size);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
               Query = QueryHelper<InventoryWeavingDocument>.Order(Query, OrderDictionary);


            return new ListResult<InventoryWeavingItemViewModel>(data.ToList(), page, size, data.Count());
            
           }

        }   
}