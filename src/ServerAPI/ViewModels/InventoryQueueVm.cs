using System;
using System.Collections.Generic;
using GenerateTables.Models;
namespace ServerAPI.ViewModels
{
    public class QueueInProgress
    {
        public string QueueInmateType { get; set; }
        public int? QueueInmateCount { get; set; }
    }

    public class QueueInRelease
    {
        public string QueueInmateType { get; set; }
        public int? QueueInmateCount { get; set; }
        public string InventorySiteOptions { get; set; }
        public InventoryQueueDetailsVm InventoryQueueDetailsVm { get; set; }
    }
         
    public class InventoryQueueIntakeDetails : Incarceration
    {
        public int? PersonalInventoryBinId { get; set; }
        public int? PersonId { get; set; }
        public int? FacilityId { get; set; }
        public int? ItemsCount { get; set; }
        public int InmateIds { get; set; }
        public List<string> ItemsBinName { get; set; }
        public string PersonalBinName { set; get; }
        public int DeleteFlag { set; get; }
        public string InmateNumber { get; set; }
        public string InmatePreBookNumber { get; set; }
        public int? InmatePreBookId { get; set; }
        public int? HousingUnitId { get; set; }
        public string PhotoFilePath { get; set; }
        public HousingDetail HousingDetails { get; set; }
        public int? InmateCurrentTrakId { get; set; }
        public string InmateCurrentTrak { get; set; }
        public int? IntakeCompleteFlagId { get; set; }
        public PrivilegeDetailsVm PrivilegesDetails { get; set; }
        public PersonInfoVm PersonInfoDetails { get; set; }
        public DateTime? InventoryReturnDate { get; set; }
        public string BinName { get; set; }
        public int? InventoryDispositionCode { get;  set; }
    } 

    public class InventoryQueueDetailsVm 
    {
        public List<InventoryQueueIntakeDetails> InventoryQueueIntakeDetails { get; set; }
        public List<PersonVm> PersonInfos { get; set; }
        public List<HousingDetail> HousingDetails { get; set; }
        public List<InventoryQueueIntakeDetails> LstInventory { get; set; }
        public List<BinNameVm> BinNameList { get; set; }
    }

    public class InventoryQueueVm
    {
        public List<QueueInProgress> QueueInProgress { get; set; }
        public List<BinViewerDetails> BinReceivingDetails { get; set; }
        public List<BinViewerDetails> BinFacilityTransferDetails { get; set; }
        public List<QueueInRelease> QueueInRelease { get; set; }
         public List<QueueInRelease> QueueScheduled { get; set; }
public List<InventoryQueueForms> InventoryQueueForms {get;set;}
        public string SiteOptionId { get; set; }
    }

    public class BinNameVm
    {
     

        public int? InmateId { get; set; }
        public List<string> BinName { get; set; }
        public int BinCount { get; set; }


    }

    public enum InventoryQueue
    {       
        Intake = 1,
        Booking=2,
        NoHousing=3,
        Release=4,
        BookAndRelease=5,
        StandardRelease=6,
        TransportRelease = 7,
        Assessment =8,
        SchStandardRelease=9,
        SchTransportRelease = 10,
    }

    public class InventoryQueueCheckVm
    {
        public InventoryQueue InventoryQueue { get; set; }
    }

    public class InventoryQueueForms
    {
        public string PropertyFormName {get;set;}
        public int ReadyQueueCount {get;set;}
        public int PendingQueueCount {get;set;} 
    }

    public class InventoryFormsDetails
    {      
       public int FormRecordId {get; set;}
       public string FormName { get; set; } 
       public string FormNote { get; set; }
       public int InmateId {get;set;} 
       public int PersonId {get;set;}
       public string InmateNumber {get; set;}
       public string InmateName {get;set;}
       public string CreatedBy {get;set;}
       public DateTime? CreatedDate {get;set;}
       public  string UpdatedBy {get;set;}
       public DateTime? UpdatedDate {get;set;}
       public string ClearedBy {get;set;}
       public DateTime? ClearedDate {get;set;}
       public int TemplateId {get;set;}     

       public int IncarcerationId {get;set;}
       public int ArrestId {get;set;}
      // public Signature SignValues { get; set; }

    }
}
