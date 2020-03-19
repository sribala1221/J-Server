namespace ServerAPI.ViewModels
{
    public class AppAoUserControlFieldsVm
    {
        public int AppAoUserControlFieldsId { get; set; }
        public int AppAoUserControlId { get; set; }
        public bool FieldVisible { get; set; }
        public bool FieldRequired { get; set; }
        public string FieldLabel { get; set; }
        public string FieldTag { get; set; }
    }
}
