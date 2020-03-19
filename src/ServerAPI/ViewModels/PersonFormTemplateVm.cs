using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class PersonFormTemplateVm
    {
        public string TemplateName { get; set; }
        public bool RequireBookingSelect { get; set; }
        public string TemplateSql { get; set; }
        public int PersonFormTemplateId { get; set; }
        public bool DeleteFlag { get; set; }
        public bool ShowInLabel { get; set; }
        public bool ShowInPersonnel { get; set; }
        public PersonFormTemplateDetailVm PersonFormTemplateDetailVm { get; set; }
    }

    public class PersonFormTemplateDetailVm
    {
        public int PersonFormTemplateId { get; set; }
        public List<PersonFormTemplateCtrlVm> LstPersonFormTemplateCtrlVm { get; set; }
        public Dictionary<string, object> TemplateData { get; set; }
    }

    public class PersonFormTemplateCtrlVm
    {
        public string Type { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Font { get; set; }
        public int FontSize { get; set; }
        public string ForeColor { get; set; }
        public string Backcolor { get; set; }
    }
}
