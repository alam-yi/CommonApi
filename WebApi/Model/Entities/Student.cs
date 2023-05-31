using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Student
    {
        [Key]
        public int ID { get; set; }
        [Display(Name = "姓名")]
        public string Name { get; set; }
        [Display(Name = "年龄")]
        public int Age { get; set; }
    }
}
